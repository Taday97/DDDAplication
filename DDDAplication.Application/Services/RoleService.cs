using AutoMapper;
using DDDAplication.Application.DTOs;
using DDDAplication.Application.Interfaces;
using DDDAplication.Domain.Entities;
using DDDAplication.Domain.Interfaces;
using DDDAplication.Infrastructure.Repositories;

namespace DDDAplication.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository, IUserService userService, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<ApiResponse<RoleDto>> AddAsync(RoleDto roleDto)
        {
            if (roleDto == null)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse("Role data cannot be null.");
            }

            try
            {
                var roleEntity = _mapper.Map<Role>(roleDto);

                var createdRole = await _roleRepository.AddAsync(roleEntity);

                var createdRoleDto = _mapper.Map<RoleDto>(createdRole);

                return ApiResponse<RoleDto>.CreateSuccessResponse("Role created successfully.", createdRoleDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse($"Error creating role: {ex.Message}");
            }
        }


        public async Task<ApiResponse<RoleDto>> GetByIdAsync(string id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse($"Role with id {id} not found.");
            }

            var roleDto = _mapper.Map<RoleDto>(role);
            return ApiResponse<RoleDto>.CreateSuccessResponse("Role retrieved successfully.", roleDto);
        }


        public async Task<ApiResponse<RoleDto>> Delete(string id)
        {

            if (string.IsNullOrEmpty(id))
            {
                return ApiResponse<RoleDto>.CreateErrorResponse("Role ID is required.");
            }
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse($"Role with id {id} not found.");
            }

            await _roleRepository.DeleteAsync(id);

            var roleDto = _mapper.Map<RoleDto>(role);

            return ApiResponse<RoleDto>.CreateSuccessResponse("Role deleted successfully.", roleDto);
        }


        public async Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync()
        {
            var roles = await _roleRepository.GetAllAsync();

            if (roles == null || !roles.Any())
            {
                return ApiResponse<IEnumerable<RoleDto>>.CreateErrorResponse("No roles found.");
            }

            var roleDtos = _mapper.Map<IEnumerable<RoleDto>>(roles);

            return ApiResponse<IEnumerable<RoleDto>>.CreateSuccessResponse("Role retrieved successfully.", roleDtos);
        }


        public async Task<ApiResponse<RoleDto>> Update(RoleDto roleDto)
        {
            if (roleDto == null)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse("Role data cannot be null.");
            }

            var role = await _roleRepository.GetByIdAsync(roleDto.Id.ToString());
            if (role == null)
            {
                return ApiResponse<RoleDto>.CreateErrorResponse($"Role with id {roleDto.Id} not found.");
            }

            _mapper.Map(roleDto, role);

            await _roleRepository.UpdateAsync(role);

            return ApiResponse<RoleDto>.CreateSuccessResponse("Role updated successfully.", roleDto);
        }
      


    }
}
