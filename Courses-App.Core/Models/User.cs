using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.Models
{
    public class User : IdentityUser
    {
        [Required]
        [DataType(DataType.Text)]
        public  string? FullName { get; set; }
        public string? PrfilePicture{ get; set; }
        public bool IsDeleted { get; set; }=false;
        public virtual ICollection<Enrollment> Enrollments { get; set; }
     //   public byte[] ? ProfileImage { get; set; }
    }
}
