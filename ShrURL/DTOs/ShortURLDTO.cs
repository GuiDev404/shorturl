using System.ComponentModel.DataAnnotations;

namespace ShrURL.DTOs {
    public record ShortURLDTO(int Id, string Original, string ShortUniqueId, DateTime CreatedAt);

    public class CreateDTOShortURL {
        [Required(ErrorMessage = "La URL es requerida")]
        [Url(ErrorMessage = "URL Invalida")]
        public string LongURL { get; set; } = default!;
    };
}
