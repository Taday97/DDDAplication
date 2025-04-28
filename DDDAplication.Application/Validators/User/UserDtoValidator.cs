using DDDAplication.Application.DTOs.User;
using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Nito.AsyncEx;
using System.Security.Claims;

namespace DDDAplication.Application.Validators.User
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {

        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserDtoValidator(IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("UserName is required.")
                .MustAsync(NameMustBeUnique).WithMessage("A user with the name already exists.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

        }
        private async Task<bool> NameMustBeUnique(UserDto dto, string name, CancellationToken cancellation)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return false;
            }

            var existingUser = await _userRepository.FindFirstUserAsync(u =>
                u.NormalizedUserName == name.ToUpper() &&
                u.Id != userId
            );

            return existingUser == null;
        }

        public override ValidationResult Validate(ValidationContext<UserDto> context)
        {
            return AsyncContext.Run(() => base.ValidateAsync(context));
        }



    }



}
