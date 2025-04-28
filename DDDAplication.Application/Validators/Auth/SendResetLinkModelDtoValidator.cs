using DDDAplication.Application.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Auth
{
    public class SendResetLinkModelDtoValidator : AbstractValidator<SendResetLinkModelDto>
    {
        public SendResetLinkModelDtoValidator()
        {
            RuleFor(x => x)
            .NotNull().WithMessage("SendResetLinkModelDto cannot be null.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
        }
    }
}
