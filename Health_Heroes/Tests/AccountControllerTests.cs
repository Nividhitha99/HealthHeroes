using Health_Heroes.Controllers;
using Health_Heroes.Data;
using Health_Heroes.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Health_Heroes.Tests
{
    public class AccountControllerTests : IDisposable
    {
        private readonly HealthHeroDbContext _context;
        private readonly AccountController _controller;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<ISession> _mockSession;

        public AccountControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<HealthHeroDbContext>()
                          .UseInMemoryDatabase(databaseName: "HealthHeroTestDb")
                          .Options;

            _context = new HealthHeroDbContext(options);

            // Mock HttpContextAccessor for session handling
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();

            // Configure a mock session
            _mockSession = new Mock<ISession>();
            httpContext.Session = _mockSession.Object;

            _mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

            _controller = new AccountController(_context)
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
            ClearDatabase(); // Clear any existing data before seeding

            var testUsers = new[]
            {
                new User { Email_Address = "testuser1@example.com", Password = "Password123", Name = "Test User 1" },
                new User { Email_Address = "testuser2@example.com", Password = "Password123", Name = "Test User 2" }
            };

            var testDonors = new[]
            {
                new Donor_Details { SSN = "123-45-6789", Name = "Test Donor 1", Email_Address = "testuser1@example.com" },
                new Donor_Details { SSN = "987-65-4321", Name = "Test Donor 2", Email_Address = "testuser2@example.com" }
            };

            _context.User.AddRange(testUsers);
            _context.Donor_Details.AddRange(testDonors);
            _context.SaveChanges();  // Save the changes to the in-memory database
        }

        // Clean up database before each test
        private void ClearDatabase()
        {
            _context.User.RemoveRange(_context.User);
            _context.Donor_Details.RemoveRange(_context.Donor_Details);
            _context.SaveChanges();
        }

        // Clean up after each test
        public void Dispose()
        {
            _context.Database.EnsureDeleted();  // Clean up the in-memory database after the test class
        }

        [Fact]
        public async Task Login_ValidUser_ShouldRedirectToHome()
        {
            SeedDatabase(); // Seed data before test

            // Mock session behavior when setting a value
            _mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

            // Act
            var result = await _controller.Login("testuser1@example.com", "Password123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Home", redirectResult.ControllerName);

            // Verify that the session was set correctly
            _mockSession.Verify(s => s.Set(It.Is<string>(key => key == "UserEmail"), It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ShouldReturnViewWithError()
        {
            SeedDatabase(); // Seed data before test

            // Act
            var result = await _controller.Login("testuser1@example.com", "wrongpassword");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Invalid credentials", _controller.ViewBag.Error);
        }

        [Fact]
        public void CheckEmailExists_ShouldReturnTrueIfEmailExists()
        {
            SeedDatabase(); // Seed data before test

            // Act
            var json = "{\"email\":\"testuser1@example.com\"}";
            var jsonDocument = JsonDocument.Parse(json);
            var result = _controller.CheckEmailExists(jsonDocument.RootElement);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }

        [Fact]
        public void CheckEmailExists_ShouldReturnFalseIfEmailDoesNotExist()
        {
            SeedDatabase(); // Seed data before test

            // Act
            var json = "{\"email\":\"nonexistent@example.com\"}";
            var jsonDocument = JsonDocument.Parse(json);
            var result = _controller.CheckEmailExists(jsonDocument.RootElement);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False((bool)jsonResult.Value);
        }

        [Fact]
        public async Task SignUp_ValidUser_ShouldCreateUserAndDonor()
        {
            ClearDatabase(); // Start with a fresh database

            // Arrange
            var newUser = new User { Email_Address = "newuser@example.com", Name = "New User", Password = "Password123" };
            var newDonorSSN = "111-22-3333";

            // Act
            var result = await _controller.SignUp(newUser, newDonorSSN);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);

            var addedUser = _context.User.FirstOrDefault(u => u.Email_Address == "newuser@example.com");
            var addedDonor = _context.Donor_Details.FirstOrDefault(d => d.SSN == "111-22-3333");

            Assert.NotNull(addedUser);
            Assert.NotNull(addedDonor);
        }

        [Fact]
        public async Task SignUp_DuplicateEmail_ShouldReturnError()
        {
            SeedDatabase(); // Seed data before test

            // Arrange
            var newUser = new User { Email_Address = "testuser1@example.com", Name = "Duplicate User", Password = "Password123" };
            var ssn = "999-88-7777";

            // Act
            var result = await _controller.SignUp(newUser, ssn);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Email already exists!", _controller.ModelState["Email_Address"].Errors.First().ErrorMessage);
        }

        [Fact]
        public async Task SignUp_DuplicateSSN_ShouldReturnError()
        {
            SeedDatabase(); // Seed data before test

            // Arrange
            var newUser = new User { Email_Address = "unique@example.com", Name = "Unique User", Password = "Password123" };
            var ssn = "123-45-6789"; // Duplicate SSN

            // Act
            var result = await _controller.SignUp(newUser, ssn);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("SSN already exists!", _controller.ModelState["SSN"].Errors.First().ErrorMessage);
        }

        [Fact]
        public void Logout_ShouldClearSessionAndRedirectToLogin()
        {
            SeedDatabase(); // Seed data before test

            // Arrange
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Session = new Mock<ISession>().Object;
            _controller.ControllerContext.HttpContext = mockHttpContext;

            // Act
            var result = _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }
    }
}
