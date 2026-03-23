using System.Globalization;
using System.Text.RegularExpressions;
using PeterO.Numbers;

namespace Lunariens_Mental_Math_Trainer
{
    public static class Helpers
    {
        public enum Modes
    {
        Exit = -1,
        Text = 0,
        Speech = 1
    }
        public static string? GetSessionDefinitionStr()
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
                    using (StreamReader reader = new("./resources/dc-help.txt"))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    return usrDigitCodeInput;
                }
            }
        }
    }
    public class DigitCode(int digitsX = -1, int digitsY = -1, int? lowerBoundX = null, int? upperBoundX = null, int? lowerBoundY = null, int? upperBoundY = null, char operation = '\0', int decimals = 0)
    {
        public EInteger DigitsX = digitsX;
        public EInteger DigitsY = digitsY;
        public EInteger? LowerBoundX = lowerBoundX;
        public EInteger? UpperBoundX = upperBoundX;
        public EInteger? LowerBoundY = lowerBoundY;
        public EInteger? UpperBoundY = upperBoundY;
        public char Operation = operation;
        public EInteger Decimals = decimals; //optional

        public DigitCode[]? Get()
        {
            string dcPattern = @"([1-9]\d*)(\+|\-|\*|\/|R|\^)([1-9]\d*)(?:\.([1-9]\d*))?"; //regex pattern to match digit codes
            Regex dcRegex = new(dcPattern);

            Console.WriteLine("Enter a digit code and (optionally) the amount of problems. Enter \"help\" to get more info.");

            while (true)
            {
                Console.Write("Digit code: ");
                string usrDigitCodeInput = Console.ReadLine();


                if (usrDigitCodeInput == "exit")
                {
                    LowerBoundX = -1;
                    UpperBoundX = -1;
                    LowerBoundY = -1;
                    UpperBoundY = -1;
                    Operation = '\0';
                    return null;
                }
                Parser parser = new(usrDigitCodeInput);
                DigitCode[] digitCodes = parser.Parse(out _);

                if (usrDigitCodeInput == "help")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    using (StreamReader reader = new("./resources/dc-help.txt"))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (digitCodes.Length == 1)
                {
                    DigitCode dc = digitCodes[0];


                    DigitsX = digitCodes[0].DigitsX;
                    DigitsY = digitCodes[0].DigitsY;
                    LowerBoundX = digitCodes[0].LowerBoundX;
                    UpperBoundX = digitCodes[0].UpperBoundX;
                    LowerBoundY = digitCodes[0].LowerBoundY;
                    UpperBoundY = digitCodes[0].UpperBoundY;
                    Operation = digitCodes[0].Operation;
                    Decimals = digitCodes[0].Decimals;
                    return digitCodes;
                }
                else
                {
                    Console.WriteLine("Digit code was in an invalid format. (Did you forget a decimal point?)");
                }

            }
        }
        public override string ToString()
        {
            string boundsX;
            string boundsY;
            if (DigitsX == EInteger.FromInt32(-1))
            {
                boundsX = LowerBoundX < UpperBoundX ? $"{{{LowerBoundX}..{UpperBoundX}}}" : $"{{{UpperBoundX}..{LowerBoundX}}}";
            }
            else
            {
                boundsX = DigitsX.ToString();
            }

            if (DigitsY == EInteger.FromInt32(-1))
            {
                boundsY = LowerBoundY < UpperBoundY ? $"{{{LowerBoundY}..{UpperBoundY}}}" : $"{{{UpperBoundY}..{LowerBoundY}}}";
            }
            else
            {
                boundsY = DigitsY.ToString();
            }

            if (Decimals == EInteger.FromInt32(0))
            {
                return $"{boundsX}{Operation}{boundsY}";
            }
            else
            {
                return $"{boundsX}{Operation}{boundsY}.{Decimals}";
            }
        }
    }
    public class Parser(string sessionDefinition)
    {
        private string[] SplitSessionDefinition(string sessionDefinition)
        {
            Regex whitespaceRegex = new(@"\s+");
            MatchCollection matches = whitespaceRegex.Matches(sessionDefinition);
            foreach (Match match in matches)
            {
                sessionDefinition.Replace(match.ToString(), " ");
            }

            return sessionDefinition.Split(' ');
        }
        private DigitCode ParseSingleRaw(string digitCode)
        {
            int digitsX = -1;
            int digitsY = -1;
            int? dcLowerX = null;
            int? dcUpperX = null;
            int? dcLowerY = null;
            int? dcUpperY = null;
            char operation = '\0';
            int decimals = 0;

            Regex baseRegex = new(@"(.*)([\+\-\*\^])(.*?)$"); //no decimal operations
            Regex baseRegexDecimals = new(@"(.*)([\/R])(.*?)\.(\d+)$"); //operations requiring decimals
            Match match;
            if (baseRegex.IsMatch(digitCode))
            {
                match = baseRegex.Match(digitCode);
            }
            else if (baseRegexDecimals.IsMatch(digitCode))
            {
                match = baseRegexDecimals.Match(digitCode);
            }
            else
            {
                throw new FormatException("No valid digit code found in entered text! (Did you forget a decimal point?)");
            }



            GroupCollection groups = match.Groups;
            Group[] groupArray = groups.Values.ToArray();
            Regex rangeRegex = new(@"\{([1-9]\d*)\.\.([1-9]\d*)\}");
            Regex rangeRegexSimple = new(@"\{([1-9]\d*)\}"); // for ranges with a single number
            Regex validNumbers = new(@"^[1-9]\d*$");
            Regex validDecimalNumbers = new(@"^[0-9]\d*$");
            for (int i = 1; i < groupArray.Length; i++)
            {

                if (groupArray[i].ToString().Contains('{') && groupArray[i].ToString().Contains('}'))
                {
                    if (i == 1) // if we're currently defining X
                    {
                        if (rangeRegexSimple.IsMatch(groupArray[i].ToString()))
                        {
                            Match rangeMatch = rangeRegexSimple.Match(groupArray[i].ToString());
                            dcLowerX = int.Parse(rangeMatch.Groups[1].ToString());
                            dcUpperX = dcLowerX; // single number range
                        }
                        else
                        {
                            Match rangeMatch = rangeRegex.Match(groupArray[i].ToString());
                            dcLowerX = int.Parse(rangeMatch.Groups[1].ToString());
                            dcUpperX = int.Parse(rangeMatch.Groups[2].ToString());
                        }
                    }
                    else
                    {
                        if (rangeRegexSimple.IsMatch(groupArray[i].ToString()))
                        {
                            Match rangeMatch = rangeRegexSimple.Match(groupArray[i].ToString());
                            dcLowerY = int.Parse(rangeMatch.Groups[1].ToString());
                            dcUpperY = dcLowerY; // single number range
                        }
                        else
                        {
                            Match rangeMatch = rangeRegex.Match(groupArray[i].ToString());
                            dcLowerY = int.Parse(rangeMatch.Groups[1].ToString());
                            dcUpperY = int.Parse(rangeMatch.Groups[2].ToString());
                        }
                    }
                }
                else if ("+-*/^R".Contains(groupArray[i].ToString()) && i != 4) //operator
                {
                    operation = groupArray[i].ToString()[0];
                }
                else if (validNumbers.IsMatch(groupArray[i].ToString()))
                {
                    int num = int.Parse(groupArray[i].ToString(), CultureInfo.InvariantCulture);
                    if (i == 1)
                    {
                        if (digitsX == -1)
                        {
                            digitsX = num;
                        }
                        else
                        {
                            digitsY = num;
                        }
                    }
                    else if (i == 4)
                    {
                        decimals = num;
                    }
                    else
                    {
                        digitsY = num;
                    }
                }
                else if (validDecimalNumbers.IsMatch(groupArray[i].ToString()) && i == 4) //decimals
                {
                    decimals = int.Parse(groupArray[i].ToString(), CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new FormatException("Improper formatting detected in a digit code!");
                }
            }
            return new DigitCode(digitsX, digitsY, dcLowerX, dcUpperX, dcLowerY, dcUpperY, operation, decimals);
        }


        public DigitCode[] Parse(out int? problemCount)
        {
            string[] definWords = SplitSessionDefinition(sessionDefinition);
            List<DigitCode> digitCodes = [];
            int[] problematicIndexes = [];

            Regex problemCountRegex = new(@"\s+(\d+)$");
            int problemCountCount = 0;
            if (problemCountRegex.IsMatch(sessionDefinition))
            {
                problemCountCount = 1;
            }

            for (int i = 0; i < definWords.Length - problemCountCount; i++)
            {
                DigitCode singleDC = ParseSingleRaw(definWords[i]);
                try
                {
                    digitCodes.Add(singleDC);
                }
                catch (FormatException)
                {
                    problematicIndexes.Append(i);
                }
            }
            if (problematicIndexes.Length != 0)
            {
                throw new FormatException($"Found invalid digit codes on indexes " + string.Join(", ", problematicIndexes));
            }

            problemCount = null;
            if (problemCountRegex.IsMatch(sessionDefinition))
            {
                problemCount = int.Parse(problemCountRegex.Match(sessionDefinition).ToString());
            }

            return digitCodes.ToArray();
        }
    }
}