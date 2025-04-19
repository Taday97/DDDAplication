using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Api.IntegrationTests.Helper
{
    public class ErrorResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string? ErrorCode { get; set; }
        public string? Details { get; set; }
    }

}
