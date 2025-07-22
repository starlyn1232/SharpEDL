using System;
using System.Runtime.Serialization;

namespace SharpEDL.Classes
{
    [Serializable]
    public class QCOMException : Exception
    {
        public QCOMException() { }
        public QCOMException(string message) : base(message) { }
        public QCOMException(string message, Exception inner) : base(message, inner) { }
        protected QCOMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
