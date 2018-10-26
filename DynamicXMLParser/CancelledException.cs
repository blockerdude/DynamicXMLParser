using System;
using System.Runtime.Serialization;

namespace DynamicXMLParser
{
    [Serializable]
    internal class CancelledException : Exception
    {
        public CancelledException()
        {
        }

        public CancelledException(string message) : base(message)
        {
        }

        public CancelledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CancelledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}