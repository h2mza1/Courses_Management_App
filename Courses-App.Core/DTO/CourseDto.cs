using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses_App.Core.DTO
{
    public class CourseDto
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public string Description { get; set; } = string.Empty;

        public int InstructorId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public bool IsActive => StartDate <=DateTime.Now && EndDate >= DateTime.Now;

        public int? CategeoryId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [DataType(dataType:DataType.ImageUrl,ErrorMessage ="That`s not Image")]
        public IFormFile? Image { get; set; }=null;
        // ✅ لاستقبال الفيديوهات من الفورم
        public List<IFormFile>? VideoFiles { get; set; }
    }
}
