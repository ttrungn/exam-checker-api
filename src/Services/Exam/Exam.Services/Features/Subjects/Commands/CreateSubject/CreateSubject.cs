using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Subjects.Commands.CreateSubject;

public record CreateSubjectCommand : IRequest<BaseServiceResponse>
{
    public Guid SemesterId { get; init; }
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
}

public class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(v => v.SemesterId)
            .NotEmpty().WithMessage("Vui lòng nhập mã học kỳ!");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Vui lòng nhập tên môn học!");

        RuleFor(v => v.Code)
            .NotEmpty().WithMessage("Vui lòng nhập mã môn học!");
    }
}

public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSubjectCommandHandler> _logger;

    public CreateSubjectCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateSubjectCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        var semester = await _unitOfWork.GetRepository<Semester>()
            .Query()
            .FirstOrDefaultAsync(s => s.Id == request.SemesterId, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", request.SemesterId);
            return new BaseServiceResponse() { Success = false, Message = "Không tìm thấy học kỳ!" };
        }

        var subject = request.ToSubject();
        await _unitOfWork.GetRepository<Subject>().InsertAsync(subject, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Subject created successfully with ID: {Id}", subject.Id);
        return new DataServiceResponse<Guid>()
        {
            Success = true,
            Message = "Tạo môn học thành công!",
            Data = subject.Id
        };
    }
}
