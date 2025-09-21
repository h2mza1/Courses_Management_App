using Corses_App.Data.Repostory;
using Courses_App.Core.DTO;
using Courses_App.Core.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;

namespace Corses_App.Controllers
{
    [Authorize(Roles ="Admin")]
    public class InstructorController : Controller
    {
        private IInstructorRepostory _repostory;
        private readonly IMemoryCache _cache;
        public InstructorController(IInstructorRepostory repostory , IMemoryCache cache)
        {
            _repostory = repostory;
            _cache = cache;
        }
        /// <summary>
        /// Return All Instructors
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var repostoires = await _repostory.GetAllUsers();
            return View(repostoires);
        }
        [HttpGet("Instructor/GetAll")]
        public async Task<ActionResult> GetAll()
        {
            // حاول جلب البيانات من الكاش
            if (!_cache.TryGetValue("instructors", out List<Instructor> allInstructor))
            {
                // لو غير موجودة في الكاش، جلبها من الريبو
                allInstructor = await _repostory.GetAllAsync();

                if (allInstructor != null)
                {
                    // تخزين البيانات في الكاش لمدة 5 دقائق
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                    _cache.Set("instructors", allInstructor, cacheOptions);
                }
            }

            if (allInstructor != null)
            {
                return Json(new { instructors = allInstructor });
            }

            return Json("Error");
        }

        //[HttpGet("Instructor/GetAllInstructor")]
        //public async Task<ActionResult> GetAllUsers()
        //{
        //    var instructors = await _repostory.GetAllUsers();
        //    if (instructors != null)
        //    {
        //        return Json(new { instructors });
        //    }
        //    return Json("Error");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserDto user)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, Data = user, message = "The Model Is Not Valid!" });
            }

            if (user.ConfirmPassword != user.Password)
            {
                return Json(new { success = false, Data = user, message = "The Password And Confirm Password Not Match!" });
            }

            string? imagePath = null;

            if (user.PrfilePicture != null && user.PrfilePicture.Length > 0)
            {
                // توليد اسم فريد باستخدام GUID + امتداد الملف الأصلي
                var extension = Path.GetExtension(user.PrfilePicture.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profilePicture", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await user.PrfilePicture.CopyToAsync(stream);
                }

                imagePath = "/img/profilePicture/" + fileName;
            }

            var instructor = new User
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.Phone,
                UserName = user.Email,
                PrfilePicture = imagePath ?? "/img/profilePicture/profileImage.jpg" // صورة افتراضية
            };

            var newUser = await _repostory.AddAsync(instructor, user.Password);
            if (newUser != null)
            {
                return Json(new
                {
                    success = true,
                    Data = new
                    {
                        id = newUser.Id,
                        fullName = newUser.FullName,
                        email = newUser.Email,
                        userName = newUser.UserName,
                        phoneNumber = newUser.PhoneNumber,
                        profileImage = newUser.PrfilePicture
                    },
                    message = "The Instructor Added Successfully"
                });
            }

            return Json(new { success = false, message = "An Error In Add User", Data = newUser });
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string id)
        {
            return View();
        }
        //Update
        [Authorize(Roles = "Admin")]

        [HttpPost("Instructor/Update/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, UpdateUserDto user)
        {
            string test = "";
            if (ModelState.IsValid)
            {
                if (user.Id == null)
                {
                    return NotFound("Instructor not found");
                }
                
                string? newPassword = string.IsNullOrWhiteSpace(user.NewPassword) ? null : user.NewPassword;
                var updatedUser = await _repostory.UpdateAsync(user, newPassword);
                test = user.Id;
                
                if (updatedUser != null)
                {
                    return Json(new { success = true, data = updatedUser, message = "User updated successfully" });
                }
                return Json(new { success = false, data = test, message = "An error occurred while updating the user." });

            }
            return Json(new { success = false, data = test, message = "An error occurred on Modal while updating the user." });

        }
        [Authorize(Roles ="Admin")]
        //Delete 
        [HttpPost("Instructor/Delete/{id}")]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Delete(string id)
        {
            if (ModelState.IsValid)
            {
                var instructor = await _repostory.GetByIdAsync(id);
                if (instructor != null)
                {
                    await _repostory.DeleteAsync(id);
                    return Json(new { success = true, Data = instructor, message = "User Deleted Successfully" });
                }
                return Json(new { success = false, Data = instructor, message = "An Error occurred Update User" });
            }
            return Json(new { success = false, Data = "null", message = "An Error occurred Model" });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeletePermanent(int id)
        {
            await _repostory.ConfirmDelete(id);
            return Ok();
        }
        [Authorize(Roles = "Admin")]

        public async Task<ActionResult> Restore(int id)
        {
            var result = await _repostory.Restore(id);
            return Ok(result);
        }
    }
        }


    

