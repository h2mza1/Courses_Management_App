using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class CourseReadDTO
    {
        public int courseId { get; set; }
        public string? Name { get; set; }
        public string? courseImage { get; set; }
        public string? InstructorName { get; set; }
        public int instructorId { get; set; }
        public string categeoryName { get; set; }
        public int? categeoryId { get; set; }
        public string? instructorImage {  get; set; }
        public int? VideoCount { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public int? EnrollmentsCount { get; set; } = 0;
        public bool? IsActive {  get; set; } 
        public string time {  get; set; }
        public string? InstructorMajor {  get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? CategoryImg { get; set; }
    }
}
