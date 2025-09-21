using Corses_App.Data.Repostory;
using Corses_App.Models;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> sManager;
        private readonly ICourseRepostory _course;
        private readonly IInstructorRepostory _instructor;
        private readonly ICategeoryRepostory _category;
        private readonly IMemoryCache _cache;
        private readonly IEnreollmentRepostory _enreollment;
        private readonly IVideoReostory _videoReostory;

        public HomeController(ILogger<HomeController> logger
            ,UserManager<User> manager 
            , SignInManager<User> signIn 
            ,ICourseRepostory course
            ,IInstructorRepostory instructor
            ,ICategeoryRepostory categeory,
            IMemoryCache cache,
            IEnreollmentRepostory enreollment,
            IVideoReostory videoReostory
            )
        {
            _logger = logger;
            userManager=manager;
            sManager = signIn;
            _course = course;
            _instructor = instructor;
            _category = categeory;
            _cache = cache;
            _enreollment = enreollment;
            _videoReostory = videoReostory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var isLoggedIn = sManager.IsSignedIn(User);

            // 🟢 الكاش للكورسات
            if (!_cache.TryGetValue("Courses", out List<CourseReadDTO> coursesDto))
            {
                var courses = await _course.GetAllAsync();
                coursesDto = courses.Select(c => new CourseReadDTO()
                {
                    categeoryId = c.categeoryId,
                    categeoryName = c.categeoryName,
                    courseId = c.courseId,
                    Name = c.Name,
                    Description = c.Description,
                    instructorId = c.instructorId,
                    Price = c.Price,
                    VideoCount = c.VideoCount,
                    EnrollmentsCount = c.EnrollmentsCount,
                    InstructorName = c.InstructorName,
                    instructorImage = c.instructorImage,
                    IsActive = c.IsActive,
                    InstructorMajor = c.InstructorMajor,
                    time = c.time,
                    courseImage = c.courseImage ?? "/img/education/courses-12.webp",
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                _cache.Set("Courses", coursesDto, cacheOptions);
            }

            // 🟢 الكاش للـ Instructors
            if (!_cache.TryGetValue("Instructors", out List<InstructorReadDto> instructors))
            {
                instructors = await _instructor.GetAllUsers();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set("Instructors", instructors, cacheOptions);
            }

            // 🟢 الكاش للـ Categories
            if (!_cache.TryGetValue("Categories", out List<CategeoryReadDto> cat))
            {
                cat = await _category.GetCategeories();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set("Categories", cat, cacheOptions);
            }

            // 🟢 ممكن تعمل كاش للـ TopCourses كمان
            if (!_cache.TryGetValue("TopCourses", out List<CourseReadDTO> topCourses))
            {
                topCourses = await _course.GetMaxEnrollmentsInCourses(3);
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10)); // هاي قصيرة عشان ممكن تتغير بسرعة
                _cache.Set("TopCourses", topCourses, cacheOptions);
            }

            var model = new HomeViewModel()
            {
                Instructors = instructors,
                Courses = coursesDto,
                topCourses = topCourses ?? Enumerable.Empty<CourseReadDTO>(),
                Categeories = cat ?? Enumerable.Empty<CategeoryReadDto>()
            };

            // 🟢 التشييك على تسجيل الدخول
            if (isLoggedIn)
            {
                var user = await userManager.GetUserAsync(User);

                if (await userManager.IsInRoleAsync(user, "User"))
                {
                    var student = new UserReadDTO()
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        profilePicture = string.IsNullOrEmpty(user.PrfilePicture) ? null : user.PrfilePicture
                    };

                    return View(model); // الصفحة الرئيسية للمستخدم
                }
                else
                {
                    ViewBag.ProfileImage = user.PrfilePicture;
                    return RedirectToAction("Index", "Admin"); // صفحة الأدمن
                }
            }

            return View(model); // المستخدم مش مسجل دخول
        }



        public IActionResult Privacy()
        {
            return View();
        }
        public async Task<ActionResult> Courses(int take)
        {
            if(!_cache.TryGetValue("Courses",out List<CourseReadDTO> courses))
            {
                 courses = await _course.GetAllAsync();
                var options = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1));
                _cache.Set("Courses", courses, options);

            }
            return View(courses);
        }
        public async Task<ActionResult> Instructors()
        {
            var instructors = await _instructor.GetAllUsers();
            return View(instructors);
        }
        [Authorize]
        public async Task<ActionResult> Enroll(int? courseId)
        {
            var model = new HomeViewModel();
            var courses = new List<CourseReadDTO>();

            if (courseId.HasValue)
            {
                var course = await _course.GetByIdAsync(courseId.Value);
                if (course != null)
                    courses.Add(course);
                model.selectedCourseId = courseId;
            }
            else
            {
                if (!_cache.TryGetValue("courses", out List<CourseReadDTO> cachedCourses))
                {
                    courses = await _course.GetAll();
                    if (!courses.IsNullOrEmpty())
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                        _cache.Set("courses", courses, cacheOptions);
                    }
                }
                else
                {
                    courses = cachedCourses;
                }
            }

            var user = await userManager.GetUserAsync(User);
            var student = new UserReadDTO()
            {
                FullName = user.FullName,
                Id = user.Id,
                profilePicture = user.PrfilePicture,
                Email = user.Email
            };

            model.Student = student;
            model.Courses = courses;

            return View(model);
        }
        public  async Task<ActionResult> MyCourses (string userId)
        {
            var enrollments = await _enreollment.GetByStudentIdAsync(userId);
            if (enrollments == null)
                return View();
            ViewData["img"] = enrollments?.FirstOrDefault()?.courseImage ?? "";
            return View(enrollments);
        }
        [Authorize]
        public async Task<ActionResult> Vedio(int courseId)
        {
            var userId =  userManager.GetUserId(User);
            var isEnroll = await _enreollment.IsEnroll(userId,courseId);
            if (!isEnroll)
                return RedirectToAction(nameof(Index));
            
            var vedios = await _videoReostory.GetVideos(courseId);
            if (vedios == null)
                return View(Enumerable.Empty<CourseVideos>());
            ViewBag.UserId=userId;
            ViewBag.Course = vedios?.FirstOrDefault()?.CourseName ?? "";
            return View(vedios);
        }
        [HttpGet]
        public async Task<ActionResult<List<CourseReadDTO>?>> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<CourseReadDTO>()); 

            var result = await _course.Search(term);

            if (result == null || !result.Any())
                return Ok(new List<CourseReadDTO>()); 

            return Ok(result);
        }
        public IActionResult Contact() 
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
