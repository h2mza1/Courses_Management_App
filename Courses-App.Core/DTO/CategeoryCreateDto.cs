using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class CategeoryCreateDto
    {
        public int? Id { get; set; }
        [Required (ErrorMessage ="The Category name is required")]
        [MaxLength(30 , ErrorMessage ="The Category name is too long")]
        public string Name { get; set; }
        public IFormFile? Icon {  get; set; }
        public IEnumerable<CourseReadDTO>? Courses { get; set; }=new HashSet<CourseReadDTO>();
    }
}
