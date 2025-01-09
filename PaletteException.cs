
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
    }
}
