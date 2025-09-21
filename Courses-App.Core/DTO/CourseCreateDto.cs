using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class CourseCreateDto
    {
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public string Description { get; set; } = string.Empty;

        public int InstructorId { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }= DateTime.Now;

        public bool IsActive => StartDate <= DateTime.Now && EndDate >= DateTime.Now;

        public int? CategeoryId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = decimal.Zero;

        [DataType(dataType: DataType.ImageUrl, ErrorMessage = "That`s not Image")]
        public IFormFile? Image { get; set; } = null;
        // ✅ لاستقبال الفيديوهات من الفورم
        public List<IFormFile>? VideoFiles { get; set; }
    }
}
