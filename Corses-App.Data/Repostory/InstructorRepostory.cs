using Corses_App.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public class InstructorRepostory : IInstructorRepostory
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private UserManager<User> _userManager;
        private ApplicationDbContext _context;

        public InstructorRepostory( UserManager<User> userManager , ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }
        public async Task<User?> AddAsync(User instructor, string password)
        {
            var result = await _userManager.CreateAsync(instructor, password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("instructor"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("instructor"));
                }
                var inst = new Instructor
                {
                    UserId=instructor.Id
                };
             
                 _context.Instructors.Add(inst);

                await _userManager.AddToRoleAsync(instructor, "instructor");

                return instructor;
            }
            return null;
        }

        public async Task DeleteAsync(string id)
        {
            var user = await _context.Instructors
                .Where(i=> i.UserId == id)
                .Include(i=> i.User)
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return;
            }
            user.User.IsDeleted = true;
           await _context.SaveChangesAsync();
        }


        public async Task<List<Instructor>> GetAllAsync()
        {

            var users = await _context.Instructors
                .AsNoTracking()
                .Where(u=> !u.User.IsDeleted)
                .Include(u=>u.User)
                .ToListAsync();

            return users;
        }

        public async Task<User?> GetByIdAsync(string id)
        {
            var user = await _context.Instructors
                .AsNoTracking()
                .Include(i => i.User)
                .Where(i => !i.User.IsDeleted && i.UserId == id).FirstOrDefaultAsync();
            if(user != null)
                return new User
                {
                    Id = user.UserId,
                    Email = user.User.Email,
                    FullName = user.User.FullName,
                    PrfilePicture = user.User.PrfilePicture ?? "",
                    PhoneNumber = user.User.PhoneNumber,
                    UserName = user.User.UserName,
                    

                };
            return null;
        }
        /// <summary>
        /// return Instructors from Users Table
        /// </summary>
        /// <returns></returns>
        public async Task<List<InstructorReadDto>> GetAllUsers()
        {
            var users = await _context.Instructors
                .AsNoTracking()
                .Where(i=> !i.User.IsDeleted)
                .Include(u => u.User)
              .Select(s => new InstructorReadDto
              {
                  UserId = s.UserId,
                  UserName = s.User.UserName ?? "",
                  Email = s.User.Email ?? "",
                  PhoneNumber = s.User.PhoneNumber ?? "",
                  FullName = s.User.FullName ?? "",
                  Sallary = s.Sallary,
                  Picture = s.User.PrfilePicture ?? "",
                  Courses = s.Courses.Count(),
                  Major = s.Major ?? ""
                  
              })
              .ToListAsync();
            return users;
        }
        public async Task<UpdateUserDto?> UpdateAsync(UpdateUserDto user, string? newPassword = null)
        {
            // 1. جلب المستخدم من الداتا بيز
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (existingUser == null) return null;

            // 2. تحديث بيانات User
            //existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.Phone;
            existingUser.FullName = user.FullName;
            existingUser.UserName = user.Email;

            var result = await _userManager.UpdateAsync(existingUser);
            if (!result.Succeeded) return null;

            // 3. تحديث بيانات Instructor إذا موجود
            var inst = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == user.Id);
            if (inst != null)
            {
                inst.Sallary = user.Sallary;
                await _context.SaveChangesAsync();
            }

            // 4. إذا في كلمة مرور جديدة
            if (!string.IsNullOrEmpty(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var passResult = await _userManager.ResetPasswordAsync(existingUser, token, newPassword);
                if (!passResult.Succeeded) return null;
            }

            // 5. إرجاع نسخة DTO محدثة
            return new UpdateUserDto
            {
                Id = existingUser.Id,
                //UserName = existingUser.UserName,
                FullName = existingUser.FullName,
                Email = existingUser.Email,
                Phone = existingUser.PhoneNumber,
                Sallary = inst?.Sallary ?? 0
            };
        }

        public async Task<List<HistoryDTO>?> GetInstructorsDeleted()
        {
            var users = await _context.Instructors
                .AsNoTracking()
                .Where(u => u.User.IsDeleted)
                .Select(h => new HistoryDTO()
                {
                    Id = h.Id,
                    Name = h.User.FullName ?? "",
                    EntityType = "Instructor",
                    DeletedAt = DateTime.Now,
                    Description = h.Description ?? "",
                    ExtraInfo ="Major : "+ h.Major ?? ""
                })
                .ToListAsync();
            if (users == null) return null;
            //var deletedUsers = await _userManager
            return users;
        }

        public async Task ConfirmDelete(int id)
        {
            var instr = await _context.Instructors.FindAsync(id);
            if (instr == null) return;
            var user = await _userManager.FindByIdAsync(instr.UserId);
            if (user != null)
            {
                // 1. احذف الـ Instructor المرتبط أولًا
                var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Id == id);
                if (instructor != null)
                {
                    _context.Instructors.Remove(instructor);
                    await _context.SaveChangesAsync();
                }

                // 2. ثم احذف المستخدم
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception("Failed to delete user: " + errors);
                }
            }
        }

        public async Task<bool> Restore(int id)
        {
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.Id == id);
            if (instructor == null)
                return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == instructor.UserId);
            if (user == null)
                return false;
            user.IsDeleted=false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetInstructorsCountAsync()
        {
            int count = await _context.Instructors.CountAsync();
            return count;
        }
    }
}
