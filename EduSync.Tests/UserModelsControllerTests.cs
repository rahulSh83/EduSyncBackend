using EduSync.Controllers;
using EduSync.Data;
using EduSync.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using webapi.DTOs;

namespace EduSync.Tests
{
    [TestFixture]
    public class UserModelsControllerTests
    {
        private AppDbContext _context = null!;
        private UserModelsController _controller = null!;
        private Mock<ILogger<UserModelsController>> _loggerMock = null!;

        [SetUp]
        public void Setup()
        {
            // Setup in-memory EF Core DB
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())  // unique DB for each test run
                .Options;

            _context = new AppDbContext(options);

            // Mock logger
            _loggerMock = new Mock<ILogger<UserModelsController>>();

            // Instantiate controller with mock dependencies
            _controller = new UserModelsController(_context, _loggerMock.Object);
        }

        [Test]
        public async Task Register_NewUser_ReturnsOkAndAddsUser()
        {
            // Arrange
            var newUserDto = new UserCreateDto
            {
                Name = "Rahul Sharma",
                Email = "rahulsharma@gmail.com",
                Role = "Student",
                PasswordHash = "Rahul123"
            };

            // Act
            var result = await _controller.Register(newUserDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result;
            var createdUser = (UserModel)okResult.Value!;

            Assert.That(createdUser.Email, Is.EqualTo(newUserDto.Email));
            Assert.That(createdUser.Name, Is.EqualTo(newUserDto.Name));
            Assert.That(createdUser.Role, Is.EqualTo(newUserDto.Role));

            // Password should be hashed, so it should not equal the plain text password
            Assert.That(createdUser.PasswordHash, Is.Not.EqualTo(newUserDto.PasswordHash));

            // The user should be in the database
            var userInDb = await _context.UserModels.FindAsync(createdUser.UserId);
            Assert.That(userInDb, Is.Not.Null);
        }

        [Test]
        public async Task Register_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var existingUser = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = "Ramesh Mehra",
                Email = "rameshmehra@gmail.com",
                Role = "Student",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
            };
            _context.UserModels.Add(existingUser);
            await _context.SaveChangesAsync();

            var newUserDto = new UserCreateDto
            {
                Name = "Ramesh Mehra",
                Email = "rameshmehra@gmail.com", // same email
                Role = "Student",
                PasswordHash = "Ramesh123"
            };

            // Act
            var result = await _controller.Register(newUserDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
            var badRequestResult = (BadRequestObjectResult)result.Result!;
            Assert.That(badRequestResult.Value, Is.EqualTo("A user with this email already exists."));
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOkWithUserData()
        {
            // Arrange
            var password = "Rahul123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = "Rahul Sharma",
                Email = "rahulsharma@gmail.com",
                Role = "Student",
                PasswordHash = hashedPassword
            };
            _context.UserModels.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new UserCreateDto
            {
                Email = user.Email,
                PasswordHash = password  // plain text password for login attempt
            };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var okResult = (OkObjectResult)result.Result;
            Assert.That(okResult.Value, Is.Not.Null);

        }

        [Test]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var password = "Rahul123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = "Rahul Sharma",
                Email = "rahulsharma@gmail.com",
                Role = "Student",
                PasswordHash = hashedPassword
            };
            _context.UserModels.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new UserCreateDto
            {
                Email = user.Email,
                PasswordHash = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = (UnauthorizedObjectResult)result.Result!;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("Invalid email or password."));
        }

        [Test]
        public async Task Login_NonExistingUser_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new UserCreateDto
            {
                Email = "dhruv@gmail.com",
                PasswordHash = "Whatever"
            };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<UnauthorizedObjectResult>());
            var unauthorizedResult = (UnauthorizedObjectResult)result.Result!;
            Assert.That(unauthorizedResult.Value, Is.EqualTo("Invalid email or password."));
        }
        [TearDown]
        public void Teardown()
        {
            _context.Dispose();
        }

    }
}
