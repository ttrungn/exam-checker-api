using Exam.Domain.Entities;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Domain.Entities.Exam>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Exam> builder)
    {
        builder.ToTable("Exam");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SemesterId)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired();

        builder.Property(x => x.StartDate)
            .IsRequired();

        builder.Property(x => x.EndDate)
            .IsRequired();

        builder.ConfigureAuditableEntity();
    }
}
