﻿namespace FractureCommonLib
{
    public class ImageException : Exception
    {
        public ImageException() : base()
        {}

        public ImageException(String msg) : base(msg)
        {}

        public ImageException(string message, System.Exception inner) : base(message, inner) 
        { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected ImageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context)
        { }
    }
}
