using WebApplication3.Services.Interfaces;
using WebApplication3.Data;
using WebApplication3.Models;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Exceptions;

namespace WebApplication3.Services
{
    public class VisitorService : IVisitorService
    {
        private readonly ApplicationDbContext _context;

        public VisitorService(ApplicationDbContext context)
        {
            _context = context;
        }
         
        public Visitor GetVisitorById(int visitorId)
        {
            var visitor = _context.Visitors
                .Include(v => v.Transactions)
                .FirstOrDefault(v => v.Id == visitorId);

            return visitor == null ? throw new LibraryException("Visitor with not found") : visitor;
        }

        public IEnumerable<Visitor> GetAllVisitors()
        {
            return [.. _context.Visitors.Include(v => v.Transactions)];
        }

        public void AddVisitor(Visitor visitor)
        {
            if (visitor == null)
                throw new LibraryException("Visitor can't be null");

            _context.Visitors.Add(visitor);
            _context.SaveChanges();
        }

        public void UpdateVisitorDetails(int id, Visitor updatedVisitor)
        {
            var existingVisitor = _context.Visitors.FirstOrDefault(v => v.Id == id) ??
                throw new LibraryException($"Visitor with ID {id} not found");
            
            existingVisitor.FirstName = updatedVisitor.FirstName;
            existingVisitor.LastName = updatedVisitor.LastName;
            existingVisitor.Email = updatedVisitor.Email;

            _context.SaveChanges();
        }


        public void AddTransactionToVisitor(int visitorId, Transaction transaction)
        {
            if (transaction == null)
                throw new LibraryException("Transaction cannot be null");

            var visitor = GetVisitorById(visitorId);

            visitor.AddTransaction(transaction);

            _context.Visitors.Update(visitor);
            _context.Transactions.Add(transaction);
            _context.SaveChanges();
        }


        public void AddFineToDebt(int visitorId, decimal fine)
        {
            var visitor = GetVisitorById(visitorId);
            visitor.AddFineToDebt(fine);
            _context.Visitors.Update(visitor);
            _context.SaveChanges();
        }

        public void PayOffDebt(int visitorId)
        {
            var visitor = GetVisitorById(visitorId);
            visitor.PayOffDebt();
            _context.Visitors.Update(visitor);
            _context.SaveChanges();
        }

        public void DeleteVisitor(Visitor visitor)
        {
            _context.Visitors.Remove(visitor);
            _context.SaveChanges();
        }
    }
}