using System;
using System.Runtime.Serialization;

namespace JoyScript
{
    [Serializable]
    public class SyntaxError : Exception
    {
        private object p;

        public SyntaxError()
        {
        }

        public SyntaxError(object p)
        {
            this.p = p;
        }

        public SyntaxError(string message) : base(message)
        {
        }

        public SyntaxError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SyntaxError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}