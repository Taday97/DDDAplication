using DDDAplication.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators
{
    public class RoleDtoValidator : AbstractValidator<RoleDto>
    {
        public RoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.");

            RuleFor(x => x.NormalizedName)
                .NotEmpty().WithMessage("NormalizedName is required.")
                .Length(3, 50).WithMessage("NormalizedName must be between 3 and 50 characters.");
        }
    }

}
