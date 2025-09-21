// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Corses_App.Repostory;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
namespace Corses_App.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IStudentRepostory _repstory;

        public IndexModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IStudentRepostory repostory
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _repstory = repostory;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            // الملف المرفوع من المستخدم
            public IFormFile ProfilePictureFile { get; set; }

            // المسار المخزن في قاعدة البيانات (للعرض)
            public string ProfilePicturePath { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            // إذا ما في صورة بالمستخدم، خليها صورة افتراضية
            if (string.IsNullOrEmpty(user.PrfilePicture))
            {
                user.PrfilePicture = "/img/profilePicture/profileImage.jpg"; // صورة افتراضية
            }

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                ProfilePicturePath = user.PrfilePicture
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
        private async Task UpdateUserClaimsAsync(User user)
        {
            var oldClaims = await _signInManager.UserManager.GetClaimsAsync(user);

            // امسح القديم
            foreach (var claim in oldClaims.Where(c => c.Type == "FullName" || c.Type == "ProfilePicture"))
            {
                await _signInManager.UserManager.RemoveClaimAsync(user, claim);
            }

            // أضف الجديد
            var newClaims = new List<Claim>
    {
        new Claim("FullName", user.FullName ?? user.UserName),
        new Claim("ProfilePicture", user.PrfilePicture ?? "/img/profilePicture/profileImage.jpg"),
         new Claim("UserId", user.Id ?? "")

    };

            await _signInManager.UserManager.AddClaimsAsync(user, newClaims);

            // 🟢 لازم تعمل Refresh عشان الكوكي يتحدث
            await _signInManager.RefreshSignInAsync(user);
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found" });
            }

            if (!ModelState.IsValid)
            {
                return new JsonResult(new { success = false, message = "Invalid input data" });
            }

            // تحديث رقم الهاتف
            if (Input.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                    return new JsonResult(new { success = false, message = "Error updating phone number" });
            }


            // تحديث صورة البروفايل
            if (Input.ProfilePictureFile != null && Input.ProfilePictureFile.Length > 0)
            {
                var extension = Path.GetExtension(Input.ProfilePictureFile.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/profilePicture");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var filePath = Path.Combine(directory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePictureFile.CopyToAsync(stream);
                }

                user.PrfilePicture = "/img/profilePicture/" + fileName;
                await _userManager.UpdateAsync(user);
                await UpdateUserClaimsAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);

            return new JsonResult(new
            {
                success = true,
                message = "Profile updated successfully",
                profilePicture = user.PrfilePicture,
                phoneNumber = user.PhoneNumber
            });
        }

    }
}
