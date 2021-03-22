using System;
using System.IO;

namespace Tri
{
    class GetCommands
    {
        static StreamReader reader;

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
                try
                {
                    reader = new StreamReader(fileName);
                }
                catch
                {
                    _ = new TRI_FILE_UNREADABLE(fileName);
                }

                while (!reader.EndOfStream)
                {
                    //Add the command
                    interpreter.AddCommand(reader.ReadLine());
                }
            }
            else
            {
                //Go to the command line
                CommandLine.MainLoop();
            }
            //Run all program that have been stored into the list
            interpreter.Run();
        }
    }
}