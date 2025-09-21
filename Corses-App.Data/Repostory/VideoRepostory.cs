using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Corses_App.Data.Repostory
{
    public class VideoRepostory : IVideoReostory
    {
        private readonly ApplicationDbContext _context;

        public VideoRepostory(ApplicationDbContext context)
        {
         _context=context;   
        }
        public async Task<CourseVideos?> AddVideo(CourseVideos courseVideos)
        {
            var course= await _context.Courses.AnyAsync(c=> c.Id == courseVideos.CourseId);
            if(!course)
            {
                return null;
            }
            await _context.videos.AddAsync(courseVideos);
            await _context.SaveChangesAsync();
            return courseVideos;
        }

        public async Task<bool> DeleteVideo(int videoId)
        {
            var video = await _context.videos.FindAsync(videoId);
            if (video == null)
            {
                return false;
            }
            var videoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.VideoPath.TrimStart('/'));

            // حذف الملف من المجلد إن وُجد
            if (System.IO.File.Exists(videoPath))
            {
                System.IO.File.Delete(videoPath);
            }

            _context.videos.Remove(video);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<CourseVideos?> GetVideoById(int videoId)
        {
            var video = await _context.videos.FirstOrDefaultAsync(v=> v.Id == videoId);
            if (video == null)
                return video;
            return video;
        }

        public async Task<List<CourseVideoDto>?> GetVideos(int courseId)
        {
            var course =await _context.Courses.AnyAsync(c=> c.Id == courseId);
            if (!course)
            {
                return null;
            }
            var videos = await _context.videos
                 .Where(v => v.CourseId == courseId && !v.IsDeleted)
                 .Select(v=> new CourseVideoDto()
                 {
                     Name = v.Name ?? "",
                     CourseId = v.CourseId,
                     Description = v.Description,
                     Id = v.Id,
                    VideoPath = v.VideoPath,
                    img = v.Course.Image ?? "",
                    CourseName = v.Course.Title ?? ""
                 })
                 .ToListAsync();

            return videos;
        }

        public async Task<CourseVideos?> UpdateVideoInfo(VideoDto courseVideos)
        {
            var video = await _context.videos.FirstOrDefaultAsync(v => v.CourseId == courseVideos.CourseId);
            if (video == null)
            {
                return null;
            }

            video.Name = courseVideos.Name;
            video.Description = courseVideos.Description;
            video.CourseId = courseVideos.CourseId;

            if (courseVideos.VideoFile != null)
            {
                // حذف الفيديو القديم
                if (!string.IsNullOrEmpty(video.VideoPath))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.VideoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // توليد اسم عشوائي للملف الجديد
                var extension = Path.GetExtension(courseVideos.VideoFile.FileName);
                var fileName = Guid.NewGuid().ToString() + extension;

                var newPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "videos", fileName);
                using (var stream = new FileStream(newPath, FileMode.Create))
                {
                    await courseVideos.VideoFile.CopyToAsync(stream);
                }

                video.VideoPath = "/videos/" + fileName;
            }

            _context.Update(video);
            await _context.SaveChangesAsync();

            return video;
        }


    }
}
