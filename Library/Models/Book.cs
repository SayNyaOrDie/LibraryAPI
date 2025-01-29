using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Enums;
using WebApplication3.Exceptions;

public class Book
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Genre { get; set; }

    [Required]
    public DateOnly PublishDate { get; set; }

    [Required]
    public Author Author { get; set; }

    [Required]
    public decimal Price { get; set; }

    public BookStatus Status { get; set; } = BookStatus.Available;

    [Required]
    public DateOnly DateAdded { get; set; }

    public Book() { }

    public Book(string title, string genre, DateOnly publishDate, Author author, decimal price)
    {
        Title = title;
        Genre = genre;
        PublishDate = publishDate;
        Author = author;
        Price = price;
        Status = BookStatus.Available;
        DateAdded = DateOnly.FromDateTime(DateTime.Now);
    }

    public void ChangeStatus(BookStatus newStatus)
    {
        if (Status == BookStatus.Decommissioned)
            throw new LibraryException("Book is already decommissioned and its status can't be changed");
        else if (Status == BookStatus.Lost)
            throw new LibraryException("Book is already lost and its status can't be changed");
        else if (Status == BookStatus.NotAvailable && newStatus == BookStatus.NotAvailable)
            throw new LibraryException("Book is already not available");
        else if (Status == BookStatus.Available && newStatus == BookStatus.Available)
            throw new LibraryException("Book is already available");

        Status = newStatus;
    }
}
