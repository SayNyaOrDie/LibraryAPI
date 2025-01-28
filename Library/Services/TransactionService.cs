using WebApplication3.Data;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces;
using WebApplication3.Enums;
using WebApplication3.Exceptions;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly IBookService _bookService;
    private readonly IVisitorService _visitorService;
    private readonly IFineService _fineService;

    public TransactionService(ApplicationDbContext context, IBookService bookService, 
        IVisitorService visitorService, IFineService fineService)
    {
        _context = context;
        _bookService = bookService;
        _visitorService = visitorService;
        _fineService = fineService;
    }

    public Transaction GetTransactionById(int transactionId)
    {
        var transaction = _context.Transactions.Find(transactionId);
        return transaction == null ?
            throw new LibraryException("Transaction not found") : transaction;
    }

    public List<Transaction> GetAllTransactions()
    {
        return [.. _context.Transactions];
    }

    public void BorrowBook(int visitorId, int bookId, DateOnly borrowDate)
    {
        var visitor = _visitorService.GetVisitorById(visitorId);
        var book = _bookService.GetBookById(bookId);

        if (visitor.Debt > 0m)
            throw new LibraryException("Visitor cannot borrow books due to unpaid debt");
        if (book.DateAdded > borrowDate)
            throw new LibraryException("Can't borrow a book before its addition to the library");

        if (book.Status == BookStatus.NotAvailable)
            throw new LibraryException("The book is currently unavailable");
        else if (book.Status == BookStatus.Lost)
            throw new LibraryException("The book is lost and can't be taken");
        else if (book.Status == BookStatus.Decommissioned)
            throw new LibraryException("The book is decommissioned and can't be taken");

        var borrowTransaction = new Transaction(visitorId, bookId, borrowDate, TransactionStatus.Borrowed);
        _context.Transactions.Add(borrowTransaction);
        _visitorService.AddTransactionToVisitor(visitorId, borrowTransaction);

        _bookService.UpdateBookAvailability(bookId, BookStatus.NotAvailable);
        _context.SaveChanges();
    }

    public decimal ReturnBook(int visitorId, int bookId, DateOnly returnDate, List<BookDamage> damages)
    {
        var visitor = _visitorService.GetVisitorById(visitorId);
        var book = _bookService.GetBookById(bookId);

        var borrowTransaction = _context.Transactions
            .Where(t => t.VisitorId == visitorId &&
                        t.BookId == bookId &&
                        t.TransactionStatus == TransactionStatus.Borrowed)
            .OrderByDescending(t => t.Date)
            .FirstOrDefault() ?? throw new LibraryException("No active borrow transaction found for this book and visitor");

        if (borrowTransaction.Date > returnDate) 
            throw new LibraryException("Return date can't be earlier than borrow date");

        var daysBorrowed = (returnDate.ToDateTime(TimeOnly.MinValue) - borrowTransaction.Date.ToDateTime(TimeOnly.MinValue)).Days;
        decimal overdueFine = _fineService.CalculateOverdueFine(daysBorrowed);

        decimal damageFine = 0m;
        var returnTransaction = new Transaction(visitorId, bookId, returnDate, TransactionStatus.Returned);
        _visitorService.AddTransactionToVisitor(visitorId, returnTransaction);
        if (damages.Count != 0)
        {

            if (damages.Any(d => d.Rate == BookDamageRate.Lost))
            {
                _bookService.ReplaceBook(bookId, BookStatus.Lost);
                damageFine = book.Price;
                visitor.AddTransaction(returnTransaction);
                _context.SaveChanges();
                return damageFine;
            }

            damageFine = _fineService.CalculateDamageFine(damages, book.Price);
            
            if (damageFine >= book.Price)
            {
                _bookService.ReplaceBook(bookId, BookStatus.Decommissioned);
                damageFine = book.Price;
                visitor.AddTransaction(returnTransaction);
                _context.SaveChanges();
                return damageFine;
            }
        }


        var totalFine = overdueFine + damageFine;

        if (totalFine > 0)
            _visitorService.AddFineToDebt(visitorId, totalFine);


        if (book.Status == BookStatus.NotAvailable)
            _bookService.UpdateBookAvailability(bookId, BookStatus.Available);

        _bookService.HandleDamages(damages);

        visitor.AddTransaction(returnTransaction);
        _context.SaveChanges();

        return totalFine;
    }


    public IEnumerable<Transaction> GetTransactionsByDateRange(DateOnly startDate, DateOnly endDate)
    {
        if (startDate > endDate)
            throw new LibraryException("Start date cannot be later than end date.");

        return _context.Transactions
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .ToList();
    }

}
