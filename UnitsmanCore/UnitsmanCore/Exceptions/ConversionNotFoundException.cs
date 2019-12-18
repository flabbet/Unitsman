using System;
using System.Collections.Generic;
using System.Text;

namespace UnitsmanCore.Exceptions
{
    public class ConversionNotFoundException : ConversionException
    {
        public ConversionNotFoundException(string message) : base(message)
        {
        }

        public ConversionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConversionNotFoundException()
        {
        }
    }
}
