using AutoMapper;
using DDDAplication.Domain.Entities;
using DDDAplication.Application.DTOs.Rol;
using DDDAplication.Application.DTOs.User;

namespace DDDAplication.Application.Profiles
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<ApplicationUser, UserPorfileDto>().ReverseMap();

            CreateMap<Role, RoleDto>().ReverseMap();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<EditRoleDto, Role>().ReverseMap();

            CreateMap<ApplicationUser, UserLoginResponseDto>()
           .ForMember(dest => dest.Roles, opt => opt.Ignore());

        }
    }
}
