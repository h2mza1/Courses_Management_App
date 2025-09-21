using Corses_App.Data.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    [Authorize(Roles = "Admin")]

    public class VideoController : Controller
    {
        private IVideoReostory _repostrory;

        public VideoController(IVideoReostory reostory) 
        {
           _repostrory=reostory;
        }
        // GET: VideoController
        public async Task<ActionResult> Index(int id)
        {
            var videos= await _repostrory.GetVideos(id);
            ViewBag.courseId = id;
            if (videos.Any())
            { var title = videos.FirstOrDefault()?.CourseName ?? "";
              ViewBag.CourseTitle = title;

            }
            else
                ViewBag.CourseTitle = "Videos Managments";
            return View(videos);
        }

        // GET: VideoController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: VideoController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: VideoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm]VideoDto dto)
        {
            if (dto.VideoFile == null || dto.VideoFile.Length == 0)
            {
                return Json(new { success = false, message = "Please upload a valid video file." });
            }

            // حفظ الملف في wwwroot/videos
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.VideoFile.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/videos", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await dto.VideoFile.CopyToAsync(stream);
            }

            // إنشاء الكائن وإضافته إلى قاعدة البيانات
            var video = new CourseVideos
            {
                Name = dto.Name,
                Description = dto.Description,
                CourseId = dto.CourseId,
                VideoPath = "/Videos/" + fileName
            };
            var result = await _repostrory.AddVideo(video);

            return Json(new { success = true, data = result, message = "Video uploaded successfully!" });
        }

        // GET: VideoController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var video = await _repostrory.GetVideoById(id);
            if (video == null)
            {
                return Json(new { success = false, data = video, message = "An error on model or video not found" });
            }
            return Json(new { success = true, data = video, message = "Video was founded  successfully" });
        }

        // POST: VideoController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, VideoDto collection)
        {
            var result = await _repostrory.GetVideoById(id);
            if (result != null)
            {
                var updatedCourse = await _repostrory.UpdateVideoInfo(collection);
                if (updatedCourse != null)
                {
                    updatedCourse.CourseId=collection.CourseId;
                    return Json(new { success = true, data = updatedCourse, message = "Video information updated successfully" });
                }
                return Json(new { success = false, data = collection, message = "An error on update video" });

            }
            return Json(new {success=false, data=collection , message="An error on model or video not found"});
            
        }

        //// GET: VideoController/Delete/5
        //[HttpGet("Video/Delete/{id}")]
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        // POST: VideoController/Delete/5
        [HttpPost("Video/DeleteConfirm/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {   
            var video = await _repostrory.GetVideoById(id);
            if (video == null)
                return Json(new { success = false, message = "Video not found." });

            var res = await _repostrory.DeleteVideo(id);

            if (res)
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", video.VideoPath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);


                return Json(new { success = true, Data = video, message = "Video Deleted Successfully" });

            }
            return Json(new { success = false,Data = video,message="Error On Model"});
        }
    }
}
