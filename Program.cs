using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.Speech.Synthesis;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using static Lunariens_Mental_Math_Trainer.Modes;

namespace Lunariens_Mental_Math_Trainer
{
    public enum Modes
    {
        Text = 0,
        Speech = 1
    }
    internal class Program
    {
        public static Modes mode;

        public static readonly Dictionary<string, string> rootMap = new Dictionary<string, string>()
        {
            { "3", "cube root of " },
            { "4", "fourth root of " },
            { "5", "fifth root of " },
            { "6", "sixth root of " },
            { "7", "seventh root of " },
            { "8", "eighth root of " },
            { "9", "ninth root of " }
        };

        public class DigitCode
        {
            //variables that store digit amounts of X and Y, the operation and optionally the decimal digits
            public int DigitsX;
            public int DigitsY;
            public char Operation;
            public int Decimals;
            public void Get()
            {
                char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                char[] operations = { '+', '-', '*', '/', '^', 'R' };

                Console.WriteLine("Enter a digit code. A digit code specifies the problem type. Enter \"help\" to get more info.");

                while (true)
                {
                    Console.Write("Digit code: ");
                    string usrDigitCode = Console.ReadLine();
                    if (usrDigitCode.Length == 3 && !usrDigitCode.Contains('/') && !usrDigitCode.Contains('R')) //then check if the first and the third symbol are numbers and the second is an operation.
                    {
                        if (numbers.Contains(usrDigitCode[0]) & operations.Contains(usrDigitCode[1]) & numbers.Contains(usrDigitCode[2]))
                        {
                            DigitsX = int.Parse(usrDigitCode[0].ToString());
                            Operation = usrDigitCode[1];
                            DigitsY = int.Parse(usrDigitCode[2].ToString());
                            return;
                        }
                        Console.WriteLine("Invalid digit code format.");
                    } //if decimals are specified, check if fifth symbol is a valid number and whether there is a dot symbol before it.
                    else if (usrDigitCode.Length == 5)
                    {
                        if (numbers.Contains(usrDigitCode[0]) & operations.Contains(usrDigitCode[1]) & numbers.Contains(usrDigitCode[2]) & usrDigitCode[3] == '.' & numbers.Contains(usrDigitCode[4]))
                        {
                            DigitsX = int.Parse(usrDigitCode[0].ToString());
                            Operation = usrDigitCode[1];
                            DigitsY = int.Parse(usrDigitCode[2].ToString());
                            Decimals = int.Parse(usrDigitCode[4].ToString());
                            return;
                        }
                        else
                            Console.WriteLine("Invalid digit code format.");
                    }
                    else if (usrDigitCode == "help")
                    {
                        Console.WriteLine("A digit code looks like the following:");
                        Console.WriteLine("XopY.Z");
                        Console.WriteLine("X and Y represent the number of digits for the first and second (random) number, respectively.");
                        Console.WriteLine("op means operation, and can be one of the following: + - * / ^ R");
                        Console.WriteLine("the ^ operator makes Y, the second number, represent the actual typed number.\nThis has to be a whole number.");
                        Console.WriteLine("So if it was 2, then it is 2 and not a random 2 digit number.");
                        Console.WriteLine("The R operator means root, and makes X represent the base of the root.");
                        Console.WriteLine("So if X was 3, then the operation is a cube root.");
                        Console.WriteLine("The last two symbols are required for / and R operations.");
                        Console.WriteLine("They represent the amount of decimal digits needed in the result.");
                        Console.WriteLine("The dot is required when specifying the amount of decimal digits. Z is the amount of decimals.");
                    }
                    else if (usrDigitCode == "exit")
                    {
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid digit code format. (Did you forget a decimal point?)");
                    }
                }
            }
            public override string ToString() => Decimals == 0 ? $"{DigitsX}{Operation}{DigitsY}" : $"{DigitsX}{Operation}{DigitsY}.{Decimals}";
        }
        
        public static int GetMode()
        {
            Console.WriteLine("Choose a mode:");
            Console.WriteLine("0 - Text mode");
            Console.WriteLine("1 - Speech mode");
            while (true)
            {
                string input = Console.ReadLine();
                Console.Write("Mode: ");
                if (!int.TryParse(input, out int i)) Console.WriteLine("Invalid mode.");
                else if (input == "exit") return -1;
                else return i;
            }
        }


        public static SpeechSynthesizer GetUSVoice()
        {
            SpeechSynthesizer synth = new();
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                if (voice.VoiceInfo.Culture.Name.Equals("en-US"))
                {
                    synth.SelectVoice(voice.VoiceInfo.Name);
                    return synth;
                }
            }
            Console.WriteLine("Unable to find en-US voice. Using default voice (not recommended at all unless your system language is english).");
            return synth;
        }

        
        public static long RandomLong(long bottom, long top)
        {
            Random randomness = new();
            ulong rangeSize = (ulong)(top - bottom);

            byte[] buf = new byte[8];
            randomness.NextBytes(buf);
            ulong result = (BitConverter.ToUInt64(buf, 0)%rangeSize)+(ulong)bottom;
            return (long)result;
        }
        
