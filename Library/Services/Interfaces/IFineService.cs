using WebApplication3.Models;

public interface IFineService
{
    decimal CalculateDamageFine(List<BookDamage> damages, decimal bookPrice);
    decimal CalculateOverdueFine(int daysBorrowed);
}
