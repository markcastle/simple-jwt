using System;
using System.Runtime.Serialization;

namespace SimpleJwt.Core
{
    /// <summary>
    /// The exception that is thrown when a JWT token operation fails.
    /// </summary>
    [Serializable]
    public class JwtTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class.
        /// </summary>
        public JwtTokenException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public JwtTokenException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public JwtTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class with a specified error code and message.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="message">The message that describes the error.</param>
        public JwtTokenException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class with a specified error code, message,
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="errorCode">The error code associated with the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public JwtTokenException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected JwtTokenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode = info.GetString("ErrorCode");
        }

        /// <summary>
        /// Gets the error code associated with the exception.
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// When overridden in a derived class, sets the SerializationInfo with information about the exception.
        /// </summary>
        /// <param name="info">The SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The StreamingContext that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ErrorCode", ErrorCode);
        }
    }
} 