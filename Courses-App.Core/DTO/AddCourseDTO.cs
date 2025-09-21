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
    public class AddCourseDTO
    {
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public string Description { get; set; } = string.Empty;

        public int InstructorId { get; set; }
        public int CategeoryId { get; set; }
        [Column("decimal(18,2)")]
        public decimal Price {  get; set; }

        // ✅ لاستقبال الفيديوهات من الفورم
        public List<IFormFile>? VideoFiles { get; set; }
    }
}
