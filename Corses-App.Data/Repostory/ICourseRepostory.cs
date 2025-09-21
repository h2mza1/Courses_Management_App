using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public interface ICourseRepostory
    {
        Task<List<CourseReadDTO>> GetAllAsync();
        Task<CourseReadDTO?> GetByIdAsync(int id);
        Task<List<CourseReadDTO>> GetCoursesByCategoryId(int id);
        Task<CourseReadDTO?> AddAsync(CourseCreateDto course);
        Task<CourseReadDTO?> UpdateAsync(int id,CourseCreateDto course);
        Task<bool> DeleteAsync(int id);
        Task <CourseVideos> AddVideo(CourseVideos video);
        Task<List<CourseReadDTO>> GetAll();
        Task<int?> GetSumOfVideosInCourse(int courseId);
        Task<List<CourseReadDTO>> GetSumOfVideosInCourses();
        Task<List<CourseReadDTO>?> GetMoreCorses(int skip, int take=10);
        Task<List<CourseReadDTO>?> Search(string? title);
        /// <summary>
        /// Gets the top courses based on the highest enrollments.
        /// </summary>
        /// <param name="number">Number of courses to retrieve</param>
        /// <returns>List of courses or null if none found</returns>
        Task<List<CourseReadDTO>?> GetMaxEnrollmentsInCourses(int number);
        Task<bool>ConfirmDeleted(int courseId);
        Task RestoreAsync(int courseId);
        Task<List<HistoryDTO>?> GetDeletedCoursesAsync();
        Task<int> GetCourseCountAsync();


    }
}
