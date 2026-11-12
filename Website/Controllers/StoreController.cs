using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Website.Controllers
{
    public class StoreController : Controller
    {
        private readonly SQLiteDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        //
        //Method Name : StoreController (Constructor)
        //Purpose : Initializes the StoreController with required services
        //Re-use : SQLiteDBContext, UserManager
        //Input Parameter : SQLiteDBContext context - Database context for accessing application data
        //Input Parameter : UserManager<ApplicationUser> userManager - Manages user operations
        //Output Type : None
        //
        public StoreController(SQLiteDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==================== STORE HOME ====================

        //
        //Method Name : Index
        //Purpose : Displays the store homepage with all published content and active promotions
        //Re-use : SQLiteDBContext
        //Input Parameter : None
        //Output Type : IActionResult - Returns store index view with list of published content and promotions
        //
        public async Task<IActionResult> Index()
        {
            var contents = await _context.Contents
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            var promotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now.Date)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Promotions = promotions ?? new List<Promotion>();
            return View(contents);
        }

        // ==================== CONTENT DETAILS ====================

        //
        //Method Name : ContentDetails
        //Purpose : Displays detailed information about a specific published content item
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the content to view
        //Output Type : IActionResult - Returns content details view or NotFound if content doesn't exist
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

        // ==================== GET CART COUNT (AJAX) ====================

        //
        //Method Name : GetCartCount
        //Purpose : Retrieves the total number of items in the user's cart for AJAX requests
        //Re-use : SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : JsonResult - Returns JSON object with cart item count
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var totalCount = cartItems.Sum(c => c.Quantity);
            return Json(new { count = totalCount });
        }

        // ==================== CART ====================

        //
        //Method Name : Cart (HttpGet)
        //Purpose : Displays the user's shopping cart with all items and total amount
        //Re-use : SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns cart view with list of cart items and total
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            ViewBag.Total = cartItems.Sum(c => c.ContentPrice * c.Quantity);
            return View(cartItems);
        }

        // ==================== ADD TO CART ====================

        //
        //Method Name : AddToCart
        //Purpose : Adds a content item to the user's shopping cart
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the content to add
        //Input Parameter : int quantity - Quantity to add (default: 1)
        //Output Type : IActionResult - Redirects to Cart view with success or error message
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var content = await _context.Contents.FindAsync(id);
            if (content == null)
            {
                TempData["Error"] = "Content not found!";
                return RedirectToAction("Index");
            }

            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ContentId == id);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.Carts.Add(new Cart
                {
                    UserId = userId,
                    ContentId = id,
                    Quantity = quantity,
                    AddedDate = DateTime.Now,
                    ContentTitle = content.Title ?? string.Empty,
                    ContentPrice = content.Price,
                    ContentThumbnailUrl = content.ThumbnailUrl
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"{content.Title} added to cart!";
            return RedirectToAction("Cart");
        }

        // ==================== UPDATE CART ====================

        //
        //Method Name : UpdateCart
        //Purpose : Updates the quantity of a cart item or removes it if quantity is zero or less
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the cart item to update
        //Input Parameter : int quantity - New quantity value (removes if <= 0)
        //Output Type : IActionResult - Redirects to Cart view with success message
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(int id, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.Carts.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cart updated!";
            }

            return RedirectToAction("Cart");
        }

        // ==================== REMOVE FROM CART ====================

        //
        //Method Name : RemoveFromCart
        //Purpose : Removes a specific item from the user's shopping cart
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the cart item to remove
        //Output Type : IActionResult - Redirects to Cart view with success message
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item removed from cart";
            }

            return RedirectToAction("Cart");
        }

        // ==================== APPLY PROMOTION CODE ====================

        //
        //Method Name : ApplyPromotionCode
        //Purpose : Applies a promotion/discount code to the current cart total
        //Re-use : SQLiteDBContext, HttpContext.Session
        //Input Parameter : string promotionCode - The promotion code to apply
        //Output Type : JsonResult - Returns JSON with success/failure status and message
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPromotionCode(string promotionCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Please login first" });
            }

            var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
            if (!cartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty" });
            }

            var subtotal = cartItems.Sum(c => c.ContentPrice * c.Quantity);

            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Title != null && p.Title.ToUpper() == promotionCode.ToUpper()
                                           && p.IsActive
                                           && p.StartDate <= DateTime.Now
                                           && p.EndDate >= DateTime.Now);

            if (promotion == null || !promotion.DiscountPercentage.HasValue)
            {
                return Json(new { success = false, message = "Invalid or expired promo code!" });
            }

            var discountPercent = promotion.DiscountPercentage.Value;
            var discountAmount = subtotal * discountPercent / 100m;

            HttpContext.Session.SetString("AppliedPromotionCode", promotion.Title ?? string.Empty);
            HttpContext.Session.SetString("AppliedPromotionDiscount", discountAmount.ToString());
            HttpContext.Session.SetString("AppliedPromotionId", promotion.Id.ToString());
            HttpContext.Session.SetString("AppliedPromotionPercentage", discountPercent.ToString());

            return Json(new { success = true, message = $"{discountPercent}% discount applied! You saved ${discountAmount:F2}" });
        }

        // ==================== REMOVE PROMOTION CODE ====================

        //
        //Method Name : RemovePromotionCode
        //Purpose : Removes the applied promotion code from the current session
        //Re-use : HttpContext.Session
        //Input Parameter : None
        //Output Type : JsonResult - Returns JSON with success status
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePromotionCode()
        {
            HttpContext.Session.Remove("AppliedPromotionCode");
            HttpContext.Session.Remove("AppliedPromotionDiscount");
            HttpContext.Session.Remove("AppliedPromotionId");
            HttpContext.Session.Remove("AppliedPromotionPercentage");
            return Json(new { success = true });
        }

        // ==================== CHECKOUT GET ====================

        //
        //Method Name : Checkout (HttpGet)
        //Purpose : Displays the checkout page with cart summary and available promotions
        //Re-use : SQLiteDBContext, HttpContext.Session
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns checkout view with cart items and discount information
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            var activePromotions = await _context.Promotions
                .Where(p => p.IsActive && p.EndDate >= DateTime.Now.Date && p.StartDate <= DateTime.Now)
                .ToListAsync();

            ViewBag.ActivePromotions = activePromotions;
            var subtotal = cartItems.Sum(c => c.ContentPrice * c.Quantity);
            ViewBag.Subtotal = subtotal;

            decimal discountAmount = 0;
            string? appliedPromoCode = null;
            decimal discountPercentage = 0;

            var sessionPromoId = HttpContext.Session.GetString("AppliedPromotionId");
            if (!string.IsNullOrEmpty(sessionPromoId))
            {
                if (int.TryParse(sessionPromoId, out int promoId))
                {
                    var promo = await _context.Promotions.FindAsync(promoId);

                    if (promo != null && promo.IsActive && promo.StartDate <= DateTime.Now && promo.EndDate >= DateTime.Now)
                    {
                        discountPercentage = promo.DiscountPercentage ?? 0;
                        discountAmount = subtotal * discountPercentage / 100m;
                        appliedPromoCode = promo.Title;
                    }
                    else
                    {
                        HttpContext.Session.Remove("AppliedPromotionCode");
                        HttpContext.Session.Remove("AppliedPromotionDiscount");
                        HttpContext.Session.Remove("AppliedPromotionId");
                        HttpContext.Session.Remove("AppliedPromotionPercentage");
                    }
                }
            }

            ViewBag.DiscountAmount = discountAmount;
            ViewBag.DiscountPercentage = discountPercentage;
            ViewBag.AppliedPromotionCode = appliedPromoCode;

            return View(cartItems);
        }

        // ==================== CHECKOUT POST ====================

        //
        //Method Name : Checkout (HttpPost)
        //Purpose : Processes the order, creates order records, and clears the cart
        //Re-use : SQLiteDBContext, HttpContext.Session
        //Input Parameter : string cardNumber - Credit card number (mock payment)
        //Input Parameter : string expiry - Card expiry date (mock payment)
        //Input Parameter : string cvv - Card CVV code (mock payment)
        //Input Parameter : string firstName - Billing first name
        //Input Parameter : string lastName - Billing last name
        //Input Parameter : string email - Billing email address
        //Output Type : IActionResult - Redirects to OrderConfirmation page on success
        //
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(string cardNumber, string expiry, string cvv, string firstName, string lastName, string email)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            var subtotal = cartItems.Sum(c => c.ContentPrice * c.Quantity);
            decimal discountAmount = 0;
            decimal discountPercentage = 0;
            string? appliedPromotionCode = null;

            var sessionPromoId = HttpContext.Session.GetString("AppliedPromotionId");
            if (!string.IsNullOrEmpty(sessionPromoId))
            {
                if (int.TryParse(sessionPromoId, out int promoId))
                {
                    var promo = await _context.Promotions.FindAsync(promoId);
                    if (promo != null && promo.IsActive && promo.StartDate <= DateTime.Now && promo.EndDate >= DateTime.Now)
                    {
                        appliedPromotionCode = promo.Title;
                        discountPercentage = promo.DiscountPercentage ?? 0;
                        discountAmount = subtotal * discountPercentage / 100m;
                    }
                }
            }

            var totalAmount = subtotal - discountAmount;
            var orderNumber = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            var order = new Order
            {
                OrderNumber = orderNumber,
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                Status = "Completed"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ContentId = item.ContentId,
                    ContentTitle = item.ContentTitle ?? string.Empty,
                    Price = item.ContentPrice,
                    Quantity = item.Quantity
                };
                _context.OrderItems.Add(orderItem);

                var content = await _context.Contents.FindAsync(item.ContentId);

                var purchase = new Purchase
                {
                    UserId = userId,
                    ContentId = item.ContentId,
                    ContentTitle = item.ContentTitle ?? string.Empty,
                    ContentCategory = content?.Category ?? string.Empty,
                    Price = item.ContentPrice,
                    ContentUrl = content?.ContentUrl,
                    ThumbnailUrl = content?.ThumbnailUrl,
                    PurchaseDate = DateTime.Now,
                    OrderNumber = orderNumber
                };
                _context.Purchases.Add(purchase);
            }

            _context.Carts.RemoveRange(cartItems);
            HttpContext.Session.Clear();
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order completed successfully! {(discountAmount > 0 ? $"You saved ${discountAmount:F2} ({discountPercentage}% OFF)" : "")}";
            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }

        // ==================== ORDER CONFIRMATION ====================

        //
        //Method Name : OrderConfirmation
        //Purpose : Displays the order confirmation page with order details
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the order to confirm
        //Output Type : IActionResult - Returns order confirmation view with order and items
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderItems = await _context.OrderItems
                .Where(oi => oi.OrderId == id)
                .ToListAsync();

            ViewBag.Order = order;
            ViewBag.OrderItems = orderItems;
            return View();
        }

        // ==================== PROMOTION DETAILS ====================

        //
        //Method Name : PromotionDetails
        //Purpose : Displays detailed information about a specific promotion
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the promotion to view
        //Output Type : IActionResult - Returns promotion details view or NotFound
        //
        public async Task<IActionResult> PromotionDetails(int id)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }

            return View(promotion);
        }

        // ==================== WATCH VIDEO ====================

        //
        //Method Name : Watch
        //Purpose : Displays the video player page for video content
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the video content to watch
        //Output Type : IActionResult - Returns watch view with content
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Watch(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            ViewBag.Content = content;
            return View(content);
        }

        // ==================== LISTEN PODCAST ====================

        //
        //Method Name : Listen
        //Purpose : Displays the audio player page for podcast content
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the podcast content to listen to
        //Output Type : IActionResult - Returns listen view with content
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Listen(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            ViewBag.Content = content;
            return View(content);
        }

        // ==================== READ ARTICLE ====================

        //
        //Method Name : Read
        //Purpose : Displays the article reader page for text content
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the article to read
        //Output Type : IActionResult - Returns read view with content
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Read(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            ViewBag.Content = content;
            return View(content);
        }

        // ==================== DOWNLOAD ====================

        //
        //Method Name : Download
        //Purpose : Initiates download of a content file (simplified version - placeholder)
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the content to download
        //Output Type : IActionResult - Redirects to ContentDetails with success message
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Download(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            TempData["Success"] = $"Downloading {content.Title}...";
            return RedirectToAction("ContentDetails", new { id });
        }

        // ==================== REGISTER WORKSHOP ====================

        //
        //Method Name : RegisterWorkshop
        //Purpose : Registers the user for a workshop content (simplified version - placeholder)
        //Re-use : SQLiteDBContext
        //Input Parameter : int id - The ID of the workshop to register for
        //Output Type : IActionResult - Redirects to ContentDetails with success message
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RegisterWorkshop(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            if (content == null) return NotFound();
            TempData["Success"] = $"Registered for {content.Title}!";
            return RedirectToAction("ContentDetails", new { id });
        }

        // ==================== MY PURCHASES ====================

        //
        //Method Name : MyPurchases
        //Purpose : Displays a list of all content purchased by the current user
        //Re-use : SQLiteDBContext
        //Input Parameter : None - User ID retrieved from authenticated user claims
        //Output Type : IActionResult - Returns my purchases view with list of purchases
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyPurchases()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var purchases = await _context.Purchases
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // ==================== DOWNLOAD PURCHASED CONTENT ====================

        //
        //Method Name : DownloadPurchasedContent
        //Purpose : Downloads a content file that the user has previously purchased
        //Re-use : SQLiteDBContext, IWebHostEnvironment
        //Input Parameter : int id - The ID of the purchase record
        //Output Type : FileResult - Returns the file for download or redirects with error
        //
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadPurchasedContent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (purchase == null)
            {
                TempData["Error"] = "You have not purchased this content!";
                return RedirectToAction("MyPurchases");
            }

            var content = await _context.Contents.FindAsync(purchase.ContentId);
            if (content == null || string.IsNullOrEmpty(content.ContentUrl))
            {
                TempData["Error"] = "Content file not found!";
                return RedirectToAction("MyPurchases");
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", content.ContentUrl.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                TempData["Error"] = "File does not exist!";
                return RedirectToAction("MyPurchases");
            }

            var fileName = Path.GetFileName(filePath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(fileBytes, "application/octet-stream", fileName);
        }
    }
}