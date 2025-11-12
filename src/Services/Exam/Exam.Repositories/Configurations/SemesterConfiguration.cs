using Exam.Domain.Entities;
using Exam.Repositories.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exam.Repositories.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("Semester");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired();

        builder.HasMany(s => s.Subjects)
            .WithOne(s => s.Semester)
            .HasForeignKey(s => s.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Exams)
            .WithOne(e => e.Semester)
            .HasForeignKey(e => e.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
