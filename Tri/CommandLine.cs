using System;
using System.Globalization;

namespace Tri
{
    class VersionInfo
    {
        public string Major { get; protected set; }
        public string Minor { get; protected set; }
        public string Patch { get; protected set; }
        public DateTime BuildTime { get; protected set; }

        public string MonthName()
        {
            switch(BuildTime.Month)
            {
                case 1: return "Jan";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Apr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Aug";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                case 12: return "Dec";
                default: return "Invalid Month";
            }
        }

        public VersionInfo()
        {
            Major = "1";
            Minor = "0";
            Patch = "3";
            BuildTime = new DateTime(2021, 3, 19, 17, 57, 10);
            
        }
    }

    class CommandLine
    {
        public static void MainLoop()
        {
            Interpreter interpreter = new Interpreter();

            VersionInfo versionInfo = new VersionInfo();

            Console.WriteLine($"Tri {versionInfo.Major}.{versionInfo.Minor} " +
                $"(v{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}, " +
                $"{versionInfo.BuildTime.Day} {versionInfo.MonthName()} {versionInfo.BuildTime.Year}, " +
                $"{versionInfo.BuildTime.Hour}:{versionInfo.BuildTime.Minute}:{versionInfo.BuildTime.Second})");
            Console.WriteLine("Type help for more info");
            
            while (true)
            {
                Console.Write(">>> ");
                string input = Console.ReadLine();
                string temp = input.ToLower();
                interpreter.ExecuteCommand(input);
            }
        }
    }
}
