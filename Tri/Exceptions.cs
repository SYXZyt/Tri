using System;

namespace Tri
{
    public class TRI_COMMAND_FAILED : Error
    {
        public TRI_COMMAND_FAILED(string message) : base(message) { }
        public TRI_COMMAND_FAILED(string message, int lineNumber) : base(message, lineNumber) { }
    }

    public class TRI_VARIABLE_DOES_NOT_EXIST : Error
    {
        public TRI_VARIABLE_DOES_NOT_EXIST(string message, int lineNumber) : base(message, lineNumber) { }
    }

    public class TRI_DIVISION_BY_ZERO : Error
    {
        public TRI_DIVISION_BY_ZERO(int lineNumber) : base("ERROR Tried to divide by 0", lineNumber) { }
    }

    public class TRI_LABEL_NOT_FOUND : Error
    {
        public TRI_LABEL_NOT_FOUND(string labelName, int lineNumber) : base($"ERROR {labelName} was not found", lineNumber) { }
    }
}
