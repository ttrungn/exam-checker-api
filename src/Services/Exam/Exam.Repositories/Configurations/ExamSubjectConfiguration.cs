using Exam.Domain.Entities;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class ExamSubjectConfiguration : IEntityTypeConfiguration<ExamSubject>
{
    public void Configure(EntityTypeBuilder<ExamSubject> builder)
    {
        builder.ToTable("ExamSubject");

        builder.HasKey(x => x.Id);
        
        builder.HasOne(es => es.Exam)
            .WithMany(e => e.ExamSubjects)
            .HasForeignKey(s => s.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Subject)
            .WithMany(s => s.ExamSubjects)
            .HasForeignKey(es => es.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.ConfigureAuditableEntity();
    }
}
