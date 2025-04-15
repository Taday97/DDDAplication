using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDDAplication.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public virtual int StatusCode { get; } = 400;

        protected BaseException(string message) : base(message) { }
    }
}
