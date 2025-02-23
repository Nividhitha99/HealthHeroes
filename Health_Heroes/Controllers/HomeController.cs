using Health_Heroes.Data;
using Health_Heroes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Encodings.Web;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Health_Heroes.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HealthHeroDbContext _context;

        public HomeController(HealthHeroDbContext context)
        {
            _context = context;
        }

        // Hardcoded list of cities
        private static readonly List<string> Cities = new List<string>
        {
            "Baltimore", "Annapolis", "Frederick", "Rockville", "Gaithersburg",
            "Bowie", "Hagerstown", "Salisbury", "Laurel", "College Park",
            "Greenbelt", "Hyattsville", "Cumberland", "Takoma Park",
            "Westminster", "Easton", "Elkton", "Bel Air", "Glen Burnie", "Bethesda"
        };

        public IActionResult Index()
        {
            // Pass the list of cities to the view
            return View(Cities);
        }

        // Action to search donors based on the city
        [HttpGet]
        public IActionResult SearchDonors(string city)
        {
            // Validate city input
            if (string.IsNullOrWhiteSpace(city) || !Cities.Contains(city))
            {
                ViewBag.Error = "Invalid city provided.";
                return View("Index", Cities);
            }

            // Query the Donor_Details table to get all donors in the selected city
            var donors = _context.Donor_Details
                                 .Where(d => d.City == city)
                                 .ToList();

            // Pass the list of donors to the SearchDonor view
            return View("SearchDonor", donors);
        }

        // GET: /Home/AddDonor
        [HttpGet]
        public async Task<IActionResult> AddDonor()
        {
            // Retrieve the email from session
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                // Redirect to login if the session is expired or user is not logged in
                return RedirectToAction("Login", "Account");
            }

            // Fetch the donor details based on the email address
            var donor = await _context.Donor_Details.FirstOrDefaultAsync(d => d.Email_Address == email);

            // If the donor exists and has a phone number, show the alert message
            if (donor != null && !string.IsNullOrEmpty(donor.PhoneNumber))
            {
                // Set the TempData message
                return Content("<script>alert('You have already been added as a donor. Sign Up as a New User to enable Add Donor'); window.location.href = '/Home/Index';</script>", "text/html");
            }

            // If the donor is not found, create a new object for binding in the view
            if (donor == null)
            {
                donor = new Donor_Details
                {
                    Email_Address = email // Pre-fill the email field
                };
            }

            // Return the AddDonor view for adding a new donor
            return View(donor);
        }

        // POST: /Home/AddDonor
        [HttpPost]
        public async Task<IActionResult> AddDonor([FromBody] Donor_Details donorDetails)
        {
            // Retrieve the email from session
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                // Return an error if the session expired or user is not logged in
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }

            if (ModelState.IsValid)
            {
                // Set the email address to the session-stored email
                donorDetails.Email_Address = email;

                // Check if the donor with the given email already exists
                var existingDonor = _context.Donor_Details.FirstOrDefault(d => d.Email_Address == email);

                if (existingDonor != null)
                {
                    // If the donor already exists, update their details (except SSN and Name)
                    existingDonor.Age = donorDetails.Age;
                    existingDonor.Blood_Type = donorDetails.Blood_Type;
                    existingDonor.PhoneNumber = donorDetails.PhoneNumber;
                    existingDonor.City = donorDetails.City;
                    existingDonor.Illness = donorDetails.Illness;
                    existingDonor.Height = donorDetails.Height;
                    existingDonor.Weight = donorDetails.Weight;
                    existingDonor.Gender = donorDetails.Gender;

                    // Save the updated donor details to the database
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Add a new donor entry if it doesn't already exist
                    _context.Donor_Details.Add(donorDetails);
                    await _context.SaveChangesAsync();
                }

                // Return success response
                return Json(new { success = true });
            }
            else
            {
                // Handle validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Invalid input: " + string.Join(", ", errors) });
            }
        }

        // GET: /Home/UpdateDonor
        [HttpGet]
        public async Task<IActionResult> UpdateDonor()
        {
            // Retrieve the logged-in user's email from the session
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                // If the user is not logged in, redirect them to the login page
                return RedirectToAction("Login", "Account");
            }

            // Fetch the donor details from the database using the email
            var donorDetails = await _context.Donor_Details
                .FirstOrDefaultAsync(d => d.Email_Address == email);

            if (donorDetails == null)
            {
                // If no donor details are found, return an error or redirect
                return NotFound();
            }

            // Check if the PhoneNumber is null, and if so, pass a flag to display a popup
            if (string.IsNullOrEmpty(donorDetails.PhoneNumber))
            {
                // Pass a flag to the view using ViewBag
                ViewBag.ShowPopup = true;
                ViewBag.PopupMessage = "Add yourself as a Donor First";
            }
            else
            {
                ViewBag.ShowPopup = false;
            }

            // Pass the donor details to the view
            return View(donorDetails);
        }

        // POST: /Home/UpdateDonor
        [HttpPost]
        public async Task<IActionResult> UpdateDonor([FromBody] Donor_Details updatedDonor)
        {
            // Retrieve the email from the session (Email is not updated)
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }

            // Fetch the existing donor details from the database using the email
            var existingDonor = await _context.Donor_Details
                .FirstOrDefaultAsync(d => d.Email_Address == email);

            if (existingDonor == null)
            {
                return Json(new { success = false, message = "Donor not found." });
            }

            // Update the donor details
            existingDonor.SSN = updatedDonor.SSN;
            existingDonor.Name = updatedDonor.Name;
            existingDonor.Age = updatedDonor.Age;
            existingDonor.Blood_Type = updatedDonor.Blood_Type;
            existingDonor.PhoneNumber = updatedDonor.PhoneNumber;
            existingDonor.City = updatedDonor.City;
            existingDonor.Illness = updatedDonor.Illness;
            existingDonor.Height = updatedDonor.Height;
            existingDonor.Weight = updatedDonor.Weight;
            existingDonor.Gender = updatedDonor.Gender;

            // Update the User table if necessary (e.g., if the Name field is also in the User table)
            var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email_Address == email);
            if (existingUser != null)
            {
                existingUser.Name = updatedDonor.Name; // Assuming Name is also stored in the User table
            }

            // Save the changes
            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Donor details updated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating donor: {ex.Message}" });
            }
        }

        // Use HTML encoding for user input displayed to prevent XSS
        public IActionResult DisplayDonorDetails(string donorInput)
        {
            string encodedInput = HtmlEncoder.Default.Encode(donorInput);
            ViewBag.EncodedInput = encodedInput;
            return View();
        }
    }
}
