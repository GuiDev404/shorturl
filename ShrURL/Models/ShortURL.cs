namespace ShrURL.Models {
    public class ShortURL {
        public int Id { get; set; }
        public string Original { get; set; } = default!;

        public string ShortUniqueId { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
