using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Exceptions
{
    public class InternalServerException: Exception
    {
        public InternalServerException()
        {
            
        }
        public InternalServerException(string message, string detail): base(message)
        {
            Detail = detail;
        }

        public string Detail { get; private set; }
    }
}
