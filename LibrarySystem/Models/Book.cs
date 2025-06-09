using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public enum BookStatus
    {
        OnTheShelf,
        Borrowed,
        Returned,
        Damaged
    }

    public class Book
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255, ErrorMessage = "Title cannoe exceed 255 characters.")]
        public string? Title { get; set; }

        [Required]
        [MaxLength(255, ErrorMessage = "Author cannoe exceed 255 characters")]
        public string? Author { get; set; }

        [Required]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "ISBN should always include 13 digits.")]
        public string? ISBN {get; set; }
        
        [Required]
        public BookStatus Status { get; set; }
    }
}
