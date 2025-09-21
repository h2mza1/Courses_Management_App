using Courses_App.Core.DTO;
using Courses_App.Core.Models;

namespace Corses_App.Repostory
{
    public interface IStudentRepostory
    {
        Task<List<User>> GetAllAsync();
        Task<User ?> GetByIdAsync(string id);
        Task<User?> AddAsync(User student , string password);
        Task<User?> UpdateAsync(User student);
        Task DeleteAsync(string id);
        /// <summary>
        /// return all students ID and FullName
        /// </summary>
        /// <returns></returns>
        Task<List<UserReadDTO>> GetAll();
        Task<List<CourseReadDTO>?>GetCoursesByStudentId(string studentId);
        Task<bool> ConfirmDelete(string id);
        Task<bool> Restore(string id);
        Task<List<HistoryDTO>?> GetDeletedStudents();
        Task<int> GetStudentsCountAsync();
    }
}
