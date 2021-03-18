namespace Tri
{
    class Program
    {
        //TRI or Terribly wRitten Interpreted language is an interpreted language project. This is just for fun.
        //There is ZERO pratical use for the language, it's called terribly written, for a reason.
        //There are no lexers or parsers used, or anything that would make the language work well
        //Think of it as a bad interface between the user, and c#
        /// <summary>
        /// Entry point
        /// </summary>
        /// <param name="args">arguments passed by user</param>
        static void Main(string[] args)
        {
            //Check to see if any arguments
            if (args == null || args.Length == 0)
            {
                //If no arguments, run a built in program
                GetCommands.Read("", true);
            }
            else
            {
                //Else, run the file provided
                GetCommands.Read(args[0], false);
            }
        }
    }
}