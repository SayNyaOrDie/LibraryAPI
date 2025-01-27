using WebApplication3.Models;

namespace WebApplication3.Services.Interfaces
{
    public interface IVisitorService
    {
        Visitor GetVisitorById(int visitorId);
        IEnumerable<Visitor> GetAllVisitors();
        void AddVisitor(Visitor visitor);
        void UpdateVisitorDetails(int id, Visitor visitor);
        void AddTransactionToVisitor(int visitorId, Transaction transaction);
        void AddFineToDebt(int visitorId, decimal fine);
        void PayOffDebt(int visitorId);
        void DeleteVisitor(Visitor visitor);

    }
}
