using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        //
        //Method Name : AdminController (Constructor)
        //Purpose : Initializes the AdminController with required services
        //Re-use : UserManager, RoleManager, LoginDBContext, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : UserManager, RoleManager, loginContext, context, webHostEnvironment
        //Output Type : None
        //
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly LoginDBContext _loginContext;
        private readonly SQLiteDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            LoginDBContext loginContext,
            SQLiteDBContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _loginContext = loginContext;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== DASHBOARD ====================
        //
        //Method Name : Index
        //Purpose : Loads the admin dashboard with user statistics and content counts
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None
        //Output Type : IActionResult - Returns the dashboard view with ViewBag data
        //
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var learners = new List<ApplicationUser>();
            var creators = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User")) learners.Add(user);
                else if (roles.Contains("Content_Creator")) creators.Add(user);
            }

            ViewBag.TotalUsers = users.Count;
            ViewBag.TotalLearners = learners.Count;
            ViewBag.TotalCreators = creators.Count;
            ViewBag.TotalContent = await _context.Contents.CountAsync();
            ViewBag.PublishedContent = await _context.Contents.CountAsync(c => c.IsPublished);

            return View();
        }

        // ==================== PROFILE MANAGEMENT ====================
        //
        //Method Name : Profile (HttpGet)
        //Purpose : Displays the admin profile page with user information and stats
        //Re-use : UserManager
        //Input Parameter : None
        //Output Type : IActionResult - Returns the profile view with ApplicationUser model
        //
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.HasImage = !string.IsNullOrEmpty(user?.ProfileImageUrl);
            ViewBag.ProfileImage = user?.ProfileImageUrl ?? "/images/default-avatar.png";

            // Get admin stats
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalContent = await _context.Contents.CountAsync();

            return View(user);
        }

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
            user.Expertise = model.Expertise;
            user.Website = model.Website;
            user.Twitter = model.Twitter;
            user.LinkedIn = model.LinkedIn;

            if (profileImage != null && profileImage.Length > 0)
            {
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "File size must be less than 5MB";
                    return RedirectToAction("Profile");
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(profileImage.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Only JPG, JPEG, and PNG files are allowed";
                    return RedirectToAction("Profile");
                }

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

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.HasImage = !string.IsNullOrEmpty(user.ProfileImageUrl);
            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(user);
        }

        // ==================== PROFILE IMAGE UPLOAD ====================
        //
        //Method Name : RemoveProfileImage
        //Purpose : Removes the profile image for the admin user
        //Re-use : UserManager, IWebHostEnvironment
        //Input Parameter : None
        //Output Type : IActionResult - Redirects to Profile view with success or error message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Profile");
                }

                if (profileImage != null && profileImage.Length > 0)
                {
                    if (profileImage.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size must be less than 5MB";
                        return RedirectToAction("Profile");
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(profileImage.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only JPG, JPEG, and PNG files are allowed";
                        return RedirectToAction("Profile");
                    }

                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(fileStream);
                    }

                    user.ProfileImageUrl = "/uploads/profiles/" + uniqueFileName;
                    await _userManager.UpdateAsync(user);

                    TempData["Success"] = "Profile image uploaded successfully!";
                }
                else
                {
                    TempData["Error"] = "No file selected";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error uploading image: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }

        // ==================== REMOVE PROFILE IMAGE ====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfileImage()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Profile");
                }

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    user.ProfileImageUrl = null;
                    await _userManager.UpdateAsync(user);

                    TempData["Success"] = "Profile image removed successfully!";
                }
                else
                {
                    TempData["Error"] = "No profile image to remove";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error removing image: {ex.Message}";
            }

            return RedirectToAction("Profile");
        }

        // ==================== CONTENT CREATOR DETAILS (ADD THIS) ====================
        //
        //Method Name : CreatorDetails
        //Purpose : Displays detailed information about a specific content creator including their content
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : string id - The user ID of the content creator
        //Output Type : IActionResult - Returns the creator details view or redirects with error message
        //
        [HttpGet]
        public async Task<IActionResult> CreatorDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Invalid user ID.";
                return RedirectToAction("ContentCreators");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "Content creator not found.";
                return RedirectToAction("ContentCreators");
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Content_Creator"))
            {
                TempData["Error"] = "This user is not a content creator.";
                return RedirectToAction("ContentCreators");
            }

            // Get creator's content
            var creatorContent = await _context.Contents
                .Where(c => c.CreatedByUserId == user.Id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.CreatorContent = creatorContent;
            ViewBag.TotalContent = creatorContent.Count;
            ViewBag.PublishedContent = creatorContent.Count(c => c.IsPublished);
            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";

            return View(user);
        }

        // ==================== LEARNERS MANAGEMENT ====================
        //
        //Method Name : Learners
        //Purpose : Displays a list of all registered learners with optional search functionality
        //Re-use : UserManager
        //Input Parameter : string searchTerm - Optional search term to filter learners by email or name
        //Output Type : IActionResult - Returns the learners view with filtered list
        //
        public async Task<IActionResult> Learners(string searchTerm)
        {
            var users = await _userManager.Users.ToListAsync();
            var learners = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("User"))
                {
                    if (string.IsNullOrEmpty(searchTerm) ||
                        user.Email.ToLower().Contains(searchTerm.ToLower()) ||
                        (user.FirstName != null && user.FirstName.ToLower().Contains(searchTerm.ToLower())) ||
                        (user.LastName != null && user.LastName.ToLower().Contains(searchTerm.ToLower())))
                    {
                        learners.Add(user);
                    }
                }
            }

            ViewBag.SearchTerm = searchTerm;
            return View(learners);
        }

        // ==================== CONTENT CREATORS MANAGEMENT ====================
        //
        //Method Name : ContentCreators
        //Purpose : Displays a list of all registered content creators with optional search functionality
        //Re-use : UserManager
        //Input Parameter : string searchTerm - Optional search term to filter creators by email or name
        //Output Type : IActionResult - Returns the content creators view with filtered list
        //
        public async Task<IActionResult> ContentCreators(string searchTerm)
        {
            var users = await _userManager.Users.ToListAsync();
            var creators = new List<ApplicationUser>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Content_Creator"))
                {
                    if (string.IsNullOrEmpty(searchTerm) ||
                        user.Email.ToLower().Contains(searchTerm.ToLower()) ||
                        (user.FirstName != null && user.FirstName.ToLower().Contains(searchTerm.ToLower())) ||
                        (user.LastName != null && user.LastName.ToLower().Contains(searchTerm.ToLower())))
                    {
                        creators.Add(user);
                    }
                }
            }

            ViewBag.SearchTerm = searchTerm;
            return View(creators);
        }

        // ==================== DELETE USER ====================
        //
        //Method Name : DeleteUser
        //Purpose : Deletes a user account and tracks the deletion in deleted records
        //Re-use : UserManager, LoginDBContext, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : string id - The user ID to delete, string returnUrl - Optional redirect URL
        //Output Type : IActionResult - Redirects to Learners or ContentCreators view
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id, string returnUrl = null)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var currentUser = await _userManager.GetUserAsync(User);
                var currentRoles = await _userManager.GetRolesAsync(currentUser);

                var deletedRecord = new DeletedRecord
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    UserRole = roles.FirstOrDefault(),
                    DeletedByUserId = currentUser.Id,
                    DeletedByEmail = currentUser.Email,
                    DeletedByRole = currentRoles.FirstOrDefault(),
                    DeletedAt = DateTime.Now,
                    Reason = "Account deleted by admin"
                };

                _loginContext.DeletedRecords.Add(deletedRecord);

                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                if (roles.Contains("Content_Creator"))
                {
                    var userContent = await _context.Contents.Where(c => c.CreatedByUserId == user.Id).ToListAsync();
                    _context.Contents.RemoveRange(userContent);
                    await _context.SaveChangesAsync();
                }

                await _userManager.DeleteAsync(user);
                await _loginContext.SaveChangesAsync();

                TempData["Success"] = $"User {user.Email} has been deleted.";
            }

            // Redirect back to the appropriate page
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.Contains("ContentCreators"))
            {
                return RedirectToAction("ContentCreators");
            }

            return RedirectToAction("Learners");
        }

        // ==================== DELETED RECORDS ====================
        //
        //Method Name : DeletedRecords
        //Purpose : Displays audit trail of all deleted user accounts with search/filter options
        //Re-use : LoginDBContext
        //Input Parameter : string searchBy - Field to search by, string searchTerm - Search value
        //Output Type : IActionResult - Returns the deleted records view with filtered list
        //
        public async Task<IActionResult> DeletedRecords(string searchBy, string searchTerm)
        {
            var records = await _loginContext.DeletedRecords
                .OrderByDescending(r => r.DeletedAt)
                .ToListAsync();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (searchBy == "DeletedUser")
                {
                    records = records.Where(r => r.UserEmail != null && r.UserEmail.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                else if (searchBy == "DeletedBy")
                {
                    records = records.Where(r => r.DeletedByEmail != null && r.DeletedByEmail.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
                else if (searchBy == "Role")
                {
                    records = records.Where(r => r.UserRole != null && r.UserRole.ToLower().Contains(searchTerm.ToLower())).ToList();
                }
            }

            ViewBag.SearchBy = searchBy;
            ViewBag.SearchTerm = searchTerm;
            return View(records);
        }

        // ==================== PROMOTIONS ====================
        //
        //Method Name : Promotions
        //Purpose : Displays a list of all promotions
        //Re-use : SQLiteDBContext
        //Input Parameter : None
        //Output Type : IActionResult - Returns the promotions view with list of promotions
        //
        public async Task<IActionResult> Promotions()
        {
            var promotions = await _context.Promotions
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(promotions);
        }
        //
        //Method Name : CreatePromotion (HttpPost)
        //Purpose : Creates a new promotion and saves it to the database
        //Re-use : SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : Promotion model - The promotion data, IFormFile image - Optional image file
        //Output Type : IActionResult - Redirects to Promotions view on success or returns view with errors
        //

        [HttpGet]
        public async Task<IActionResult> CreatePromotion()
        {
            var contents = await _context.Contents
                .Where(c => c.IsPublished)
                .ToListAsync();

            ViewBag.Contents = contents;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePromotion(Promotion model, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "promotions");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    model.ImageUrl = "/uploads/promotions/" + uniqueFileName;
                }
                else
                {
                    model.ImageUrl = "/images/default-promo.jpg";
                }

                model.CreatedAt = DateTime.Now;
                _context.Promotions.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Promotion created successfully!";
                return RedirectToAction("Promotions");
            }

            var contents = await _context.Contents.Where(c => c.IsPublished).ToListAsync();
            ViewBag.Contents = contents;
            return View(model);
        }
        //
        //Method Name : DeletePromotion
        //Purpose : Deletes a promotion and removes its associated image file
        //Re-use : SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : int id - The ID of the promotion to delete
        //Output Type : IActionResult - Redirects to Promotions view with success message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePromotion(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                if (!string.IsNullOrEmpty(promotion.ImageUrl))
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, promotion.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Promotion deleted successfully!";
            }

            return RedirectToAction("Promotions");
        }
    }
}