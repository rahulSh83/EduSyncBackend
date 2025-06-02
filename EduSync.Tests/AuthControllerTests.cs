//using System;
//using System.Threading.Tasks;
//using Xunit;
//using Moq;
//using Microsoft.EntityFrameworkCore;
//using EduSync.Controllers;
//using EduSync.Data;
//using EduSync.Models;
//using webapi.DTOs;
//using Microsoft.AspNetCore.Mvc;

//namespace EduSync.Tests
//{
//    public class AuthControllerTests
//    {
//        private AppDbContext GetDbContext()
//        {
//            var options = new DbContextOptionsBuilder<AppDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString())
//                .Options;

//            var context = new AppDbContext(options);

//            // Seed user
//            context.UserModels.Add(new UserModel
//            {
//                UserId = Guid.NewGuid(),
//                Email = "rahulsharma@gmail.com",
//                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Rahul123")
//            });

//            context.SaveChanges();
//            return context;
//        }

//        [Fact]
//        public async Task ForgotPassword_ValidEmail_ChangesPassword()
//        {
//            // Arrange
//            var context = GetDbContext();
//            var controller = new AuthController(context);

//            var dto = new ForgotPasswordDto
//            {
//                Email = "rahulsharma@gmail.com",
//                NewPassword = "Rahul1234"
//            };

//            // Act
//            var result = await controller.ForgotPassword(dto);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            Assert.Contains("Password is changed successfully.", okResult.Value.ToString());
//        }

//        [Fact]
//        public async Task ForgotPassword_NonExistentEmail_ReturnsNotFound()
//        {
//            // Arrange
//            var context = GetDbContext();
//            var controller = new AuthController(context);

//            var dto = new ForgotPasswordDto
//            {
//                Email = "rameshyogi@gmail.com",
//                NewPassword = "Ramesh123"
//            };

//            // Act
//            var result = await controller.ForgotPassword(dto);

//            // Assert
//            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//            Assert.Equal("User with this email does not exist.", notFoundResult.Value);
//        }

//        [Fact]
//        public async Task ForgotPassword_EmptyEmail_ReturnsNotFound()
//        {
//            // Arrange
//            var context = GetDbContext();
//            var controller = new AuthController(context);

//            var dto = new ForgotPasswordDto
//            {
//                Email = "",
//                NewPassword = "abc123"
//            };

//            // Act
//            var result = await controller.ForgotPassword(dto);

//            // Assert
//            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
//            Assert.Equal("User with this email does not exist.", notFoundResult.Value);
//        }

//        [Fact]
//        public async Task ForgotPassword_EmptyPassword_ShouldHashAnyway()
//        {
//            // Arrange
//            var context = GetDbContext();
//            var controller = new AuthController(context);

//            var dto = new ForgotPasswordDto
//            {
//                Email = "rahulsharma@gmail.com",
//                NewPassword = ""
//            };

//            // Act
//            var result = await controller.ForgotPassword(dto);

//            // Assert
//            var okResult = Assert.IsType<OkObjectResult>(result);
//            Assert.Contains("Password is changed successfully.", okResult.Value.ToString());

//            var user = await context.UserModels.FirstOrDefaultAsync(u => u.Email == dto.Email);
//            Assert.True(BCrypt.Net.BCrypt.Verify("", user.PasswordHash)); // should match hashed empty string
//        }

//        [Fact]
//        public async Task ForgotPassword_SameAsOldPassword_StillHashesNew()
//        {
//            // Arrange
//            var context = GetDbContext();
//            var controller = new AuthController(context);

//            var dto = new ForgotPasswordDto
//            {
//                Email = "rahulsharma@gmail.com",
//                NewPassword = "Rahul123"
//            };

//            var oldHash = await context.UserModels
//                .Where(u => u.Email == dto.Email)
//                .Select(u => u.PasswordHash)
//                .FirstOrDefaultAsync();

//            // Act
//            var result = await controller.ForgotPassword(dto);

//            var user = await context.UserModels.FirstOrDefaultAsync(u => u.Email == dto.Email);
//            var newHash = user.PasswordHash;

//            // Assert
//            Assert.NotEqual(oldHash, newHash); // Even same plaintext should produce new hash
//        }
//    }
//}
