using System.Reflection;
using Exam.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Exam.Repositories.Repositories.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    public DbSet<Submission> Submissions { get; set; }

    public DbSet<Violation> Violations { get; set; }
    
    public DbSet<Assessment> Assessments { get; set; }  
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Domain.Entities.Exam> Exams => Set<Domain.Entities.Exam>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ExamSubject> ExamSubjects => Set<ExamSubject>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
