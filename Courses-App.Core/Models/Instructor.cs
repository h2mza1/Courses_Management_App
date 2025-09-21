using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.Models
{
    public class Instructor
    {
        public int Id { get; set; }
            
        public string UserId { get; set; }
        public virtual User User { get; set; }
        public double Sallary { get; set; } = 0;
        public string? Major {  get; set; }
        public string? Description { get; set; }
        public virtual ICollection<Course>? Courses { get; set; }
    }

}
