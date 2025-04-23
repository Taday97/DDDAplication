using DDDAplication.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators
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
