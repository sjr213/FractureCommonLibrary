namespace FractureCommonLib
{
    public class ImageException : Exception
    {
        public ImageException() : base()
        {}

        public ImageException(String msg) : base(msg)
        {}

        public ImageException(string message, System.Exception inner) : base(message, inner) 
        { }

    }
}
