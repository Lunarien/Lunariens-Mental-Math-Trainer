using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Media;
using PeterO.Numbers;
using System.Diagnostics;
using System.Globalization;
using NAudio.Wave;
using static Lunariens_Mental_Math_Trainer.Modes;
using static Lunariens_Mental_Math_Trainer.Formatting;
using static Lunariens_Mental_Math_Trainer.FileHandler;

namespace Lunariens_Mental_Math_Trainer
{
    class Training
    {
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
        public static readonly Dictionary<string, string> rootMap = new()
        {
            { "3", "cube root of " },
            { "4", "fourth root of " },
            { "5", "fifth root of " },
            { "6", "sixth root of " },
            { "7", "seventh root of " },
            { "8", "eighth root of " },
            { "9", "ninth root of " }
        };
        public static void PlaySound(string fileName)
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
        public static void OutputProblem(string problem, SpeechSynthesizer synth, Modes mode)
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
                        problem = problem.Insert(problem.LastIndexOf(num2), new string(' ', spacesToAdd - 1));
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

        public static void OpenTrainingScreen(Stopwatch stopWatch, IFormatProvider ifp, DigitCode[] digitCodes, SpeechSynthesizer speechSynth, Modes mode, int? problemCount = null)
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
                        EInteger precision = digitCodes[dcChoice].Decimals + digitCodes[dcChoice].DigitsX - digitCodes[dcChoice].DigitsY + 1;
                        ctx = new(precision, ERounding.HalfDown, EInteger.FromInt32(-10000), EInteger.FromInt32(10000), true);

                        int quantizationZeros = digitCodes[dcChoice].Decimals.ToInt32Checked() - 1;
                        string quantization;
                        if (quantizationZeros <= -1)
                        {
                            quantization = "1";
                        }
                        else quantization = "0." + new string('0', quantizationZeros) + "1";

                        decResult = EDecimal.FromEInteger(x).Divide(EDecimal.FromEInteger(y), ctx).Quantize(EDecimal.FromString(quantization), ctx);
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

                    if (usrResult.ToLower() == "exit")
                    {
                        GoodConsoleClear();
                        return;
                    }

                    // truncate the result down to the number of decimals specified in the digit code, so that extra decimals don't cause the answer to be marked wrong

                    int decimalIndex = usrResult.IndexOf('.') + digitCodes[dcChoice].Decimals.ToInt32Unchecked() + 1;

                    if (usrResult.Length > decimalIndex && usrResult.Contains('.'))
                    {
                        usrResult = usrResult[..decimalIndex];
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
                                SaveStatistic(digitCodes[dcChoice].ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true, mode);

                                if (problemCount != null) problemCount -= 1;
                                if (problemCount <= 0) training = false;
                                break;
                            }
                            else
                            {
                                SaveStatistic(digitCodes[dcChoice].ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false, mode);
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

                            EDecimal usrResultDec = EDecimal.FromString(usrResult, ctx);
                            if (decResult.CompareToValue(usrResultDec) == 0) // if correct
                            {
                                SaveStatistic(digitCodes.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, true, mode);
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
                                SaveStatistic(digitCodes.ToString(), problem, usrResult, stopWatch.Elapsed.TotalSeconds, DateTime.Now, false, mode);
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
                    { //so why is this here?.... I guess it's for debugging purposes..
                        GoodConsoleClear();
                        Console.WriteLine("Invalid result.");
                        break;
                    }
                }
            }
        }
    }
}