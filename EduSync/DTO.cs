// DTOs/UserCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace webapi.DTOs
{
    public class UserCreateDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string PasswordHash { get; set; } = null!;
    }
}

// DTOs/AssessmentCreateDto.cs
namespace webapi.DTOs
{
    public class AssessmentCreateDto
    {
        [Required]
        public Guid CourseId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        public string Questions { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max score must be grater than zero")]
        public int MaxScore { get; set; }

    }
}

// DTOs/CourseCreateDto.cs
namespace webapi.DTOs
{
    public class CourseCreateDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public Guid? InstructorId { get; set; }
        public string MediaUrl { get; set; } = null!;
        public string InstructorName { get; set; } = "Unknown";
    }
}

// DTOs/ResultCreateDto.cs
namespace webapi.DTOs
{
    public class ResultCreateDto
    {
        public Guid AssessmentId { get; set; }
        public Guid UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
    }
}

namespace webapi.DTOs
{
    public class EnrollmentCreateDto
    {
        public Guid UserId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EnrolledOn { get; set; } = DateTime.UtcNow;
    }
}

namespace webapi.DTOs
{
    public class ForgotPasswordDto
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

}

namespace webapi.DTOs
{
    public class SasTokenResponse
    {
        public string UploadUrl { get; set; }
        public string BlobUrl { get; set; }
    }

}