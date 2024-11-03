using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.DTOs
{
    public class ConfirmEmailModelDto
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}
