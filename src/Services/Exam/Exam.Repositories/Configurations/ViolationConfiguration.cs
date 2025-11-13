using Exam.Domain.Entities;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class ViolationConfiguration : IEntityTypeConfiguration<Violation>
{
    public void Configure(EntityTypeBuilder<Violation> builder)
    {
        builder.ToTable("Violation");
        builder.HasKey(x => x.Id);
        
        builder.ConfigureAuditableEntity();
        
        builder.HasOne(v => v.Submission)
            .WithMany(s => s.Violations)
            .HasForeignKey(v => v.SubmissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
