using System;
using System.Collections.Generic;
using System.Text;

namespace UnitsmanCore.Exceptions
{
    public class UnitNotFoundException : ConversionException
    {
        public UnitNotFoundException(string message) : base(message)
        {
        }

        public UnitNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnitNotFoundException()
        {
        }
    }
}
