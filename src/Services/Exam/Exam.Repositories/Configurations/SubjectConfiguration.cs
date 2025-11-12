using Exam.Domain.Entities;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subject");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SemesterId)
            .IsRequired();

        builder.Property(x => x.Name)
            .IsRequired();

        builder.Property(x => x.Code)
            .IsRequired();

        builder.ConfigureAuditableEntity();
    }
}
