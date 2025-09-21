using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
namespace Corses_App.Data.Repostory
{
    public class CategeoryRepostory : ICategeoryRepostory
    {
        private readonly ApplicationDbContext _context;

        public CategeoryRepostory(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CategeoryReadDto> AddCategeory(CategeoryCreateDto dto)
        {
            var categeory = new Categeory()
            {
                Name = dto.Name,
            };

            if (dto?.Icon != null && dto.Icon.Length > 0)
            {
                // توليد اسم فريد للملف
                var iconName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Icon.FileName);

                // المسار الفعلي على السيرفر
                var folderPath = Path.Combine("wwwroot", "img", "categories");
          
                

                var iconPath = Path.Combine(folderPath, iconName);

                // نسخ الملف فعليًا
                using (var stream = new FileStream(iconPath, FileMode.Create))
                {
                    await dto.Icon.CopyToAsync(stream);
                }

                // تخزين المسار بالنسبة للـ wwwroot
                categeory.Icon = Path.Combine("img", "categories", iconName).Replace("\\", "/");
            }

            await _context.AddAsync(categeory);
            await _context.SaveChangesAsync();
            var result = new CategeoryReadDto()
            {
                Id = categeory.Id,
                Name = categeory.Name,
                Icon = categeory.Icon ?? "",

            };
            if (categeory.Courses.Any())
                result.CoursesCount = categeory.Courses?.Count() ?? 0;
            return result;
        }


        public async Task<CategeoryReadDto?> AddCategeoryAsync(CategeoryCreateDto categeory)
        {
            var cat = new Categeory()
            {
                Name = categeory.Name,
            };

            if (categeory.Icon != null && categeory.Icon.Length > 0)
            {
                // اسم الملف + امتداده
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categeory.Icon.FileName);

                // مكان التخزين (wwwroot/images/categories)
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "categories");

                // لو الفولدر مش موجود، أنشئه
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // مسار الصورة النهائي
                var filePath = Path.Combine(folderPath, fileName);

                // خزّن الصورة في الفولدر
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categeory.Icon.CopyToAsync(stream);
                }

                // خزن المسار بالنسبة للمشروع
                cat.Icon = $"/img/categories/{fileName}";
            }

            // هون لازم تضيف الكاتيجوري لقاعدة البيانات
             await _context.Categeories.AddAsync(cat);
             await _context.SaveChangesAsync();

            return new CategeoryReadDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Icon = cat.Icon,
                
            };
        }

        public async Task<bool?> Delete(int id)
        {
            var cat = await _context.Categeories
                .Include(c => c.Courses)  // جلب الكورسات المرتبطة
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null)
                return null;
            if (cat.Courses.Any())
                return false;
             _context.Categeories.Remove(cat);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(cat.Icon))
            {
                var filePath = Path.Combine( "img", "categories", cat.Icon);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            return true;
        }

        public async Task<List<CategeoryReadDto>?> GetCategeories()
        {
            var categeories = await _context.Categeories
             .Include(c => c.Courses) // يجلب الكورسات التابعة
             .AsNoTracking()
             .ToListAsync();

            var result = categeories.Select(c => new CategeoryReadDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon ?? "",
                CoursesCount = c.Courses.Count(),
                Courses = c.Courses.Select(course => new CourseReadDTO
                {
                    courseId = course.Id,
                    Name = course.Title,
                    Price = course.Price
                }).ToList()
            }).ToList();
            return result;
        }

        public async Task<CategeoryReadDto?> GetCategeoryById(int id)
        {
            var cat = await _context.Categeories.FindAsync(id);
            if (cat == null)
                return null;
            return new CategeoryReadDto()
            {
                Id = cat.Id,
                Name = cat.Name,
                Icon = cat.Icon,
                
            };
        }

        public async Task<int> GetCategoriesCountAsync()
        {
            int count = await _context.Categeories.CountAsync();
            return count;
        }

        public async Task<CategeoryReadDto?> UpdateCategory(CategeoryCreateDto categeory, int id)
        {
            var cat = await _context.Categeories
                .Include(c => c.Courses)
                .ThenInclude(c => c.Instructor)
                .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cat == null)
                return null;

            // تحديث الاسم
            cat.Name = categeory.Name;

            // تحديث الصورة (لو تم رفع صورة جديدة)
            if (categeory.Icon != null && categeory.Icon.Length > 0)
            {
                // حذف الصورة القديمة (إن وجدت)
                if (!string.IsNullOrEmpty(cat.Icon))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cat.Icon.TrimStart('/'));
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }
                }

                // حفظ الصورة الجديدة
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(categeory.Icon.FileName);
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "categories");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await categeory.Icon.CopyToAsync(stream);
                }

                // تحديث الـ path في الداتابيس
                cat.Icon = $"/img/categories/{fileName}";
            }

            await _context.SaveChangesAsync();

            return new CategeoryReadDto()
            {
                Id = cat.Id,
                Name = cat.Name,
                Icon = cat.Icon ?? "",
                Courses = cat.Courses?
                    .Select(c => new CourseReadDTO()
                    {
                        courseId = c.Id,
                        Description = c.Description,
                        Name = c.Title,
                        Price = c.Price,
                        InstructorName = c.Instructor?.User?.FullName ?? ""
                    }).ToList() ?? new List<CourseReadDTO>()
            };
        }

    }
}
