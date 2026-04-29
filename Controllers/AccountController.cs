using System.Security.Claims;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseHelper _db;
        private readonly PasswordService _passwords;

        public AccountController(DatabaseHelper db, PasswordService passwords)
        {
            _db = db;
            _passwords = passwords;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                return View(model);
            } 

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            // Check if email is already registered
            using (var check = new SqlCommand(
                "SELECT COUNT(*) FROM Members WHERE Email = @Email", conn))
            {
                check.Parameters.AddWithValue("@Email", model.Email);
                var count = (int)(await check.ExecuteScalarAsync() ?? 0);
                if (count > 0)
                {
                    ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
                    return View(model);
                }
            }

            var hash = _passwords.Hash(model.Password);

            using var insert = new SqlCommand(@"
            INSERT INTO Members (Email, FirstName, LastName, PasswordHash)
            VALUES (@Email, @First, @Last, @Hash)", conn);
            insert.Parameters.AddWithValue("@Email", model.Email);
            insert.Parameters.AddWithValue("@First", model.FirstName);
            insert.Parameters.AddWithValue("@Last", model.LastName);
            insert.Parameters.AddWithValue("@Hash", hash);
            await insert.ExecuteNonQueryAsync();

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) 
            {
                return View(model);
            }
            
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            using var cmd = new SqlCommand(@"
            SELECT MemberID, FirstName, PasswordHash
            FROM Members WHERE Email = @Email", conn);
            cmd.Parameters.AddWithValue("@Email", model.Email);

            int memberId;
            string firstName;
            string? hash;

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync() || reader.IsDBNull(2))
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(model);
                }

                memberId = reader.GetInt32(0);
                firstName = reader.GetString(1);
                hash = reader.GetString(2);
            }

            if (!_passwords.Verify(hash, model.Password))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
            new(ClaimTypes.NameIdentifier, memberId.ToString()),
            new(ClaimTypes.Name, firstName),
            new(ClaimTypes.Email, model.Email)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
