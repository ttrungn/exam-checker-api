using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Exam.Repositories.Extensions;
namespace Exam.Repositories.Configurations;

public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessment");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.StudentCode)
            .HasMaxLength(55)
            .IsRequired();

        builder.Property(a => a.SubmissionName)
            .HasMaxLength(105)
            .IsRequired();
        
        builder.Property(a => a.Score)
            .HasPrecision(5, 2); // decimal(5,2)

        builder.Property(a => a.ScoreDetails)
            .HasColumnType("text"); // giống ERD

        builder.Property(a => a.Comment)
            .HasMaxLength(1000);

        builder.Property(a => a.GradedAt);

        // status enum -> lưu string (nếu muốn lưu int thì xoá HasConversion)
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Quan hệ với Submission
        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Assessments)  // nhớ thêm ICollection<Assessment> trong Submission
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        // created_at, updated_at, deleted_at, is_active...
        builder.ConfigureAuditableEntity();
    }
}
