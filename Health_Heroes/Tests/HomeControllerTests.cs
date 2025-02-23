using Health_Heroes.Controllers;
using Health_Heroes.Data;
using Health_Heroes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Health_Heroes.Tests
{
    public class HomeControllerTests
    {
        private readonly HealthHeroDbContext _context;
        private readonly HomeController _controller;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public HomeControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<HealthHeroDbContext>()
                          .UseInMemoryDatabase(databaseName: "HealthHeroTestDb")
                          .Options;

            _context = new HealthHeroDbContext(options);

            // Ensure the database is empty before seeding
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Seed the in-memory database with test data
            SeedDatabase();

            // Mock HttpContextAccessor for session handling
            _mockSession = new Mock<ISession>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext
            {
                Session = _mockSession.Object // Set mocked session
            };
            _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

            _controller = new HomeController(_context)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };
        }

        // Method to seed the in-memory database
        private void SeedDatabase()
        {
            if (!_context.User.Any())
            {
                var testUsers = new[]
                {
                    new User { Email_Address = "testuser1@example.com", Password = "Password123", Name = "Test User 1" },
                    new User { Email_Address = "testuser2@example.com", Password = "Password123", Name = "Test User 2" }
                };

                _context.User.AddRange(testUsers);
            }

            if (!_context.Donor_Details.Any())
            {
                var testDonors = new[]
                {
                    new Donor_Details { SSN = "123-45-6789", Name = "Test Donor 1", Email_Address = "testuser1@example.com", PhoneNumber = "123-456-7890", City = "Baltimore" },
                    new Donor_Details { SSN = "987-65-4321", Name = "Test Donor 2", Email_Address = "testuser2@example.com", PhoneNumber = null, City = "Annapolis" }
                };

                _context.Donor_Details.AddRange(testDonors);
            }

            _context.SaveChanges();  // Save the changes to the in-memory database
        }

        // Clean up after tests
        public void Dispose()
        {
            _context.Database.EnsureDeleted();  // Clean up the in-memory database
        }

        [Fact]
        public void AddDonor_ValidSession_ShouldReturnViewWithDonorDetails()
        {
            // Arrange
            byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes("testuser1@example.com");
            _mockSession.Setup(s => s.TryGetValue("UserEmail", out emailBytes)).Returns(true);

            // Act
            var result = _controller.AddDonor();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Donor_Details>(viewResult.Model);
            Assert.Equal("testuser1@example.com", model.Email_Address);
        }

        [Fact]
        public void AddDonor_InvalidSession_ShouldRedirectToLogin()
        {
            // Arrange
            byte[] emailBytes = null;
            _mockSession.Setup(s => s.TryGetValue("UserEmail", out emailBytes)).Returns(false);

            // Act
            var result = _controller.AddDonor();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

       

        [Fact]
        public async Task UpdateDonor_InvalidSession_ShouldRedirectToLogin()
        {
            // Arrange
            byte[] emailBytes = null;
            _mockSession.Setup(s => s.TryGetValue("UserEmail", out emailBytes)).Returns(false);

            // Act
            var result = await _controller.UpdateDonor();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }

        [Fact]
        public async Task AddDonor_ValidInput_ShouldAddOrUpdateDonor()
        {
            // Arrange
            byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes("testuser3@example.com");
            _mockSession.Setup(s => s.TryGetValue("UserEmail", out emailBytes)).Returns(true);

            var newDonor = new Donor_Details
            {
                SSN = "222-33-4444",
                Name = "Test Donor 3",
                Age = 25,
                Blood_Type = "O+",
                PhoneNumber = "333-444-5555",
                City = "Frederick",
                Height = 5.9f,
                Weight = 160.0f,
                Gender = 'M'
            };

            // Act
            var result = await _controller.AddDonor(newDonor);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value));

            var addedDonor = _context.Donor_Details.FirstOrDefault(d => d.SSN == "222-33-4444");
            Assert.NotNull(addedDonor);
            Assert.Equal("Frederick", addedDonor.City);
        }

        [Fact]
        public async Task SearchDonors_ValidCity_ShouldReturnDonorsInCity()
        {
            // Act
            var result = _controller.SearchDonors("Baltimore");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Donor_Details>>(viewResult.Model);
            //Assert.Single(model);
            Assert.Equal("Baltimore", model[0].City);
        }

        [Fact]
        public async Task SearchDonors_InvalidCity_ShouldReturnNoDonors()
        {
            // Act
            var result = _controller.SearchDonors("NonExistentCity");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Donor_Details>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task UpdateDonor_ValidInput_ShouldUpdateDonorDetails()
        {
            // Arrange
            byte[] emailBytes = System.Text.Encoding.UTF8.GetBytes("testuser1@example.com");
            _mockSession.Setup(s => s.TryGetValue("UserEmail", out emailBytes)).Returns(true);

            var updatedDonor = new Donor_Details
            {
                SSN = "123-45-6789", // Same SSN as testuser1
                Name = "Updated Donor 1",
                Age = 30,
                Blood_Type = "A+",
                PhoneNumber = "123-456-7890",
                City = "Baltimore",
                Height = 5.8f,
                Weight = 150.0f,
                Gender = 'M'
            };

            // Act
            var result = await _controller.UpdateDonor(updatedDonor);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value.GetType().GetProperty("success").GetValue(jsonResult.Value));

            var existingDonor = _context.Donor_Details.FirstOrDefault(d => d.SSN == "123-45-6789");
            Assert.NotNull(existingDonor);
            Assert.Equal("Updated Donor 1", existingDonor.Name);
            Assert.Equal(30, existingDonor.Age);
        }
    }
}
