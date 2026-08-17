using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IETCD.Controllers.Admin
{
    [Route("Admin/Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true &&
                User.IsInRole("Admin"))
            {
                return Redirect("/Admin/Dashboard");
            }

            return View("~/Views/Admin/Account/Login.cshtml");
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            string email,
            string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                return View("~/Views/Admin/Account/Login.cshtml");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null ||
                !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                ModelState.AddModelError(
                    "",
                    "Invalid admin credentials.");

                return View("~/Views/Admin/Account/Login.cshtml");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                false,
                false);

            if (result.Succeeded)
            {
                return Redirect("/Admin/Dashboard");
            }

            ModelState.AddModelError(
                "",
                "Invalid admin credentials.");

            return View("~/Views/Admin/Account/Login.cshtml");
        }

        [HttpPost("Logout")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Redirect("/Admin/Account/Login");
        }
    }
}