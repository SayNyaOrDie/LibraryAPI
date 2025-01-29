using Microsoft.AspNetCore.Mvc;
using WebApplication3.Models;
using WebApplication3.Services.Interfaces;
using WebApplication3.Exceptions;
using WebApplication3.DTOs;
using WebApplication3.Enums;


namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        private bool IsInternetExplorer()
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            return userAgent.Contains("MSIE") || userAgent.Contains("Trident/");
        }

        [HttpGet("{id}")]
        public IActionResult GetTransactionById(int id)
        {
            if (IsInternetExplorer())
                return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });
            

            try
            {
                _logger.LogInformation($"Fetching transaction with ID {id}.");
                var transaction = _transactionService.GetTransactionById(id);
                return Ok(transaction);
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Transaction with ID {id} not found: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching transaction with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("getAllTransactions")]
        public IActionResult GetAllTransactions()
        {
            if (IsInternetExplorer())
                return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


            try
            {
                _logger.LogInformation("Fetching all transactions.");
                var transactions = _transactionService.GetAllTransactions();
                if (transactions == null || transactions.Count == 0)
                {
                    _logger.LogWarning("No transactions found.");
                    return NotFound(new { message = "No transactions found." });
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching all transactions: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPost("borrowTransaction")]
        public IActionResult BorrowBook(int visitorId, int bookId, DateOnly borrowDate)
        {
            if (IsInternetExplorer())
                return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


            try
            {
                _logger.LogInformation($"Processing borrow transaction: Visitor ID {visitorId}, Book ID {bookId}, Borrow Date {borrowDate}");
                _transactionService.BorrowBook(visitorId, bookId, borrowDate);
                return Ok(new { Message = "Book borrowed successfully" });
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Failed to borrow book: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while processing borrow transaction: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPost("returnTransaction")]
        public IActionResult ReturnBook(int visitorId, int bookId, DateOnly returnDate, [FromBody] List<BookDamageDto> damagesDto)
        {
            if (IsInternetExplorer())
                return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


            try
            {
                _logger.LogInformation($"Processing return transaction: Visitor ID {visitorId}, Book ID {bookId}, Return Date {returnDate}");

                var processedDamages = damagesDto.Select(damageDto =>
                {
                    if (damageDto.Rate < 0 || damageDto.Rate > 3)
                    {
                        _logger.LogWarning($"Invalid rate value for book ID {bookId}: {damageDto.Rate}");
                        throw new LibraryException($"Invalid rate value: {damageDto.Rate}");
                    }

                    return new BookDamage
                    {
                        BookId = bookId,
                        Description = damageDto.Description,
                        Rate = (BookDamageRate)damageDto.Rate,
                        DateReported = returnDate
                    };
                }).ToList();

                var fine = _transactionService.ReturnBook(visitorId, bookId, returnDate, processedDamages);
                _logger.LogInformation($"Return transaction completed for Visitor ID {visitorId}, Book ID {bookId} with Fine {fine}");
                return Ok(new { Message = "Transaction finished successfully", Fine = fine });
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Error in return transaction: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while processing return transaction: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("getTransactionsByDateRange")]
        public IActionResult GetTransactionsByDateRange(DateOnly startDate, DateOnly endDate)
        {
            if (IsInternetExplorer())
                return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });

            try
            {
                _logger.LogInformation($"Fetching transactions between {startDate} and {endDate}.");

                var transactions = _transactionService.GetTransactionsByDateRange(startDate, endDate);

                if (transactions == null || !transactions.Any())
                {
                    _logger.LogWarning($"No transactions found between {startDate} and {endDate}.");
                    return NotFound(new { message = "No transactions found in the given date range." });
                }

                return Ok(transactions);
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Invalid date range: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching transactions: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

    }
}
