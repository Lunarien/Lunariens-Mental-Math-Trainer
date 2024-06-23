using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Speech.Synthesis;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using ConsoleTables;
using static Lunariens_Mental_Math_Trainer.Modes;


namespace Lunariens_Mental_Math_Trainer
{
    public enum Modes
    {
        Exit = -1,
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
            public int DigitsX = 0;
            public int DigitsY = 0;
            public char Operation;
            public int Decimals = 0;

            private int[] ParseNumber(string digitCode)
            {
                char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                int[] parsedNumbers = new int[3];
                bool decimalFound = false;

                for (int i = 0; i < digitCode.Length; i++)
                {
                    if (numbers.Contains(digitCode[i]))
                    {
                        if (!decimalFound)
                        {
                            if (i == 0)
                            {
                                parsedNumbers[0] = int.Parse(digitCode[i].ToString());
                            }
                            else if (i == 1)
                            {
                                if (digitCode.Length > 3)
                                {
                                    parsedNumbers[0] = int.Parse(digitCode.Substring(0, 2));
                                    parsedNumbers[1] = int.Parse(digitCode.Substring(2, digitCode.Length - 3));
                                }
                                else
                                {
                                    parsedNumbers[0] = int.Parse(digitCode[0].ToString());
                                    parsedNumbers[1] = int.Parse(digitCode[2].ToString());
                                }
                            }
                            else if (i == 2)
                            {
                                parsedNumbers[1] = int.Parse(digitCode[i].ToString());
                            }
                        }
                        else
                        {
                            parsedNumbers[2] = int.Parse(digitCode.Substring(i, digitCode.Length - i));
                            break;
                        }
                    }
                    else if (digitCode[i] == '.')
                    {
                        decimalFound = true;
                    }
                }

                return parsedNumbers;
            }

            private char GetOperator(string digitCode)
            {
                char[] operations = { '+', '-', '*', '/', '^', 'R' };

                foreach (char operation in operations)
                {
                    if (digitCode.Contains(operation))
                    {
                        return operation;
                    }
                }

                return '\0';
            }

