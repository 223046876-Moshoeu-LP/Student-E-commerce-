using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers
{
    [Authorize(Roles = "User")]
    public class LearnerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SQLiteDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        //
        //Method Name : LearnerController (Constructor)
        //Purpose : Initializes the LearnerController with required services
        //Re-use : UserManager, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : UserManager<ApplicationUser> userManager - Manages user operations (create, update, delete users)
        //Input Parameter : SQLiteDBContext context - Database context for accessing application data
        //Input Parameter : IWebHostEnvironment webHostEnvironment - Provides web root path for file uploads
        //Output Type : None
        //
        public LearnerController(
            UserManager<ApplicationUser> userManager,
            SQLiteDBContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== DASHBOARD ====================

        //
        //Method Name : Dashboard
        //Purpose : Displays the learner dashboard with published content and user statistics
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns dashboard view with list of published content
        //
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var publishedContent = await _context.Contents
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedDate)
                .Take(6)
                .ToListAsync();

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            ViewBag.TotalCourses = await _context.Contents.CountAsync(c => c.IsPublished);
            ViewBag.UserName = user.Email;

            return View(publishedContent);
        }

        // ==================== PROFILE ====================

        //
        //Method Name : Profile (HttpGet)
        //Purpose : Displays the learner profile page with user information
        //Re-use : UserManager
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns profile view with ApplicationUser model
        //
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(user);
        }

        //
        //Method Name : Profile (HttpPost)
        //Purpose : Updates the learner profile information including personal details and profile image
        //Re-use : UserManager, IWebHostEnvironment
        //Input Parameter : ApplicationUser model - The updated user data
        //Input Parameter : IFormFile profileImage - Optional new profile image file
        //Output Type : IActionResult - Redirects to Profile view with success or error message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ApplicationUser model, IFormFile? profileImage)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated!";
                return RedirectToAction("Profile");
            }

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(user);
        }

        // ==================== CONTENT DETAILS ====================

        //
        //Method Name : ContentDetails
        //Purpose : Displays detailed information about a specific published content item
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the content to view
        //Output Type : IActionResult - Returns content details view or NotFound if content doesn't exist or isn't published
        //
        public async Task<IActionResult> ContentDetails(int id)
        {
            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);

            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        // ==================== MY COURSES ====================

        //
        //Method Name : MyCourses
        //Purpose : Displays a list of courses purchased/enrolled by the learner
        //Re-use : None
        //Input Parameter : None
        //Output Type : IActionResult - Returns my courses view with empty list (placeholder for future implementation)
        //
        public async Task<IActionResult> MyCourses()
        {
            return View(new List<Content>());
        }

        // ==================== MY PURCHASES (Redirect to Store) ====================

        //
        //Method Name : MyPurchases
        //Purpose : Redirects to the Store controller's MyPurchases action to display purchase history
        //Re-use : None
        //Input Parameter : None
        //Output Type : IActionResult - Redirects to MyPurchases action in Store controller
        //
        [HttpGet]
        public IActionResult MyPurchases()
        {
            return RedirectToAction("MyPurchases", "Store");
        }
    }
}