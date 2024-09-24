using DDDAplication.Application.DTOs;
using AutoMapper;
using DDDAplication.Domain.Entities;

namespace DDDAplication.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
        }
    }
}
