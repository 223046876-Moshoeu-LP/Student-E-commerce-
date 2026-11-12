using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;

namespace Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SQLiteDBContext _context;
        //
        //Method Name : HomeController (Constructor)
        //Purpose : Initializes the HomeController with required services
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : UserManager<ApplicationUser> userManager - Manages user operations
        //Input Parameter : SQLiteDBContext context - Database context for promotions and content
        //Output Type : None
        //
        public HomeController(UserManager<ApplicationUser> userManager, SQLiteDBContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        //
        //Method Name : Index
        //Purpose : Displays the home page landing page; redirects authenticated users to their respective dashboards
        //Re-use : UserManager, SQLiteDBContext
        //Input Parameter : None - User information retrieved from authenticated user claims
        //Output Type : IActionResult - Redirects to role-specific dashboard or returns landing page view
        //
        public async Task<IActionResult> Index()
        {
            // Get promotions for the landing page
            var promotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now.Date && p.StartDate <= DateTime.Now)
                .ToListAsync();

            ViewBag.Promotions = promotions;

            // Check if user is authenticated before trying to get user info
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);

                // Check if user exists
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var userRole = roles.FirstOrDefault();

                    // Redirect based on role
                    if (userRole == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (userRole == "Content_Creator")
                    {
                        return RedirectToAction("Dashboard", "ContentCreator");
                    }
                    else if (userRole == "User")
                    {
                        return RedirectToAction("Dashboard", "Learner");
                    }
                }
            }

            // Not logged in or no role match - show landing page
            return View();
        }
        //
        //Method Name : Privacy
        //Purpose : Displays the privacy policy page
        //Re-use : None
        //Input Parameter : None
        //Output Type : IActionResult - Returns privacy policy view
        //
        public IActionResult Privacy()
        {
            return View();
        }
        //
        //Method Name : About
        //Purpose : Displays the about us page with company information
        //Re-use : None
        //Input Parameter : None
        //Output Type : IActionResult - Returns about us view
        //
        public IActionResult About()
        {
            return View();
        }
        //
        //Method Name : Contact (HttpGet)
        //Purpose : Displays the contact us page with contact form and map
        //Re-use : None
        //Input Parameter : None
        //Output Type : IActionResult - Returns contact us view
        //
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        //
        //Method Name : Contact (HttpPost)
        //Purpose : Processes the contact form submission
        //Re-use : TempData for success message
        //Input Parameter : string name - Sender's name
        //Input Parameter : string email - Sender's email address
        //Input Parameter : string message - Message content from the sender
        //Output Type : IActionResult - Redirects back to contact page with success message
        //
        public IActionResult Contact(string name, string email, string message)
        {
            TempData["Success"] = "Your message has been sent!";
            return RedirectToAction("Contact");
        }
        [HttpGet]
        public async Task<IActionResult> AddSamplePromotions()
        {
            var existingPromos = await _context.Promotions.ToListAsync();
            if (existingPromos.Any())
            {
                return Content("Promotions already exist! Total: " + existingPromos.Count);
            }

            var promotions = new List<Promotion>
    {
        new Promotion
        {
            Title = "SUMMER SALE",
            Description = "Get 30% off on all programming courses! Limited time offer.",
            DiscountPercentage = 30,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(7),
            IsActive = true,
            ImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=1200",
            CreatedAt = DateTime.Now
        },
        new Promotion
        {
            Title = "NEW YEAR SPECIAL",
            Description = "50% off on all design courses. Start the new year with new skills!",
            DiscountPercentage = 50,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(14),
            IsActive = true,
            ImageUrl = "https://images.unsplash.com/photo-1467810563316-b5476525c0f9?w=1200",
            CreatedAt = DateTime.Now
        },
        new Promotion
        {
            Title = "FLASH SALE",
            Description = "40% off on all video courses. This weekend only!",
            DiscountPercentage = 40,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(3),
            IsActive = true,
            ImageUrl = "https://images.unsplash.com/photo-1487088678257-3a541e6e3925?w=1200",
            CreatedAt = DateTime.Now
        },
        new Promotion
        {
            Title = "WEEKEND DEAL",
            Description = "25% off on all business courses. Upskill yourself today!",
            DiscountPercentage = 25,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(2),
            IsActive = true,
            ImageUrl = "https://images.unsplash.com/photo-1557804506-669a67965ba0?w=1200",
            CreatedAt = DateTime.Now
        },
        new Promotion
        {
            Title = "STUDENT DISCOUNT",
            Description = "15% off for students. Verify your student email to get discount.",
            DiscountPercentage = 15,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30),
            IsActive = true,
            ImageUrl = "https://images.unsplash.com/photo-1523050854058-8df90110c9f1?w=1200",
            CreatedAt = DateTime.Now
        }
    };

            await _context.Promotions.AddRangeAsync(promotions);
            await _context.SaveChangesAsync();

            return Content($"Added {promotions.Count} sample promotions!");
        }
    }
}