        public static string AddCommas(string number)
        {
            if (number.Length <= 3)
                return number;
            for (int i = number.Length-3; i >= 0; i -= 3)
            {
                number = number.Insert(i, ",");
            }
            return number;
        }
        
        public static void OutputProblem(string problem, SpeechSynthesizer synth)
        {
            
            if (mode == 0) //text mode
            {
                Console.WriteLine(problem);
            }
            if (mode is Speech) //speech mode
            {
                
                char op = char.Parse(Regex.Replace(problem, @"[\d\n]", string.Empty));
                
                problem = Regex.Replace(problem, @"[*^/+\-]", " ");
                problem = Regex.Replace(problem, @"√", "");
                string[] numbers;
                if (op == '√')
                {
                    numbers = problem.Split(" ");
                }
                else
                {
                    numbers = problem.Split(" \n");
                }

                for (int i = 0; i < numbers.Length; i++)
                {
                    numbers[i] = AddCommas(numbers[i]);
                }
                
                switch (op)
                {
                    case '+':
                        problem = string.Join(" plus ", numbers);
                        synth.Speak(problem);
                        break;
                    case '-':
                        problem = string.Join(" minus ", numbers);
                        synth.Speak(problem);
                        break;
                    case '*':
                        problem = string.Join(" times ", numbers);
                        synth.Speak(problem);
                        break;
                    case '/':
                        problem = string.Join(" divided by ", numbers);
                        synth.Speak(problem);
                        break;
                    case '^':
                        problem = string.Join(" to the power of ", numbers);
                        synth.Speak(problem);
                        break;
                    case '√':
                        if (numbers.Length == 1)
                        {
                            problem = "square root of " + numbers[0];
                        }
                        else if (numbers.Length == 2)
                        {
                            if (!rootMap.TryGetValue(numbers[0], out var root))
                            {
                                Console.WriteLine("An error occured during processing of the problem for speech output.");
                            }
                            problem = root + numbers[1];
                        }
                        synth.Speak(problem);
                        break;
                    default:
                        Console.WriteLine("An error occured during processing of the problem for speech output.");
                        break;
                }
            }
        }
        
        public static void OpenStatistic(string digitCode, Modes mode)
        {
            if (digitCode.Contains('/'))
            {
                digitCode = digitCode.Replace('/', '÷');
            }
            else if (digitCode.Contains('*'))
            {
                digitCode = digitCode.Replace('*', 'x');
            }
            
            string path = $@"./stats/{digitCode}m{(int)mode}.csv";
            if (!File.Exists(path))
            {
                Console.WriteLine("No statistic found for the selected digit code.");
                return;
            }

            var reader = new StreamReader(path);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            
                var records = csv.GetRecords<Statistic>();
                var plt = new Plot(900, 600);

                List<double> solveTimes = records.Select(record => Math.Round(record.SolveTime, 3)).ToList();

                var signal = plt.AddSignal(solveTimes.ToArray());
                signal.LineWidth = 2;
                signal.MarkerSize = 0;
                signal.Color = System.Drawing.ColorTranslator.FromHtml("#bf616a");

                var modeName = mode.ToString();
                
                plt.YAxis.SetBoundary(0, 1.1 * solveTimes.Max());
                plt.XAxis.SetBoundary(-0.5, solveTimes.Count+0.5);
                plt.Title("Solve times for " + digitCode + " (mode " + modeName + ")");
                plt.YLabel("Solve time (s)");
                
                plt.XAxis.Ticks(false);
                plt.XAxis.Line(false);
                plt.YAxis2.Line(false);
                plt.XAxis2.Line(false);
                
                plt.Palette = ScottPlot.Palette.OneHalfDark;
                plt.Style(ScottPlot.Style.Gray1);
                var bnColor = System.Drawing.ColorTranslator.FromHtml("#2e3440");
                plt.Style(figureBackground: bnColor, dataBackground: bnColor);

                
                var pltWindow = new FormsPlotViewer(plt, 900, 600,
                    "Solve times for " + digitCode + " (mode " + modeName + ")");
                pltWindow.BackColor = bnColor;
                pltWindow.ShowDialog();
            
        }
        public class Statistic
        {
            public string Problem { get; set; }
            public string UsrSolution { get; set; }
            public double SolveTime { get; set; }
            public DateTime Date { get; set; }
            public bool Correctness { get; set; }
        }
        
