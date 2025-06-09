using LibrarySystem.DbContexts;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services
{
    public class BookService
    {
        private readonly LibraryContext _libraryContext;

        public BookService(LibraryContext libraryContext)
        {
            _libraryContext = libraryContext;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            return await _libraryContext.Books.FindAsync(id);
        }

        public async Task UpdateBookAsync(int id, Book book)
        {
            var findExistingItem = await _libraryContext.Books.FindAsync(id);

            if (findExistingItem == null)
            {
                throw new KeyNotFoundException($"Book with id={id} was not found");
            }

            findExistingItem.Title = book.Title;
            findExistingItem.Author = book.Author;
            findExistingItem.ISBN = book.ISBN;
            findExistingItem.Status = book.Status;

            await _libraryContext.SaveChangesAsync();
        }

        public async Task<(List<Book>?, int totalItems, int totalPages, int currentPage, int currentSize)> GetAllBooksAsync(int? pageNo, int? pageSize, string sortBy, string sortOrder)
        {
            if (_libraryContext.Books == null)
            {
                throw new InvalidOperationException($"Context Error: Book context could not be initialized correclty");
            }

            IQueryable<Book> books = _libraryContext.Books.AsQueryable();

            int currentPage = pageNo ?? 1;
            int currentSize = pageSize ?? 10;
            string currentSortBy = sortBy ?? "Id";
            string currentSortOrder = sortOrder ?? "asc";

            int totalItems = await books.CountAsync();

            books = currentSortBy.ToLower() switch
            {
                "title" => currentSortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) ?
                            books.OrderBy(x => x.Title) :
                            books.OrderByDescending(x => x.Title),
                "author" => currentSortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) ?
                            books.OrderBy(x => x.Author) :
                            books.OrderByDescending(x => x.Author),
                _ => currentSortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase) ?
                     books.OrderBy(x => x.Id) :
                     books.OrderByDescending(x => x.Id)
            };

            int totalPages = (int)Math.Ceiling(totalItems/(double)currentSize);

            var result = await books
                .Skip((currentPage - 1) * currentSize)
                .Take(currentSize)
                .ToListAsync();

            return (result, totalItems, totalPages, currentPage, currentSize);
        }

        public async Task ChangeBookStatusAsync(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException($"Status cannot be empty");
            }

            if (!Enum.TryParse(typeof(BookStatus), status, true, out var enumStatus))
            {
                throw new ArgumentException($"Invalid status {status} was provided");
            }

            var book = await _libraryContext.Books.FindAsync(id);
            if (book == null) {
                throw new KeyNotFoundException($"Book with id={id} was not found");
            }

            if (!ShouldChangeStatus(book.Status, (BookStatus)enumStatus))
            {
                throw new InvalidOperationException($"Cannot change status from {book.Status} to {status}. This transition is not allowed");
            }

            book.Status = (BookStatus)enumStatus;
            await _libraryContext.SaveChangesAsync();
        }

        private bool ShouldChangeStatus(BookStatus currentStatus, BookStatus newStatus)
        {
            return (currentStatus, newStatus) switch
            {
                (BookStatus.Returned, BookStatus.OnTheShelf) => true,
                (BookStatus.Damaged, BookStatus.OnTheShelf) => true,
                (BookStatus.OnTheShelf, BookStatus.Borrowed) => true,
                (BookStatus.Borrowed, BookStatus.Returned) => true,
                (BookStatus.OnTheShelf, BookStatus.Damaged) => true,
                (BookStatus.Returned, BookStatus.Damaged) => true,
                _ => false
            };
        }
    }
}
