using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Courses_App.Core.DTO
{
    public class CourseVideoDto
    {
        [Required]
        public int Id {  get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; } = "";
        [Required]
        public int CourseId { get; set; }
        [Required]
        public string VideoPath { get; set; }

        public float? VideosCount { get; set; }= 0.0f;
        public string? img { get; set; } = "";
        public string? CourseName { get; set; } = "";


    }
}
