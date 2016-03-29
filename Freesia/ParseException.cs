using System;

namespace Freesia
{
    public class ParseException : Exception
    {
        public ParseException(string text, int index)
            : base(String.Format("{0}, Position: {1}", text, index))
        {
        }
    }
}