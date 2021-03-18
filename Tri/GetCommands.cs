using System;
using System.IO;

namespace Tri
{
    class GetCommands
    {
        /// <summary>
        /// Reads a file and stores all of the read commands into a list, before executing the commands
        /// </summary>
        /// <param name="fileName">What file, put null if hardcode true</param>
        /// <param name="hardCode">Should the program hardcode the commands</param>
        public static void Read(string fileName, bool hardCode)
        {
            Interpreter interpreter = new Interpreter();

            if (!hardCode)
            {
                StreamReader reader = new StreamReader(fileName);

                while (!reader.EndOfStream)
                {
                    //Add the command
                    interpreter.AddCommand(reader.ReadLine());
                }
            }
            else
            {
                //Load this sample program into the command list
                interpreter.AddCommand("draw_title No program loaded");
                interpreter.AddCommand("draw_text No program loaded");
            }
            //Run all program that have been stored into the list
            interpreter.Run();
        }
    }
}