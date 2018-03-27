using System;

namespace Stannieman.DI
{
    public class ContainerException : Exception
    {
        public ContainerErrorCodes ErrorCode { get; }

        public ContainerException(ContainerErrorCodes errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}
