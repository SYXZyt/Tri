using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Tri
{
    struct Coord
    {
        public int X
        {
            get;
            set;
        }
        public int Y
        {
            get;
            set;
        }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    class Interpreter
    {
        readonly List<string> commands = new List<string>(); //Stores all commands
        readonly Dictionary<string, string> variables = new Dictionary<string, string>(); //Stores all variables the program is using
        readonly Dictionary<string, int> labels = new Dictionary<string, int>(); //Stores a list of all labels in the program
        public StreamWriter writer; //Used for reading data
        public StreamReader reader;
        public int lineNumber; //Current line number

        public enum Direction
        {
            U,
            UR,
            R,
            DR,
            D,
            DL,
            L,
            UL
        }

        /// <summary>
        /// Read the specific line of a text file
        /// </summary>
        /// <param name="fileName">Path of file to read</param>
        /// <param name="targetLine">What line to read</param>
        /// <returns>Text stored on that line</returns>
        string ReadLineOfFile(int targetLine)
        {
            StreamReader sr = reader;
            for (int i = 1; i < targetLine; i++)
            {
                sr.ReadLine();
            }
            return sr.ReadLine();
        }

        /// <summary>
        /// Calculates lerp
        /// </summary>
        /// <param name="start">Start value</param>
        /// <param name="end">Target value</param>
        /// <param name="value">How far, think of speed if working with translations</param>
        /// <returns></returns>
        public float Lerp(float start, float end, float value)
        {
            return start * (1 - value) + end * value;
        }

        /// <summary>
        /// Add a command
        /// </summary>
        /// <param name="command">commandToAdd</param>
        public void AddCommand(string command)
        {
            commands.Add(command);
        }

        /// <summary>
        /// Check every line, and add labels to the list if a label found. This is to allow to jump to labels, even if the program has not gotten that far
        /// </summary>
        void CheckForLabels()
        {
            //Loop for every command in the file, in order to check it
            int num = 0;
            foreach (string command in commands)
            {
                string[] tokens = command.Split(' ');

                //Only bother checking the first token, to avoid goto commands being added to the list
                if (tokens[0].StartsWith(":"))
                {
                    try
                    {
                        labels.Add(tokens[0], num);
                    }
                    catch
                    {
                        //Throw an error if a label already exists
                        Error error = new TRI_COMMAND_FAILED($"Label already exists - Line {num+1}");
                    }
                }
                num++;
            }
        }

        /// <summary>
        /// Execute the condition, in the if statement
        /// </summary>
        /// <param name="tokens">Split command to check</param>
        /// <returns>True if the statement is true, false otherwise</returns>
        bool ExecuteCondition(string[] tokens)
        {
            switch (tokens[2])
            {
                //Check the command, and return a different value, based on the operator
                case "==": return tokens[1] == tokens[3];
                case "!=": return tokens[1] != tokens[3];
                case ">": return int.Parse(tokens[1]) > int.Parse(tokens[3]);
                case ">=": return int.Parse(tokens[1]) >= int.Parse(tokens[3]);
                case "<": return int.Parse(tokens[1]) < int.Parse(tokens[3]);
                case "<=": return int.Parse(tokens[1]) <= int.Parse(tokens[3]);
                default:
                    string temp = "";
                    foreach (string x in tokens)
                    {
                        temp += (x + " ");
                    }
                    _ = new TRI_OPERATOR_NOT_RECOGNISED(temp, lineNumber);
                    return false;
            }
        }

        /// <summary>
        /// Run all of the commands stored in memory
        /// </summary>
        public void Run()
        {
            //Check to see if the program has any labels
            CheckForLabels();

            while (lineNumber < commands.Count)
            {
                if (lineNumber == commands.Count)
                {
                    return;
                }
                if (!ExecuteCommand(commands[lineNumber]))
                {
                    //Throw an error, if the command failed for some reason
                    Error error = new TRI_COMMAND_FAILED($"The command failed to execute: {commands[lineNumber]}");
                }
                lineNumber++;
            }
        }

        /// <summary>
        /// Converts a Tri colour const into a console colour
        /// </summary>
        /// <param name="colour">Tri colour</param>
        /// <returns>ConsoleColor</returns>
        public ConsoleColor GetColour(string colour)
        {
            //Get rid of cl_
            colour = colour.Remove(0, 3);

            switch(colour)
            {
                case "White": return ConsoleColor.White;
                case "Black": return ConsoleColor.Black;
                case "Red": return ConsoleColor.Red;
                case "Green": return ConsoleColor.Green;
                case "Blue": return ConsoleColor.DarkBlue;
                case "Yellow": return ConsoleColor.DarkYellow;
                case "Cyan": return ConsoleColor.Cyan;
                case "Magenta": return ConsoleColor.Magenta;

                default: return ConsoleColor.White;
            }
        }

        /// <summary>
        /// Check to see if the variable exists, and return it
        /// </summary>
        /// <param name="variableName">Name of the variable to check</param>
        /// <returns>What value it contains</returns>
        public string GetVariable(string variableName)
        {
            //Check if the variable exists
            if (variables.ContainsKey(variableName))
            {
                //Return it
                string var = variables[variableName];
                return var;
            }
            else
            {
                //Throw an error if it does not exist
                Error error = new TRI_VARIABLE_DOES_NOT_EXIST($"{variableName} does not exist", lineNumber);
                return "";
            }
        }

        static Coord MoveBuffer(Coord coord, Direction dir)
        {
            switch (dir)
            {
                case Direction.U:
                    coord.X--;
                    break;
                case Direction.UR:
                    coord.Y--;
                    coord.X++;
                    break;
                case Direction.R:
                    coord.X++;
                    break;
                case Direction.DR:
                    coord.Y++;
                    coord.X++;
                    break;
                case Direction.D:
                    coord.Y++;
                    break;
                case Direction.DL:
                    coord.Y++;
                    coord.X--;
                    break;
                case Direction.L:
                    coord.X--;
                    break;
                case Direction.UL:
                    coord.Y--;
                    coord.X--;
                    break;
            }
            return coord;
        }

        /// <summary>
        /// Set a variable
        /// </summary>
        /// <param name="variableName">What the name is</param>
        /// <param name="value">What value to save</param>
        public void SetVariable(string variableName, string value)
        {
            //If infinity is set, since something has gone wrong, because infinity never needs to be set
            if (value == "∞")
            {
                //Most likely caused by a division by 0 error, so throw that error
                Error error = new TRI_DIVISION_BY_ZERO(lineNumber);
            }

            //Update the variable if it exists, otherwise create it
            if (variables.ContainsKey(variableName))
            {
                variables[variableName] = value;
            }
            else
            {
                variables.Add(variableName, value);
            }
        }

        Direction GetDirection(Coord s, Coord e)
        {
            if (e.Y < s.Y && e.X == s.X)
            {
                return Direction.U;
            }
            else if (e.Y < s.Y && e.X < s.X)
            {
                return Direction.UL;
            }
            else if (e.Y == s.Y && e.X < s.X)
            {
                return Direction.L;
            }
            else if (e.Y > s.Y && e.X < s.X)
            {
                return Direction.DL;
            }
            else if (e.Y > s.Y && e.X == s.X)
            {
                return Direction.D;
            }
            else if (e.Y > s.Y && e.X > s.X)
            {
                return Direction.DR;
            }
            else if (e.Y == s.Y && e.X > s.X)
            {
                return Direction.R;
            }
            else
            {
                return Direction.UR;
            }
        }

        /// <summary>
        /// Execute a command
        /// </summary>
        /// <param name="command">What command to run</param>
        /// <returns>True if the command ran correctly</returns>
        public bool ExecuteCommand(string command)
        {
            //Convert to array
            string[] tokens = command.Split(' ');

            //Skip if a comment, blank or a label
            if (tokens[0] == "rem" || tokens[0] == "" || tokens[0] == " " || tokens[0] == null || tokens[0].StartsWith(":"))
            {
                return true;
            }

            //Sub tokens, ie var,variable will be replaced with the value stored
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].StartsWith("$var"))
                {
                    string[] splitToken = tokens[i].Split(',');

                    tokens[i] = GetVariable(splitToken[1]);
                }

                if (tokens[i].Contains("_ENDL_"))
                {
                    tokens[i] = tokens[i].Replace("_ENDL_", Environment.NewLine);
                }

                if (tokens[i].StartsWith("cl_"))
                {
                    tokens[i] = GetColour(tokens[i]).ToString();
                }

                if (tokens[i].Contains("_PI_"))
                {
                    tokens[i] = tokens[i].Replace("_PI_", "3.141592653589793");
                }

                if (tokens[i].Contains("_CURR_X_"))
                {
                    tokens[i] = tokens[i].Replace("_CURR_X_", Console.CursorLeft.ToString());
                }

                if (tokens[i].Contains("_CURR_Y_"))
                {
                    tokens[i] = tokens[i].Replace("_CURR_Y_", Console.CursorTop.ToString());
                }

                if (tokens[i].Contains("_SCREEN_WID_"))
                {
                    tokens[i] = tokens[i].Replace("_SCREEN_WID_", Console.WindowWidth.ToString());
                }

                if (tokens[i].Contains("_SCREEN_HEI_"))
                {
                    tokens[i] = tokens[i].Replace("_SCREEN_HEI_", Console.WindowHeight.ToString());
                }

                if (tokens[i].Contains("_BLANK_"))
                {
                    tokens[i] = tokens[i].Replace("_BLANK_", " ");
                }

                if (tokens[i].Contains("_CURR_USER_"))
                {
                    tokens[i] = tokens[i].Replace("_CURR_USER_", Environment.UserName);
                }
            }

            try
            {
                switch (tokens[0])
                {
                    case "end":
                        {
                            Environment.Exit(0);
                        }
                        return true;
                    case "help":
                        {
                            Console.WriteLine("Non available");
                        }
                        return true;
                    case "dir":
                        {
                            Console.WriteLine(Directory.GetCurrentDirectory());
                        }
                        return true;
                    case "cd":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            Directory.SetCurrentDirectory(x);
                        }
                        return true;
                    case "ls":
                        {
                            Console.WriteLine("=== Listing File ===");
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            StreamReader y = new StreamReader(x);
                            while (!y.EndOfStream)
                            {
                                Console.WriteLine(y.ReadLine());
                            }
                            y.Close();

                        }
                        return true;
                    case "file_del":
                        {
                            File.Delete(tokens[1]);
                        }
                        return true;
                    case "file_open_write":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            writer = new StreamWriter(x);
                        }
                        return true;
                    case "file_open_read":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            reader = new StreamReader(x);
                        }
                        return true;
                    case "file_read":
                        {
                            SetVariable(tokens[1], ReadLineOfFile(int.Parse(tokens[2])));
                        }
                        return true;
                    case "file_write":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            writer.WriteLine(x);
                        }
                        return true;
                    case "file_close_write":
                        {
                            writer.Close();
                        }
                        return true;
                    case "file_close_read":
                        {
                            reader.Close();
                        }
                        return true;
                    case "file_exist":
                        {
                            bool doesExist = File.Exists(tokens[2]);
                            if (doesExist)
                            {
                                SetVariable(tokens[1], "1");
                            }
                            else
                            {
                                SetVariable(tokens[1], "0");
                            }
                        }
                        return true;
                    case "exc":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            ExecuteCommand(x);
                        }
                        return true;
                    case "draw_line":
                        {
                            //Calculate direction
                            Direction dir;

                            Coord s = new Coord(int.Parse(tokens[1]), int.Parse(tokens[2]));
                            Coord e = new Coord(int.Parse(tokens[3]), int.Parse(tokens[4]));
                            dir = GetDirection(s, e);
                            float xDif = e.X - s.X;
                            float yDif = e.Y - s.Y;
                            int lineLength = (int)Math.Sqrt((xDif * xDif) + (yDif * yDif));
                            Coord currentCursorPos = new Coord(Console.CursorLeft, Console.CursorTop);
                            Coord buffer = s;
                            for (int i = 0; i < lineLength; i++)
                            {
                                Console.SetCursorPosition(buffer.X, buffer.Y);
                                Console.Write(tokens[5]);
                                buffer = MoveBuffer(buffer, dir);
                            }
                            Console.SetCursorPosition(currentCursorPos.X, currentCursorPos.Y);
                        }
                        return true;
                    case "run":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            Process.Start(x);
                            
                        }
                        return true;
                    case "inc":
                        {
                            int x = int.Parse(GetVariable(tokens[1])) + 1;
                            SetVariable(tokens[1], x.ToString());
                        }
                        return true;
                    case "dec":
                        {
                            int x = int.Parse(GetVariable(tokens[1])) - 1;
                            SetVariable(tokens[1], x.ToString());
                        }
                        return true;
                    case "sleep":
                        {
                            System.Threading.Thread.Sleep(int.Parse(tokens[1]));
                        }
                        return true;
                    case "if":
                        {
                            
                            string newCommand = "";
                            for (int i = 4; i < tokens.Length; i++)
                            {
                                newCommand += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    newCommand += " ";
                                }
                            }
                            bool returned = ExecuteCondition(tokens);
                            if (returned) 
                            {
                                ExecuteCommand(newCommand);
                            }
                        }
                        return true;
                    case "set":
                        {
                            string x = "";
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            SetVariable(tokens[1], x);
                        }
                        return true;
                    case "lerp":
                        {
                            SetVariable(tokens[4], Lerp(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3])).ToString());
                        }
                        return true;
                    case "mod":
                        {
                            float val = float.Parse(tokens[1]);
                            float mod = float.Parse(tokens[2]);
                            val %= mod;
                            SetVariable(tokens[3], val.ToString());
                        }
                        return true;
                    case "min":
                        {
                            float val = float.Parse(tokens[1]);
                            float min = float.Parse(tokens[2]);
                            val = Math.Min(val, min);
                            SetVariable(tokens[3], val.ToString());
                        }
                        return true;
                    case "max":
                        {
                            float val = float.Parse(tokens[1]);
                            float min = float.Parse(tokens[2]);
                            val = Math.Max(val, min);
                            SetVariable(tokens[3], val.ToString());
                        }
                        return true;
                    case "pow":
                        {
                            float val = float.Parse(tokens[1]);
                            float power = float.Parse(tokens[2]);
                            SetVariable(tokens[3], Math.Pow(val, power).ToString());
                        }
                        return true;
                    case "clamp":
                        {
                            float val = float.Parse(tokens[1]);
                            float min = float.Parse(tokens[2]);
                            float max = float.Parse(tokens[3]);
                            if (val < min)
                            {
                                val = min;
                            }
                            else if (val > max)
                            {
                                val = max;
                            }
                            SetVariable(tokens[4], val.ToString());
                        }
                        return true;
                    case "sin":
                        {
                            float val = (float)Math.Sin(float.Parse(tokens[1]));
                            SetVariable(tokens[2], val.ToString());
                        }
                        return true;
                    case "cos":
                        {
                            float val = (float)Math.Cos(float.Parse(tokens[1]));
                            SetVariable(tokens[2], val.ToString());
                        }
                        return true;
                    case "tan":
                        {
                            float val = (float)Math.Tan(float.Parse(tokens[1]));
                            SetVariable(tokens[2], val.ToString());
                        }
                        return true;
                    case "sqr":
                        {
                            float val = float.Parse(tokens[1]) * float.Parse(tokens[1]);
                            SetVariable(tokens[2], val.ToString());
                        }
                        return true;
                    case "srt":
                        {
                            float val = (float)Math.Sqrt(float.Parse(tokens[1]));
                            SetVariable(tokens[2], val.ToString());
                        }
                        return true;
                    case "add":
                        {
                            float sum = float.Parse(tokens[1]) + float.Parse(tokens[2]);
                            string str = sum.ToString();
                            SetVariable(tokens[3], str);
                        }
                        return true;
                    case "sub":
                        {
                            float sum = int.Parse(tokens[1]) - float.Parse(tokens[2]);
                            string str = sum.ToString();
                            SetVariable(tokens[3], str);
                        }
                        return true;
                    case "mul":
                        {
                            float sum = float.Parse(tokens[1]) * float.Parse(tokens[2]);
                            string str = sum.ToString();
                            SetVariable(tokens[3], str);
                        }
                        return true;
                    case "div":
                        {
                            float sum = float.Parse(tokens[1]) / float.Parse(tokens[2]);
                            string str = sum.ToString();
                            SetVariable(tokens[3], str);
                        }
                        return true;
                    case "draw_set_colour_fore":
                        {
                            Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tokens[1]);
                        }
                        return true;
                    case "draw_set_colour_back":
                        {
                            Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), tokens[1]);
                        }
                        return true;
                    case "draw_clear":
                        {
                            Console.Clear();
                        }
                        return true;
                    case "draw_title":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i] + " ";
                            }
                            Console.Title = x;
                        }
                        return true;
                    case "draw_text":
                        {
                            string x = "";
                            for (int i = 1; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            Console.Write(x);
                        }
                        return true;
                    case "draw_char":
                        {
                            int val = int.Parse(tokens[1]);
                            Console.Write((char)val);
                        }
                        return true;
                    case "draw_set_position":
                        {
                            Console.SetCursorPosition(int.Parse(tokens[1]), int.Parse(tokens[2]));
                        }
                        return true;
                    case "_VES_INFO_":
                        {
                            VersionInfo versionInfo = new VersionInfo();
                            Console.WriteLine($"Tri {versionInfo.Major}.{versionInfo.Minor} " +
                            $"(v{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Patch}, " +
                            $"{versionInfo.BuildTime.Day} {versionInfo.MonthName()} {versionInfo.BuildTime.Year}, " +
                            $"{versionInfo.BuildTime.Hour}:{versionInfo.BuildTime.Minute}:{versionInfo.BuildTime.Second})");
                        }
                        return true;
                    case "exit":
                        {
                            Environment.Exit(0);
                        }
                        return true;
                    case "input_line":
                        {
                            string x = "";
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            Console.Write(x);
                            SetVariable(tokens[1], Console.ReadLine());
                        }
                        return true;
                    case "input_char":
                        {
                            string x = "";
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            Console.Write(x);
                            int y = (int)Console.ReadKey().Key;
                            SetVariable(tokens[1], y.ToString());
                        }
                        return true;
                    case "goto":
                        {
                            if (!labels.ContainsKey(tokens[1]))
                            {
                                _ = new TRI_LABEL_NOT_FOUND(tokens[1], lineNumber);
                            }
                            else
                            {
                                if (tokens[1].StartsWith(":"))
                                {
                                    lineNumber = labels[tokens[1]];
                                }
                                else
                                {
                                    lineNumber = int.Parse(tokens[1]);
                                }
                            }
                        }
                        return true;
                    default:
                        {
                            _ = new TRI_COMMAND_FAILED($"{command} is not a recognised command", lineNumber);
                        }
                        return false;
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }

        public Interpreter()
        {
            lineNumber = 0;
        }
    }
}