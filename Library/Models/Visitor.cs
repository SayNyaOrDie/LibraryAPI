using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebApplication3.Exceptions;

namespace WebApplication3.Models
{
    public class Visitor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public decimal Debt { get; set; } = 0;

        [NotMapped]
        public ICollection<Transaction> Transactions { get; private set; } = [];

        public Visitor() { }

        public Visitor(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
        }

        public void AddFineToDebt(decimal fine)
        {
            if (fine <= 0) throw new LibraryException("Fine amount must be greater than zero");
            Debt += fine;
        }

        public void PayOffDebt()
        {
            Debt = 0;
        }

    }
}
