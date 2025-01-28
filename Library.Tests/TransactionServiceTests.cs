using Microsoft.EntityFrameworkCore;
using Moq;
using WebApplication3.Data;
using WebApplication3.Enums;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces;

namespace WebApplication3.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly Mock<IVisitorService> _mockVisitorService;
        private readonly Mock<IFineService> _mockFineService;
        private readonly TransactionService _transactionService;
        private readonly ApplicationDbContext _dbContext;

        public TransactionServiceTests()
        {
            _mockBookService = new Mock<IBookService>();
            _mockVisitorService = new Mock<IVisitorService>();
            _mockFineService = new Mock<IFineService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);

            _transactionService = new TransactionService(
                _dbContext,
                _mockBookService.Object,
                _mockVisitorService.Object,
                _mockFineService.Object);
        }

        private void ClearDatabase()
        {
            _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
            _dbContext.SaveChanges();
        }

        [Theory]
        [InlineData(1, 1, 1, BookStatus.Available, 0, false)] // Успешный случай: дата выдачи позже даты добавления
        [InlineData(2, 2, 1, BookStatus.NotAvailable, 0, true)] // Книга недоступна
        [InlineData(3, 3, 1, BookStatus.Lost, 0, true)] // Книга потеряна
        [InlineData(4, 4, 1, BookStatus.Available, 10, true)] // Задолженность у посетителя
        [InlineData(2, 2, 1, BookStatus.Decommissioned, 0, true)] // Книга списана
        [InlineData(2, 2, -10, BookStatus.Decommissioned, 0, true)] // Дата выдачи раньше даты добавления
        public void BorrowBook_ShouldHandleScenariosCorrectly(
            int visitorId,
            int bookId,
            int borrowDaysOffset,
            BookStatus bookStatus,
            decimal visitorDebt,
            bool shouldThrow)
        {

            ClearDatabase();
            var today = DateOnly.FromDateTime(DateTime.Today);
            var borrowDate = today.AddDays(borrowDaysOffset);

            var visitor = new Visitor
            {
                Id = visitorId,
                Debt = visitorDebt
            };

            var book = new Book
            {
                Id = bookId,
                Status = bookStatus,
                DateAdded = today
            };

            _mockVisitorService.Setup(v => v.GetVisitorById(visitorId))
                .Returns(visitor);

            _mockBookService.Setup(b => b.GetBookById(bookId))
                .Returns(book);

            if (shouldThrow)
            {
                Assert.Throws<LibraryException>(() =>
                    _transactionService.BorrowBook(visitorId, bookId, borrowDate));
            }
            else
            {
                _transactionService.BorrowBook(visitorId, bookId, borrowDate);
                _mockVisitorService.Verify(v => v.AddTransactionToVisitor(visitorId, It.IsAny<Transaction>()), Times.Once);
                _mockBookService.Verify(b => b.UpdateBookAvailability(bookId, BookStatus.NotAvailable), Times.Once);
            }
        }


        [Theory]
        [InlineData(1, 1, 1, false, false, false, 0)] // Успешный возврат без повреждений и штрафов
        [InlineData(2, 2, -5, false, false, true, 0)] // Ошибка: дата возврата раньше даты выдачи
        [InlineData(3, 3, 1, true, true, false, 300)] // Повреждения - потеря книги, штраф = цена книги
        public void ReturnBook_ShouldHandleScenariosCorrectly(
        int visitorId,
        int bookId,
        int returnDaysOffset,
        bool hasDamages,
        bool isLost,
        bool shouldThrow,
        decimal expectedFine)
        {
            ClearDatabase();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var borrowDate = today;
            var returnDate = today.AddDays(returnDaysOffset);

            var visitor = new Visitor
            {
                Id = visitorId,
                Debt = 0
            };

            var book = new Book
            {
                Id = bookId,
                Price = 300,
                Status = BookStatus.NotAvailable,
                DateAdded = today
            };

            var damages = new List<BookDamage>();

            if (hasDamages)
            {
                damages.Add(new BookDamage
                {
                    Rate = BookDamageRate.Lost,
                    Description = "Lost book"
                });
            }

            _mockVisitorService.Setup(v => v.GetVisitorById(visitorId))
                .Returns(visitor);

            _mockBookService.Setup(b => b.GetBookById(bookId))
                .Returns(book);

            var borrowTransaction = new Transaction(visitorId, bookId, borrowDate, TransactionStatus.Borrowed);
            _dbContext.Transactions.Add(borrowTransaction);
            _dbContext.SaveChanges();

            if (shouldThrow)
            {
                Assert.Throws<LibraryException>(() =>
                    _transactionService.ReturnBook(visitorId, bookId, returnDate, damages));
            }
            else
            {
                var totalFine = _transactionService.ReturnBook(visitorId, bookId, returnDate, damages);

                Assert.Equal(expectedFine, totalFine);

                if (isLost)
                {
                    _mockBookService.Verify(b => b.ReplaceBook(bookId, BookStatus.Lost), Times.Once);
                }

                if (expectedFine > 0 && !isLost)
                {
                    _mockVisitorService.Verify(v => v.AddFineToDebt(visitorId, expectedFine), Times.Once);
                }
            }
        }


        [Fact]
        public void ReturnBook_NoBorrowTransaction()
        {
            ClearDatabase();

            var visitorId = 1;
            var bookId = 1;
            var returnDate = DateOnly.FromDateTime(DateTime.Today);

            var visitor = new Visitor { Id = visitorId, Debt = 0 };
            var book = new Book { Id = bookId, Price = 300 };

            _mockVisitorService.Setup(v => v.GetVisitorById(visitorId)).Returns(visitor);
            _mockBookService.Setup(b => b.GetBookById(bookId)).Returns(book);
            Assert.Throws<LibraryException>(() =>
                _transactionService.ReturnBook(visitorId, bookId, returnDate, []));
        }

        [Theory]
        [InlineData("2025-01-01", "2025-01-31", 2)] // Успешный сценарий: 2 транзакции в заданном диапазоне
        [InlineData("2025-01-05", "2025-01-05", 1)] // Успешный сценарий: 1 транзакция в день
        [InlineData("2025-01-01", "2025-01-01", 0)] // Успешный сценарий: нет транзакций
        [InlineData("2025-02-01", "2025-01-31", 0)] // Ошибка: startDate позже endDate
        public void GetTransactionsByDateRange_ShouldHandleScenariosCorrectly(
                string startDateStr, string endDateStr, int expectedTransactionCount)
        {
            ClearDatabase();

            var startDate = DateOnly.Parse(startDateStr);
            var endDate = DateOnly.Parse(endDateStr);

            _dbContext.Transactions.RemoveRange(_dbContext.Transactions);
            _dbContext.SaveChanges();

            if (startDate <= endDate)
            {
                _dbContext.Transactions.AddRange(
                    new Transaction { Date = DateOnly.Parse("2025-01-05"), VisitorId = 1, BookId = 1, TransactionStatus = TransactionStatus.Borrowed },
                    new Transaction { Date = DateOnly.Parse("2025-01-15"), VisitorId = 2, BookId = 2, TransactionStatus = TransactionStatus.Borrowed }
                );
                _dbContext.SaveChanges();
            }

            if (startDate > endDate)
            {
                Assert.Throws<LibraryException>(() =>
                    _transactionService.GetTransactionsByDateRange(startDate, endDate));
            }
            else
            {
                var transactions = _transactionService.GetTransactionsByDateRange(startDate, endDate).ToList();
                Assert.Equal(expectedTransactionCount, transactions.Count);
            }
        }
    }
}