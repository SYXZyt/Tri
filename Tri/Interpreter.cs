using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Tri
{
    class Interpreter
    {
        List<string> commands = new List<string>();
        Dictionary<string, string> variables = new Dictionary<string, string>();
        Dictionary<string, int> labels = new Dictionary<string, int>();
        public StreamWriter writer;
        public int lineNumber;

        string ReadLineOfFile(string fileName, int targetLine)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                for (int i = 1; i < targetLine; i++)
                {
                    sr.ReadLine();
                }
                return sr.ReadLine();
            }
        }

        public float Lerp(float start, float end, float value)
        {
            return start * (1 - value) + end * value;
        }

        public void AddCommand(string command)
        {
            commands.Add(command);
        }

        void CheckForLabels()
        {
            int num = 0;
            foreach (string command in commands)
            {
                string[] tokens = command.Split(' ');

                if (tokens[0].StartsWith(":"))
                {
                    try
                    {
                        labels.Add(tokens[0], num);
                    }
                    catch
                    {
                        Error error = new TRI_COMMAND_FAILED($"Label already exists - Line {num+1}");
                    }
                }
                num++;
            }
        }

        bool ExecuteCondition(string[] tokens)
        {
            switch (tokens[2])
            {
                case "EQU": return tokens[1] == tokens[3];
                case "NEQ": return tokens[1] != tokens[3];
                case "GRT": return int.Parse(tokens[1]) > int.Parse(tokens[3]);
                case "EGR": return int.Parse(tokens[1]) >= int.Parse(tokens[3]);
                case "LWR": return int.Parse(tokens[1]) < int.Parse(tokens[3]);
                case "ELR": return int.Parse(tokens[1]) <= int.Parse(tokens[3]);
                default:    return tokens[1] == tokens[3];
            }
        }

        public void Run()
        {
            CheckForLabels();

            while (lineNumber < commands.Count)
            {
                if (lineNumber == commands.Count)
                {
                    return;
                }
                if (!ExecuteCommand(commands[lineNumber]))
                {
                    Error error = new TRI_COMMAND_FAILED($"The command failed to execute: {commands[lineNumber]}");
                }
                lineNumber++;
            }
        }

        public ConsoleColor GetColour(string colour)
        {
            colour = colour.Remove(0, 3);

            switch(colour)
            {
                case "White": return ConsoleColor.White;
                case "Black": return ConsoleColor.Black;
                case "Red": return ConsoleColor.Red;
                case "Green": return ConsoleColor.Green;
                case "Blue": return ConsoleColor.Blue;
                case "Yellow": return ConsoleColor.DarkYellow;
                case "Cyan": return ConsoleColor.Cyan;
                case "Magenta": return ConsoleColor.Magenta;

                default: return ConsoleColor.White;
            }
        }

        public string GetVariable(string variableName)
        {
            if (variables.ContainsKey(variableName))
            {
                string var = variables[variableName];
                return var;
            }
            else
            {
                Error error = new TRI_VARIABLE_DOES_NOT_EXIST($"{variableName} does not exist", lineNumber);
                return "";
            }
        }

        public void SetVariable(string variableName, string value)
        {
            //If infinity is set
            if (value == "∞")
            {
                //Most likely caused by a division by 0 error, so throw that error
                Error error = new TRI_DIVISION_BY_ZERO(lineNumber);
            }

            if (variables.ContainsKey(variableName))
            {
                variables[variableName] = value;
            }
            else
            {
                variables.Add(variableName, value);
            }
        }

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
                if (tokens[i].StartsWith("var"))
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

                if (tokens[i] == "_PI_")
                {
                    tokens[i] = 3.141592653589793.ToString();
                }

                if (tokens[i] == "_CURR_X_")
                {
                    tokens[i] = Console.CursorLeft.ToString();
                }

                if (tokens[i] == "_CURR_Y_")
                {
                    tokens[i] = Console.CursorTop.ToString();
                }

                if (tokens[i] == "_SCREEN_WID_")
                {
                    tokens[i] = Console.WindowWidth.ToString();
                }

                if (tokens[i] == "_SCREEN_HEI_")
                {
                    tokens[i] = Console.WindowHeight.ToString();
                }

                if (tokens[i] == "_BLANK_")
                {
                    tokens[i] = " ";
                }
            }

            try
            {
                switch (tokens[0])
                {
                    case "file_del":
                        {
                            File.Delete(tokens[1]);
                        }
                        return true;
                    case "file_read":
                        {
                            SetVariable(tokens[1], ReadLineOfFile(tokens[2], int.Parse(tokens[3])));
                        }
                        return true;
                    case "file_write":
                        {
                            writer = new StreamWriter(tokens[1]);
                            string x = "";
                            for (int i = 2; i < tokens.Length; i++)
                            {
                                x += tokens[i];
                                if (i < tokens.Length - 1)
                                {
                                    x += " ";
                                }
                            }
                            writer.WriteLine(x);

                            writer.Close();
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
                            Console.WriteLine(x);
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
                            Console.WriteLine(x);
                            int y = (int)Console.ReadKey().Key;
                            SetVariable(tokens[1], y.ToString());
                        }
                        return true;
                    case "goto":
                        {
                            if (!labels.ContainsKey(tokens[1]))
                            {
                                Error error;
                                if (!tokens[1].StartsWith(":"))
                                {
                                    error = new TRI_LABEL_NOT_FOUND($"{tokens[1]} does not fit the label format, therefore it", lineNumber);
                                }
                                else
                                {
                                    error = new TRI_LABEL_NOT_FOUND(tokens[1], lineNumber);
                                }
                            }
                            else
                            {
                                lineNumber = labels[tokens[1]];
                            }
                        }
                        return true;
                    case "break":
                        { }
                        return true;
                    default:
                        {
                            Error error = new TRI_COMMAND_FAILED($"{command} is not a recognised command", lineNumber);
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