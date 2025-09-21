using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public string UserId { get; set; }
        public User? User { get; set; }
        public bool IsDeleted { get; set; } = false;

        public DateTime EnrollmentDate { get; set; }
        
    }
}
