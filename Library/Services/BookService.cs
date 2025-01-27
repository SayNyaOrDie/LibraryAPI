using WebApplication3.Enums;
using WebApplication3.Exceptions;
using WebApplication3.Services.Interfaces;
using WebApplication3.Data;
using WebApplication3.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class BookService : IBookService
{
    private readonly ApplicationDbContext _context;

    public BookService(ApplicationDbContext context)
    {
        _context = context;
    }

    public Book GetBookById(int bookId)
    {
        var book = _context.Books.Find(bookId);
        return book ?? throw new LibraryException("Book not found");
    }

    public void AddBook(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Author.Name) ||
            string.IsNullOrWhiteSpace(book.Author.Surname))
            throw new LibraryException("Author's name and surname cannot be null or empty");

        if (book.Price <= 0)
            throw new LibraryException("Price must be greater than zero");

        if (book.PublishDate == default)
            throw new LibraryException("PublishDate is required.");

        if (book.PublishDate > DateOnly.FromDateTime(DateTime.Now))
            throw new LibraryException("PublishDate cannot be in the future.");

        if (book.DateAdded == default)
            throw new LibraryException("DateAdded cannot be empty.");

        if (book.DateAdded == default)
        {
            book.DateAdded = DateOnly.FromDateTime(DateTime.Now);
        }

        _context.Books.Add(book);
        _context.SaveChanges();
    }

    public List<Book> GetAllBooks()
    {
        return _context.Books.ToList();
    }

    public void UpdateBookDetails(int id, Book updatedBook)
    {
        var existingBook = _context.Books.FirstOrDefault(b => b.Id == id) ??
            throw new LibraryException($"Book with ID {id} not found");

        if (string.IsNullOrWhiteSpace(updatedBook.Author.Name) ||
            string.IsNullOrWhiteSpace(updatedBook.Author.Surname))
            throw new LibraryException("Author's name and surname cannot be null or empty");

        if (updatedBook.Price <= 0)
            throw new LibraryException("Price must be greater than zero");

        if (updatedBook.PublishDate == default)
            throw new LibraryException("PublishDate is required.");

        if (updatedBook.PublishDate > DateOnly.FromDateTime(DateTime.Now))
            throw new LibraryException("PublishDate cannot be in the future.");

        existingBook.Title = updatedBook.Title;
        existingBook.Genre = updatedBook.Genre;
        existingBook.PublishDate = updatedBook.PublishDate;
        existingBook.Author = updatedBook.Author;
        existingBook.Price = updatedBook.Price;

        if (updatedBook.DateAdded == default)
            throw new LibraryException("DateAdded cannot be empty.");

        existingBook.DateAdded = updatedBook.DateAdded;

        _context.Books.Update(existingBook);
        _context.SaveChanges();
    }

    public void DeleteBook(Book book)
    {
        _context.Books.Remove(book);
        _context.SaveChanges();
    }

    public List<Book> GetBooksByAuthor(Author author)
    {
        if (string.IsNullOrWhiteSpace(author.Name) ||
            string.IsNullOrWhiteSpace(author.Surname))
        {
            throw new LibraryException("Invalid author details provided");
        }

        var books = _context.Books
            .Where(b => b.Author.Name == author.Name && b.Author.Surname == author.Surname)
            .ToList();

        if (!books.Any())
        {
            throw new LibraryException($"No books found for author {author.Name} {author.Surname}");
        }

        return books;
    }

    public List<BookDamage> GetBookDamages(int bookId)
    {
        var book = _context.Books.Find(bookId) ?? throw new LibraryException("Book not found");
        var damages = _context.BookDamages.Where(bd => bd.BookId == bookId).ToList();
        return damages;
    }

    public void UpdateBookAvailability(int bookId, BookStatus newAvailability)
    {
        var book = GetBookById(bookId);
        book.ChangeStatus(newAvailability);

        _context.Books.Update(book);
        _context.SaveChanges();
    }

    public void HandleDamages(IEnumerable<BookDamage> bookDamages)
    {
        foreach (var damage in bookDamages)
        {
            if (damage == null)
            {
                throw new LibraryException("Invalid damage entry.");
            }
            
            _context.BookDamages.Add(damage);
        }
    }

    public void ReplaceBook(int bookId, BookStatus status)
    {
        var oldBook = GetBookById(bookId);

        var newAuthor = new Author
        {
            Name = oldBook.Author.Name,
            Surname = oldBook.Author.Surname
        };

        var newBook = new Book(
            oldBook.Title,
            oldBook.Genre,
            oldBook.PublishDate,
            newAuthor,
            oldBook.Price
        )
        {
            DateAdded = DateOnly.FromDateTime(DateTime.Now) };

        _context.Books.Add(newBook);

        oldBook.ChangeStatus(status);

        _context.Books.Update(oldBook);

        _context.SaveChanges();
    }


}