            public void Get()
            {
                char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                char[] operations = { '+', '-', '*', '/', '^', 'R' };

                Console.WriteLine("Enter a digit code. A digit code specifies the problem type. Enter \"help\" to get more info.");

                while (true)
                {
                    Console.Write("Digit code: ");
                    string usrDigitCode = Console.ReadLine();

                    if (usrDigitCode == "exit")
                    {
                        DigitsX = -1;
                        DigitsY = -1;
                        Operation = '\0';
                        return;
                    }

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
                        Console.ForegroundColor = ConsoleColor.Yellow;
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
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.WriteLine("Invalid digit code format. (Did you forget a decimal point? Did you type a two digit number somewhere?)");
                    }

                }
            }
            public override string ToString() => Decimals == 0 ? $"{DigitsX}{Operation}{DigitsY}" : $"{DigitsX}{Operation}{DigitsY}.{Decimals}";
        }

        public static void GoodConsoleClear()
        {
            Console.Clear(); // Clearing the console works goofily in case of Windows Terminal. This abomination is a workaround for that.
            Console.WriteLine("\f\u001bc\x1b[3J");
            Console.Clear();
        }

        public static Modes GetMode()
        {
            int[] possibleModes = { 0, 1 };
            Console.WriteLine("Choose a mode:");
            Console.WriteLine("0 - Text mode");
            Console.WriteLine("1 - Text to speech mode");
            Console.WriteLine("(Type \"exit\" to return to the main menu.)");
            while (true)
            {
                Console.Write("Mode: ");
                string input = Console.ReadLine();
                if (!int.TryParse(input, out int result))
                {
                    if (input != "exit")
                    {
                        Console.WriteLine("Error: Invalid mode");
                        continue;
                    }
                    return Exit;
                }
                else if (int.TryParse(input, out int number))
                    if (possibleModes.Contains(number))
                    {
                        Modes selectedMode = (Modes)number;
                        return selectedMode;
                    }
                    else
                    {
                        Console.WriteLine("Error: Invalid mode");
                        continue;
                    }
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
            Console.WriteLine("Unable to find en-US voice. Using default voice (not recommended at all unless your system language is some form of english).");
            return synth;
        }


        public static long RandomLong(long bottom, long top)
        {
            Random randomness = new();
            ulong rangeSize = (ulong)(top - bottom);

            byte[] buf = new byte[8];
            randomness.NextBytes(buf);
            ulong result = (BitConverter.ToUInt64(buf, 0) % rangeSize) + (ulong)bottom;
            return (long)result;
        }

        public static string AddCommas(string number)
        {
            if (number.Length <= 3)
                return number;
            for (int i = number.Length - 3; i >= 0; i -= 3)
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

                problem = Regex.Replace(problem, @"[*^/+\-]", " "); //put a space between numbers instead of the operator
                problem = Regex.Replace(problem, @"√", "");
                string[] numbers;
                if (op == '√' || op == '^') //split the problem into individual numbers
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
                        if (numbers[1] == "2")
                        {
                            problem = numbers[0] + " squared";
                        }
                        else if (numbers[1] == "3")
                        {
                            problem = numbers[0] + " cubed";
                        }
                        else
                        {
                            problem = string.Join(" to the power of ", numbers);
                        }
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
                        Console.WriteLine("An error occured during processing of the problem for speech output. This happened during switch of operations (dev note)");
                        Thread.Sleep(5000);
                        break;
                }
            }
        }

        public static void OpenStatisticGraph(string digitCode, Modes mode)
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

            var records = csv.GetRecords<Statistic>().ToList();
            var plt = new Plot(900, 600);

            List<double> solveTimes = records.Select(record => Math.Round(record.SolveTime, 3)).ToList();
            List<bool> solveCorrectnesses = records.Select(record => record.Correctness).ToList();

            var signal = plt.AddSignal(solveTimes.ToArray());
            signal.LineWidth = 2;
            signal.MarkerSize = 3;
            signal.Color = System.Drawing.ColorTranslator.FromHtml("#bf616a");

            
            for (int i = 0; i < solveCorrectnesses.Count; i++)
            {
                if (solveCorrectnesses[i] == false)
                {
                    var hSpan = plt.AddHorizontalSpan(i - 0.25, i + 0.25);
                    hSpan.Color = System.Drawing.Color.FromArgb(128, 191, 97, 106);
                    
                }
            }

            var modeName = mode.ToString();
            if (solveTimes.Count == 0)
            {
                return;
            }
            plt.YAxis.SetBoundary(0, 1.1 * solveTimes.Max());
            plt.XAxis.SetBoundary(-0.5, solveTimes.Count + 0.5);
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


            var pltWindow = new ScottPlot.FormsPlotViewer(plt, 900, 600, "Solve times for " + digitCode + " (mode " + modeName + ")");
            pltWindow.BackColor = bnColor;
            pltWindow.ShowDialog();
        }
        public static void OpenStatisticScreen(string digitCode, Modes mode)
        {
            if (digitCode.Contains("/")) //replace the slash with a different character so the file can be found. the file name cannot contain a slash.
            {
                digitCode = digitCode.Replace("/", "÷");
            }
            else if (digitCode.Contains("*")) //replace the asterisk with a different character so the file can be found. the file name cannot contain an asterisk.
            {
                digitCode = digitCode.Replace("*", "x");
            }

            string path = $@"./stats/{digitCode}m{(int)mode}.csv";
            if (!File.Exists(path))
            {
                Console.WriteLine("No statistic found for the selected digit code.");
                return;
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Statistic>();
                Statistic[] recordsArray = records.ToList().ToArray();
                // menu for viewing the statistics follows.

                //Print out the statistics in a table.
                ConsoleTable table = new ConsoleTable("Problem", "Your solution", "Solve time", "Date", "Correct?");
                foreach (Statistic record in recordsArray)
                {
                    string boolWord;
                    if (record.Correctness == true)
                    {
                        boolWord = "Yes";
                    }
                    else
                    {
                        boolWord = "No";
                    }

                    table.AddRow(record.Problem, record.UsrSolution, record.SolveTime.ToString() + "s", record.Date.ToString(), boolWord);
                }
                table.Write();
            }
            Console.Write("Press any key to go back to main menu...");
            Console.ReadKey();

            GoodConsoleClear();
            return;
        }
        public class Statistic
        {
            public string? Problem { get; set; }
            public string? UsrSolution { get; set; }
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
            string dirPath = "./stats/";
            string path = $@"{dirPath}{digitCode}m{(int)mode}.csv";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            if (File.Exists(path)) return;


            StreamWriter sw = new StreamWriter(path, true);
            var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteHeader<Statistic>();
            csv.NextRecord();
            csv.Flush();
            sw.Close();
        }

        public static void SaveStatistic(string statName, string problem, string usrSolution, double time, DateTime date, bool correctness)
        {
            string path = $@".\stats\{statName}m{(int)mode}.csv";
            path = path.Replace('*', 'x'); //replace * with x because windows doesn't allow * in file names

            path = path.Replace("/", "÷");
            var record = new Statistic { Problem = problem.Replace("\n", ""), UsrSolution = usrSolution, SolveTime = time, Date = date, Correctness = correctness };

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
            GoodConsoleClear();
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
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (mode == Modes.Speech)
                    {
                        Console.WriteLine("Type \"exit\" to return to the main menu. Enter nothing to repeat the problem");
                    }
                    else
                    {
                        Console.WriteLine("Type \"exit\" to return to the main menu.");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Your result: ");

                    stopWatch.Start();
                    string usrResult = Console.ReadLine();
                    stopWatch.Stop();

                    // truncate the result down to the number of decimals specified in the digit code, so that extra decimals don't cause the answer to be marked wrong
                    {
                        int decimalIndex = usrResult.IndexOf('.') + digitCode.Decimals + 1;

                        if (usrResult.Length > decimalIndex && usrResult.Contains('.'))
                        {
                            usrResult = usrResult.Substring(0, decimalIndex + digitCode.Decimals);
                        }
                    }

                    // start verifying the answer, checking whether it's a number or the exit command
                    if (usrResult == "exit") //check for the exit command first to combat issues.
                    {
                        GoodConsoleClear();
                        return;
                    }
                    else if (intResult != null && usrResult != "") //if the result is an int. in other words, if there is a result that is of type int.
                    {
                        if (long.TryParse(usrResult, out long _))
                        {
                            if (long.Parse(usrResult) == intResult) // if correct
                            {
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true);

                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                GoodConsoleClear();
                                Console.WriteLine("Wrong, correct was: " + intResult);

                                break;
                            }
                        }
                        else
                        {
                            GoodConsoleClear();
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
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;

                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCode.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                GoodConsoleClear();
                                Console.WriteLine("Wrong, correct was: " + decResult?.ToString(ifp));

                                break;
                            }
                        }
                        else
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Invalid input, try again.");
                            continue;
                        }

                    }
                    else if (usrResult == "" && mode is Speech)
                    {
                        GoodConsoleClear();
                        continue;
                    }
                    else if (decResult != null || intResult != null) // works like an else block, since at all times, at least one of the results has a value.
                    { //so why is this here?....
                        GoodConsoleClear();
                        Console.WriteLine("Invalid result.");
                        break;
                    }

                }
            }
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Stopwatch sw = new();

            IFormatProvider ifp = new CultureInfo("en-US");

            Console.ForegroundColor = ConsoleColor.White;


            SpeechSynthesizer synth = GetUSVoice();
            Console.WriteLine("Welcome to LMMT! (Lunarien's Mental Math Trainer)");

            while (true) //when the program starts. this loop will ensure the existence of the main menu with its functionality.
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Main menu:");
                Console.WriteLine("1) Start (infinite) training");
                Console.WriteLine("2) View a statistic graph from a list");
                Console.WriteLine("3) View a statistic graph for a specific problem type");
                Console.WriteLine("4) View console statistic from a list");
                Console.WriteLine("5) View console statistic for a specific problem type");
                Console.WriteLine("9) Exit LMMT");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Your choice: ");
                string usrChoice = Console.ReadLine();

                if (usrChoice == "1")
                {
                    GoodConsoleClear();
                    mode = GetMode(); //writing to the global variable mode. it is used for determining the output type. (text or speech)
                    if (mode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }
                    GoodConsoleClear();
                    DigitCode usrDC = new();
                    usrDC.Get();

                    if (usrDC.DigitsX == -1 && usrDC.DigitsY == -1 && usrDC.Operation == '\0') //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }
                    InitStatistic(usrDC.ToString(), mode);
                    OpenTrainingScreen(sw, ifp, usrDC, synth);
                }
                else if (usrChoice == "2")
                {
                    string[] files = Array.Empty<string>();
                    //check if there are any files, if yes, then list them.
                    if (Directory.Exists("./stats")) files = Directory.GetFiles("stats");


                    bool isGotoUsed = false;
                    menuChoice2Start: //label for the goto statement later on.
                    if (files.Length != 0)
                    {
                        GoodConsoleClear();
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
                            GoodConsoleClear();
                            continue;
                        }
                        else
                        {
                            GoodConsoleClear();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Invalid choice, try again.");
                            Console.ForegroundColor = ConsoleColor.White;
                            if (!isGotoUsed)
                            {
                                isGotoUsed = true;
                                goto menuChoice2Start;
                            }
                            goto menuChoice2Start;
                        }

                        if (int.TryParse(usrFileChoice, out int _))
                        {
                            if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                if (files[int.Parse(usrFileChoice) - 1].Substring(6).Length == 9)
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 3);
                                    OpenStatisticGraph(statDigitCode, (Modes)fileMode);
                                }
                                else
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 5);
                                    OpenStatisticGraph(statDigitCode, (Modes)fileMode);
                                }
                                string[] statLines = File.ReadAllLines(files[int.Parse(usrFileChoice) - 1]);
                                if (statLines.Length == 1)
                                {
                                    GoodConsoleClear();
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("No statistics found inside the selected file.");
                                    Console.ForegroundColor = ConsoleColor.White;
                                    continue;
                                }
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.WriteLine("Invalid choice, try again.");
                            }
                        }
                        else
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Invalid choice, try again.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no statistic files. Go calculate!");
                    }

                    GoodConsoleClear();
                }
                else if (usrChoice == "3")
                {
                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }

                    GoodConsoleClear();
                    DigitCode usrDC = new();
                    usrDC.Get();

                    OpenStatisticGraph(usrDC.ToString(), selectedMode);
                    GoodConsoleClear();
                }
                else if (usrChoice == "4") // view a list of digit code statistic files and let user choose which one to view
                {
                    GoodConsoleClear();
                    // list existing statistic files
                    string[] files = Directory.GetFiles("stats");
                    if (files.Length != 0)
                    {
                        GoodConsoleClear();
                        Console.WriteLine("Existing statistics:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.WriteLine($"{i + 1}) {files[i].Substring(6)}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Statistic to open: ");
                        string usrFileChoice = Console.ReadLine();

                        Modes fileMode;

                        // retrieve the mode from the file name. This is used in the OpenStatisticScreen method below.
                        if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                        {
                            if (files[int.Parse(usrFileChoice) - 1].Substring(6).Length == 9) //length 9 comes from the digit code of length 5, including the mode specifier (m0 || m1) and then the file extension. (.csv)
                            {
                                fileMode = (Modes)int.Parse(files[int.Parse(usrFileChoice) - 1].Substring(6)[4].ToString());
                            }
                            else
                            {
                                fileMode = (Modes)int.Parse(files[int.Parse(usrFileChoice) - 1].Substring(6)[6].ToString());
                            }
                        }
                        else if (usrFileChoice == "exit")
                        {
                            GoodConsoleClear();
                            continue;
                        }
                        else
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Invalid choice, try again.");
                            Thread.Sleep(1000);
                            GoodConsoleClear();
                            continue;
                        }

                        if (int.TryParse(usrFileChoice, out int _))
                        {
                            if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                if (files[int.Parse(usrFileChoice) - 1].Substring(6).Length == 9)
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 3);
                                    GoodConsoleClear();
                                    OpenStatisticScreen(statDigitCode, fileMode);
                                }
                                else
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(6, 5);
                                    GoodConsoleClear();
                                    OpenStatisticScreen(statDigitCode, fileMode);
                                }
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.WriteLine("Invalid choice, try again.");
                            }
                        }
                    }

                }
                else if (usrChoice == "5") // view console statistics for specific digit code
                {
                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }

                    GoodConsoleClear();
                    DigitCode usrDC = new();
                    usrDC.Get();
                    GoodConsoleClear();
                    OpenStatisticScreen(usrDC.ToString(), selectedMode);
                }
                else if (usrChoice == "9")
                {
                    Console.WriteLine("Exiting...");
                    Thread.Sleep(20);
                    Environment.Exit(0);
                }
                else
                {
                    GoodConsoleClear();
                    Console.WriteLine("Invalid choice, try again.");
                }

            }
        }
    }
}