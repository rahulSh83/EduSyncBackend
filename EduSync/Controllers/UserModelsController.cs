using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSync.Data;
using EduSync.Models;
using Humanizer;
using webapi.DTOs;
using Microsoft.CodeAnalysis.Scripting;


namespace EduSync.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly ILogger<UserModelsController> _logger;
        public UserModelsController(AppDbContext context, ILogger<UserModelsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/UserModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUserModels()
        {
            _logger.LogInformation("Fetching all user models");
            return await _context.UserModels.ToListAsync();
        }

        // GET: api/UserModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserModel>> GetUserModel(Guid id)
        {
            _logger.LogInformation("Fetching user model with ID {UserId}", id);
            var userModel = await _context.UserModels.FindAsync(id);

            if (userModel == null)
            {
                _logger.LogWarning("User model with ID {UserId} not found", id);
                return NotFound();
            }

            return userModel;
        }

        // PUT: api/UserModels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserModel(Guid id, UserModel userModel)
        {
            if (id != userModel.UserId)
            {
                _logger.LogWarning("Bad request: ID in route does not match user model ID");
                return BadRequest();
            }

            _context.Entry(userModel).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User model with ID {UserId} updated successfully", id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserModelExists(id))
                {
                    _logger.LogWarning("User model with ID {UserId} not found during update", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("Concurrency error while updating user model with ID {UserId}", id);
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserModels/register (Register a new user)
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("register")]
        public async Task<ActionResult<UserModel>> Register(UserCreateDto userDto)
        {
            _logger.LogInformation("Registering new user with email {Email}", userDto.Email);

            if (await _context.UserModels.AnyAsync(u => u.Email == userDto.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", userDto.Email);

                return BadRequest("A user with this email already exists.");
            }

            // Hash the password before storing
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.PasswordHash);

            var userModel = new UserModel
            {
                UserId = Guid.NewGuid(),
                Name = userDto.Name,
                Email = userDto.Email,
                Role = userDto.Role,
                PasswordHash = hashedPassword // You can hash this later
            };

            _context.UserModels.Add(userModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully with ID {UserId}", userModel.UserId);

            return Ok(userModel);
        }

        // POST: api/UserModels/login (Login user)
        //[HttpPost("login")]
        //public async Task<ActionResult<UserModel>> Login(UserCreateDto dto)
        //{
        //    var userModel = new UserModel
        //    {
        //        UserId = Guid.NewGuid(),
        //        Name = dto.Name,
        //        Email = dto.Email,
        //        Role = dto.Role,
        //        PasswordHash = dto.PasswordHash
        //    };

        //    _context.UserModels.Add(userModel);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetUserModel), new { id = userModel.UserId }, userModel);
        //}
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(UserCreateDto loginDto)
        {
            _logger.LogInformation("Login attempt for email {Email}", loginDto.Email);
            try
            {
                var user = await _context.UserModels
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.PasswordHash, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for email {Email}", loginDto.Email);
                    return Unauthorized("Invalid email or password.");
                }

                _logger.LogInformation("User {UserId} logged in successfully", user.UserId);

                return Ok(new
                {
                    user.UserId,
                    user.Name,
                    user.Email,
                    user.Role,
                    Token = "fake-jwt-token"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed due to server error for email {Email}", loginDto.Email);

                return StatusCode(500, "Server error during login");
            }
        }

        // DELETE: api/UserModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserModel(Guid id)
        {
            var userModel = await _context.UserModels.FindAsync(id);
            if (userModel == null)
            {
                _logger.LogWarning("User deletion failed: ID {UserId} not found", id);
                return NotFound();
            }

            _context.UserModels.Remove(userModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User model with ID {UserId} deleted", id);
            return NoContent();
        }

        private bool UserModelExists(Guid id)
        {
            return _context.UserModels.Any(e => e.UserId == id);
        }
    }
}
