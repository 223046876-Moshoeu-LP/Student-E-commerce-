using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Website.Models;

namespace Website.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            Input = new InputModel();
            ExternalLogins = new List<AuthenticationScheme>();
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? ReturnUrl { get; set; }

        public string? Role { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            public string? AdminSecretCode { get; set; }
        }

        public async Task OnGetAsync(string? role = "User", string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            Role = role;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var selectedRole = Request.Form["Role"].ToString();
            if (string.IsNullOrEmpty(selectedRole))
            {
                selectedRole = "User";
            }

            if (selectedRole == "Admin")
            {
                var adminSecretCode = Request.Form["AdminSecretCode"].ToString();
                var expectedSecretCode = "Admin123!";

                if (string.IsNullOrEmpty(adminSecretCode))
                {
                    ModelState.AddModelError(string.Empty, "Admin secret code is required.");
                    return Page();
                }

                if (adminSecretCode != expectedSecretCode)
                {
                    ModelState.AddModelError(string.Empty, "Invalid admin secret code.");
                    return Page();
                }
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(selectedRole))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(selectedRole));
                    }

                    await _userManager.AddToRoleAsync(user, selectedRole);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (selectedRole == "User")
                    {
                        return RedirectToAction("Dashboard", "Learner");
                    }
                    else if (selectedRole == "Content_Creator")
                    {
                        return RedirectToAction("Dashboard", "ContentCreator");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return Page();
        }
    }
}