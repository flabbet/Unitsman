﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UnitsmanCore.Exceptions
{
    public class ConversionException : Exception
    {
        public ConversionException() { }
        public ConversionException(string message) : base(message) { }
        public ConversionException(string message, Exception inner) : base(message, inner) { }
        protected ConversionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
