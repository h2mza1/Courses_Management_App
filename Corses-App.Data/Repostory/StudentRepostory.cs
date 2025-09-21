    using Corses_App.Data;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Corses_App.Repostory
{
    public class StudentRepostory : IStudentRepostory
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudentRepostory(ApplicationDbContext dbContext
            ,UserManager<User> userManager
           )
        {
            _context = dbContext;
            _userManager = userManager;
        }
        public async Task<User?> AddAsync(User student, string password)
        {
          var result =   await _userManager.CreateAsync(student , password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(student, "User");
               
                return student;
            }
            return null;
        }

        public async Task<bool> ConfirmDelete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    //    Debug الخطأ
                    return false;
                }
            }
            return true;

        }

        public async Task DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsDeleted = true;
                //var result = await _userManager.DeleteAsync(user);

                //if (!result.Succeeded)
                //{
                //     Debug الخطأ
                //    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                //    throw new Exception("Failed to delete user: " + errors);
                //}
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<UserReadDTO>> GetAll()
        {

            var students = await (from u in _context.Users
                                  join ur in _context.UserRoles on u.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  where u.IsDeleted == false && r.Name == "User"
                                  select new UserReadDTO
                                  {
                                      Id = u.Id,
                                      FullName = u.FullName
                                  }).ToListAsync();



            return students;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var usersInRole = await (from u in _context.Users
                                     join ur in _context.UserRoles on u.Id equals ur.UserId
                                     join r in _context.Roles on ur.RoleId equals r.Id
                                     where u.IsDeleted == false && r.Name == "User"
                                     select u).ToListAsync();

            return usersInRole;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var user= await _userManager.FindByIdAsync(id);
            if (user.IsDeleted)
                return null;
            return user;   
            }

        public async Task<List<CourseReadDTO>?> GetCoursesByStudentId(string studentId)
        {
           var courses = await _context.Enrollments
                .Where(c => c.UserId == studentId && c.User.IsDeleted == false)
                 .Select(s => new CourseReadDTO
                 {
                     courseId = s.CourseId,
                     Name = s.Course.Title ?? "",
               
                 })
                .ToListAsync();
            return courses;
        }

        public async Task<List<HistoryDTO>?> GetDeletedStudents()
        {
            var users = await _context.Users
                .Where(u=> u.IsDeleted)
                .Select(u=> new HistoryDTO()
                {
                    Id=0,
                    Description = u.UserName ?? "",
                    EntityType = "Student",
                    DeletedAt = DateTime.Now,
                    Name =u.FullName ?? "",
                    ExtraInfo = "Phone : "+u.PhoneNumber ?? "" + "Email : "+u.Email,
                    UserId = u.Id

                }).ToListAsync();
            return users;
        }

        public async Task<int> GetStudentsCountAsync()
        {
            var st = await _userManager.GetUsersInRoleAsync("User");
            int count = st.Count();
            return count;
        }

        public async Task<bool> Restore(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;
            user.IsDeleted = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public  async Task<User?> UpdateAsync(User student)
        {
            var user = await _userManager.FindByIdAsync(student.Id);
            if (user != null && !user.IsDeleted)
            {
                await _userManager.UpdateAsync(student);
                user.UserName = student.Email;
                await _context.SaveChangesAsync();
                return student;
            }
            return null;
        }
    }
}
