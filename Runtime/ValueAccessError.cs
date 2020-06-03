using System;
using System.Runtime.Serialization;

namespace JoyScript
{
    [Serializable]
    internal class ValueAccessError : Exception
    {
        public ValueAccessError()
        {
        }

        public ValueAccessError(string message) : base(message)
        {
        }

        public ValueAccessError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValueAccessError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}