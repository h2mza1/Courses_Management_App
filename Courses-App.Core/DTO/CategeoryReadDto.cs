using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class CategeoryReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Icon { get; set; }
        public int? CoursesCount { get; set; }
        public IEnumerable<CourseReadDTO>? Courses { get; set; } = new List<CourseReadDTO>();

    }
}
