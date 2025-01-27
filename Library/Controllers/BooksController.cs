using Microsoft.AspNetCore.Mvc;
using WebApplication3.Services.Interfaces;
using WebApplication3.Exceptions;
using WebApplication3.DTOs;
using Microsoft.Extensions.Logging;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        private bool IsInternetExplorer()
        {
            var userAgent = Request.Headers["User-Agent"].ToString();
            return userAgent.Contains("MSIE") || userAgent.Contains("Trident/");
        }

        [HttpPost("addNewBook")]
        public IActionResult AddBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                if (bookDTO == null)
                {
                    _logger.LogWarning("Received null book data.");
                    return BadRequest(new { message = "Invalid data" });
                }

                var book = new Book(
                    bookDTO.Title,
                    bookDTO.Genre,
                    bookDTO.PublishDate,
                    bookDTO.author,
                    bookDTO.Price
                );

                _bookService.AddBook(book);
                _logger.LogInformation($"Book with title '{bookDTO.Title}' added successfully.");

                return CreatedAtAction(nameof(GetBookById), new { id = book.Id }, book);
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Library error while adding book: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while adding book: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetBookById(int id)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                _logger.LogInformation($"Fetching book with ID {id}.");
                var book = _bookService.GetBookById(id);
                if (book == null)
                {
                    _logger.LogWarning($"Book with ID {id} not found.");
                    return NotFound(new { message = $"Book with ID {id} not found" });
                }
                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching book with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("getAllBooks")]
        public IActionResult GetAllBooks()
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                _logger.LogInformation("Fetching all books.");
                var books = _bookService.GetAllBooks();
                if (books == null || books.Count == 0)
                {
                    _logger.LogWarning("No books found.");
                    return NotFound(new { message = "No books found." });
                }
                return Ok(books);
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Library error while fetching all books: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching all books: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpGet("author/getBooksByAuthor")]
        public IActionResult GetBooksByAuthor([FromQuery] string name, [FromQuery] string surname)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                _logger.LogInformation($"Fetching books for author: {name} {surname}");
                var author = new Author { Name = name, Surname = surname };
                var books = _bookService.GetBooksByAuthor(author);
                if (books == null || books.Count == 0)
                {
                    _logger.LogWarning($"No books found for author '{name} {surname}'.");
                    return NotFound(new { message = $"No books found for author '{name} {surname}'" });
                }
                return Ok(books);
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Library error while fetching books by author: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while fetching books by author: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpPut("{id}/update")]
        public IActionResult UpdateBook(int id, [FromBody] BookDTO bookDTO)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                if (bookDTO == null)
                {
                    _logger.LogWarning($"Received null data for updating book with ID {id}.");
                    return BadRequest(new { message = "Invalid data" });
                }

                var existingBook = _bookService.GetBookById(id);
                if (existingBook == null)
                {
                    _logger.LogWarning($"Book with ID {id} not found for update.");
                    return NotFound(new { message = $"Book with ID {id} not found" });
                }

                existingBook.Title = bookDTO.Title;
                existingBook.Genre = bookDTO.Genre;
                existingBook.PublishDate = bookDTO.PublishDate;
                existingBook.Author = bookDTO.author;
                existingBook.Price = bookDTO.Price;

                _bookService.UpdateBookDetails(id, existingBook);

                _logger.LogInformation($"Book with ID {id} updated successfully.");
                return Ok(new { message = $"Book with ID {id} has been successfully updated" });
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Library error while updating book with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while updating book with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBook(int id)
        {
            try
            {
                if (IsInternetExplorer())
                    return BadRequest(new { message = "Internet Explorer is not supported. Please use a modern browser." });


                _logger.LogInformation($"Deleting book with ID {id}.");
                var book = _bookService.GetBookById(id);
                if (book == null)
                {
                    _logger.LogWarning($"Book with ID {id} not found for deletion.");
                    return NotFound(new { message = $"Book with ID {id} not found" });
                }

                _bookService.DeleteBook(book);
                _logger.LogInformation($"Book with ID {id} deleted successfully.");
                return Ok(new { message = $"Book with ID {id} has been successfully deleted" });
            }
            catch (LibraryException ex)
            {
                _logger.LogWarning($"Library error while deleting book with ID {id}: {ex.Message}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error while deleting book with ID {id}: {ex.Message}");
                return StatusCode(500, new { message = "An unexpected error occurred", details = ex.Message });
            }
        }
    }
}
