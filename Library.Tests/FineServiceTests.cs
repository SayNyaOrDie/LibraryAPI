using WebApplication3.Enums;
using WebApplication3.Exceptions;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Tests
{
    public class FineServiceTests
    {
        private readonly FineService _fineService;

        public FineServiceTests()
        {
            _fineService = new FineService();
        }

        [Theory]
        [InlineData(BookDamageRate.Light, 300, 60)]
        [InlineData(BookDamageRate.Medium, 300, 180)] 
        [InlineData(BookDamageRate.Critical, 300, 240)]
        [InlineData(BookDamageRate.Lost, 300, 330)]
        public void CalculateDamageFine_ShouldCalculateCorrectFine(BookDamageRate damageRate, decimal bookPrice, decimal expectedFine)
        {
            var damages = new List<BookDamage>
            {
                new BookDamage { Rate = damageRate, Description = "Test damage" },
                new BookDamage { Rate = BookDamageRate.Light, Description = "Test damage" }
            };

            var result = _fineService.CalculateDamageFine(damages, bookPrice);

            Assert.Equal(expectedFine, result);
        }


        [Theory]
        [InlineData(10, 0)] 
        [InlineData(15, 0.5)] 
        [InlineData(20, 3)]
        public void CalculateOverdueFine_ShouldCalculateCorrectFine(int daysBorrowed, decimal expectedFine)
        {
            // Выполнение метода
            var result = _fineService.CalculateOverdueFine(daysBorrowed);

            // Проверка результата
            Assert.Equal(expectedFine, result);
        }

        [Fact]
        public void CalculateDamageFine_ShouldThrowExceptionForUnknownDamageRate()
        {
            var damages = new List<BookDamage>
            {
                new BookDamage { Rate = (BookDamageRate)101, Description = "Unknown damage" }
            };

            Assert.Throws<LibraryException>(() => _fineService.CalculateDamageFine(damages, 300));
        }
    }
}
