using Exam.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Exam.Repositories.Repositories.Contexts;

public static class InitializerExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext coffeeStoreDbContext)
    {
        _logger = logger;
        _context = coffeeStoreDbContext;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        // Seeding semester
        var semesters = new List<Semester>()
        {
            new Semester()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Học kỳ 1",
                CreatedAt = DateTime.Parse("2025-11-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Học kỳ 2",
                CreatedAt = DateTime.Parse("2025-11-02"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Học kỳ 3",
                CreatedAt = DateTime.Parse("2025-11-03"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new  Semester()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Học kỳ 4",
                CreatedAt = DateTime.Parse("2025-11-04"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Học kỳ 5",
                CreatedAt = DateTime.Parse("2025-11-05"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Học kỳ 6",
                CreatedAt = DateTime.Parse("2025-11-06"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Học kỳ 7",
                CreatedAt = DateTime.Parse("2025-11-07"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Học kỳ 8",
                CreatedAt = DateTime.Parse("2025-11-08"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Semester()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Học kỳ 9",
                CreatedAt = DateTime.Parse("2025-11-09"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            }
        };

        // Seeding Exams
        var exams = new List<Domain.Entities.Exam>()
        {
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "FALL25",
                StartDate = DateTime.Parse("2025-09-15"),
                EndDate = DateTime.Parse("2025-09-30"),
                CreatedAt = DateTime.Parse("2025-08-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "SPRING25",
                StartDate = DateTime.Parse("2026-03-15"),
                EndDate = DateTime.Parse("2026-03-30"),
                CreatedAt = DateTime.Parse("2026-02-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "SUMMER25",
                StartDate = DateTime.Parse("2026-07-15"),
                EndDate = DateTime.Parse("2026-07-30"),
                CreatedAt = DateTime.Parse("2026-06-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "WINTER25",
                StartDate = DateTime.Parse("2026-12-10"),
                EndDate = DateTime.Parse("2026-12-25"),
                CreatedAt = DateTime.Parse("2026-11-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                SemesterId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "FALL24",
                StartDate = DateTime.Parse("2024-09-15"),
                EndDate = DateTime.Parse("2024-09-30"),
                CreatedAt = DateTime.Parse("2024-08-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                SemesterId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "SPRING24",
                StartDate = DateTime.Parse("2024-03-15"),
                EndDate = DateTime.Parse("2024-03-30"),
                CreatedAt = DateTime.Parse("2024-02-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
                SemesterId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "SUMMER24",
                StartDate = DateTime.Parse("2024-07-15"),
                EndDate = DateTime.Parse("2024-07-30"),
                CreatedAt = DateTime.Parse("2024-06-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
                SemesterId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Code = "FALL23",
                StartDate = DateTime.Parse("2023-09-15"),
                EndDate = DateTime.Parse("2023-09-30"),
                CreatedAt = DateTime.Parse("2023-08-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("c3c3c3c3-c3c3-c3c3-c3c3-c3c3c3c3c3c3"),
                SemesterId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Code = "SPRING23",
                StartDate = DateTime.Parse("2023-03-15"),
                EndDate = DateTime.Parse("2023-03-30"),
                CreatedAt = DateTime.Parse("2023-02-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("d4d4d4d4-d4d4-d4d4-d4d4-d4d4d4d4d4d4"),
                SemesterId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Code = "SUMMER23",
                StartDate = DateTime.Parse("2023-07-15"),
                EndDate = DateTime.Parse("2023-07-30"),
                CreatedAt = DateTime.Parse("2023-06-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("e5e5e5e5-e5e5-e5e5-e5e5-e5e5e5e5e5e5"),
                SemesterId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Code = "FALL22",
                StartDate = DateTime.Parse("2022-09-15"),
                EndDate = DateTime.Parse("2022-09-30"),
                CreatedAt = DateTime.Parse("2022-08-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("f6f6f6f6-f6f6-f6f6-f6f6-f6f6f6f6f6f6"),
                SemesterId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "MIDFALL25",
                StartDate = DateTime.Parse("2025-10-15"),
                EndDate = DateTime.Parse("2025-10-30"),
                CreatedAt = DateTime.Parse("2025-09-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("a7a7a7a7-a7a7-a7a7-a7a7-a7a7a7a7a7a7"),
                SemesterId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "MIDSPRING26",
                StartDate = DateTime.Parse("2026-04-15"),
                EndDate = DateTime.Parse("2026-04-30"),
                CreatedAt = DateTime.Parse("2026-03-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("b8b8b8b8-b8b8-b8b8-b8b8-b8b8b8b8b8b8"),
                SemesterId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "MIDSUMMER26",
                StartDate = DateTime.Parse("2026-08-15"),
                EndDate = DateTime.Parse("2026-08-30"),
                CreatedAt = DateTime.Parse("2026-07-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("c9c9c9c9-c9c9-c9c9-c9c9-c9c9c9c9c9c9"),
                SemesterId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Code = "FINAL25",
                StartDate = DateTime.Parse("2025-11-15"),
                EndDate = DateTime.Parse("2025-11-30"),
                CreatedAt = DateTime.Parse("2025-10-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("d0d0d0d0-d0d0-d0d0-d0d0-d0d0d0d0d0d0"),
                SemesterId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Code = "FINAL26",
                StartDate = DateTime.Parse("2026-05-15"),
                EndDate = DateTime.Parse("2026-05-30"),
                CreatedAt = DateTime.Parse("2026-04-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("e1e1e1e1-e1e1-e1e1-e1e1-e1e1e1e1e1e1"),
                SemesterId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Code = "RETAKE25",
                StartDate = DateTime.Parse("2025-12-15"),
                EndDate = DateTime.Parse("2025-12-30"),
                CreatedAt = DateTime.Parse("2025-11-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("f2f2f2f2-f2f2-f2f2-f2f2-f2f2f2f2f2f2"),
                SemesterId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Code = "RETAKE26",
                StartDate = DateTime.Parse("2026-06-15"),
                EndDate = DateTime.Parse("2026-06-30"),
                CreatedAt = DateTime.Parse("2026-05-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3"),
                SemesterId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Code = "SPECIAL25",
                StartDate = DateTime.Parse("2025-08-15"),
                EndDate = DateTime.Parse("2025-08-30"),
                CreatedAt = DateTime.Parse("2025-07-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Domain.Entities.Exam()
            {
                Id = Guid.Parse("b4b4b4b4-b4b4-b4b4-b4b4-b4b4b4b4b4b4"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "MAKEUP26",
                StartDate = DateTime.Parse("2026-09-15"),
                EndDate = DateTime.Parse("2026-09-30"),
                CreatedAt = DateTime.Parse("2026-08-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            }
        };

        // Seeding Subjects
        var subjects = new List<Subject>()
        {
            new Subject()
            {
                Id = Guid.Parse("f1e1d1c1-b1a1-9191-8181-717171717171"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Lập trình đa nền tảng với ASP.NET Core",
                Code = "PRN212",
                CreatedAt = DateTime.Parse("2025-10-01"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Lập trình mvc với ASP.NET Core",
                Code = "PRN222",
                CreatedAt = DateTime.Parse("2025-10-02"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("d3c3b3a3-9393-8383-7373-636363636363"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "Lập trình web với ASP.NET Core",
                Code = "PRN232",
                CreatedAt = DateTime.Parse("2025-10-03"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("c4b4a4a4-8484-7474-6464-545454545454"),
                SemesterId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Lập trình nâng cao với C#",
                Code = "PRN211",
                CreatedAt = DateTime.Parse("2025-10-04"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("b5a5a5a5-7575-6565-5555-454545454545"),
                SemesterId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Lập trình hướng đối tượng",
                Code = "PRO192",
                CreatedAt = DateTime.Parse("2025-10-05"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("a6a6a6a6-6666-5656-4646-363636363636"),
                SemesterId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Cấu trúc dữ liệu và giải thuật",
                Code = "CSD201",
                CreatedAt = DateTime.Parse("2025-10-06"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("a7a7a7a7-5757-4747-3737-272727272727"),
                SemesterId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Cơ sở dữ liệu",
                Code = "DBI202",
                CreatedAt = DateTime.Parse("2025-10-07"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("a8a8a8a8-4848-3838-2828-181818181818"),
                SemesterId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Lập trình Java",
                Code = "JPD123",
                CreatedAt = DateTime.Parse("2025-10-08"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("a9a9a9a9-3939-2929-1919-191919191919"),
                SemesterId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Lập trình Python",
                Code = "PYT101",
                CreatedAt = DateTime.Parse("2025-10-09"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("b1b1b1b1-2121-1111-0101-010101010101"),
                SemesterId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Phát triển ứng dụng Web",
                Code = "SWP391",
                CreatedAt = DateTime.Parse("2025-10-10"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("c2c2c2c2-1212-0202-9191-929292929292"),
                SemesterId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Kiểm thử phần mềm",
                Code = "SWT301",
                CreatedAt = DateTime.Parse("2025-10-11"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("d3d3d3d3-0303-9393-8383-838383838383"),
                SemesterId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Yêu cầu phần mềm",
                Code = "SWR302",
                CreatedAt = DateTime.Parse("2025-10-12"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("e4e4e4e4-9494-8484-7474-747474747474"),
                SemesterId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Thiết kế phần mềm",
                Code = "SWD392",
                CreatedAt = DateTime.Parse("2025-10-13"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("f5f5f5f5-8585-7575-6565-656565656565"),
                SemesterId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Lập trình mobile với React Native",
                Code = "MMA301",
                CreatedAt = DateTime.Parse("2025-10-14"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("a0a0a0a0-7676-6666-5656-565656565656"),
                SemesterId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "Lập trình IoT",
                Code = "IOT102",
                CreatedAt = DateTime.Parse("2025-10-15"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("b0b0b0b0-6767-5757-4747-474747474747"),
                SemesterId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Name = "Trí tuệ nhân tạo",
                Code = "AIG201",
                CreatedAt = DateTime.Parse("2025-10-16"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("c0c0c0c0-5858-4848-3838-383838383838"),
                SemesterId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Name = "Machine Learning",
                Code = "MLE301",
                CreatedAt = DateTime.Parse("2025-10-17"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("d0d0d0d0-4949-3939-2929-292929292929"),
                SemesterId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Name = "Cloud Computing",
                Code = "CLD201",
                CreatedAt = DateTime.Parse("2025-10-18"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("e0e0e0e0-3a3a-2a2a-1a1a-1a1a1a1a1a1a"),
                SemesterId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Name = "DevOps và CI/CD",
                Code = "DOP301",
                CreatedAt = DateTime.Parse("2025-10-19"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            },
            new Subject()
            {
                Id = Guid.Parse("f0f0f0f0-2b2b-1b1b-0b0b-0b0b0b0b0b0b"),
                SemesterId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "An toàn và bảo mật thông tin",
                Code = "SEC301",
                CreatedAt = DateTime.Parse("2025-10-20"),
                UpdatedAt = DateTime.MinValue,
                DeletedAt = DateTime.MinValue,
                IsActive = true
            }
        };
        var violationStructureJson = """
                                     {
                                       "KeywordCheck": {
                                         "Keywords": ["bear", "leopard", "chatgpt"],
                                         "FileExtensions": [".cs", ".cshtml", ".html"]
                                       },
                                       "NameFormatMismatch": {
                                         "NameFormat": "PE_PRN222_SU25_{StudentName}"
                                       },
                                       "CompilationError": true
                                     }
                                     """;
        var examSubjects = new List<ExamSubject>
        {
           new ExamSubject
            {
                Id = Guid.Parse("11111111-aaaa-bbbb-cccc-000000000001"),
                ExamId = Guid.Parse("c9c9c9c9-c9c9-c9c9-c9c9-c9c9c9c9c9c9"), // FINAL25
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"), // PRN222
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("22222222-aaaa-bbbb-cccc-000000000002"),
                ExamId = Guid.Parse("e1e1e1e1-e1e1-e1e1-e1e1-e1e1e1e1e1e1"), // RETAKE25
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"), // PRN222
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("33333333-aaaa-bbbb-cccc-000000000003"),
                ExamId = Guid.Parse("f6f6f6f6-f6f6-f6f6-f6f6-f6f6f6f6f6f6"), // MIDFALL25
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"), // PRN222
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("44444444-aaaa-bbbb-cccc-000000000004"),
                ExamId = Guid.Parse("a3a3a3a3-a3a3-a3a3-a3a3-a3a3a3a3a3a3"), // SPECIAL25
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"), // PRN222
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("55555555-aaaa-bbbb-cccc-000000000005"),
                ExamId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), // SPRING25
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"), // PRN222
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("66666666-aaaa-bbbb-cccc-000000000006"),
                ExamId = Guid.Parse("d0d0d0d0-d0d0-d0d0-d0d0-d0d0d0d0d0d0"), // FINAL26
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"),
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("77777777-aaaa-bbbb-cccc-000000000007"),
                ExamId = Guid.Parse("f2f2f2f2-f2f2-f2f2-f2f2-f2f2f2f2f2f2"), // RETAKE26
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"),
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            },
            new ExamSubject
            {
                Id = Guid.Parse("88888888-aaaa-bbbb-cccc-000000000008"),
                ExamId = Guid.Parse("b4b4b4b4-b4b4-b4b4-b4b4-b4b4b4b4b4b4"), // MAKEUP26
                SubjectId = Guid.Parse("e2d2c2b2-a2a2-9292-8282-727272727272"),
                ScoreStructure    = null,
                ViolationStructure = violationStructureJson,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.MinValue,
                DeletedAt         = DateTime.MinValue,
                IsActive          = true
            }
        };

        await _context.Semesters.AddRangeAsync(semesters);
        await _context.Exams.AddRangeAsync(exams);
        await _context.Subjects.AddRangeAsync(subjects);
        await _context.ExamSubjects.AddRangeAsync(examSubjects);
        await _context.SaveChangesAsync();
    }
}
