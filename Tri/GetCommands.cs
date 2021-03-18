using System;
using System.IO;

namespace Tri
{
    class GetCommands
    {
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
                interpreter.AddCommand("draw_title No program loaded");
                interpreter.AddCommand("draw_text No program loaded");
            }
            interpreter.Run();
        }
    }
}