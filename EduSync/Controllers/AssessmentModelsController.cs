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
    public class AssessmentModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AssessmentModelsController> _logger;

        public AssessmentModelsController(AppDbContext context, ILogger<AssessmentModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/AssessmentModels
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<AssessmentModel>>> GetAssessmentModels()
        //{
        //    return await _context.AssessmentModels
        //        .Include(a => a.Course)
        //        .ToListAsync();
        //}

        // GET: api/AssessmentModels?instructorId=some-guid
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentModel>>> GetAssessmentModels([FromQuery] Guid? instructorId = null)
        {
            _logger.LogInformation("Fetching assessments. InstructorId filter: {InstructorId}", instructorId);

            var query = _context.AssessmentModels
                .Include(a => a.Course)
                .AsQueryable();

            if (instructorId.HasValue)
            {
                query = query.Where(a => a.Course.InstructorId == instructorId.Value);
            }

            var assessments = await query.ToListAsync();
            _logger.LogInformation("Fetched {Count} assessments", assessments.Count);

            return assessments;
        }

        // GET: api/AssessmentModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentModel>> GetAssessmentModel(Guid id)
        {
            _logger.LogInformation("Fetching assessment with ID: {AssessmentId}", id);

            var assessmentModel = await _context.AssessmentModels.FindAsync(id);

            if (assessmentModel == null)
            {
                _logger.LogWarning("Assessment not found with ID: {AssessmentId}", id);
                return NotFound();
            }

            return assessmentModel;
        }

        // GET: api/AssessmentModels/instructor/{instructorId}
        [HttpGet("instructor/{instructorId}")]
        public async Task<ActionResult<IEnumerable<AssessmentModel>>> GetAssessmentsByInstructor(Guid instructorId)
        {
            _logger.LogInformation("Fetching assessments for instructor {InstructorId}", instructorId);

            var assessments = await _context.AssessmentModels
                .Include(a => a.Course)
                .Where(a => a.Course.InstructorId == instructorId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} assessments for instructor {InstructorId}", assessments.Count, instructorId);

            return Ok(assessments);
        }

        // PUT: api/AssessmentModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessmentModel(Guid id, AssessmentModel assessmentModel)
        {
            if (id != assessmentModel.AssessmentId)
            {
                _logger.LogWarning("Assessment ID mismatch on update. Provided: {ProvidedId}, Expected: {ExpectedId}", id, assessmentModel.AssessmentId);
                return BadRequest();
            }

            _context.Entry(assessmentModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated assessment with ID: {AssessmentId}", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentModelExists(id))
                {
                    _logger.LogWarning("Attempted to update non-existent assessment with ID: {AssessmentId}", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency exception while updating assessment with ID: {AssessmentId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AssessmentModels
        [HttpPost]
        public async Task<ActionResult<AssessmentModel>> PostAssessmentModel(AssessmentCreateDto dto)
        {
            _logger.LogInformation("Creating assessment for course {CourseId}", dto.CourseId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for new assessment.");
                return BadRequest(ModelState);
            }

            var course = await _context.CourseModels.FindAsync(dto.CourseId);
            if (course == null)
            {
                _logger.LogWarning("Course not found for ID: {CourseId}", dto.CourseId);
                return BadRequest($"Course with id {dto.CourseId} not found.");
            }

            var assessmentModel = new AssessmentModel
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = dto.CourseId,
                Title = dto.Title,
                Questions = dto.Questions,
                MaxScore = dto.MaxScore
            };

            _context.AssessmentModels.Add(assessmentModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assessment created successfully with ID: {AssessmentId}", assessmentModel.AssessmentId);

            var createdAssessment = await _context.AssessmentModels
                .Include(a => a.Course)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(a => a.AssessmentId == assessmentModel.AssessmentId);

            return CreatedAtAction(nameof(GetAssessmentModel), new { id = assessmentModel.AssessmentId }, assessmentModel);
        }

        // DELETE: api/AssessmentModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessmentModel(Guid id)
        {
            _logger.LogInformation("Attempting to delete assessment with ID: {AssessmentId}", id);

            var assessmentModel = await _context.AssessmentModels
                .Include(a => a.ResultModels)
                .FirstOrDefaultAsync(a => a.AssessmentId == id);

            if (assessmentModel == null)
            {
                _logger.LogWarning("Assessment not found for deletion with ID: {AssessmentId}", id);
                return NotFound();
            }

            var results = await _context.ResultModels
                .Where(r => r.AssessmentId == id)
                .ToListAsync();

            _context.ResultModels.RemoveRange(results);
            _context.AssessmentModels.Remove(assessmentModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted assessment with ID: {AssessmentId} and {ResultCount} associated results", id, results.Count);

            return NoContent();
        }

        private bool AssessmentModelExists(Guid id)
        {
            return _context.AssessmentModels.Any(e => e.AssessmentId == id);
        }
    }
}
