using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public class CourseRepostory : ICourseRepostory
    {
        private readonly ApplicationDbContext _context;

        public CourseRepostory(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<CourseReadDTO?> AddAsync(CourseCreateDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                InstructorId = dto.InstructorId,
                CategeoryId = dto.CategeoryId,
                Price = dto.Price,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Videos = new List<CourseVideos>()
            };

            // حفظ الصورة
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "CoursesImages");
                Directory.CreateDirectory(imageFolder);

                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var imagePath = Path.Combine(imageFolder, imageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                course.Image = $"/img/CoursesImages/{imageName}";
            }

            // حفظ الفيديوهات
            if (dto.VideoFiles != null)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Videos");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var video in dto.VideoFiles)
                {
                    if (video != null && video.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(video.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await video.CopyToAsync(stream);
                        }

                        course.Videos.Add(new CourseVideos
                        {
                            Name = Path.GetFileNameWithoutExtension(video.FileName),
                            VideoPath = $"/Videos/{uniqueFileName}"
                        });
                    }
                }
            }

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return new CourseReadDTO
            {
                courseId = course.Id,
                Name = course.Title,
                Description = course.Description,
                courseImage = course.Image,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                instructorId = course.InstructorId,
                InstructorName = course.Instructor?.User?.FullName ?? "",
                IsActive = course.IsActive,
                categeoryId = course.CategeoryId,
                categeoryName = course.Categeory?.Name ?? "",
                Price = course.Price,
                time = course.DurationDays
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return false;

            // حذف الصورة
            //if (!course.Image.IsNullOrEmpty())
            //{
            //    var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.Image.TrimStart('/'));
            //    if (File.Exists(imgPath))
            //    {
            //        File.Delete(imgPath);
            //    }
            //}

            //// حذف الفيديوهات
            //foreach (var video in course.Videos)
            //{
            //    if (!string.IsNullOrEmpty(video.VideoPath))
            //    {
            //        var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.VideoPath.TrimStart('/'));
            //        if (File.Exists(videoPath))
            //        {
            //            File.Delete(videoPath);
            //        }
            //    }
            //}

            course.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<CourseReadDTO>> GetAllAsync()
        {
            var courses = await _context.Courses
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    Course = c,
                    EnrollmentsCount = c.Enrollments.Count(), // EF رح يولد GROUP BY هنا
                    VideoCount = c.Videos.Count()
                })
                .OrderByDescending(x => x.EnrollmentsCount)
                .Take(6)
                .Select(x => new CourseReadDTO
                {
                    courseId = x.Course.Id,
                    Name = x.Course.Title ?? "",
                    Description = x.Course.Description ?? "",
                    categeoryId = x.Course.CategeoryId,
                    categeoryName = x.Course.Categeory.Name ?? "",
                    instructorId = x.Course.InstructorId,
                    InstructorName = x.Course.Instructor.User.FullName ?? "",
                    Price = x.Course.Price,
                    EndDate = x.Course.EndDate,
                    StartDate = x.Course.StartDate,
                    EnrollmentsCount = x.EnrollmentsCount,
                    VideoCount = x.VideoCount,
                    instructorImage = x.Course.Instructor.User.PrfilePicture ?? "",
                    InstructorMajor = x.Course.Instructor.Major ?? "",
                    IsActive = x.Course.IsActive,
                    time = x.Course.DurationDays,
                    courseImage = x.Course.Image ?? "/img/education/courses-3.webp",
                    CategoryImg = x.Course.Categeory.Icon ?? ""
                })
                .AsNoTracking()
                .ToListAsync();

            return courses;
        }


        public async Task<CourseReadDTO?> GetByIdAsync(int id)
        {
            var course = await _context.Courses
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CourseReadDTO
                {
                    courseId = c.Id,
                    categeoryId = c.CategeoryId,
                    categeoryName = c.Categeory != null ? c.Categeory.Name : "",
                    Name = c.Title ?? "",
                    Description = c.Description ?? "",
                    Price = c.Price,
                    instructorId = c.InstructorId,
                    InstructorName = c.Instructor != null && c.Instructor.User != null ? c.Instructor.User.FullName : "",
                    VideoCount = c.Videos.Count(),
                    EnrollmentsCount = c.Enrollments.Count(),
                    courseImage = c.Image ?? "",
                    StartDate = c.StartDate,
                    EndDate = c.EndDate
                })
                .AsNoTracking()
                .SingleOrDefaultAsync();

            return course;
        }


        public async Task<CourseReadDTO?> UpdateAsync(int id, CourseCreateDto dto)
        {
            var existingCourse = await _context.Courses.FindAsync(id);
            if (existingCourse == null) return null;

            existingCourse.Title = dto.Title;
            existingCourse.Description = dto.Description;
            existingCourse.InstructorId = dto.InstructorId;
            existingCourse.CategeoryId = dto.CategeoryId;
            existingCourse.Price = dto.Price;
            existingCourse.StartDate = dto.StartDate;
            existingCourse.EndDate = dto.EndDate;
            existingCourse.IsActive = dto.IsActive;

            // تعديل الصورة
            if (dto.Image != null && dto.Image.Length > 0)
            {
                if (!existingCourse.Image.IsNullOrEmpty())
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCourse.Image.TrimStart('/'));
                    if (File.Exists(oldImagePath))
                        File.Delete(oldImagePath);
                }

                var imageFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "CoursesImages");
                Directory.CreateDirectory(imageFolder);

                var imageName = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                var imagePath = Path.Combine(imageFolder, imageName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                existingCourse.Image = $"/img/CoursesImages/{imageName}";
            }

            _context.Courses.Update(existingCourse);
            await _context.SaveChangesAsync();

            return new CourseReadDTO
            {
                courseId = id,
                Name = existingCourse.Title,
                Description = existingCourse.Description,
                instructorId = existingCourse.InstructorId,
                categeoryId = existingCourse.CategeoryId,
                Price = existingCourse.Price,
                StartDate = existingCourse.StartDate,
                EndDate = existingCourse.EndDate,
                courseImage = existingCourse.Image ?? ""
            };
        }

        public async Task<CourseVideos?> AddVideo(CourseVideos video)
        {
            var exists = await _context.Courses.AnyAsync(v => v.Id == video.CourseId);
            if (!exists) return null;

            await _context.videos.AddAsync(video);
            await _context.SaveChangesAsync();

            return video;
        }

        public async Task<List<CourseReadDTO>> GetAll()
        {
            return await _context.Courses
                .Where(c=> c.IsDeleted == false)
                .AsNoTracking()
                .Select(c => new CourseReadDTO
                {
                    courseId = c.Id,
                    Name = c.Title
                })
                .ToListAsync();
        }

        public async Task<int?> GetSumOfVideosInCourse(int courseId)
        {
            return await _context.Courses
                .Where(c => c.Id == courseId && c.IsDeleted==false)
                .Select(c => (int?)c.Videos.Count())
                .FirstOrDefaultAsync();
        }

        public Task<List<CourseReadDTO>> GetSumOfVideosInCourses()
        {
            throw new NotImplementedException();
        }

        public async Task<List<CourseReadDTO>> Search(string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<CourseReadDTO>();

            searchTerm = $"%{searchTerm.Trim()}%";

            return await _context.Courses
                .AsNoTracking()
                .Where(c => !c.IsDeleted &&
                        (EF.Functions.Like(c.Title, searchTerm) ||
                         EF.Functions.Like(c.Description, searchTerm) ||
                         c.Videos.Any(v => EF.Functions.Like(v.Name, searchTerm)))

                )
                .OrderBy(c => c.Title)
                .Select(c => new CourseReadDTO
                {
                    courseId = c.Id,
                    Name = c.Title
                })
                .ToListAsync();
        }

        public async Task<List<CourseReadDTO>?> GetMoreCorses(int skip, int take = 6)
        {
            var courses = await _context.Courses
                .Where(c=> !c.IsDeleted)
                .Skip(skip)
                .Take(take)
                .Select(c=> new CourseReadDTO()
                {
                    courseId = c.Id,
                    Name = c.Title ?? "",
                    Description = c.Description ?? "",
                    categeoryId = c.CategeoryId,
                    categeoryName = c.Categeory != null ? c.Categeory.Name : "",
                    instructorId = c.InstructorId,
                    InstructorName = c.Instructor != null && c.Instructor.User != null ? c.Instructor.User.FullName : "",
                    Price = c.Price,
                    EndDate = c.EndDate,
                    StartDate = c.StartDate,
                    EnrollmentsCount = c.Enrollments.Count, // مباشرة بدون ??
                    VideoCount = c.Videos.Count,            // مباشرة بدون ??
                    instructorImage = c.Instructor != null && c.Instructor.User != null ? c.Instructor.User.PrfilePicture : "",
                    InstructorMajor = c.Instructor != null ? c.Instructor.Major : "",
                    IsActive = c.IsActive,
                    time = c.DurationDays,
                    courseImage = c.Image ?? "/img/education/courses-3.webp",
                    CategoryImg = c.Categeory != null ? c.Categeory.Icon : ""
                })
                .AsNoTracking()
                .ToListAsync();

            return courses;
        }

        public async Task<List<CourseReadDTO>?> GetMaxEnrollmentsInCourses(int number)
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c=> !c.IsDeleted)
                .OrderByDescending(c => c.Enrollments.Count())
                .Take(number)
                .Select(c => new CourseReadDTO
                {
                    categeoryId = c.Id,
                    Name = c.Title,
                    EnrollmentsCount = c.Enrollments.Count(),
                    categeoryName = c.Categeory.Name,
                    CategoryImg = c.Categeory.Icon ?? ""
                })
                .ToListAsync();

            return courses.Any() ? courses : null;
        }

        public async Task<bool> ConfirmDeleted(int courseId)
        {
            var course = await _context.Courses
                .Include(c=> c.Videos)
               .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return false;

            // حذف الصورة
            if (!course.Image.IsNullOrEmpty())
            {
                var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", course.Image.TrimStart('/'));
                if (File.Exists(imgPath))
                {
                    File.Delete(imgPath);
                }
            }

            //// حذف الفيديوهات
            foreach (var video in course.Videos)
            {
                if (!string.IsNullOrEmpty(video.VideoPath))
                {
                    var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.VideoPath.TrimStart('/'));
                    if (File.Exists(videoPath))
                    {
                        File.Delete(videoPath);
                    }
                }
            }
             _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task RestoreAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return ;
            course.IsDeleted = false;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task<List<HistoryDTO>?> GetDeletedCoursesAsync()
        {
            var courses = await _context.Courses
                .Where(c=> c.IsDeleted)
                .AsNoTracking()
                .Select (c => new HistoryDTO()
                {
                    Id = c.Id,
                    Name = c.Title ?? "",
                    Description = c.Description ?? "",
                    EntityType = "Course",
                    DeletedAt = DateTime.Now, // أو تاريخ الحذف لو مخزن
                    ExtraInfo = c.Categeory.Name ?? ""
                }
                )
                .ToListAsync();
            if(!courses.Any())
                return null;
            return courses;
        }

        public async Task<List<CourseReadDTO>> GetCoursesByCategoryId(int id)
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.CategeoryId == id && !c.IsDeleted)
                .Select(c => new CourseReadDTO()
                {
                    courseId = c.Id,
                    Name = c.Title,
                    InstructorName = c.Instructor.User.FullName ?? "",
                    IsActive = c.IsActive ,
                    time = c.DurationDays,
                    Price = c.Price,
                    categeoryName = c.Categeory.Name ?? "",
                    VideoCount = c.Videos.Count,
                    instructorImage = c.Instructor.User.PrfilePicture ?? "",
                    courseImage = c.Image ?? "",

                }
                    ).ToListAsync();
            return courses;
        }

        public async Task<int> GetCourseCountAsync()
        {
            int count = await _context.Courses.CountAsync();
            return count;
        }
    }
}
