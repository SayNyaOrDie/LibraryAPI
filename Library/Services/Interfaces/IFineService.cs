using WebApplication3.Models;

namespace WebApplication3.Services.Interfaces
{
    public interface IFineService
    {
        decimal CalculateDamageFine(List<BookDamage> damages, decimal bookPrice);
        decimal CalculateOverdueFine(int daysBorrowed);
    }
}