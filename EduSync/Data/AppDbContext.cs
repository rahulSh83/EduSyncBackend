using System;
using System.Collections.Generic;
using EduSync.Models;
using Microsoft.EntityFrameworkCore;

namespace EduSync.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssessmentModel> AssessmentModels { get; set; }

    public virtual DbSet<CourseModel> CourseModels { get; set; }

    public virtual DbSet<EnrollmentModel> EnrollmentModels { get; set; }

    public virtual DbSet<ResultModel> ResultModels { get; set; }

    public virtual DbSet<UserModel> UserModels { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=tcp:rahulserver1.database.windows.net,1433;Initial Catalog=DataModels;Persist Security Info=False;User ID=rahulserver;Password=Server@rahul1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<AssessmentModel>(entity =>
        {
            entity.HasKey(e => e.AssessmentId);

            entity.ToTable("AssessmentModel");

            entity.Property(e => e.AssessmentId).ValueGeneratedNever();
            entity.Property(e => e.Questions).IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Course)
                .WithMany(p => p.AssessmentModels)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AssessmentModel_CourseModel");
        });

        modelBuilder.Entity<CourseModel>(entity =>
        {
            entity.HasKey(e => e.CourseId);

            entity.ToTable("CourseModel");

            entity.Property(e => e.CourseId).ValueGeneratedNever();
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.MediaUrl)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Instructor).WithMany(p => p.CourseModels)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_CourseModel_UserModel1");
        });

        modelBuilder.Entity<EnrollmentModel>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId);

            entity.ToTable("EnrollmentModel");

            entity.Property(e => e.EnrollmentId).ValueGeneratedNever();
            entity.Property(e => e.EnrolledOn).HasColumnType("datetime");

            entity.HasOne(d => d.Course)
                .WithMany(p => p.EnrollmentModels)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_EnrollmentModel_CourseModel1");

            entity.HasOne(d => d.User)
                .WithMany(p => p.EnrollmentModels)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_EnrollmentModel_UserModel1");
        });

        modelBuilder.Entity<ResultModel>(entity =>
        {
            entity.HasKey(e => e.ResultId);

            entity.ToTable("ResultModel");

            entity.Property(e => e.ResultId).ValueGeneratedNever();
            entity.Property(e => e.AttemptDate).HasColumnType("datetime");

            entity.HasOne(d => d.Assessment).WithMany(p => p.ResultModels)
                .HasForeignKey(d => d.AssessmentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ResultModel_AssessmentModel");

            entity.HasOne(d => d.User).WithMany(p => p.ResultModels)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ResultModel_UserModel");
        });

        modelBuilder.Entity<UserModel>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.ToTable("UserModel");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
