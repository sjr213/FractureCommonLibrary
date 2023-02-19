using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FractureCommonLib
{
    public class ColorPointException : Exception
    {
        public ColorPointException() : base()
        {}

        public ColorPointException(String msg) : base(msg)
        {}

        public ColorPointException(string message, System.Exception inner) : base(message, inner) 
        { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ColorPointException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            :base(info, context)
        { }

    }
}
