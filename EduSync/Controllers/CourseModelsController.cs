using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using webapi.DTOs;
using Microsoft.Extensions.Logging;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CourseModelsController> _logger;

        public CourseModelsController(AppDbContext context, ILogger<CourseModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/CourseModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCourseModels()
        {
            _logger.LogInformation("Fetching all courses with instructor details.");

            var courses = await _context.CourseModels
                .Include(c => c.Instructor)
                .Select(c => new CourseCreateDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title ?? "",
                    Description = c.Description ?? "",
                    MediaUrl = c.MediaUrl ?? "",
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor != null ? c.Instructor.Name ?? "Unknown" : "Unknown"
                })
                .ToListAsync();
            return Ok(courses);
        }

        // GET: api/CourseModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseModel>> GetCourseModel(Guid id)
        {
            _logger.LogInformation("Fetching course with ID: {CourseId}", id);
            var courseModel = await _context.CourseModels
                .Include(c => c.Instructor)
                .Where(c => c.CourseId == id)
                .Select(c => new CourseCreateDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title ?? "",
                    Description = c.Description ?? "",
                    MediaUrl = c.MediaUrl ?? "",
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor != null ? c.Instructor.Name ?? "Unknown" : "Unknown"
                })
                .FirstOrDefaultAsync();

            if (courseModel == null)
            {
                _logger.LogWarning("Course not found with ID: {CourseId}", id);
                return NotFound();
            }

            return Ok(courseModel);
        }

        // GET: api/CourseModels/instructor/{instructorId}
        [HttpGet("instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<CourseCreateDto>>> GetCoursesByInstructor(Guid instructorId)
        {
            _logger.LogInformation("Fetching courses for instructor ID: {InstructorId}", instructorId);

            var courses = await _context.CourseModels
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.Instructor)
                .Select(c => new CourseCreateDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title ?? "",
                    Description = c.Description ?? "",
                    MediaUrl = c.MediaUrl ?? "",
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor != null ? c.Instructor.Name ?? "Unknown" : "Unknown"
                })
                .ToListAsync();

            return Ok(courses);
        }


        // PUT: api/CourseModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourseModel(Guid id, CourseModel courseModel)
        {
            if (id != courseModel.CourseId)
            {
                _logger.LogWarning("Course ID mismatch during update. Provided: {ProvidedId}, Expected: {ExpectedId}", id, courseModel.CourseId);

                return BadRequest();
            }

            _context.Entry(courseModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Course updated successfully with ID: {CourseId}", id);

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseModelExists(id))
                {
                    _logger.LogWarning("Attempted to update non-existent course with ID: {CourseId}", id);

                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency exception while updating course with ID: {CourseId}", id);

                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CourseModels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CourseModel>> PostCourseModel(CourseCreateDto courseDto)
        {
            var courseModel = new CourseModel
            {
                CourseId = Guid.NewGuid(),
                Title = courseDto.Title,
                Description = courseDto.Description,
                InstructorId = courseDto.InstructorId,
                MediaUrl = courseDto.MediaUrl
            };

            _context.CourseModels.Add(courseModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New course created with ID: {CourseId}", courseModel.CourseId);


            return CreatedAtAction(nameof(GetCourseModel), new { id = courseModel.CourseId }, courseModel);
        }

        // DELETE: api/CourseModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourseModel(Guid id)
        {
            _logger.LogInformation("Deleting course with ID: {CourseId}", id);

            var courseModel = await _context.CourseModels.FindAsync(id);
            if (courseModel == null)
            {
                _logger.LogWarning("Course not found for deletion with ID: {CourseId}", id);

                return NotFound();
            }

            // Step 1: Find all assessments for the course
            var assessments = await _context.AssessmentModels
                .Where(a => a.CourseId == id)
                .ToListAsync();

            // Step 2: Delete all related results for those assessments
            var assessmentIds = assessments.Select(a => a.AssessmentId).ToList();
            var results = await _context.ResultModels
                .Where(r => assessmentIds.Contains((Guid)r.AssessmentId))
                .ToListAsync();
            _context.ResultModels.RemoveRange(results);

            _logger.LogInformation("Deleted {Count} result(s) related to course ID: {CourseId}", results.Count, id);

            // Step 3: Delete the assessments themselves
            _context.AssessmentModels.RemoveRange(assessments);

            _logger.LogInformation("Deleted {Count} assessment(s) related to course ID: {CourseId}", assessments.Count, id);


            // Step 4: Delete course enrollments
            var enrollments = await _context.EnrollmentModels
                .Where(e => e.CourseId == id)
                .ToListAsync();
            _context.EnrollmentModels.RemoveRange(enrollments);
            _logger.LogInformation("Deleted {Count} enrollment(s) for course ID: {CourseId}", enrollments.Count, id);


            // Step 5: Delete the course
            _context.CourseModels.Remove(courseModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted course with ID: {CourseId}", id);


            return NoContent();
        }

        private bool CourseModelExists(Guid id)
        {
            return _context.CourseModels.Any(e => e.CourseId == id);
        }
    }
}
