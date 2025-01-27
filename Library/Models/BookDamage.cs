using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Enums;

namespace WebApplication3.Models
{
    public class BookDamage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }
        [ForeignKey("Book")]
        public int BookId { get; set; }
        [Required]
        public string Description { get; set; }
        public DateOnly DateReported { get; set; }
        [Required]
        public BookDamageRate Rate { get; set; }

        public BookDamage() { }
        public BookDamage(int bookId, string description, DateOnly dateReported, BookDamageRate rate)
        {
            BookId = bookId;
            Description = description;
            DateReported = dateReported;
            Rate = rate;
        }
    }
}
