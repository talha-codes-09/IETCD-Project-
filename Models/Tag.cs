using System.ComponentModel.DataAnnotations;

namespace IETCD.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<CourseTag> CourseTags { get; set; } = new List<CourseTag>();
    }
}