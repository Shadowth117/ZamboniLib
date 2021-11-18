using System;
using System.Runtime.Serialization;

namespace Zamboni
{
    /// <summary>
    /// Zamboni内で発生した例外
    /// </summary>
    class ZamboniException : Exception
    {
        public ZamboniException(Exception e) : base()
        {
        }

        public ZamboniException(string message) : base(message)
        {
        }

        public ZamboniException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ZamboniException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}