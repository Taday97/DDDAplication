using DDDAplication.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Auth
{
    public class ConfirmEmailModelDtoValidator : AbstractValidator<ConfirmEmailModelDto>
    {
        public ConfirmEmailModelDtoValidator()
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format.")
                .NotEmpty().WithMessage("Email is required.");
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");
        }
    }
}
