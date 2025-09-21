using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses_App.Core.Models
{
    public class CourseVideos
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [DataType(DataType.Text)]
        public string? Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        public bool ? IsWathed { get; set; } = false;
        [Required]
        public int CourseId { get; set; }  // غيّره إلى int إذا المفتاح الأساسي للكورس int
        public bool IsDeleted { get; set; } = false;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }
        [Required(ErrorMessage = "Video path is required.")]
        public string VideoPath { get; set; } = string.Empty;
    }
}
