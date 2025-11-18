using Exam.Domain.Entities;
using Exam.Domain.Enums;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submission");

        builder.HasKey(x => x.Id);

        builder.ConfigureAuditableEntity();
        
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasDefaultValue(SubmissionStatus.Processing);
        
        builder.Property(x => x.GradeStatus)
            .HasConversion<string>()
            .HasDefaultValue(GradeStatus.NotGraded);
        
        builder.HasOne(x => x.ExamSubject)
            .WithMany(es => es.Submissions)
            .HasForeignKey(x => x.ExamSubjectId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
