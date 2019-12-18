using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace UnitsmanCore.Exceptions
{
    public class ConversionMismatchException : ConversionException
    {
        public ConversionMismatchException(string message) : base(message)
        {
        }

        public ConversionMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConversionMismatchException()
        {
        }

        protected ConversionMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
