using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Application.DTOs.Auth
{
    public class SendResetLinkModelDto
    {
        [Required]
        public string Email { get; set; }
    }
}
