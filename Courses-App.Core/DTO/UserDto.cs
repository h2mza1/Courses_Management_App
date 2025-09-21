using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class UserDto
    {
        [Required]
        [DataType(DataType.Text)]
        public string? FullName { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string ?Phone { get; set; }
        [Required]
        [EmailAddress]
        public string ?Email { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string? Password { get; set; }
        [DataType(DataType.Password)]
        [Required]
        public string? ConfirmPassword { get; set; }
        public float? Sallary { get; set; }
        public IFormFile? PrfilePicture { get; set; }


    }
}
