using System;

namespace Jay.IEnumerators
{
    public class IEnumeratorException : Exception
    {
        public string ErrorReason { get; }
        public IEnumeratorException(string Message, string ErrorReason) : base(Message)
        {
            this.ErrorReason = ErrorReason;
        }
    }
}