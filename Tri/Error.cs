using System;

namespace Tri
{
    public class Error
    {
        readonly string message;
        readonly int lineNumber;

        /// <summary>
        /// Display the error message
        /// </summary>
        /// <param name="showLineNum">Should the line number, that caused an error, be displayed</param>
        public virtual void DisplayError(bool showLineNum)
        {
            ConsoleColor prevFrg = Console.ForegroundColor;
            ConsoleColor prevBkg = Console.BackgroundColor;

            //Set the console colour, to make the error stand out, and look important
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Black;
            //Draw the message, and the line number if allowed
            Console.WriteLine(message);
            if (showLineNum) { Console.WriteLine($"Error at line {lineNumber + 1}"); }

            Console.ForegroundColor = prevFrg;
            Console.BackgroundColor = prevBkg;
        }

        /// <summary>
        /// Creates an error object
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="lineNumber">What line caused the error</param>
        public Error(string message, int lineNumber)
        {
            this.message = message;
            this.lineNumber = lineNumber;
            DisplayError(true);
        }
        /// <summary>
        /// Create an error object
        /// </summary>
        /// <param name="message">Message to show</param>
        public Error(string message)
        {
            this.message = message;
            DisplayError(false);
        }
    }
}
