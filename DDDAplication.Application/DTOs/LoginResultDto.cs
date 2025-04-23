using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.DTOs
{
    public class LoginResultDto
    {
        public string Token { get; set; }
        public UserLoginResponseDto User { get; set; }
    }

}
