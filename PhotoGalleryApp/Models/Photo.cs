using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGalleryApp.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = "";

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; } = "";

        // Comma-separated tags, e.g. "vacation,beach,summer"
        public string? Tags { get; set; }

        public DateTime UploadedAt { get; set; }

        [ForeignKey("UploadedBy")]
        public int UploadedById { get; set; }
        public ApplicationUser? UploadedBy { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}
