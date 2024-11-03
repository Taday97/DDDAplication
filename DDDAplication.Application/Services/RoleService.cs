using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;

namespace DDDAplication.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _rolRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository rolRepository, IMapper mapper)
        {
            _rolRepository = rolRepository;
            _mapper = mapper;
        }

        public async Task<RoleDto> AddAsync(RoleDto RoleDto)
        {
            if (RoleDto == null) throw new ArgumentNullException(nameof(RoleDto));

            var rol = _mapper.Map<Role>(RoleDto); // Map DTO to Domain entity
            var createdRole = await _rolRepository.AddAsync(rol);
            return _mapper.Map<RoleDto>(createdRole); // Map back to DTO for returning
        }

        public async Task<RoleDto> Delete(string id)
        {
            var deletedRole = await _rolRepository.DeleteAsync(id);
            return _mapper.Map<RoleDto>(deletedRole);
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _rolRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles); // Map to DTOs
        }

        public async Task<RoleDto> GetByIdAsync(string id)
        {
            var role = await _rolRepository.GetByIdAsync(id);
            return _mapper.Map<RoleDto>(role); // Map to DTO
        }

        public async Task<RoleDto> Update(RoleDto RoleDto)
        {
            if (RoleDto == null) throw new ArgumentNullException(nameof(RoleDto));

            var rol = _mapper.Map<Role>(RoleDto); // Map DTO to Domain entity
            var updatedRole = await _rolRepository.UpdateAsync(rol);
            return _mapper.Map<RoleDto>(updatedRole); // Map back to DTO for returning
        }
    }
}
