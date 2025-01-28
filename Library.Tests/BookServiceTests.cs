using WebApplication3.Enums;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.Data;
using Moq;
using Xunit;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Tests
{
    public class BookServiceTests
    {
        private readonly BookService _bookService;
        private readonly ApplicationDbContext _dbContext;

        public BookServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _bookService = new BookService(_dbContext);
        }


        [Theory]
        [InlineData("Test Book", "Test", "Author", 100, "2025-01-01", "2025-01-28", true)] // Правильные данные
        [InlineData("Test Book", "", "Author", 100, "2025-01-01", "2025-01-28", false)]  // Неправильное имя автора 
        [InlineData("Test Book", "Test", "", 100, "2025-01-01", "2025-01-28", false)]  // Неправильная фамилия автора 
        [InlineData("Test Book", "Test", "Author", -100, "2025-01-01", "2025-01-28", false)]  // Неправильная цена
        public void AddBook_ShouldAddBook_WhenValidData(
            string title,
            string authorName,
            string authorSurname,
            decimal price,
            string publishDate,
            string dateAdded,
            bool shouldSucceed)
        {
            var book = new Book
            {
                Title = title,
                Author = new Author { Name = authorName, Surname = authorSurname },
                Price = price,
                PublishDate = DateOnly.Parse(publishDate),
                DateAdded = DateOnly.Parse(dateAdded),
                Genre = "genre"
            };

            if (shouldSucceed)
            {
                _bookService.AddBook(book);
                var addedBook = _dbContext.Books.FirstOrDefault(b => b.Title == title);
                Assert.NotNull(addedBook);
            }
            else
            {
                Assert.Throws<LibraryException>(() => _bookService.AddBook(book));
            }
        }

        [Theory]
        [InlineData("Updated Book", "Updated", "Author", 150, "2025-01-01", true)]
        [InlineData("Invalid Book", "", "", -50, "2025-01-01", false)] 
        public void UpdateBookDetails_ShouldUpdateBook_WhenValidData(
            string title,
            string authorName,
            string authorSurname,
            decimal price,
            string publishDate,
            bool shouldSucceed)
        {
            var book = new Book
            {
                Title = "Original Title",
                Author = new Author { Name = "Original", Surname = "Author" },
                Price = 100,
                PublishDate = DateOnly.FromDateTime(DateTime.Now),
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                Genre = "genre"
            };
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();

            var updatedBook = new Book
            {
                Title = title,
                Author = new Author { Name = authorName, Surname = authorSurname },
                Price = price,
                PublishDate = DateOnly.Parse(publishDate),
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                Genre = "genre"
            };

            if (shouldSucceed)
            {
                _bookService.UpdateBookDetails(book.Id, updatedBook);
                var result = _dbContext.Books.First(b => b.Id == book.Id);
                Assert.Equal(title, result.Title);
            }
            else
            {
                Assert.Throws<LibraryException>(() => _bookService.UpdateBookDetails(book.Id, updatedBook));
            }
        }

        [Fact]
        public void DeleteBook_ShouldRemoveBook_WhenBookExists()
        {
            
            var book = new Book
            {
                Title = "Book to be deleted",
                Author = new Author { Name = "Author", Surname = "To Be Deleted" },
                Price = 100,
                PublishDate = DateOnly.FromDateTime(DateTime.Now),
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                Genre = "TestGenre"
            };
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();
            _bookService.DeleteBook(book);
            var result = _dbContext.Books.FirstOrDefault(b => b.Id == book.Id);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(true)] 
        [InlineData(false)] 
        public void GetBooksByAuthor_ShouldReturnBooks_WhenBooksExist(bool shouldSucceed)
        {
            var author = new Author { Name = "authorName", Surname = "authorSurname" };
            var book = new Book
            {
                Title = "Test Book",
                Author = author,
                Price = 100,
                PublishDate = DateOnly.FromDateTime(DateTime.Now),
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                Genre = "genre"
            };
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();

            if (shouldSucceed)
            {
                var result = _bookService.GetBooksByAuthor(author);
                Assert.Contains(result, b => b.Title == "Test Book");
            }
            else
            {
                Assert.Throws<LibraryException>(() => _bookService.GetBooksByAuthor(new Author { Name = "1", Surname = "2" }));
            }
        }

        [Theory]
        [InlineData(1, BookStatus.NotAvailable, true)]
        [InlineData(999, BookStatus.NotAvailable, false)]
        public void UpdateBookAvailability_ShouldUpdateAvailability(int bookId, BookStatus status, bool shouldSucceed)
        {
            var book = new Book
            {
                Title = "Available Book",
                Author = new Author { Name = "Author", Surname = "Name" },
                Price = 100,
                PublishDate = DateOnly.FromDateTime(DateTime.Now),
                DateAdded = DateOnly.FromDateTime(DateTime.Now),
                Genre = "TestGenre"
            };
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();

            if (shouldSucceed)
            {
                _bookService.UpdateBookAvailability(bookId, status);
                var updatedBook = _dbContext.Books.First(b => b.Id == bookId);
                Assert.Equal(status, updatedBook.Status);
            }
            else
            {
                Assert.Throws<LibraryException>(() => _bookService.UpdateBookAvailability(bookId, status));
            }
        }
    }
}
