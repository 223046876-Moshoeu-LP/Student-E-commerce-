using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Website.Data;
using Website.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.IISIntegration;
/* 
 * 223046876 Moshoeu LP
 * 223000460 Sithole A
 * 223050336 Pheko ST
 * 224027806 Motone TMC
 * 224000274 Nkuna MA
 */
var builder = WebApplication.CreateBuilder(args);

// ==================== ADD SESSION SUPPORT ====================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".EduPlatform.Session";
});
// ==================== END SESSION SUPPORT ====================

// ==================== ADD FILE UPLOAD SIZE LIMIT ====================
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104857600; // 100MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 104857600; // 100MB
    options.MemoryBufferThreshold = int.MaxValue;
});
// ==================== END FILE UPLOAD SIZE LIMIT ====================

// Add DbContexts
builder.Services.AddDbContext<LoginDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<SQLiteDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity with ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<LoginDBContext>();

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ManagePromotions", policy => policy.RequireRole("Admin", "Moderator"));
    options.AddPolicy("PublishContent", policy => policy.RequireRole("Content_Creator", "Admin"));
});

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ==================== IMPORTANT: Add Session middleware ====================
// Session MUST be added before Authentication and Authorization
app.UseSession(); // THIS IS THE KEY LINE THAT WAS MISSING
// ==================== END SESSION MIDDLEWARE ====================

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ==================== SEED ROLES ====================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "Admin", "User", "Content_Creator", "Vendor", "Moderator" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

// ==================== SEED SAMPLE CONTENT ====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SQLiteDBContext>();

    if (!await context.Contents.AnyAsync())
    {
        var sampleContent = new List<Content>
        {
            new Content
            {
                Title = "ASP.NET Core Masterclass",
                Description = "Learn ASP.NET Core from scratch and build modern web applications.",
                Category = "Course",
                Price = 49.99m,
                ThumbnailUrl = "https://placehold.co/400x200/008080/white?text=ASP.NET",
                IsPublished = true,
                CreatedDate = DateTime.Now.AddDays(-30),
                Duration = 320,
                ContentUrl = "https://www.youtube.com/watch?v=example1"
            },
            new Content
            {
                Title = "React for Beginners",
                Description = "Build modern user interfaces with React and Next.js.",
                Category = "Course",
                Price = 39.99m,
                ThumbnailUrl = "https://placehold.co/400x200/20B2AA/white?text=React",
                IsPublished = true,
                CreatedDate = DateTime.Now.AddDays(-20),
                Duration = 280,
                ContentUrl = "https://www.youtube.com/watch?v=example2"
            },
            new Content
            {
                Title = "SQL Database Design",
                Description = "Master database design and SQL queries.",
                Category = "Course",
                Price = 29.99m,
                ThumbnailUrl = "https://placehold.co/400x200/DAA520/white?text=SQL",
                IsPublished = true,
                CreatedDate = DateTime.Now.AddDays(-15),
                Duration = 240,
                ContentUrl = "https://www.youtube.com/watch?v=example3"
            }
        };

        await context.Contents.AddRangeAsync(sampleContent);
        await context.SaveChangesAsync();
        Console.WriteLine("Sample content added successfully!");
    }
}

// ==================== SEED SAMPLE PROMOTIONS ====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SQLiteDBContext>();

    if (!await context.Promotions.AnyAsync())
    {
        var samplePromotions = new List<Promotion>
        {
            new Promotion
            {
                Title = "SUMMER30",  // Use this code for 30% off
                Description = "Get 30% off on all programming courses!",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                DiscountPercentage = 30,
                IsActive = true,
                ImageUrl = "https://placehold.co/400x200/FFD700/343a40?text=Summer+Sale",
                CreatedAt = DateTime.Now
            },
            new Promotion
            {
                Title = "NEWYEAR50",  // Use this code for 50% off
                Description = "Buy one course, get one free!",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14),
                DiscountPercentage = 50,
                IsActive = true,
                ImageUrl = "https://placehold.co/400x200/FFD700/343a40?text=New+Year",
                CreatedAt = DateTime.Now
            },
            new Promotion
            {
                Title = "FLASH40",  // Use this code for 40% off
                Description = "Limited time offer! 40% off on all video courses.",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(3),
                DiscountPercentage = 40,
                IsActive = true,
                ImageUrl = "https://placehold.co/400x200/FFD700/343a40?text=Flash+Sale",
                CreatedAt = DateTime.Now
            }
        };

        await context.Promotions.AddRangeAsync(samplePromotions);
        await context.SaveChangesAsync();
        Console.WriteLine("Sample promotions added successfully!");
    }
}

app.Run();