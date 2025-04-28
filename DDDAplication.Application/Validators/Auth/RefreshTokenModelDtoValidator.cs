using DDDAplication.Application.DTOs.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.Validators.Auth
{
    public class RefreshTokenModelDtoValidator : AbstractValidator<RefreshTokenModelDto>
    {
        public RefreshTokenModelDtoValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.");


        }
    }

}