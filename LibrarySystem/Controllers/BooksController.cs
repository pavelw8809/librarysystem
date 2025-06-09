using LibrarySystem.DbContexts;
using LibrarySystem.Models;
using LibrarySystem.Services;
using Microsoft.AspNetCore.Mvc;

// All the action errors are handled by ErrorHandlerMiddleware.
// All the more complicated data retrieve actions are handled by BookService to keep the controllers code more readable and clean.

namespace LibrarySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LibraryContext _libraryContext;
        private readonly BookService _bookService;

        public BooksController(LibraryContext libraryContext, BookService bookService)
        {
            _libraryContext = libraryContext;
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNo = 1, [FromQuery] int? pageSize = 10, [FromQuery] string sortBy = "Id", [FromQuery] string sortOrder = "asc")
        {
            if (pageNo <= 0 || pageSize <= 0) { 
                return BadRequest();
            }

            var (books, totalItems, totalPages, currentPage, currentSize) = await _bookService.GetAllBooksAsync(pageNo, pageSize, sortBy, sortOrder);

            var result = new
            {
                totalItems,
                totalPages,
                currentPage,
                itemsPerPage = currentSize,
                books
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var book = await _libraryContext.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewBook([FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _libraryContext.Books.Add(book);
            await _libraryContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook([FromRoute] int id, [FromBody] Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _bookService.UpdateBookAsync(id, book);

            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> ChangeBookStatus([FromRoute] int id, [FromBody] string bookStatus)
        {
            await _bookService.ChangeBookStatusAsync(id, bookStatus);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _libraryContext.Books.FindAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            _libraryContext.Books.Remove(book);
            await _libraryContext.SaveChangesAsync();

            return NoContent();
        }
    }
}