        public static void InitStatistic(string digitCode, Modes mode)
        {
            if (digitCode.Contains('/'))
            {
                digitCode = digitCode.Replace("/", "÷");
            }
            else if (digitCode.Contains("*"))
            {
                digitCode = digitCode.Replace("*", "x");
            }
            string dirPath = "./stats";
            string path = $@"{dirPath}{digitCode}m{(int)mode}.csv";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            if (File.Exists(path)) return;
            

            StreamWriter sw = new StreamWriter(path, true);
            var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteHeader<Statistic>();
            csv.NextRecord();
        }

        public static void SaveStatistic(string statName, string problem, string usrSolution, double time, DateTime date, bool correctness)
        {
            string path = $@".\stats\{statName}m{(int)mode}.csv";
            path = path.Replace('*', 'x'); //replace * with x because windows doesn't allow * in file names
            //if path contains three slashes, remove the last one
            
            path = path.Replace("/", "÷");
            var record = new Statistic {Problem = problem.Replace("\n", ""), UsrSolution = usrSolution, SolveTime = time, Date = date, Correctness = correctness};
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };

            using (var stream = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecord<Statistic>(record);
                csv.NextRecord();
            }
        }
        
        public static void OpenTrainingScreen(Stopwatch stopWatch, IFormatProvider ifp, DigitCode digitCode, SpeechSynthesizer speechSynth)
        {
            Console.Clear();
            while (true)
            {
                // make random numbers corresponding to the number of digits in the digit code
                long xRangeBottom = Convert.ToInt64(Math.Round(Math.Pow(10, digitCode.DigitsX - 1), 0)); //the bottom of the range for X. it is 10^(DigitsX - 1).
                long xRangeTop = Convert.ToInt64(Math.Round(Math.Pow(10, digitCode.DigitsX), 0)); //the top of the range for X. it is 10^DigitsX
                long x = Math.Abs(RandomLong(xRangeBottom, xRangeTop));

                long yRangeBottom = Convert.ToInt64(Math.Round(Math.Pow(10, digitCode.DigitsY - 1), 0));
                long yRangeTop = Convert.ToInt64(Math.Round(Math.Pow(10, digitCode.DigitsY), 0));
                long y = Math.Abs(RandomLong(yRangeBottom, yRangeTop));

                // make a problem string with the random numbers and the operation
                string problem;
                if (digitCode.Operation == '^')
                {
                    problem = x.ToString() + digitCode.Operation + digitCode.DigitsY;
                }
                else if (digitCode.Operation == 'R')
                {
                    if (digitCode.DigitsX == 2)
                    {
                        problem = '√' + y.ToString();
                    }
                    else
                    {
                        problem = digitCode.DigitsX.ToString() + '√' + y;
                    }
                    
                }
                else
                {
                    problem = x.ToString() + digitCode.Operation + "\n" + y;
                }
    		    // precompute the correct solution
    		    long? intResult = null; //the null value indicates that the problem does not have a solution of this type. this is utilized later.
                decimal? decResult = null;
                switch (digitCode.Operation)
                {
                    case '+':
                        intResult = x + y;
                        break;
                    case '-':
                        if (digitCode.DigitsX == digitCode.DigitsY)
                        {
                            if (x < y)
                                intResult = y - x;
                            else
                                intResult = x - y;
                            break;
                        }
                        intResult = x - y;
                        break;
                        
                        
                    case '*':
                        intResult = x * y;
                        break;
                    case '/':
                        decResult = Math.Round(Convert.ToDecimal(x) / Convert.ToDecimal(y), digitCode.Decimals, MidpointRounding.ToZero);
                        break;
                    case '^':
                        intResult = (long)Math.Pow(x, digitCode.DigitsY);
                        break;
                    case 'R':
                        decResult = Math.Round((decimal)Math.Pow(Convert.ToDouble(y), 1 / Convert.ToDouble(digitCode.DigitsX)), digitCode.Decimals, MidpointRounding.ToZero);
                        break;
                    default: //should be impossible to reach here.
                        intResult = 0;
                        decResult = 0.0m;
                        break;
                }

                while (true)
                {
                    stopWatch.Reset();
                    OutputProblem(problem, speechSynth);
                    
                    Console.Write("Your result: ");

                    stopWatch.Start();
                    string usrResult = Console.ReadLine();
                    stopWatch.Stop();

                    // start verifying the answer
                    if (intResult != null && usrResult != "") //if the result is an int. in other words, if there is a result that is of type int.
                    {
                        if (long.TryParse(usrResult, out long _))
                        {
                            if (long.Parse(usrResult) == intResult) // if correct
                            {
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true);
                                
                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                Console.Clear();
                                Console.WriteLine("Wrong, correct was: " + intResult);
                                
                                break;
                            }
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid input, try again.");
                        }
                        
                    }
                    else if (decResult != null && usrResult != "") //if the result is a decimal
                    {
                        
                        if (decimal.TryParse(usrResult, NumberStyles.AllowDecimalPoint, ifp, out decimal _))
                        {
                            if (decResult == decimal.Parse(usrResult, CultureInfo.InvariantCulture))
                            {
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true);
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;
                                
                                break;
                            }  
                            else
                            {
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                Console.Clear();
                                Console.WriteLine("Wrong, correct was: " + decResult?.ToString(ifp));
                                
                                break;
                            }
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid input, try again.");
                            continue;
                        }
                            
                    }
                    else if (usrResult == "" && mode is Speech)
                    {
                        Console.Clear();
                        continue;
                    }
                    else if (decResult != null || intResult != null) // works like an else block, since at all times, at least one of the results has a value.
                    { //so why is this here?....
                        Console.Clear();
                        Console.WriteLine("Invalid result.");
                        break;
                    }

                    if (usrResult == "exit")
                    {
                        Console.Clear();
                        return;
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Stopwatch sw = new();

            IFormatProvider ifp = new CultureInfo("en-US");

            Console.ForegroundColor = ConsoleColor.White;

            SpeechSynthesizer synth = GetUSVoice();
            Console.WriteLine("Welcome to LMMT! (Lunarien's Mental Math Trainer)");

            while (true) //when the program starts. this loop will have the main menu with its functionality.
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Main menu:");
                Console.WriteLine("1) Start (infinite) training");
                Console.WriteLine("2) View a statistic file from a list");
                Console.WriteLine("3) View statistics for a specific problem type");
                Console.WriteLine("9) Exit LMMT");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Your choice: ");
                string usrChoice = Console.ReadLine();

                if (usrChoice == "1")
                {
                    Console.Clear();
                    mode = (Modes)GetMode(); //writing to the global variable mode. it is used for determining the output type. (text or speech)
                    
                    Console.Clear();
                    DigitCode usrDC = new();
                    usrDC.Get();
                    InitStatistic(usrDC.ToString(), mode);
                    OpenTrainingScreen(sw, ifp, usrDC, synth);
                }
                else if (usrChoice == "2") //TODO: Why is this not in the OpenStatistics inside the Statistic class
                {
                    string[] files = Array.Empty<string>();
                    //check if there any any files, if yes, then list them.             
                    if (Directory.Exists("stats")) files = Directory.GetFiles("stats");
                    else
                    {
                        Console.WriteLine("No statistics found. Press enter to continue.");
                        Console.ReadLine();
                    }
      


                    if (files.Length != 0)
                    {
                        Console.Clear();
                        Console.WriteLine("Existing statistics:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.WriteLine($"{i + 1}) {files[i][6..]}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Statistic to open: ");
                        string usrFileChoice = Console.ReadLine();

                        int fileMode;
                        
                        // retrieve the mode from the file name. This is used in the OpenStatistic method below.
                        if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                        {
                            if (files[int.Parse(usrFileChoice) - 1][6..].Length == 9) //length 9 comes from the digit code of length 5, including the mode specifier (m0 || m1) and then the file extension. (.csv)
                                fileMode = int.Parse(files[int.Parse(usrFileChoice) - 1][6..][4].ToString());
                            
                            else fileMode = int.Parse(files[int.Parse(usrFileChoice) - 1][6..][6].ToString());
                            
                        }
                        else if (usrFileChoice == "exit")
                        {
                            Console.Clear();
                            continue;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid choice, try again.");
                            Thread.Sleep(1000);
                            Console.Clear();
                            continue;
                        }
                        
                        if (int.TryParse(usrFileChoice, out int _))
                        {
                            if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                if (files[int.Parse(usrFileChoice) - 1].Substring(6).Length == 9)
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 3);
                                    OpenStatistic(statDigitCode, (Modes)fileMode);
                                }
                                else
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 5);
                                    OpenStatistic(statDigitCode, (Modes)fileMode);
                                }
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine("Invalid choice, try again.");
                            }
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Invalid choice, try again.");
                        }
                    }
                    Console.Clear();
                }
                else if (usrChoice == "3") //TODO: Why is this not in the OpenStatistics(Specific) inside the Statistic class
                {
                    Console.Clear();
                    int selectedMode = GetMode();
                    if (selectedMode == -1) //if user wants to exit
                    {
                        Console.Clear();
                        continue;
                    }

                    Console.Clear();
                    DigitCode usrDC = new();
                    usrDC.Get();
                    
                    OpenStatistic(usrDC.ToString(), (Modes)selectedMode);
                    Console.Clear();
                }
                else if (usrChoice == "9")
                {
                    Console.WriteLine("Exiting...");
                    Thread.Sleep(100);
                    Environment.Exit(0);
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Invalid choice, try again.");
                    Thread.Sleep(1000);
                }
                
            }
        }
    }
}