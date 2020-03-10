using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace Jay.BMPScript
{
    public class BMPScriptException : Exception
    {
        public string Module;
        public string ExceptionType;
        public BMPScriptException(string Module, string ExceptionType, string Message)
            : base(Message)
        {
            this.Module = Module;
            this.ExceptionType = ExceptionType;
        }
    }
}