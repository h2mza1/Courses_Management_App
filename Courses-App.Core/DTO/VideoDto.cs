using Microsoft.AspNetCore.Http;

namespace Courses_App.Core.DTO
{
    public class VideoDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CourseId { get; set; }

        public IFormFile VideoFile { get; set; } = null!;
    }
}
