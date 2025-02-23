using Health_Heroes.Data;
using Health_Heroes.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text.Json;

namespace Health_Heroes.Controllers
{
    public class AccountController : Controller
    {
        private readonly HealthHeroDbContext _context;
        private static readonly ConcurrentDictionary<string, (int FailedAttempts, DateTime? LockoutEndTime)> LoginAttempts = new ConcurrentDictionary<string, (int, DateTime?)>();

        public AccountController(HealthHeroDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string message)
        {
            ViewBag.Message = message;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Check if the user is locked out or increment failed attempts
            if (LoginAttempts.TryGetValue(email, out var attemptInfo))
            {
                if (attemptInfo.LockoutEndTime.HasValue && attemptInfo.LockoutEndTime > DateTime.Now)
                {
                    ViewBag.Error = "Your account is locked due to multiple failed login attempts. Please try again after 5 minutes.";
                    return View("Login");
                }
            }

            // Retrieve user from the database
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email_Address == email);

            // Handle successful login
            if (user != null && VerifyPassword(password, user.Password))
            {
                // Clear login attempts on successful login
                LoginAttempts.TryRemove(email, out _);

                // Store user email in session
                HttpContext.Session.SetString("UserEmail", email);

                // Create claims for authentication
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email_Address),
            new Claim(ClaimTypes.NameIdentifier, user.Email_Address)
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                };

                // Sign in the user
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                // Redirect to the home page
                return RedirectToAction("Index", "Home");
            }

            // Handle failed login attempt
            if (!LoginAttempts.ContainsKey(email))
            {
                LoginAttempts[email] = (1, null);
                ViewBag.Error = "Invalid credentials. You have 2 attempt(s) remaining.";
            }
            else
            {
                var currentInfo = LoginAttempts[email];
                int failedAttempts = currentInfo.FailedAttempts + 1;

                if (failedAttempts >= 3)
                {
                    LoginAttempts[email] = (failedAttempts, DateTime.Now.AddMinutes(5));
                    ViewBag.Error = "Your account is locked due to multiple failed login attempts. Please try again after 5 minutes.";
                }
                else
                {
                    LoginAttempts[email] = (failedAttempts, null);
                    ViewBag.Error = $"Invalid credentials. You have {3 - failedAttempts} attempt(s) remaining.";
                }
            }

            // Return login view with error message
            return View("Login");
        }

        private string HandleLoginLockoutAndFailedAttempts(string email)
        {
            // Check if the email already exists in the login attempts dictionary
            if (!LoginAttempts.TryGetValue(email, out var attemptInfo))
            {
                // First failed login attempt: Add the email to the dictionary
                LoginAttempts[email] = (1, null);
                return "Invalid credentials. You have 2 attempt(s) remaining.";
            }

            // Check if the user is locked out
            if (attemptInfo.LockoutEndTime.HasValue && attemptInfo.LockoutEndTime > DateTime.Now)
            {
                return "Your account is locked due to multiple failed login attempts. Please try again after 5 minutes.";
            }

            // Increment failed attempts
            int failedAttempts = attemptInfo.FailedAttempts + 1;

            if (failedAttempts >= 3)
            {
                // Lock the user for 5 minutes
                LoginAttempts[email] = (failedAttempts, DateTime.Now.AddMinutes(5));
                return "Your account is locked due to multiple failed login attempts. Please try again after 5 minutes.";
            }

            // Update failed attempts without lockout
            LoginAttempts[email] = (failedAttempts, null);
            return $"Invalid credentials. You have {3 - failedAttempts} attempt(s) remaining.";
        }





        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(User newUser, string SSN)
        {
            // Server-side validation for Email and SSN
            if (_context.User.Any(u => u.Email_Address == newUser.Email_Address))
            {
                ModelState.AddModelError("Email_Address", "Email already exists!");
                return View(newUser);
            }

            if (_context.Donor_Details.Any(d => d.SSN == SSN))
            {
                ModelState.AddModelError("SSN", "SSN already exists!");
                return View(newUser);
            }

            if (ModelState.IsValid)
            {
                // Hash the password
                newUser.Password = HashPassword(newUser.Password);

                // Step 1: Save the new user to the User table
                _context.User.Add(newUser);
                await _context.SaveChangesAsync();

                // Step 2: Save the corresponding donor to the Donor_Details table
                Donor_Details newDonor = new Donor_Details
                {
                    SSN = SSN,
                    Name = newUser.Name,
                    Email_Address = newUser.Email_Address
                };

                _context.Donor_Details.Add(newDonor);
                await _context.SaveChangesAsync();

                // Redirect to login page after successful registration
                return RedirectToAction("Login", "Account");
            }

            return View(newUser);
        }
       

        [HttpPost]
        public JsonResult CheckSSNExists([FromBody] JsonElement data)
        {
            // Extract the "ssn" value from the JSON object
            if (data.TryGetProperty("ssn", out JsonElement ssnElement))
            {
                string ssn = ssnElement.GetString();
                bool exists = _context.Donor_Details.Any(d => d.SSN == ssn);
                return Json(exists);
            }

            // If "ssn" property is not found, return false
            return Json(false);
        }

        [HttpPost]
        public JsonResult CheckEmailExists([FromBody] JsonElement data)
        {
            // Extract the "email" value from the JSON object
            if (data.TryGetProperty("email", out JsonElement emailElement))
            {
                string email = emailElement.GetString();
                bool exists = _context.User.Any(u => u.Email_Address == email);
                return Json(exists);
            }

            // If "email" property is not found, return false
            return Json(false);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Clear the session
            HttpContext.Session.Clear();

            // Sign out the user to remove authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the login page
            return RedirectToAction("Login", "Account");
        }

        // Utility: Hash password using Argon2
        private string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = GenerateSalt();

            // Hash the password with the salt
            using (var argon2 = new Argon2i(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536; // 64 MB
                argon2.Iterations = 4;

                // Generate the hash
                byte[] hash = argon2.GetBytes(32); // 32-byte hash

                // Combine the salt and hash
                byte[] saltAndHash = new byte[salt.Length + hash.Length];
                Array.Copy(salt, 0, saltAndHash, 0, salt.Length);
                Array.Copy(hash, 0, saltAndHash, salt.Length, hash.Length);

                // Encode the salt+hash as a single Base64 string
                return Convert.ToBase64String(saltAndHash);
            }
        }

        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // Define a 16-byte salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Decode the stored hash into bytes
            byte[] saltAndHashBytes = Convert.FromBase64String(storedHash);

            // Extract the salt (first 16 bytes) and hash
            byte[] salt = new byte[16];
            byte[] storedHashBytes = new byte[saltAndHashBytes.Length - 16];

            Array.Copy(saltAndHashBytes, 0, salt, 0, 16); // Extract salt
            Array.Copy(saltAndHashBytes, 16, storedHashBytes, 0, storedHashBytes.Length); // Extract hash

            // Hash the entered password with the extracted salt
            using (var argon2 = new Argon2i(Encoding.UTF8.GetBytes(enteredPassword)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = 8;
                argon2.MemorySize = 65536;
                argon2.Iterations = 4;

                // Generate hash for the entered password
                byte[] enteredPasswordHash = argon2.GetBytes(storedHashBytes.Length);

                // Compare the generated hash with the stored hash
                return enteredPasswordHash.SequenceEqual(storedHashBytes);
            }
        }



    }
}
