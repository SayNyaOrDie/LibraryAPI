namespace WebApplication3.DTOs
{
    public class BookDTO
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public Author author { get; set; }
        public DateOnly PublishDate { get; set; }
        public decimal Price { get; set; }
    }
}
