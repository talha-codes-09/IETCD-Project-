using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace IETCD.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ThumbnailUrl { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [MaxLength(150)]
        public string InstructorName { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int CategoryId { get; set; }

        [ValidateNever]
        public Category Category { get; set; } = null!;

        [ValidateNever]
        public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
    }
}