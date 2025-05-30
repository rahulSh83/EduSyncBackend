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
    public class EnrollmentModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EnrollmentModelsController> _logger;

        public EnrollmentModelsController(AppDbContext context, ILogger<EnrollmentModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/EnrollmentModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnrollmentModel>>> GetEnrollmentModels()
        {
            _logger.LogInformation("Fetching all enrollment records.");
            return await _context.EnrollmentModels.ToListAsync();
        }

        // GET: api/EnrollmentModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EnrollmentModel>> GetEnrollmentModel(Guid id)
        {
            _logger.LogInformation("Fetching enrollment with ID: {EnrollmentId}", id);
            var enrollmentModel = await _context.EnrollmentModels.FindAsync(id);

            if (enrollmentModel == null)
            {
                _logger.LogWarning("Enrollment not found with ID: {EnrollmentId}", id);
                return NotFound();
            }

            return enrollmentModel;
        }

        // GET: api/EnrollmentModels/IsEnrolled?userId={userId}&courseId={courseId}
        [HttpGet("IsEnrolled")]
        public async Task<ActionResult<bool>> IsEnrolled(Guid userId, Guid courseId)
        {
            _logger.LogInformation("Checking if user {UserId} is enrolled in course {CourseId}", userId, courseId);
            var isEnrolled = await _context.EnrollmentModels
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);
            return Ok(isEnrolled);
        }

        // GET: api/EnrollmentModels/student/{userId}
        [HttpGet("student/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEnrolledCourses(Guid userId)
        {
            _logger.LogInformation("Fetching courses enrolled by user {UserId}", userId);

            var enrolledCourses = await _context.EnrollmentModels
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .Select(e => new
                {
                    e.Course.CourseId,
                    e.Course.Title
                })
                .ToListAsync();

            return Ok(enrolledCourses);
        }

        // PUT: api/EnrollmentModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollmentModel(Guid id, EnrollmentModel enrollmentModel)
        {
            if (id != enrollmentModel.EnrollmentId)
            {
                _logger.LogWarning("Enrollment ID mismatch during update. Provided: {ProvidedId}, Expected: {ExpectedId}", id, enrollmentModel.EnrollmentId);
                return BadRequest();
            }

            _context.Entry(enrollmentModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Enrollment updated successfully with ID: {EnrollmentId}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentModelExists(id))
                {
                    _logger.LogWarning("Attempted to update non-existent enrollment with ID: {EnrollmentId}", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error while updating enrollment with ID: {EnrollmentId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/EnrollmentModels
        [HttpPost]
        public async Task<ActionResult<EnrollmentModel>> PostEnrollmentModel([FromBody] EnrollmentCreateDto dto)
        {
            _logger.LogInformation("Creating enrollment for user {UserId} in course {CourseId}", dto.UserId, dto.CourseId);

            var enrollment = new EnrollmentModel
            {
                EnrollmentId = Guid.NewGuid(),
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                EnrolledOn = DateTime.Now
            };

            _context.EnrollmentModels.Add(enrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Enrollment created successfully with ID: {EnrollmentId}", enrollment.EnrollmentId);

            return CreatedAtAction(nameof(GetEnrollmentModel), new { id = enrollment.EnrollmentId }, enrollment);
        }

        // DELETE: api/EnrollmentModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollmentModel(Guid id)
        {
            _logger.LogInformation("Attempting to delete enrollment with ID: {EnrollmentId}", id);

            var enrollmentModel = await _context.EnrollmentModels.FindAsync(id);
            if (enrollmentModel == null)
            {
                _logger.LogWarning("Enrollment not found for deletion with ID: {EnrollmentId}", id);
                return NotFound();
            }

            _context.EnrollmentModels.Remove(enrollmentModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Enrollment deleted successfully with ID: {EnrollmentId}", id);

            return NoContent();
        }

        private bool EnrollmentModelExists(Guid id)
        {
            return _context.EnrollmentModels.Any(e => e.EnrollmentId == id);
        }
    }
}
