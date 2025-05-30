using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentModel_CourseId",
                table: "AssessmentModel",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseModel_InstructorId",
                table: "CourseModel",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentModel_CourseId",
                table: "EnrollmentModel",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnrollmentModel_UserId",
                table: "EnrollmentModel",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultModel_AssessmentId",
                table: "ResultModel",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ResultModel_UserId",
                table: "ResultModel",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnrollmentModel");

            migrationBuilder.DropTable(
                name: "ResultModel");

            migrationBuilder.DropTable(
                name: "AssessmentModel");

            migrationBuilder.DropTable(
                name: "CourseModel");

            migrationBuilder.DropTable(
                name: "UserModel");
        }
    }
}
