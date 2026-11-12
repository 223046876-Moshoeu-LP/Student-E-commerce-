using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Features;
using Website.Data;
using Website.Models;

namespace Website.Controllers
{
    [Authorize(Roles = "Content_Creator")]
    public class ContentCreatorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SQLiteDBContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        //
        //Method Name : ContentCreatorController (Constructor)
        //Purpose : Initializes the ContentCreatorController with required services
        //Re-use : UserManager, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : UserManager - Manages user operations (create, update, delete users)
        //Input Parameter : SQLiteDBContext context - Database context for accessing application data
        //Input Parameter : IWebHostEnvironment webHostEnvironment - Provides web root path for file uploads
        //Output Type : None
        //
        public ContentCreatorController(
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
        //Purpose : Displays the content creator dashboard with content statistics and recent content
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns dashboard view with content list and statistics
        //
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var contents = await _context.Contents
                .Where(c => c.CreatedByUserId != null && c.CreatedByUserId == user.Id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            // FIXED: Client-side calculation to avoid SQLite decimal issue
            var contentIds = contents.Select(c => c.Id).ToList();
            var orderItems = await _context.OrderItems
                .Where(oi => contentIds.Contains(oi.ContentId))
                .ToListAsync();

            var totalSales = orderItems.Sum(oi => oi.Quantity);
            var totalRevenue = orderItems.Sum(oi => oi.Price * oi.Quantity);

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            ViewBag.TotalContent = contents.Count;
            ViewBag.PublishedContent = contents.Count(c => c.IsPublished);
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.User = user;

            return View(contents);
        }

        // ==================== PROFILE IMAGE UPLOAD ====================

        //
        //Method Name : UploadProfileImage
        //Purpose : Uploads a new profile image for the content creator
        //Re-use : UserManager, IWebHostEnvironment
        //Input Parameter : IFormFile profileImage - The image file to upload (max 5MB, JPG/PNG/GIF)
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
                    // Validate file size (max 5MB)
                    if (profileImage.Length > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "File size must be less than 5MB";
                        return RedirectToAction("Profile");
                    }

                    // Validate file extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(profileImage.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only JPG, JPEG, PNG, and GIF files are allowed";
                        return RedirectToAction("Profile");
                    }

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save new image
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profileImage.FileName);
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

        //
        //Method Name : RemoveProfileImage
        //Purpose : Removes the profile image for the content creator
        //Re-use : UserManager, IWebHostEnvironment
        //Input Parameter : None
        //Output Type : IActionResult - Redirects to Profile view with success or error message
        //
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

        // ==================== CHANGE PASSWORD ====================

        //
        //Method Name : ChangePassword
        //Purpose : Changes the password for the currently logged-in content creator
        //Re-use : UserManager
        //Input Parameter : string CurrentPassword - User's current password
        //Input Parameter : string NewPassword - New password to set
        //Input Parameter : string ConfirmPassword - Confirmation of new password
        //Output Type : IActionResult - Redirects to Profile view with success or error message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New passwords do not match!";
                return RedirectToAction("Profile");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "User not found";
                return RedirectToAction("Profile");
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Password changed successfully!";
            }
            else
            {
                TempData["Error"] = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password";
            }

            return RedirectToAction("Profile");
        }

        // ==================== RESET PASSWORD ====================

        //
        //Method Name : ResetPassword
        //Purpose : Initiates password reset process by sending reset link to user's email
        //Re-use : UserManager
        //Input Parameter : string Email - The email address of the user requesting password reset
        //Output Type : IActionResult - Redirects to Profile view with success or error message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                TempData["Success"] = $"Password reset link has been sent to {Email}";
            }
            else
            {
                TempData["Error"] = "Email address not found";
            }
            return RedirectToAction("Profile");
        }

        // ==================== PROFILE MANAGEMENT ====================

        //
        //Method Name : Profile (HttpGet)
        //Purpose : Displays the content creator profile page with user information and statistics
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns profile view with ApplicationUser model and stats
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
            ViewBag.HasImage = !string.IsNullOrEmpty(user.ProfileImageUrl);

            // Get creator stats for profile
            var contents = await _context.Contents
                .Where(c => c.CreatedByUserId != null && c.CreatedByUserId == user.Id)
                .ToListAsync();

            ViewBag.ContentCount = contents.Count;

            // FIXED: Client-side calculation
            var contentIds = contents.Select(c => c.Id).ToList();
            var orderItems = await _context.OrderItems
                .Where(oi => contentIds.Contains(oi.ContentId))
                .ToListAsync();
            ViewBag.TotalSales = orderItems.Sum(oi => oi.Quantity);

            return View(user);
        }

        //
        //Method Name : Profile (HttpPost)
        //Purpose : Updates the content creator profile information including personal details and profile image
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

            // Update basic info
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Bio = model.Bio;
            user.Expertise = model.Expertise;
            user.Website = model.Website;
            user.Twitter = model.Twitter;
            user.LinkedIn = model.LinkedIn;

            // Handle profile image upload
            if (profileImage != null && profileImage.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Error"] = "Only image files (JPG, PNG, GIF) are allowed!";
                    return RedirectToAction("Profile");
                }

                // Validate file size (max 5MB)
                if (profileImage.Length > 5 * 1024 * 1024)
                {
                    TempData["Error"] = "Image size cannot exceed 5MB!";
                    return RedirectToAction("Profile");
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                Directory.CreateDirectory(uploadsFolder);

                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profileImage.FileName);
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
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(user);
        }

        // ==================== CONTENT MANAGEMENT ====================

        //
        //Method Name : CreateContent (HttpGet)
        //Purpose : Displays the form to create new content
        //Re-use : UserManager
        //Input Parameter : None
        //Output Type : IActionResult - Returns create content view
        //
        [HttpGet]
        public async Task<IActionResult> CreateContent()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.ProfileImage = user?.ProfileImageUrl ?? "/images/default-avatar.png";
            return View();
        }

        //
        //Method Name : CreateContent (HttpPost)
        //Purpose : Creates new content with thumbnail and content file uploads
        //Re-use : UserManager, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : Content model - The content data
        //Input Parameter : IFormFile thumbnail - Thumbnail image file (optional)
        //Input Parameter : IFormFile contentFile - Content file (video, audio, PDF, etc.)
        //Output Type : IActionResult - Redirects to MyContent on success or returns view with errors
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(104857600)] // 100MB limit for file uploads
        public async Task<IActionResult> CreateContent(Content model, IFormFile? thumbnail, IFormFile? contentFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                // Handle Thumbnail Upload
                if (thumbnail != null && thumbnail.Length > 0)
                {
                    var thumbnailFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thumbnails");
                    Directory.CreateDirectory(thumbnailFolder);

                    var uniqueThumbnailName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(thumbnail.FileName);
                    var thumbnailPath = Path.Combine(thumbnailFolder, uniqueThumbnailName);

                    using (var stream = new FileStream(thumbnailPath, FileMode.Create))
                    {
                        await thumbnail.CopyToAsync(stream);
                    }

                    model.ThumbnailUrl = "/uploads/thumbnails/" + uniqueThumbnailName;
                }
                else
                {
                    model.ThumbnailUrl = "/images/default-course.jpg";
                }

                // Handle Content File Upload
                if (contentFile != null && contentFile.Length > 0)
                {
                    var contentFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "content");
                    Directory.CreateDirectory(contentFolder);

                    var uniqueContentName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(contentFile.FileName);
                    var contentPath = Path.Combine(contentFolder, uniqueContentName);

                    using (var stream = new FileStream(contentPath, FileMode.Create))
                    {
                        await contentFile.CopyToAsync(stream);
                    }

                    model.ContentUrl = "/uploads/content/" + uniqueContentName;
                }

                model.CreatedByUserId = user.Id;
                model.CreatedDate = DateTime.Now;

                _context.Contents.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Content created successfully!";
                return RedirectToAction("MyContent");
            }

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(model);
        }

        //
        //Method Name : MyContent
        //Purpose : Displays a list of all content created by the current content creator
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns my content view with list of user's content
        //
        public async Task<IActionResult> MyContent()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var contents = await _context.Contents
                .Where(c => c.CreatedByUserId != null && c.CreatedByUserId == user.Id)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(contents);
        }

        //
        //Method Name : EditContent (HttpGet)
        //Purpose : Displays the edit content form for a specific content item
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : int id - The ID of the content to edit
        //Output Type : IActionResult - Returns edit content view or NotFound if content doesn't belong to user
        //
        [HttpGet]
        public async Task<IActionResult> EditContent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId != null && c.CreatedByUserId == user.Id);

            if (content == null)
            {
                return NotFound();
            }

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            return View(content);
        }

        //
        //Method Name : EditContent (HttpPost)
        //Purpose : Updates existing content with new data and optional file uploads
        //Re-use : UserManager, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : int id - The ID of the content to edit
        //Input Parameter : Content model - Updated content data
        //Input Parameter : IFormFile thumbnail - New thumbnail image (optional)
        //Input Parameter : IFormFile contentFile - New content file (optional)
        //Output Type : IActionResult - Redirects to MyContent on success or returns view with errors
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(int id, Content model, IFormFile? thumbnail, IFormFile? contentFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var existingContent = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId != null && c.CreatedByUserId == user.Id);

            if (existingContent == null)
            {
                return NotFound();
            }

            // Handle Thumbnail Upload
            if (thumbnail != null && thumbnail.Length > 0)
            {
                var thumbnailFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thumbnails");
                Directory.CreateDirectory(thumbnailFolder);

                if (!string.IsNullOrEmpty(existingContent.ThumbnailUrl) && !existingContent.ThumbnailUrl.Contains("default"))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingContent.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uniqueThumbnailName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(thumbnail.FileName);
                var thumbnailPath = Path.Combine(thumbnailFolder, uniqueThumbnailName);

                using (var stream = new FileStream(thumbnailPath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(stream);
                }

                existingContent.ThumbnailUrl = "/uploads/thumbnails/" + uniqueThumbnailName;
            }

            // Handle Content File Upload
            if (contentFile != null && contentFile.Length > 0)
            {
                var contentFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "content");
                Directory.CreateDirectory(contentFolder);

                if (!string.IsNullOrEmpty(existingContent.ContentUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, existingContent.ContentUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var uniqueContentName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(contentFile.FileName);
                var contentPath = Path.Combine(contentFolder, uniqueContentName);

                using (var stream = new FileStream(contentPath, FileMode.Create))
                {
                    await contentFile.CopyToAsync(stream);
                }

                existingContent.ContentUrl = "/uploads/content/" + uniqueContentName;
            }

            // Update properties
            existingContent.Title = model.Title;
            existingContent.Description = model.Description;
            existingContent.Category = model.Category;
            existingContent.Price = model.Price;
            existingContent.Duration = model.Duration;
            existingContent.IsPublished = model.IsPublished;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Content updated successfully!";
            return RedirectToAction("MyContent");
        }

        //
        //Method Name : TogglePublish
        //Purpose : Toggles the published status of a content item (publish/unpublish)
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : int id - The ID of the content to toggle publish status
        //Output Type : IActionResult - Redirects to MyContent with success message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId != null && c.CreatedByUserId == user.Id);

            if (content != null)
            {
                content.IsPublished = !content.IsPublished;
                await _context.SaveChangesAsync();
                TempData["Success"] = content.IsPublished ? "Content published and now visible to learners!" : "Content unpublished.";
            }

            return RedirectToAction("MyContent");
        }

        //
        //Method Name : DeleteContent
        //Purpose : Deletes a content item and its associated files (thumbnail and content file)
        //Re-use : UserManager, SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : int id - The ID of the content to delete
        //Output Type : IActionResult - Redirects to MyContent with success message
        //
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteContent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var content = await _context.Contents
                .FirstOrDefaultAsync(c => c.Id == id && c.CreatedByUserId != null && c.CreatedByUserId == user.Id);

            if (content != null)
            {
                // Delete thumbnail file
                if (!string.IsNullOrEmpty(content.ThumbnailUrl) && !content.ThumbnailUrl.Contains("default"))
                {
                    var thumbPath = Path.Combine(_webHostEnvironment.WebRootPath, content.ThumbnailUrl.TrimStart('/'));
                    if (System.IO.File.Exists(thumbPath))
                    {
                        System.IO.File.Delete(thumbPath);
                    }
                }

                // Delete content file
                if (!string.IsNullOrEmpty(content.ContentUrl))
                {
                    var contentPath = Path.Combine(_webHostEnvironment.WebRootPath, content.ContentUrl.TrimStart('/'));
                    if (System.IO.File.Exists(contentPath))
                    {
                        System.IO.File.Delete(contentPath);
                    }
                }

                _context.Contents.Remove(content);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Content deleted successfully!";
            }

            return RedirectToAction("MyContent");
        }

        // ==================== ANALYTICS ====================

        //
        //Method Name : Analytics
        //Purpose : Displays analytics dashboard with content performance metrics and sales data
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns analytics view with content statistics and best-selling content
        //
        public async Task<IActionResult> Analytics()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var contents = await _context.Contents
                .Where(c => c.CreatedByUserId != null && c.CreatedByUserId == user.Id)
                .ToListAsync();

            // FIXED: Client-side calculation
            var contentIds = contents.Select(c => c.Id).ToList();
            var orderItems = await _context.OrderItems
                .Where(oi => contentIds.Contains(oi.ContentId))
                .ToListAsync();

            var totalSales = orderItems.Sum(oi => oi.Quantity);
            var totalRevenue = orderItems.Sum(oi => oi.Price * oi.Quantity);

            var bestSelling = contents
                .Select(c => new { Content = c, Sales = orderItems.Count(oi => oi.ContentId == c.Id) })
                .OrderByDescending(x => x.Sales)
                .FirstOrDefault();

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            ViewBag.TotalContent = contents.Count;
            ViewBag.PublishedContent = contents.Count(c => c.IsPublished);
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.BestSelling = bestSelling?.Content;
            ViewBag.User = user;

            return View(contents);
        }

        // ==================== PUBLIC CREATOR PROFILE (for Learners) ====================

        //
        //Method Name : PublicProfile
        //Purpose : Displays a public profile page for a content creator (viewable by learners)
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : string id - The user ID of the content creator to display
        //Output Type : IActionResult - Returns public profile view with creator's published content
        //
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PublicProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var isContentCreator = await _userManager.IsInRoleAsync(user, "Content_Creator");
            if (!isContentCreator)
            {
                return NotFound();
            }

            var contents = await _context.Contents
                .Where(c => c.CreatedByUserId != null && c.CreatedByUserId == user.Id && c.IsPublished)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            ViewBag.ProfileImage = user.ProfileImageUrl ?? "/images/default-avatar.png";
            ViewBag.ContentCount = contents.Count;

            // FIXED: Client-side calculation
            var contentIds = contents.Select(c => c.Id).ToList();
            var orderItems = await _context.OrderItems
                .Where(oi => contentIds.Contains(oi.ContentId))
                .ToListAsync();
            ViewBag.TotalSales = orderItems.Sum(oi => oi.Quantity);

            ViewBag.User = user;
            return View(contents);
        }
    }
}