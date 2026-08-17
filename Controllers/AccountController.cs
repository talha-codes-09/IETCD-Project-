using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IETCD.Controllers
{
    [Route("Account")]
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
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return Redirect("/Admin/Dashboard");

                return Redirect("/");
            }

            return View("~/Views/Account/Login.cshtml");
        }

        [HttpPost("Login")]
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

                return View("~/Views/Account/Login.cshtml");
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View("~/Views/Account/Login.cshtml");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                false,
                false);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return Redirect("/Admin/Dashboard");

                return Redirect("/");
            }

            ModelState.AddModelError(
                "",
                "Invalid email or password.");

            return View("~/Views/Account/Login.cshtml");
        }

        [HttpGet("Register")]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string email,
            string password,
            string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Email and password are required.");

                return View("~/Views/Account/Register.cshtml");
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "Passwords do not match.");

                return View("~/Views/Account/Register.cshtml");
            }

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "",
                    "An account with this email already exists.");

                return View("~/Views/Account/Register.cshtml");
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result =
                await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    user,
                    "Student");

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                return Redirect("/");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Redirect("/");
        }
    }
}