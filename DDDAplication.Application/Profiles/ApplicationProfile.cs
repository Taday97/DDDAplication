using DDDAplication.Application.DTOs;
using AutoMapper;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Application.Profiles
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<Role, RoleDto>().ReverseMap();
        }
    }
}
