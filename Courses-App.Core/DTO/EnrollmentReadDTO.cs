using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class EnrollmentReadDTO
    {
        public int Id { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }  // اسم الطالب
        public int CourseId { get; set; }
        public string? CourseName { get; set; }   // اسم الكورس
        public DateTime EnrollmentDate { get; set; }
        public int? Videos { get; set; }
        public string? InstructorName { get; set; }
        public string? InstructorImg { get; set; }
        public string? categoryName { get; set; }
        public string? categoryImg { get; set; }
        public string? courseImage { get; set; }

    }
}
