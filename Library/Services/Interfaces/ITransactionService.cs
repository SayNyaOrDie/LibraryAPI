using WebApplication3.Models;

namespace WebApplication3.Services.Interfaces
{
    public interface ITransactionService
    {
        Transaction GetTransactionById(int transactionId);
        List<Transaction> GetAllTransactions();
        void BorrowBook(int visitorId, int bookId, DateOnly borrowDate);
        public IEnumerable<Transaction> GetTransactionsByDateRange(DateOnly startDate, DateOnly endDate);
        decimal ReturnBook(int visitorId, int bookId, DateOnly returnDate, List<BookDamage> damages);
    }
}
