using System;

namespace Tri
{
    public class Error
    {
        string message;
        int lineNumber;

        public virtual void DisplayError(bool showLineNum)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine(message);
            if (showLineNum) { Console.WriteLine($"Error at line {lineNumber + 1}"); }
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public Error(string message, int lineNumber)
        {
            this.message = message;
            this.lineNumber = lineNumber;
            DisplayError(true);
        }
        public Error(string message)
        {
            this.message = message;
            DisplayError(false);
        }
    }
}
