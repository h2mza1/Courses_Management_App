using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public class EnreollmentRepostory : IEnreollmentRepostory
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        public EnreollmentRepostory(ApplicationDbContext context , UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<EnrollmentReadDTO?> AddAsync(EnrollmentCreateDTO enrollment)
        {
            var student = await _context.Enrollments
                .Where(c => c.CourseId == enrollment.CourseId && c.UserId == enrollment.StudentId).FirstOrDefaultAsync();
                
            if (student != null)
                return null;
            var newEnrollment = new Enrollment
            {
                UserId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                EnrollmentDate = enrollment.EnrollmentDate
            };

            await _context.Enrollments.AddAsync(newEnrollment);
            await _context.SaveChangesAsync();
            var r = await _context.Enrollments
                .Where(e => e.Id == newEnrollment.Id)
                .Select(r => new EnrollmentReadDTO()
                {
                  CourseId= r.CourseId,
                  EnrollmentDate = r.EnrollmentDate,
                  CourseName=  r.Course.Title,
                  StudentId=r.UserId,
                  StudentName = r.User.FullName,

                }).FirstOrDefaultAsync();
            
            //var result = new EnrollmentReadDTO
            //{
            //    CourseName =  r.courseName,
            //    CourseId =    r.CourseId,
            //    StudentName = r.User.FullName,
            //    StudentId =   r.StudentId,
            //    EnrollmentDate=r.EnrollmentDate
            //};
            return r;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(i => i.Id == id);
            if (enrollment == null)
                return false;

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<EnrollmentReadDTO>> GetAllAsync()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .ToListAsync();

            return enrollments.Select(e => new EnrollmentReadDTO
            {
                Id = e.Id,
                StudentId = e.UserId,
                StudentName = e.User.FullName,
                CourseId = e.CourseId,
                CourseName = e.Course.Title,
                EnrollmentDate = e.EnrollmentDate,
                courseImage = e.Course?.Image ?? "/img/education/courses-12.webp",
                InstructorImg =e.User?.PrfilePicture ?? "img/profilePicture/profileImage.jpg"
            });
        }

        public async Task<IEnumerable<EnrollmentReadDTO>?> GetByCourseIdAsync(int courseId)
        {
            var courseExists = await _context.Courses.AnyAsync(i => i.Id == courseId);
            if (!courseExists)
                return null;

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .Where(i => i.CourseId == courseId)
                .ToListAsync();

            return enrollments.Select(e => new EnrollmentReadDTO
            {
                Id = e.Id,
                StudentId = e.UserId,
                StudentName = e.User.FullName,
                CourseId = e.CourseId,
                CourseName = e.Course.Title,
                EnrollmentDate = e.EnrollmentDate
            });
        }

        public async Task<EnrollmentReadDTO?> GetByIdAsync(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (enrollment == null)
                return null;

            return new EnrollmentReadDTO
            {
                Id = enrollment.Id,
                StudentId = enrollment.UserId,
                StudentName = enrollment.User.FullName,
                CourseId = enrollment.CourseId,
                CourseName = enrollment.Course.Title,
                EnrollmentDate = enrollment.EnrollmentDate
            };
        }

        public async Task<IEnumerable<EnrollmentReadDTO>?> GetByStudentIdAsync(string studentId)
        {
            var userExists = await _context.Users.AnyAsync(i => i.Id == studentId);
            if (!userExists)
                return null;
           
            var enrollments = await _context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                .ThenInclude(c=> c.Categeory)
                .Include(e => e.User)
                .Where(i => i.UserId == studentId)
                .Select(e=> new EnrollmentReadDTO()
                {
                    Id = e.Id,
                    StudentId = e.UserId ?? "",
                    StudentName = e.User.FullName ,
                    CourseId = e.CourseId,
                    CourseName = e.Course.Title ,
                    EnrollmentDate = e.EnrollmentDate,
                    Videos = e.Course.Videos.Count(),
                    InstructorName = e.Course.Instructor.User.FullName ?? "",
                    InstructorImg = e.Course.Instructor.User.PrfilePicture ?? "/img/profilePicture/profileImage.jpg",
                    categoryName = e.Course.Categeory.Name ,
                    categoryImg = e.Course.Categeory.Icon,
                    courseImage = e.Course.Image ?? "/img/education/courses-12.webp"
                })
                .ToListAsync();
            //var courses = enrollments.Select(e=> new CourseReadDTO()
            //{
            //    categeoryName = e.Course.Categeory.Name , 
            //    InstructorMajor = e
            //})
            return enrollments;
        }

        public async Task<int> GetEnrollmentsCountAsync()
        {
            int count = await _context.Categeories.CountAsync();
            return count;
        }

        public async Task<List<UserReadDTO>?> GetStudentsNotEnrolledInCourseAsync(int courseId)
        {
            var students = await _context.Users
                .Where(u => !u.Enrollments.Any(e => e.CourseId == courseId))
                .ToListAsync();

            var filteredStudents = new List<UserReadDTO>();

            foreach (var s in students)
            {
                var roles = await _userManager.GetRolesAsync(s); // UserManager<User>
                if (roles.Contains("User"))
                {
                    filteredStudents.Add(new UserReadDTO
                    {
                        Id = s.Id,
                        FullName = s.FullName
                    });
                }
            }

            return filteredStudents;

        }

        public async Task<bool> IsEnroll(string userId, int courseId)
        {
            var enroll = await _context.Enrollments
                .AsNoTracking()
                .Select(e => new
                {
                    e.Id,
                    e.CourseId,
                    e.UserId
                })
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            return enroll;
        }

        public async Task<bool> UpdateAsync(EnrollmentReadDTO enrollment)
        {
            var existingEnrollment = await _context.Enrollments.FirstOrDefaultAsync(i => i.Id == enrollment.Id);
            if (existingEnrollment == null)
                return false;

            existingEnrollment.UserId = enrollment.StudentId;
            existingEnrollment.CourseId = enrollment.CourseId;
            existingEnrollment.EnrollmentDate = enrollment.EnrollmentDate;

            _context.Enrollments.Update(existingEnrollment);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
