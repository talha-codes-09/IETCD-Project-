using System.ComponentModel.DataAnnotations;

namespace IETCD.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}