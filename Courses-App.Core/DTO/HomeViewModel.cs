using Courses_App.Core.DTO;

namespace Corses_App.Models
{
    public class HomeViewModel
    {
        public UserReadDTO Student { get; set; }
        public IEnumerable<CourseReadDTO> Courses { get; set; }
        public IEnumerable<CourseReadDTO> topCourses { get; set; }

        public IEnumerable<InstructorReadDto > Instructors { get; set; }
        public IEnumerable<CategeoryReadDto> Categeories { get; set; }
        public int? selectedCourseId { get; set; }
    }
}
