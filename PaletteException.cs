
namespace FractureCommonLib
{
    public class PaletteException: Exception
    {
        public PaletteException() : base()
        {}

        public PaletteException(String msg) : base(msg)
        {}

        public PaletteException(string message, System.Exception inner) : base(message, inner) 
        { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected PaletteException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
            : base(info, context)    
        { }
    }
}
