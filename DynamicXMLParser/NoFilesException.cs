using System;
using System.Runtime.Serialization;

namespace DynamicXMLParser
{
    [Serializable]
    internal class NoFilesException : Exception
    {
        public NoFilesException()
        {
        }

        public NoFilesException(string message) : base(message)
        {
        }

        public NoFilesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoFilesException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}