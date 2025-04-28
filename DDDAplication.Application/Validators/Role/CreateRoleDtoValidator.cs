using DDDAplication.Application.DTOs.Rol;
using DDDAplication.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Nito.AsyncEx;

namespace DDDAplication.Application.Validators.Role
{
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        private readonly IRoleRepository _roleRepository;

        public CreateRoleDtoValidator(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Length(3, 50).WithMessage("Name must be between 3 and 50 characters.")
                .MustAsync(NameMustBeUnique).WithMessage($"A role with the name already exists.");
        }
        private async Task<bool> NameMustBeUnique(CreateRoleDto dto, string name, CancellationToken cancellation)
        {
            var existingRole = await _roleRepository.FindFirstRoleAsync(r => r.Name == name);
            return existingRole == null;
        }
        public override ValidationResult Validate(ValidationContext<CreateRoleDto> context)
        {
            return AsyncContext.Run(() => base.ValidateAsync(context));
        }
    }

}
