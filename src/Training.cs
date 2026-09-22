using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Media;
using PeterO.Numbers;
using System.Diagnostics;
using NAudio.Wave;
using static Lunariens_Mental_Math_Trainer.Modes;
using static Lunariens_Mental_Math_Trainer.Formatting;

namespace Lunariens_Mental_Math_Trainer
{
    class Training
    {
        internal static Modes GetMode()
        {
            int[] possibleModes = [0, 1, 2];
            Console.WriteLine("Choose a mode:");
            Console.WriteLine("1 - Text mode");
            Console.WriteLine("2 - Text to speech mode");
            Console.WriteLine("3 - Flash anzan mode");
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
                {
                    number -= 1;
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
        }
        private static readonly Dictionary<string, string> rootMap = new()
        {
            { "3", "cube root of " },
            { "4", "fourth root of " },
            { "5", "fifth root of " },
            { "6", "sixth root of " },
            { "7", "seventh root of " },
            { "8", "eighth root of " },
            { "9", "ninth root of " }
        };
        private static string TokenToWords(string symbol)
        {
            Regex numRegex = new(@"^\d+$");
            switch (symbol)
            {
                case var a when numRegex.IsMatch(a):
                    return NumToWords(a);
                case "(":
                    return "Open parenthesis";
                case ")":
                    return "Close parenthesis";
                case "+":
                    return "plus";
                case "-":
                    return "minus";
                case "*":
                    return "times";
                case "/":
                    return "divided by";
                case "^":
                    return "to the power of";
                case "√":
                    return "root"; //TODO: think about how to do the bases (square/cube/fourth root)
                default:
                    throw new FormatException();
            }
        }
        internal static void PlaySound(string fileName)
        {
            using var audioFile = new AudioFileReader("speech.wav");
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(5);
            }
        }
        private static void OutputProblem(string problem, SpeechSynthesizer synth, Modes mode)
        {

            if (mode is Text) //text mode
            {
                //TODO: make it spaced out in a column like before.

                // if the first number is bigger than the second number, add num1.Length - num2.Length spaces to the beginning of the second number
                // Regex numRegex = new(@"\d+");
                // MatchCollection numbers = numRegex.Matches(problem);
                // Regex exceptionSymbols = new(@"[⁰¹²³⁴⁵⁶⁷⁸⁹√^]+");
                // int spacesToAdd;
                // if (numbers.Count == 2 && !exceptionSymbols.IsMatch(problem))
                // {
                //     string num1 = numbers[0].Value;
                //     string num2 = numbers[1].Value;
                //     if (num1.Length > num2.Length)
                //     {
                //         spacesToAdd = num1.Length - num2.Length;
                //         problem = problem.Insert(problem.LastIndexOf(num2), new string(' ', spacesToAdd - 1));
                //     }
                //     else if (num1.Length < num2.Length)
                //     {
                //         spacesToAdd = num2.Length - num1.Length;
                //         problem = "  " + new string(' ', spacesToAdd - 1) + problem;
                //     }
                //     else
                //     {
                //         problem = " " + problem;
                //     }
                // }

                Console.WriteLine(problem);
            }
            if (mode is Speech) //speech mode
            {

                Regex tokenRegex = new(@"(\d+|\+|\-|\*|/|\^|√|\(|\))");
                MatchCollection tokens = tokenRegex.Matches(problem);
                string problemWords = "";
                foreach (Match token in tokens)
                {
                    problemWords += $"{TokenToWords(token.ToString())} ";
                }

                synth.SetOutputToWaveFile("speech.wav");
                synth.Speak(problemWords);
                synth.SetOutputToNull();
                TrimAudioEnd("speech.wav", "speech-cut.wav", -45);  
                SoundPlayer player = new("speech-cut.wav");
                player.Play();
            }
        }

        internal static void OpenTrainingScreen(Stopwatch stopWatch, IFormatProvider ifp, string[] digitCodes, SpeechSynthesizer speechSynth, Modes mode, int? problemCount = null)
        {
            GoodConsoleClear();

            // validate digit codes
            bool failed = false;
            foreach (string digitCode in digitCodes)
            {
                try
                {
                    List<Tokenizer.Token> tokens = Tokenizer.Tokenize(digitCode);
                    ASTBuilder.Expression expr = ASTBuilder.Build(ref tokens);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Digit code {digitCode} was parsed successfully");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                catch (DigitCodeException e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Digit code {digitCode} couldn't be parsed: {e.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                    failed = true;
                }
            }
            if (failed)
                return;

            SessionConfiguration.problemCount = problemCount;
            if (problemCount == 0)
                return;

            Random random = new(); // use for picking the digit code (from multiple) to decide the type of the generated problem

            while (true)
            {
                if (digitCodes.Length == 0)
                {
                    GoodConsoleClear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Entered text has no valid digit codes!");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }

                if (problemCount <= 0)
                    return;

                int dcChoice = random.Next(0, digitCodes.Length);

                while (problemCount > 0 || problemCount == null)
                {
                    string problem = "";
                    EDecimal result = 0;
                    try
                    {
                        List<Tokenizer.Token> tokens = Tokenizer.Tokenize(digitCodes[dcChoice]);
                        ASTBuilder.Expression expr = ASTBuilder.Build(ref tokens);
                        (problem, result) = ProblemGenerator.GenerateProblem(expr);
                    }
                    catch (DigitCodeException e)
                    {
                        GoodConsoleClear();
                        Console.WriteLine(e.Message);
                        return;
                    }

                    if (!stopWatch.IsRunning)
                    {
                        stopWatch.Reset();
                    }
                    OutputProblem(problem, speechSynth, mode);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    if (mode == Speech)
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
                    else
                    {
                        continue;
                    }

                    if (usrResult.ToLower() == "exit")
                    {
                        GoodConsoleClear();
                        return;
                    }

                    if (usrResult.StartsWith(result.ToString())) // if result is correct. this checks if the user input starts with the correct answer.
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Correct");
                        Console.ForegroundColor = ConsoleColor.White;
                        problemCount--;
                        break;
                    }
                    else
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Wrong, correct was " + result.ToString());
                        Console.ForegroundColor = ConsoleColor.White;
                        problemCount--;
                        break;
                    }
                }
            }
        }
    }
}