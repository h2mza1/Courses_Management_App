using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public interface IEnreollmentRepostory
    {

        Task<IEnumerable<EnrollmentReadDTO>> GetAllAsync();
        Task<EnrollmentReadDTO ?> GetByIdAsync(int id);
        Task<EnrollmentReadDTO?> AddAsync(EnrollmentCreateDTO enrollment);
        Task<bool> UpdateAsync(EnrollmentReadDTO enrollment);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<EnrollmentReadDTO>?> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<EnrollmentReadDTO>?> GetByCourseIdAsync(int courseId);
        Task<List<UserReadDTO>?> GetStudentsNotEnrolledInCourseAsync(int courseId);
        Task<bool> IsEnroll(string userId , int courseId);
        Task<int> GetEnrollmentsCountAsync();
    }
}
