using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSync.Migrations
{
    /// <inheritdoc />
    public partial class EnableCascadeDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentModel_CourseModel",
                table: "AssessmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseModel_UserModel1",
                table: "CourseModel");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentModel_CourseModel1",
                table: "EnrollmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentModel_UserModel1",
                table: "EnrollmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultModel_AssessmentModel",
                table: "ResultModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultModel_UserModel",
                table: "ResultModel");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentModel_CourseModel",
                table: "AssessmentModel",
                column: "CourseId",
                principalTable: "CourseModel",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseModel_UserModel1",
                table: "CourseModel",
                column: "InstructorId",
                principalTable: "UserModel",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentModel_CourseModel1",
                table: "EnrollmentModel",
                column: "CourseId",
                principalTable: "CourseModel",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentModel_UserModel1",
                table: "EnrollmentModel",
                column: "UserId",
                principalTable: "UserModel",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultModel_AssessmentModel",
                table: "ResultModel",
                column: "AssessmentId",
                principalTable: "AssessmentModel",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResultModel_UserModel",
                table: "ResultModel",
                column: "UserId",
                principalTable: "UserModel",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentModel_CourseModel",
                table: "AssessmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseModel_UserModel1",
                table: "CourseModel");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentModel_CourseModel1",
                table: "EnrollmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_EnrollmentModel_UserModel1",
                table: "EnrollmentModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultModel_AssessmentModel",
                table: "ResultModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ResultModel_UserModel",
                table: "ResultModel");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentModel_CourseModel",
                table: "AssessmentModel",
                column: "CourseId",
                principalTable: "CourseModel",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseModel_UserModel1",
                table: "CourseModel",
                column: "InstructorId",
                principalTable: "UserModel",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentModel_CourseModel1",
                table: "EnrollmentModel",
                column: "CourseId",
                principalTable: "CourseModel",
                principalColumn: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnrollmentModel_UserModel1",
                table: "EnrollmentModel",
                column: "UserId",
                principalTable: "UserModel",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultModel_AssessmentModel",
                table: "ResultModel",
                column: "AssessmentId",
                principalTable: "AssessmentModel",
                principalColumn: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResultModel_UserModel",
                table: "ResultModel",
                column: "UserId",
                principalTable: "UserModel",
                principalColumn: "UserId");
        }
    }
}
