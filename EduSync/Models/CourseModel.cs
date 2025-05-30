using System;
using System.Collections.Generic;

namespace EduSync.Models;

public partial class CourseModel
{
    public Guid CourseId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Guid? InstructorId { get; set; }

    public string? MediaUrl { get; set; }

    public virtual ICollection<AssessmentModel> AssessmentModels { get; set; } = new List<AssessmentModel>();

    public virtual ICollection<EnrollmentModel> EnrollmentModels { get; set; } = new List<EnrollmentModel>();

    public virtual UserModel? Instructor { get; set; }
}
