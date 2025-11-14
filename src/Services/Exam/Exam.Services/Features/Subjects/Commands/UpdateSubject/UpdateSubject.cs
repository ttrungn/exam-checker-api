using Exam.Domain.Entities;
using Exam.Repositories.Interfaces.Repositories;
using Exam.Services.Exceptions;
using Exam.Services.Mappers;
using Exam.Services.Models.Responses;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Exam.Services.Features.Subjects.Commands.UpdateSubject;

public record UpdateSubjectCommand : IRequest<BaseServiceResponse>
{
    public Guid Id { get; init; }
    public Guid SemesterId { get; init; }
    public string Name { get; init; } = null!;
    public string Code { get; init; } = null!;
}

public class UpdateSubjectCommandValidator : AbstractValidator<UpdateSubjectCommand>
{
    public UpdateSubjectCommandValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Vui lòng nhập Id!");

        RuleFor(c => c.SemesterId)
            .NotEmpty().WithMessage("Vui lòng nhập mã học kỳ!");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vui lòng nhập tên môn học!");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Vui lòng nhập mã môn học!");
    }
}

public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, BaseServiceResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubjectCommandHandler> _logger;

    public UpdateSubjectCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateSubjectCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BaseServiceResponse> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<Subject>();
        var subject = await repository.Query()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);
        if (subject == null)
        {
            _logger.LogError("Failed to retrieve subject with ID: {Id}", request.Id);
            throw new NotFoundException("Không tìm thấy môn học!");
        }

        var semester = await  _unitOfWork.GetRepository<Semester>()
            .Query()
            .FirstOrDefaultAsync(s => s.Id == request.SemesterId, cancellationToken);
        if (semester == null)
        {
            _logger.LogError("Failed to retrieve semester with ID: {Id}", request.SemesterId);
            throw new NotFoundException("Không tìm thấy học kỳ!");
        }

        subject.UpdateSubject(request);
        await repository.UpdateAsync(subject, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Subject updated successfully with ID: {Id}", subject.Id);
        return new BaseServiceResponse()
        {
            Success = true,
            Message = "Chỉnh sửa môn học thành công!",
        };
    }
}
