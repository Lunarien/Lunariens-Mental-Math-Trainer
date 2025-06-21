using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Speech.Synthesis;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using ConsoleTables;
using static Lunariens_Mental_Math_Trainer.Modes;
using NAudio.Wave;
using System.Media;
using PeterO.Numbers;
using NAudio.Wave.SampleProviders;


namespace Lunariens_Mental_Math_Trainer
{
    public static class Configuration // this is to be saved and loaded to/from a config file in the future
    {
        public static int SpeechDelay = 500;
    }
    public static class SessionConfiguration
    {
        public static int? problemCount = null;
    }

    public enum Modes
    {
        Exit = -1,
        Text = 0,
        Speech = 1
    }
    public class Program
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
        public static string NumToWords(string number, char magnitudeSep)
        {
            string[] parts = number.Split(magnitudeSep);
            if (parts.Length == 1)
            {
                return number;
            }
            string[] magnitudes = { "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion", "duodecillion", "tredecillion", "quattuordecillion", "quindecillion", "sexdecillion", "septendecillion", "octodecillion", "novemdecillion", "vigintillion", "unvigintillion", "duovigintillion", "trevigintillion", "quattuorvigintillion", "quinvigintillion", "sexvigintillion", "septenvigintillion", "octovigintillion", "novemvigintillion", "trigintillion", "untrigintillion", "duotrigintillion", "tretrigintillion", "quattuortrigintillion", "quintrigintillion", "sextrigintillion", "septentrigintillion", "octotrigintillion", "novemtrigintillion", "quadragintillion", "unquadragintillion", "duoquadragintillion", "trequadragintillion", "quattuorquadragintillion", "quinquadragintillion", "sexquadragintillion", "septenquadragintillion", "octoquadragintillion", "novemquadragintillion", "quinquagintillion", "unquinquagintillion", "duoquinquagintillion", "trequinquagintillion", "quattuorquinquagintillion", "quinquinquagintillion", "sexquinquagintillion", "septenquinquagintillion", "octoquinquagintillion", "novemquinquagintillion", "sexagintillion", "unsexagintillion", "duosexagintillion", "tresexagintillion", "quattuorsexagintillion", "quinsexagintillion", "sexsexagintillion", "septensexagintillion", "octosexagintillion", "novemsexagintillion", "septuagintillion", "unseptuagintillion", "duoseptuagintillion", "treseptuagintillion", "quattuorseptuagintillion", "quinseptuagintillion", "sexseptuagintillion", "septenseptuagintillion", "octoseptuagintillion", "novemseptuagintillion", "octogintillion", "unoctogintillion", "duooctogintillion", "treoctogintillion", "quattuoroctogintillion", "quinoctogintillion", "sexoctogintillion", "septenoctogintillion", "octooctogintillion", "novemoctogintillion", "nonagintillion", "unnonagintillion", "duononagintillion", "trenonagintillion", "quattuornonagintillion", "quinnonagintillion", "sexnonagintillion", "septennonagintillion", "octononagintillion", "novemnonagintillion", "centillion", "uncentillion", "duocentillion", "trecentillion", "quattuorcentillion", "quincentillion", "sexcentillion", "septencentillion", "octocentillion", "novemcentillion", "duocentillion", "treduocentillion", "quattuorduocentillion", "quinduocentillion", "sexduocentillion", "septenduocentillion", "octoduocentillion", "novemduocentillion", "trecentillion", "quattuortrecentillion", "quintrencentillion", "sextrencentillion", "septentrencentillion", "octotrencentillion", "novemtrencentillion" };
            List<string> words = new List<string>();
            int magnitudeIndex = parts.Length - 2;
            for (int i = 0; i < parts.Length; i++, magnitudeIndex--)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                {
                    words.Add(parts[i] + (magnitudeIndex >= 0 && magnitudeIndex < magnitudes.Length ? " " + magnitudes[magnitudeIndex] : ""));
                }
            }
            return string.Join(" ", words).Trim();
        }
        public static DigitCode[] ParseDigitCodes(string? input)
        {
            if (input == null)
                return [new DigitCode(-1, -1, '\0')];
            string dcPattern = @"(\d+)(\+|\-|\*|\/|R|\^)(\d+)(?:\.(\d+))?";
            Regex dcRegex = new(dcPattern);

            MatchCollection matches = dcRegex.Matches(input);
            List<DigitCode> digitCodes = new();
            foreach (Match dc in matches)
            {
                int digitsX = int.Parse(dc.Groups[1].ToString());
                int digitsY = int.Parse(dc.Groups[3].ToString());
                char op = dc.Groups[2].ToString()[0];
                int decimals = dc.Groups[4].Success ? int.Parse(dc.Groups[4].ToString()) : 0;
                DigitCode newDc = new(digitsX, digitsY, op, decimals);
                digitCodes.Add(newDc);
            }
            return digitCodes.ToArray();
        }

        public static string? GetDC()
        {
            Console.WriteLine("Enter digit code(s) and (optionally) the amount of problems. Enter \"help\" to get more info.");

            while (true)
            {
                Console.Write("Digit code(s): ");
                string usrDigitCodeInput = Console.ReadLine();

                if (usrDigitCodeInput == "exit")
                {
                    return null;
                }
                else if (usrDigitCodeInput == "help")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("A digit code defines the type of the problem and has the following format:");
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
                    Console.WriteLine();
                    Console.WriteLine("By default, you get an infinite supply of problems.");
                    Console.WriteLine("You can set a specific amount of problems by typing a number at the end of the session definition, preceded by a space.");
                    Console.WriteLine();
                    Console.WriteLine("You can also enter multiple digit codes to train with multiple problem types.");
                    Console.WriteLine("They should be separated by a space in between each one.");
                    Console.WriteLine();
                    Console.WriteLine("Instead of the digit amount, you may enter a curly brace-delimited precise number range of the following format:");
                    Console.WriteLine("{b..t}");
                    Console.WriteLine("\"b\" means bottom, and \"t\" means top, corresponding to the bottom- and top-most number in the range.");
                    Console.WriteLine();
                    Console.WriteLine("Example digit code inputs:");
                    Console.WriteLine("3+3 10");
                    Console.WriteLine("5/2.2 5");
                    Console.WriteLine("2R3.4");
                    Console.WriteLine("{11..35}^2 100");

                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    return usrDigitCodeInput;
                }
            }
        }

        /// <summary>
        /// Trims the end of an audio file if its volume is under a specified threshold.
        /// </summary>
        /// <param name="inputFilePath">Path to the input audio file.</param>
        /// <param name="outputFilePath">Path to save the trimmed audio file.</param>
        /// <param name="volumeThresholdDb">Volume threshold in decibels. Values lower represent quieter sounds.</param>
        public static void TrimAudioEnd(string inputFilePath, string outputFilePath, float volumeThresholdDb)
        {
            using (var reader = new AudioFileReader(inputFilePath))
            {
                float[] buffer = new float[1024];
                TimeSpan lastNonSilentPosition = reader.TotalTime;

                while (reader.Position < reader.Length)
                {
                    int samplesRead = reader.Read(buffer, 0, buffer.Length);
                    float volumeDb = GetRmsVolume(buffer, samplesRead);
                    if (volumeDb > volumeThresholdDb)
                    {
                        lastNonSilentPosition = reader.CurrentTime;
                    }
                }

                using (var writer = new WaveFileWriter(outputFilePath, reader.WaveFormat))
                {
                    reader.Position = 0;
                    while (reader.CurrentTime <= lastNonSilentPosition && reader.Position < reader.Length)
                    {
                        int samplesRead = reader.Read(buffer, 0, buffer.Length);
                        writer.WriteSamples(buffer, 0, samplesRead);
                    }
                }
            }
        }

        private static float GetRmsVolume(float[] samples, int sampleCount)
        {
            double sum = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                sum += samples[i] * samples[i];
            }
            double mean = sum / sampleCount;
            return 20 * (float)Math.Log10(Math.Sqrt(mean) + float.Epsilon);
        }


        public static void GoodConsoleClear()
        {   // Clearing the console doesn't work well in case of Windows Terminal. This abomination is a workaround for that.
            Console.Clear();
            Console.WriteLine("\f\u001bc\x1b[3J");
            Console.Clear();
        }

        public static Modes GetMode()
        {
            int[] possibleModes = [0, 1];
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


        public static EInteger RandomEInt(EInteger bottom, EInteger top)
        {
            if (bottom > top)
            {
                throw new ArgumentException("Bottom must be less than or equal to top.");
            }
            Random randomness = new();
            EInteger rangeSize = top - bottom;

            EContext ctx = new(1000, ERounding.HalfDown, -10000, 10000, false);
            EFloat rangeLog2 = EFloat.FromEInteger(rangeSize).Log(ctx).Divide(EFloat.FromInt32(2).Log(ctx), ctx);
            EInteger byteCount = rangeLog2.RoundToPrecision(ctx).ToEInteger();
            byte[] buf = new byte[byteCount.ToInt32Unchecked() + 5];

            randomness.NextBytes(buf);
            EInteger result = (EInteger.FromBytes(buf, 0, buf.Length, false).Abs() % rangeSize) + bottom;
            return result;
        }

        public static string ToSuperScript(string number)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < number.Length; i++)
            {
                result += sups[int.Parse(number[i].ToString())].ToString(); // this appends the superscript of the given number to the string result.
            }
            return result;
        }

        public static string ToSuperScript(int number)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < number.ToString().Length; i++)
            {
                result += sups[int.Parse(number.ToString()[i].ToString())].ToString(); // this monstrosity appends the superscript of the given number to the string result.
            }
            return result;
        }
        public static string SuperscriptToNum(string sup)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < sup.Length; i++)
            {
                result += sups.IndexOf(sup[i]).ToString(); // this appends the number corresponding to the superscript to the string result.
            }
            return result;
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

        public static void PlaySound(string fileName)
        {
            using (var audioFile = new AudioFileReader("speech.wav"))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(5);
                }
            }
        }

        public static void OutputProblem(string problem, SpeechSynthesizer synth)
        {

            if (mode is Text) //text mode
            {
                // if the first number is bigger than the second number, add num1.Length - num2.Length spaces to the beginning of the second number
                Regex numRegex = new(@"\d+");
                MatchCollection numbers = numRegex.Matches(problem);
                Regex exceptionSymbols = new(@"[⁰¹²³⁴⁵⁶⁷⁸⁹√^]+");
                int spacesToAdd;
                if (numbers.Count == 2 && !exceptionSymbols.IsMatch(problem))
                {
                    string num1 = numbers[0].Value;
                    string num2 = numbers[1].Value;
                    if (num1.Length > num2.Length)
                    {
                        spacesToAdd = num1.Length - num2.Length;
                        problem = problem.Insert(problem.IndexOf(num2), new string(' ', spacesToAdd - 1));
                    }
                    else if (num1.Length < num2.Length)
                    {
                        spacesToAdd = num2.Length - num1.Length;
                        problem = "  " + new string(' ', spacesToAdd - 1) + problem;
                    }
                    else
                    {
                        problem = " " + problem;
                    }
                }

                Console.WriteLine(problem);
            }
            if (mode is Speech) //speech mode
            {
                synth.SetOutputToWaveFile("speech.wav");
                SoundPlayer player = new(@"speech-cut.wav");

                char op = char.Parse(Regex.Replace(problem, @"[⁰¹²³⁴⁵⁶⁷⁸⁹\d\n]", string.Empty));

                problem = Regex.Replace(problem, @"[*^/+\-\n]", " "); //put a space between numbers instead of the operator
                problem = Regex.Replace(problem, @"[\n\s]+", " "); //remove multi spaces
                problem = Regex.Replace(problem, @"√", " ");
                string[] numbers;
                //split the problem into individual numbers
                numbers = problem.Split(" ");

                for (int i = 0; i < numbers.Length; i++)
                {
                    numbers[i] = AddCommas(numbers[i]);
                    numbers[i] = NumToWords(numbers[i], ','); //convert the number to words
                }
                Thread.Sleep(Configuration.SpeechDelay);
                switch (op)
                {
                    case '+':
                        problem = string.Join(" plus ", numbers);
                        synth.Speak(problem);
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
                        break;
                    case '-':
                        problem = string.Join(" minus ", numbers);
                        synth.Speak(problem);
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
                        break;
                    case '*':
                        problem = string.Join(" times ", numbers);
                        synth.Speak(problem);
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
                        break;
                    case '/':
                        problem = string.Join(" divided by ", numbers);
                        synth.Speak(problem);
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
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
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
                        break;
                    case '√':
                        if (numbers.Length == 1)
                        {
                            problem = "square root of " + numbers[0];
                        }
                        else if (numbers.Length == 2)
                        {
                            if (rootMap.TryGetValue(SuperscriptToNum(numbers[0]), out var root))
                            {
                                problem = root + numbers[1];
                            }
                            else
                            {
                                problem = "root of base " + SuperscriptToNum(numbers[0]) + " of " + numbers[1];
                            }
                        }
                        synth.Speak(problem);
                        synth.SetOutputToNull();
                        TrimAudioEnd("speech.wav", "speech-cut.wav", -30);
                        player.PlaySync();
                        break;
                    default:
                        Console.WriteLine("An error occured during processing of the problem for speech output. This happened during switch of operations (dev note). MAKE SURE TO  SCREENSHOT THIS ERROR (Shift+Win+S).");
                        Thread.Sleep(45000);
                        Console.ReadLine();
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

            string path = $@"../stats/{digitCode}m{(int)mode}.csv";
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

            string path = $@"../stats/{digitCode}m{(int)mode}.csv";
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
            Console.Write("Press any key to go back to the main menu...");
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
            string dirPath = "../stats/";
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
            string path = $@"..\stats\{statName}m{(int)mode}.csv";
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

        public static void OpenTrainingScreen(Stopwatch stopWatch, IFormatProvider ifp, DigitCode[] digitCodes, SpeechSynthesizer speechSynth, int? problemCount = null)
        {
            GoodConsoleClear();
            EContext ctx;

            SessionConfiguration.problemCount = problemCount;
            if (problemCount == 0)
                return;

            Random random = new(); // use for picking the digit code (from multiple) to decide the type of the generated problem

            bool training = true;
            while (training)
            {
                int dcChoice = random.Next(0, digitCodes.Length);

                ctx = new(digitCodes[dcChoice].Decimals, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                // make random numbers corresponding to the number of digits in the digit code
                EInteger xRangeBottom;
                EInteger xRangeTop;
                EInteger x;
                EInteger yRangeBottom;
                EInteger yRangeTop;
                EInteger y;
                EInteger ten = 10; // helper
                if (digitCodes[dcChoice].DigitsX < 1) // {b..t} number format (b = bottom; t = top)
                {
                    xRangeBottom = digitCodes[dcChoice].LowerBoundX;
                    xRangeTop = digitCodes[dcChoice].UpperBoundX;
                    if (xRangeBottom > xRangeTop)
                    {
                        EInteger temp = xRangeBottom;
                        xRangeBottom = xRangeTop;
                        xRangeTop = temp;
                    }
                    x = RandomEInt(xRangeBottom, xRangeTop + 1);
                }
                else if (digitCodes[dcChoice].Operation == 'R')
                {
                    x = digitCodes[dcChoice].DigitsX;
                }
                else
                {
                    xRangeBottom = ten.Pow(digitCodes[dcChoice].DigitsX - 1); //the bottom of the range for X. it is 10^(DigitsX - 1)
                    xRangeTop = ten.Pow(digitCodes[dcChoice].DigitsX); //the top of the range for X. it is 10^DigitsX
                    if (xRangeBottom > xRangeTop)
                    {
                        var temp = xRangeBottom;
                        xRangeBottom = xRangeTop;
                        xRangeTop = temp;
                    }
                    x = RandomEInt(xRangeBottom, xRangeTop).Abs();
                }

                if (digitCodes[dcChoice].DigitsY <= 0) // {b..t} number format (b = bottom; t = top)
                {
                    yRangeBottom = digitCodes[dcChoice].LowerBoundY;
                    yRangeTop = digitCodes[dcChoice].UpperBoundY;
                    if (yRangeBottom > yRangeTop)
                    {
                        var temp = yRangeBottom;
                        yRangeBottom = yRangeTop;
                        yRangeTop = temp;
                    }
                    y = RandomEInt(yRangeBottom, yRangeTop + 1);
                }
                else if (digitCodes[dcChoice].Operation == '^')
                {
                    y = digitCodes[dcChoice].DigitsY;
                }
                else
                {
                    yRangeBottom = ten.Pow(digitCodes[dcChoice].DigitsY - 1);
                    yRangeTop = ten.Pow(digitCodes[dcChoice].DigitsY);
                    if (yRangeBottom > yRangeTop)
                    {
                        var temp = yRangeBottom;
                        yRangeBottom = yRangeTop;
                        yRangeTop = temp;
                    }
                    y = RandomEInt(yRangeBottom, yRangeTop).Abs();
                }

                // make a problem string with the random numbers and the operation
                string problem;
                if (digitCodes[dcChoice].Operation == '^')
                {
                    problem = x.ToString() + "^" + y.ToString();
                }
                else if (digitCodes[dcChoice].Operation == 'R')
                {
                    if (x == EInteger.FromInt32(2))
                    {
                        problem = '√' + y.ToString();
                    }
                    else
                    {
                        problem = ToSuperScript(x.ToString()) + '√' + y;
                    }

                }
                else
                {
                    problem = x.ToString() + "\n" + digitCodes[dcChoice].Operation + y;
                }
                // precompute the correct solution
                EInteger? intResult = null; //the null value indicates that the problem does not have a solution of this type. this is utilized later.
                EDecimal? decResult = null;
                switch (digitCodes[dcChoice].Operation)
                {
                    case '+':
                        intResult = x + y;
                        break;
                    case '-':
                        intResult = x - y;
                        break;
                    case '*':
                        intResult = x * y;
                        break;
                    case '/':
                        ctx = new(digitCodes[dcChoice].Decimals + digitCodes[dcChoice].DigitsX - digitCodes[dcChoice].DigitsY + 1, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        decResult = EDecimal.FromEInteger(x).Divide(EDecimal.FromEInteger(y), ctx).Quantize(EDecimal.FromString($"0.{new String('0', digitCodes[dcChoice].Decimals.ToInt32Checked() - 1)}1"), ctx);
                        break;
                    case '^':
                        intResult = x.Pow(y);
                        break;
                    case 'R':

                        if (digitCodes[dcChoice].DigitsX == EInteger.FromInt32(-1) && digitCodes[dcChoice].DigitsY == EInteger.FromInt32(-1)) // DigitsX = -1; DigitsY = -1;
                        {
                            ctx = new(digitCodes[dcChoice].Decimals + y.ToString().Length / x.ToString().Length + 1, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        }
                        else if (digitCodes[dcChoice].DigitsX == EInteger.FromInt32(-1) && digitCodes[dcChoice].DigitsY != EInteger.FromInt32(-1)) // DigitsX = -1; DigitsY = (int)digit count;
                        {
                            ctx = new(digitCodes[dcChoice].Decimals + digitCodes[dcChoice].DigitsY / x.ToString().Length + 1, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        }
                        else if (digitCodes[dcChoice].DigitsX == EInteger.FromInt32(-1) && digitCodes[dcChoice].DigitsY == EInteger.FromInt32(-1))  // DigitsX = (int)something else;; DigitsY = -1;
                        {
                            ctx = new(digitCodes[dcChoice].Decimals + y.ToString().Length / digitCodes[dcChoice].DigitsX + 1, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        }
                        else
                        {
                            ctx = new(digitCodes[dcChoice].Decimals + digitCodes[dcChoice].DigitsY / digitCodes[dcChoice].DigitsX + 1, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        }
                        EContext ctxExponent = new(10000, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                        EDecimal a = EDecimal.FromEInteger(y);
                        EDecimal exponent = EDecimal.One.Divide(EDecimal.FromEInteger(x), ctxExponent);
                        EDecimal b = a.Pow(exponent, ctx);
                        EDecimal c = b.Quantize(EDecimal.FromString($"0.{new String('0', digitCodes[dcChoice].Decimals.ToInt32Checked() - 1)}1"), ctx);
                        decResult = c;
                        break;
                    default: //should be impossible to reach here.
                        intResult = 0;
                        decResult = 0.0m;
                        break;
                }

                while (problemCount > 0 || problemCount == null)
                {
                    if (!stopWatch.IsRunning)
                    {
                        stopWatch.Reset();
                    }
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
                    if (usrResult != "")
                    {
                        stopWatch.Stop();
                    }

                    if (usrResult.ToLower() == "exit")
                    {
                        GoodConsoleClear();
                        return;
                    }

                    // truncate the result down to the number of decimals specified in the digit code, so that extra decimals don't cause the answer to be marked wrong

                    int decimalIndex = usrResult.IndexOf('.') + digitCodes[dcChoice].Decimals.ToInt32Unchecked();

                    if (usrResult.Length > decimalIndex && usrResult.Contains('.'))
                    {
                        usrResult = usrResult.Substring(0, decimalIndex + digitCodes[dcChoice].Decimals.ToInt32Unchecked());
                    }


                    // start verifying the answer, checking whether it's a number or the exit command
                    if (usrResult.ToLower() == "exit") //check for the exit command first to avoid issues.
                    {
                        GoodConsoleClear();
                        return;
                    }
                    else if (intResult != null && usrResult != "") //if the result is an int.
                    {
                        bool isNumeric;
                        try
                        {
                            EInteger.FromString(usrResult);
                            isNumeric = true;
                        }
                        catch (FormatException)
                        {
                            isNumeric = false;
                        }
                        if (isNumeric)
                        {
                            if (EInteger.FromString(usrResult).Equals(intResult)) // if correct
                            {
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;
                                SaveStatistic(digitCodes[dcChoice].ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true);

                                if (problemCount != null) problemCount -= 1;
                                if (problemCount <= 0) training = false;
                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCodes[dcChoice].ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                GoodConsoleClear();
                                Console.WriteLine("Wrong, correct was: " + intResult);
                                Thread.Sleep(1000);

                                if (problemCount != null) problemCount -= 1;
                                if (problemCount <= 0) training = false;
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
                        bool isNumericDecimal;
                        try
                        {
                            EDecimal.FromString(usrResult);
                            isNumericDecimal = true;
                        }
                        catch (FormatException)
                        {
                            isNumericDecimal = false;
                        }
                        if (decimal.TryParse(usrResult, NumberStyles.AllowDecimalPoint, ifp, out decimal _))
                        {
                            if (decResult.ToString().Contains($".{new String('0', decResult.Exponent.Abs().ToInt32Checked())}"))
                            {
                                ctx = new(usrResult.Length, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);
                            }

                            if (decResult.Equals(EDecimal.FromString(usrResult, ctx)))
                            {
                                SaveStatistic(digitCodes.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true);
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Correct");
                                Console.ForegroundColor = ConsoleColor.White;

                                if (problemCount != null) problemCount -= 1;
                                if (problemCount <= 0) training = false;
                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCodes.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false);
                                GoodConsoleClear();
                                Console.WriteLine("Wrong, correct was: " + decResult?.ToString());

                                if (problemCount != null) problemCount -= 1;
                                if (problemCount <= 0) training = false;
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
                    else if (usrResult == "")
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
                Console.WriteLine("1) Start training");
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

                    DigitCode[] usrDCs = [];
                    bool gettingInput = true;
                    string? usrSessionDefinition = null;
                    while (gettingInput)
                    {
                        usrSessionDefinition = GetDC();
                        if (usrSessionDefinition != null)
                        {
                            try
                            {
                                Parser parser = new(usrSessionDefinition);
                                usrDCs = parser.Parse(out SessionConfiguration.problemCount);
                                gettingInput = false;
                            }
                            catch (FormatException e)
                            {
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(e.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                        else
                        {
                            gettingInput = false;
                        }
                    }
                    if (usrSessionDefinition == null)
                    {
                        GoodConsoleClear();
                        continue;
                    }


                    foreach (DigitCode dc in usrDCs)
                    {
                        InitStatistic(dc.ToString(), mode);
                    }


                    OpenTrainingScreen(sw, ifp, usrDCs, synth, SessionConfiguration.problemCount);
                }
                else if (usrChoice == "2") //view stats from a list
                {
                    GoodConsoleClear();
                    string[] files = Array.Empty<string>();
                    //check if there are any files. list them if so.
                    if (Directory.Exists("../stats"))
                    {
                        files = Directory.GetFiles("../stats");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    bool inOption2 = true;
                    while (inOption2)
                    {
                        if (files.Length != 0)
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Existing statistics:");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            for (int i = 0; i < files.Length; i++)
                            {
                                Console.WriteLine($"{i + 1}) {files[i][9..]}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Statistic to open: ");
                            string usrFileChoice = Console.ReadLine();

                            int fileMode = -1;


                            // retrieve the mode from the file name. This is used in the OpenStatistic method below.
                            if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                if (files[int.Parse(usrFileChoice) - 1][9..].Length == 9)
                                {  //length 9 comes from the digit code of length 5, including the mode specifier (m0 || m1) and then the file extension. (.csv)
                                    fileMode = int.Parse(files[int.Parse(usrFileChoice) - 1][9..][4].ToString());
                                }
                                else
                                {
                                    fileMode = int.Parse(files[int.Parse(usrFileChoice) - 1][9..][6].ToString());
                                }

                            }
                            else if (usrFileChoice == "exit")
                            {
                                GoodConsoleClear();
                                break;
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Invalid choice, try again.");
                                Console.ForegroundColor = ConsoleColor.White;
                                if (inOption2)
                                {
                                    inOption2 = false;
                                    continue;
                                }
                                inOption2 = true;
                            }

                            if (int.TryParse(usrFileChoice, out int _))
                            {
                                if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                                {
                                    if (files[int.Parse(usrFileChoice) - 1][9..].Length == 9)
                                    {
                                        string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(9, 3);
                                        OpenStatisticGraph(statDigitCode, (Modes)fileMode);
                                    }
                                    else
                                    {
                                        string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(9, 5);
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
                }
                else if (usrChoice == "3")
                {
                    string[] files = Array.Empty<string>();
                    if (Directory.Exists("../stats"))
                    {
                        files = Directory.GetFiles("../stats");
                    }
                    else
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit back to menu
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

                    string[] files = Array.Empty<string>();
                    if (Directory.Exists("../stats"))
                    {
                        files = Directory.GetFiles("../stats");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    if (files.Length != 0)
                    {
                        GoodConsoleClear();
                        Console.WriteLine("Existing statistics:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        for (int i = 0; i < files.Length; i++)
                        {
                            Console.WriteLine($"{i + 1}) {files[i][9..]}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Statistic to open: ");
                        string usrFileChoice = Console.ReadLine();

                        Modes fileMode;

                        // retrieve the mode from the file name. This is used in the OpenStatisticScreen method below.
                        if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                        {
                            string file = files[int.Parse(usrFileChoice) - 1];
                            fileMode = (Modes)int.Parse(file[file.Length - 5].ToString());
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
                                if (files[int.Parse(usrFileChoice) - 1][9..].Length == 9)
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(9, 3);
                                    GoodConsoleClear();
                                    OpenStatisticScreen(statDigitCode, fileMode);
                                }
                                else
                                {
                                    string statDigitCode = files[int.Parse(usrFileChoice) - 1].Substring(9, 5);
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
                    string[] files = Array.Empty<string>();
                    if (Directory.Exists("../stats"))
                    {
                        files = Directory.GetFiles("../stats");
                    }
                    else
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }

                    GoodConsoleClear();
                    DigitCode usrDC = new();

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
