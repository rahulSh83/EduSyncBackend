// ﻿using EduSync.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using webapi.DTOs;
// using BCrypt.Net;

// namespace EduSync.Controllers
// {
//     // [Route("api/[controller]")]
//     [Route("api/auth")]
//     [ApiController]
//     public class AuthController : ControllerBase
//     {
//         private readonly AppDbContext _context;

//         public AuthController(AppDbContext context)
//         {
//             _context = context;
//         }

//         [HttpPost("forgot-password")]
//         public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
//         {
//             var user = await _context.UserModels.FirstOrDefaultAsync(u => u.Email == dto.Email);
//             if (user == null)
//             {
//                 return NotFound("User with this email does not exist.");
//             }

//             // Here we just generate a new password for demo purposes (in real app, use email token)
//             user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
//             await _context.SaveChangesAsync();

//             return Ok(new { message = "Password has been reset successfully." });
//         }
//     }
// }
