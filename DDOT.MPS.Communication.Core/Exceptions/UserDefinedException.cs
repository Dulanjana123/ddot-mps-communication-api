namespace DDOT.MPS.Communication.Core.Exceptions
{
    public class UserDefinedException : Exception
    {
        public UserDefinedException(string message) : base(message) { }

        public class UDInvalidOperationException : UserDefinedException
        {
            public UDInvalidOperationException(string message) : base(message)
            {
            }
        }

        public class UDNotFoundException : UserDefinedException
        {
            public UDNotFoundException(string message) : base(message)
            {
            }
        }

        public class UDUnauthorizedAccessException : UserDefinedException
        {
            public UDUnauthorizedAccessException(string message) : base(message)
            {
            }
        }

        public class UDArgumentException : UserDefinedException
        {
            public UDArgumentException(string message) : base(message)
            {
            }
        }

        public class UDValiationException : UserDefinedException
        {
            public UDValiationException(string message) : base(message)
            {
            }
        }
    }
}
