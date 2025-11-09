using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalytics.Application.Exceptions
{
    public class SalesDataHandlerException : Exception
    {
        public SalesDataHandlerException()
        {
        }
        public SalesDataHandlerException(string message)
            : base(message)
        {
        }
        public SalesDataHandlerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
