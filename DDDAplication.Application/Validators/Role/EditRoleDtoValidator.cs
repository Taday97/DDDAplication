using DDDAplication.Application.DTOs.Rol;
using DDDAplication.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Nito.AsyncEx;

namespace DDDAplication.Application.Validators.Role
{
    public class EditRoleDtoValidator : AbstractValidator<EditRoleDto>
    {
        private readonly IRoleRepository _roleRepository;

        public EditRoleDtoValidator(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
            RuleFor(x => x.Id)
                .NotEmpty().When(dto => !string.IsNullOrEmpty(dto.Id)).WithMessage("Role ID cannot be empty.")
                .Must(Id => Guid.TryParse(Id, out _)).WithMessage("Role ID must be a valid GUID.")
                .Must((dto, Id) => Id == dto.Id).When(dto => !string.IsNullOrEmpty(dto.Id)).WithMessage("Role ID in request body does not match the ID in the URL.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.")
                .MustAsync(NameMustBeUnique).WithMessage($"A role with the name already exists.");
        }
        private async Task<bool> NameMustBeUnique(EditRoleDto dto, string name, CancellationToken cancellation)
        {
            if (!Guid.TryParse(dto.Id, out var guid))
            {
                return true;
            }
            var existingRole = await _roleRepository.FindFirstRoleAsync(r => r.Name == name && r.Id != guid);
            return existingRole == null;
        }
        public override ValidationResult Validate(ValidationContext<EditRoleDto> context)
        {
            return AsyncContext.Run(() => base.ValidateAsync(context));
        }
    }

}
