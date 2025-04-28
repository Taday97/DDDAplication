using DDDAplication.Application.DTOs.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Auth
{
    public class ChangePasswordModelDtoValidator : AbstractValidator<ChangePasswordModelDto>
    {
        public ChangePasswordModelDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters long.")
                .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches(@"\d").WithMessage("New password must contain at least one number.")
                .Matches(@"[\W]").WithMessage("New password must contain at least one special character.");
        }
    }

}