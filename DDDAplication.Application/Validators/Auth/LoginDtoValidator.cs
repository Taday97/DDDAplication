using DDDAplication.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Auth
{
    public class LoginModelDtoValidator : AbstractValidator<LoginModelDto>
    {
        public LoginModelDtoValidator()
        {
            RuleFor(x => x)
               .NotNull().WithMessage("LoginModelDto cannot be null.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
