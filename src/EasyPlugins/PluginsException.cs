﻿using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace EasyPlugins
{
    [Serializable]
    public class PluginsException : Exception
    {
        public PluginsException()
        {
        }

        internal PluginsException(ErrorCode errorCode): 
            this(errorCode, (Exception)null)
        {
        }

        internal PluginsException(ErrorCode errorCode, Exception inner) :
            this(errorCode, ErrorMessages.ResourceManager.GetString(errorCode.ToString()), inner)
        {
        }

        internal PluginsException(ErrorCode errorCode, string message): 
            this(errorCode, message, null)
        {
        }

        internal PluginsException(ErrorCode errorCode, string message, Exception inner)
            : base(message, inner)
        {
            ErrorCode = errorCode;
        }

        public ErrorCode ErrorCode { get; set; }

        protected PluginsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ErrorCode errorCode;
            ErrorCode = Enum.TryParse<ErrorCode>(info.GetString("ErrorCode"), out errorCode)
                ? errorCode
                : ErrorCode.NotSpecified;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            info.AddValue("ErrorCode", ErrorCode.ToString());
            base.GetObjectData(info, context);
        }
    }
}
