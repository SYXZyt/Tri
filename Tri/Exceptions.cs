namespace Tri
{
    public class TRI_COMMAND_FAILED : Error
    {
        public TRI_COMMAND_FAILED(string message) : base(message) { }
        public TRI_COMMAND_FAILED(string message, int lineNumber) : base(message, lineNumber) { }
    }

    public class TRI_FILE_UNREADABLE : Error
    {
        public TRI_FILE_UNREADABLE(string fileName) : base($"The file {fileName} could not be opened") { }
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

    public class TRI_OPERATOR_NOT_RECOGNISED : Error
    {
        public TRI_OPERATOR_NOT_RECOGNISED(string line, int lineNumber) : base($"ERROR {line}used an unrecognised operator ", lineNumber) { }
    }
}
