using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Enums;

namespace WebApplication3.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Visitor")]
        public int VisitorId { get;  set; }

        [ForeignKey("Book")]
        public int BookId { get;  set; }

        [Required]
        public DateOnly Date { get; set; }
            
        [Required]
        public TransactionStatus TransactionStatus { get; set; }

        public Transaction() { }

        public Transaction(int visitorId, int bookId, DateOnly date, TransactionStatus status)
        {
            VisitorId = visitorId;
            BookId = bookId;
            Date = date;
            TransactionStatus = status;
        }
    }
}
