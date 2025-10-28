using FluentValidation;
using MediatR;
using User.Services.Models.Responses;

namespace User.Services.Features.Account.Commands.AssignARoleToAnAccount;

public record AssignARoleToAnAccountCommand : IRequest<BaseServiceResponse>
{
    public Guid UserId { get; init; }
    public Guid AppRoleId { get; init; }
}

public class AssignARoleToAnAccountCommandValidator : AbstractValidator<AssignARoleToAnAccountCommand>
{
    public AssignARoleToAnAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.AppRoleId)
            .NotEmpty().WithMessage("AppRoleId is required.");
    }
}
