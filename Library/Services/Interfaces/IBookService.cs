using WebApplication3.Enums;
using WebApplication3.Models;

namespace WebApplication3.Services.Interfaces
{
    public interface IBookService
    {
        Book GetBookById(int bookId);
        void AddBook(Book book);
        List<Book> GetAllBooks();
        void DeleteBook(Book book);
        void UpdateBookDetails(int id, Book updatedBook);
        void UpdateBookAvailability(int bookId, BookStatus newAvailability);
        void ReplaceBook(int bookId, BookStatus status);
        List<Book> GetBooksByAuthor(Author author);
        List<BookDamage> GetBookDamages(int bookId);

    }
}
