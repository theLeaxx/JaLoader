using System;
using System.Runtime.Serialization;

namespace JaLoader.Common
{
    [Serializable]
    public class ModException : Exception
    {
        public string ModID { get; }
        public int ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the ModException class.
        /// </summary>
        public ModException() : base() { }

        /// <summary>
        /// Initializes a new instance of the ModException class with a specified error message.
        /// <param name="message">The message that describes the error.</param>
        /// </summary>
        public ModException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the ModException class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public ModException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the ModException class with a specified error message
        /// and custom mod-related information.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="modID">The ID of the mod that caused the exception.</param>
        /// <param name="errorCode">A custom error code associated with the mod load failure.</param>
        public ModException(string message, string modID, int errorCode = 0) : base(message)
        {
            ModID = modID;
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the ModException class with serialized data.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The System.Runtime.Serialization.StreamingContext that contains contextual information about the source or destination.</param>
        protected ModException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Deserialize custom properties here
            ModID = info.GetString("ModID");
            ErrorCode = info.GetInt32("ErrorCode");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ModID", ModID);
            info.AddValue("ErrorCode", ErrorCode);
        }
    }
}
