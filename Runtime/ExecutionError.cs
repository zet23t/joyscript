using System;
using System.Runtime.Serialization;

namespace JoyScript
{
    [Serializable]
    internal class ExecutionError : Exception
    {
        public ExecutionError()
        {
        }

        public ExecutionError(string message) : base(message)
        {
        }

        public ExecutionError(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExecutionError(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}