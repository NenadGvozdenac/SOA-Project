namespace blogs_service.src.Blogs.API.DTOs
{
    public class BlogDTO
    {
        public string? Title { get; set; }
        public string? DescriptionMarkdown { get; set; }
        public string? ImageBase64 { get; set; } // slika kao base64 string
        //public string? Image { get; set; }
    }
}