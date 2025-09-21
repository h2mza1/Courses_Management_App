using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class UserReadDTO
    {
        [Required]
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? profilePicture { get; set; }
        public string? Email { get; set; }
    }
}
