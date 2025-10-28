using FluentValidation;
using MediatR;
using User.Services.Models.Responses;

namespace User.Services.Features.Account.Queries.GetUserProfile;

public class GetUserProfileQuery : IRequest<DataServiceResponse<UserProfileDto>>
{
    public Guid UserId { get; init; }
}

public class GetUserProfileValidator : AbstractValidator<GetUserProfileQuery>
{
    public GetUserProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
