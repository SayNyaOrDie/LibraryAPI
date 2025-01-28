using WebApplication3.Enums;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces;

namespace WebApplication3.Services
{
    public class FineService : IFineService
    {
        public decimal CalculateDamageFine(List<BookDamage> damages, decimal bookPrice)
        {
            decimal totalFine = 0m;

            foreach (var damage in damages)
            {
                totalFine += CalculateFine(damage.Rate, bookPrice);
            }

            return totalFine;
        }

        public decimal CalculateOverdueFine(int daysBorrowed)
        {
            const int maxAllowedDays = 14;
            const decimal dailyFineRate = 0.5m;

            if (daysBorrowed > maxAllowedDays)
            {
                var overdueDays = daysBorrowed - maxAllowedDays;
                return overdueDays * dailyFineRate;
            }

            return 0m;
        }

        private decimal CalculateFine(BookDamageRate damageRate, decimal bookPrice)
        {
            const decimal lightMultiplier = 0.1m;
            const decimal mediumMultiplier = 0.5m;
            const decimal criticalMultiplier = 0.7m;

            return damageRate switch
            {
                BookDamageRate.Light => bookPrice * lightMultiplier,
                BookDamageRate.Medium => bookPrice * mediumMultiplier,
                BookDamageRate.Critical => bookPrice * criticalMultiplier,
                BookDamageRate.Lost => bookPrice,
                _ => throw new LibraryException($"Unknown damage rate: {damageRate}")
            };
        }
    }
}