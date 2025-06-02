using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EduSync.Controllers;
using EduSync.Data;
using EduSync.Models;
using webapi.DTOs;
using System.Linq;

namespace EduSync.Tests
{
    public class CourseModelsControllerTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public CourseModelsControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private CourseModelsController CreateController(AppDbContext context)
        {
            var loggerMock = new Mock<ILogger<CourseModelsController>>();
            return new CourseModelsController(context, loggerMock.Object);
        }

        [Fact]
        public async Task GetCourseModels_ReturnsAllCourses()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var instructor = new UserModel { UserId = Guid.NewGuid(), Name = "Rahul Sharma" };
                context.UserModels.Add(instructor);
                context.CourseModels.Add(new CourseModel { CourseId = Guid.NewGuid(), Title = "c++", Description = "c++ course", InstructorId = instructor.UserId });
                await context.SaveChangesAsync();

                var controller = CreateController(context);
                var result = await controller.GetCourseModels();

                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var courses = Assert.IsAssignableFrom<IEnumerable<CourseCreateDto>>(okResult.Value);
                Assert.Single(courses);
            }
        }

        [Fact]
        public async Task GetCourseModel_ValidId_ReturnsCourse()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var instructor = new UserModel { UserId = Guid.NewGuid(), Name = "Rishabh" };
                var courseId = Guid.NewGuid();
                context.UserModels.Add(instructor);
                context.CourseModels.Add(new CourseModel { CourseId = courseId, Title = "Maths", InstructorId = instructor.UserId });
                await context.SaveChangesAsync();

                var controller = CreateController(context);
                var result = await controller.GetCourseModel(courseId);

                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var course = Assert.IsType<CourseCreateDto>(okResult.Value);
                Assert.Equal("Maths", course.Title);
            }
        }

        [Fact]
        public async Task PostCourseModel_CreatesNewCourse()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var instructorId = Guid.NewGuid();
                context.UserModels.Add(new UserModel { UserId = instructorId, Name = "Shyam Sundar" });
                await context.SaveChangesAsync();

                var controller = CreateController(context);
                var dto = new CourseCreateDto
                {
                    Title = "Sanskrit",
                    Description = "Basic sanskrit",
                    InstructorId = instructorId,
                    MediaUrl = "http://sanskrit.com/sanskrit.pdf"
                };

                var result = await controller.PostCourseModel(dto);

                var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
                var course = Assert.IsType<CourseModel>(createdResult.Value);
                Assert.Equal("Sanskrit", course.Title);
            }
        }

        [Fact]
        public async Task DeleteCourseModel_DeletesCourseAndDependencies()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var courseId = Guid.NewGuid();
                var assessmentId = Guid.NewGuid();
                var course = new CourseModel { CourseId = courseId, Title = "Maths" };
                var assessment = new AssessmentModel { AssessmentId = assessmentId, CourseId = courseId };
                var result = new ResultModel { ResultId = Guid.NewGuid(), AssessmentId = assessmentId };
                var enrollment = new EnrollmentModel { EnrollmentId = Guid.NewGuid(), CourseId = courseId };

                context.CourseModels.Add(course);
                context.AssessmentModels.Add(assessment);
                context.ResultModels.Add(result);
                context.EnrollmentModels.Add(enrollment);
                await context.SaveChangesAsync();

                var controller = CreateController(context);
                var response = await controller.DeleteCourseModel(courseId);

                Assert.IsType<NoContentResult>(response);
                Assert.False(context.CourseModels.Any());
                Assert.False(context.AssessmentModels.Any());
                Assert.False(context.ResultModels.Any());
                Assert.False(context.EnrollmentModels.Any());
            }
        }
    }
}
