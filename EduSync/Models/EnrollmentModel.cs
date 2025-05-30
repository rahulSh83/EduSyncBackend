using System;
using System.Collections.Generic;

namespace EduSync.Models;

public partial class EnrollmentModel
{
    public Guid EnrollmentId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? CourseId { get; set; }

    public DateTime? EnrolledOn { get; set; }

    public virtual CourseModel? Course { get; set; }

    public virtual UserModel? User { get; set; }
}
