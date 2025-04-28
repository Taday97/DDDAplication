using DDDAplication.Application.DTOs.Rol;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Role
{
    public class RoleDtoValidator : AbstractValidator<RoleDto>
    {
        public RoleDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().When(dto => !string.IsNullOrEmpty(dto.Id)).WithMessage("Role ID cannot be empty.")
                .Must((dto, id) => id == dto.Id).When(dto => !string.IsNullOrEmpty(dto.Id)).WithMessage("Role ID in request body does not match the ID in the URL.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.");

        }
    }

}
