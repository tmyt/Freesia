using System;

namespace Freesia
{
    public class ParseException : Exception
    {
        public ParseException(string text, int index)
            : base($"{text}, Position: {index}")
        {
        }
    }
}