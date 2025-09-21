using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public interface IInstructorRepostory
    {
        Task<User?> AddAsync(User instructor, string password);
        Task DeleteAsync(string id);
        Task<List<Instructor>> GetAllAsync();
        Task<User?> GetByIdAsync(string id);
        Task<UpdateUserDto?> UpdateAsync(UpdateUserDto instructor , string? newPassword = null);
        /// <summary>
        /// <code>
        /// بترجع المعلمين من جدول المستخدمسن العام
        /// </code>
        /// </summary>
        /// <returns> return All Instructors from users Table</returns>
        Task<List<InstructorReadDto>> GetAllUsers();
        Task<List<HistoryDTO>?> GetInstructorsDeleted();
        Task ConfirmDelete(int id);
        Task<bool> Restore(int id);
        Task<int> GetInstructorsCountAsync();
    }

}
