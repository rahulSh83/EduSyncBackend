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
using EduSyncAPI.Services;

namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultModelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EventHubService _eventHubService;
        private readonly ILogger<ResultModelsController> _logger;

        public ResultModelsController(AppDbContext context, ILogger<ResultModelsController> logger, EventHubService eventHubService)
        {
            _context = context;
            _logger = logger;
            _eventHubService = eventHubService;
        }

        // GET: api/ResultModels
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ResultModel>>> GetResultModels()
        //{
        //    return await _context.ResultModels
        //    .Include(r => r.User)
        //    .Include(r => r.Assessment)
        //    .ThenInclude(a => a.Course)
        //    .ToListAsync();
        //}

        // GET: api/ResultModels?instructorId=some-guid
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultModel>>> GetResultModels([FromQuery] Guid? instructorId = null)
        {
            _logger.LogInformation("Fetching result models. Instructor ID: {InstructorId}", instructorId);

            var query = _context.ResultModels
                .Include(r => r.User)
                .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                .AsQueryable();

            if (instructorId.HasValue)
            {
                query = query.Where(r => r.Assessment.Course.InstructorId == instructorId.Value);
            }

            return await query.ToListAsync();
        }

        // GET: api/ResultModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultModel>> GetResultModel(Guid id)
        {
            _logger.LogInformation("Fetching result model with ID: {ResultId}", id);

            var resultModel = await _context.ResultModels.FindAsync(id);

            if (resultModel == null)
            {
                _logger.LogWarning("Result model not found for ID: {ResultId}", id);

                return NotFound();
            }

            return resultModel;
        }

        // Instructor-focused endpoint: Get courses by instructor
        [HttpGet("instructor/{instructorId}/courses")]
        public async Task<ActionResult<IEnumerable<object>>> GetInstructorCourses(Guid instructorId)
        {
            _logger.LogInformation("Fetching courses for instructor ID: {InstructorId}", instructorId);

            var courses = await _context.CourseModels
                .Where(c => c.InstructorId == instructorId)
                .Select(c => new {
                    c.CourseId,
                    c.Title
                })
                .ToListAsync();

            return Ok(courses);
        }

        // Instructor-focused endpoint: Get assessments for a course
        [HttpGet("courses/{courseId}/assessments")]
        public async Task<ActionResult<IEnumerable<object>>> GetAssessmentsByCourse(Guid courseId)
        {
            _logger.LogInformation("Fetching assessments for course ID: {CourseId}", courseId);
            var assessments = await _context.AssessmentModels
                .Where(a => a.CourseId == courseId)
                .Select(a => new {
                    a.AssessmentId,
                    a.Title
                })
                .ToListAsync();

            return Ok(assessments);
        }

        // Instructor-focused endpoint: Get results for an assessment
        [HttpGet("assessments/{assessmentId}/results")]
        public async Task<ActionResult<IEnumerable<object>>> GetResultsByAssessment(Guid assessmentId)
        {
            _logger.LogInformation("Fetching results for assessment ID: {AssessmentId}", assessmentId);

            var results = await _context.ResultModels
                .Where(r => r.AssessmentId == assessmentId)
                .Include(r => r.User)
                .Include(r => r.Assessment)
                .Select(r => new {
                    r.ResultId,
                    StudentName = r.User.Name,
                    r.Score,
                    r.AttemptDate,
                    r.Assessment.MaxScore
                })
                .ToListAsync();

            return Ok(results);
        }


        [HttpGet("student/{userId}/results")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentResults(Guid userId)
        {
            _logger.LogInformation("Fetching results for student ID: {UserId}", userId);
            try {
                var results = await _context.ResultModels
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Assessment)
                    .ThenInclude(a => a.Course)
                    .Select(r => new
                    {
                        r.Assessment.AssessmentId,
                        AssessmentTitle = r.Assessment.Title,
                        CourseTitle = r.Assessment.Course != null ? r.Assessment.Course.Title : "Unknown",
                        r.Score,
                        MaxScore = r.Assessment.MaxScore,
                        r.AttemptDate
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch(Exception ex){
                _logger.LogError(ex, "Failed to load student results for user {UserId}", userId);
                return StatusCode(500, "An error occurred while fetching student results.");
            
            }}



        // PUT: api/ResultModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResultModel(Guid id, ResultModel resultModel)
        {
            if (id != resultModel.ResultId)
            {
                _logger.LogWarning("Mismatched result ID in PUT request. Path ID: {PathId}, Body ID: {BodyId}", id, resultModel.ResultId);

                return BadRequest();
            }

            _context.Entry(resultModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated result model with ID: {ResultId}", id);

                // Send update event
                await _eventHubService.SendEventAsync(resultModel, "ResultUpdated");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultModelExists(id))
                {
                    _logger.LogWarning("Concurrency exception: Result model not found for update. ID: {ResultId}", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency exception occurred while updating result model with ID: {ResultId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<ResultModel>> PostResultModel(ResultCreateDto dto)
        {
            var resultModel = new ResultModel
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = dto.AssessmentId,
                UserId = dto.UserId,
                Score = dto.Score,
                AttemptDate = dto.AttemptDate
            };

            _context.ResultModels.Add(resultModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new result model with ID: {ResultId}", resultModel.ResultId);

            // Send event to Event Hub
            try
            {
                await _eventHubService.SendEventAsync(resultModel, "ResultCreated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send ResultCreated event to Event Hub.");
                // Optionally continue without failing the whole request
            }

            return CreatedAtAction(nameof(GetResultModel), new { id = resultModel.ResultId }, resultModel);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResultModel(Guid id)
        {
            _logger.LogInformation("Deleting result model with ID: {ResultId}", id);
            var resultModel = await _context.ResultModels.FindAsync(id);
            if (resultModel == null)
            {
                _logger.LogWarning("Result model not found for deletion. ID: {ResultId}", id);
                return NotFound();
            }

            _context.ResultModels.Remove(resultModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted result model with ID: {ResultId}", id);

            // Send delete event
            await _eventHubService.SendEventAsync(resultModel, "ResultDeleted");

            return NoContent();
        }

        private bool ResultModelExists(Guid id)
        {
            return _context.ResultModels.Any(e => e.ResultId == id);
        }
    }
}