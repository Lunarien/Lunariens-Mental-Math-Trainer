using System.Text.RegularExpressions;
using ConsoleTables;
using PeterO.Numbers;

namespace Lunariens_Mental_Math_Trainer
{
    enum TokenType
    {
        Number,
        Operator,   // + - * / R
        Dot,        // .
        Dash,       // -
        Caret,      // ^
        LBrace,     // {
        RBrace,     // }
        Whitespace,
        Unknown,
        EndOfInput
    }
    class Token
    {
        public TokenType Type { get; set; }
        public string? Value { get; set; }
    }


    public static class DCUtilities
    {
        public static DigitCode[] ParseDigitCodes(string? input)
        {
            if (input == null)
                return [new DigitCode(-1, -1, '\0')];
            string dcPattern = @"([1-9]\d*)(\+|\-|\*|\/|R|\^)(\d+)(?:\.([1-9]\d*))?";
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
            string problemCountPattern = @" \d+\s*$";
            Regex dcRegex = new(dcPattern);
            Regex problemCountRegex = new(problemCountPattern);

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
                MatchCollection digitCodeMatch = dcRegex.Matches(usrDigitCodeInput);
                Match problemCountMatch = problemCountRegex.Match(usrDigitCodeInput);

                if (usrDigitCodeInput == "help")
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
                    Console.WriteLine("You can set a specific amount of problems by typing a number after the digit code, preceded by a space.");

                    Console.WriteLine();
                    Console.WriteLine("Example digit code inputs:");
                    Console.WriteLine("3+3 10");
                    Console.WriteLine("5/2.2 5");
                    Console.WriteLine("2R3.4");
                    Console.WriteLine("5^2 100");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (digitCodeMatch.Count > 0)
                {
                    // Extract groups
                    return DCUtilities.ParseDigitCodes(usrDigitCodeInput);

                    // this giant commented section of code might still be used in the future. if it is not used for three major (0.X.0) releases in a row, delete it.
                    // Last used in the making of release 0.3.0

                    // string firstNumber = digitCodeMatch[0].Groups[1].Value; // First number
                    // string operatorSymbol = digitCodeMatch[0].Groups[2].Value; // Operator
                    // string secondNumber = digitCodeMatch[0].Groups[3].Value; // Second number
                    // string? decimalPart = digitCodeMatch[0].Groups[4].Success ? digitCodeMatch[0].Groups[4].Value : null; // Decimal part (optional)

                    // SessionConfiguration.problemCount = problemCountMatch.Length == 0 ? null : int.Parse(problemCountMatch.ToString());

                    // if ("R^".Contains(operatorSymbol))
                    // {
                    //     DigitsX = int.Parse(firstNumber);
                    //     Operation = operatorSymbol[0];
                    //     DigitsY = int.Parse(secondNumber);
                    //     if ((operatorSymbol == "R" || operatorSymbol == "^") && decimalPart != null)
                    //     {
                    //         Decimals = int.Parse(decimalPart);

                    //     }
                    //     else
                    //     {
                    //         Console.WriteLine("Invalid digit code format.");
                    //     }
                    // }
                    // else if (operatorSymbol == "/")
                    // {
                    //     DigitsX = int.Parse(firstNumber);
                    //     Operation = operatorSymbol[0];
                    //     DigitsY = int.Parse(secondNumber);
                    //     if (decimalPart != null)
                    //     {
                    //         Decimals = int.Parse(decimalPart);
                    //         // return new DigitCode(digitsX, digitsY, Operation);
                    //     }
                    //     else
                    //     {
                    //         Console.WriteLine("Invalid digit code format.");
                    //     }
                    // }

                    // DigitsX = int.Parse(firstNumber);
                    // Operation = operatorSymbol[0];
                    // DigitsY = int.Parse(secondNumber);
                    // Decimals = digitCodeMatch[0].Groups[4].Success ? int.Parse(decimalPart) : 0;
                    // // return new DigitCode(digitsX, digitsY, Operation);
                }
                else
                {
                    Console.WriteLine("Invalid digit code format. (Did you forget a decimal point? Did you type a two digit number somewhere?)");
                }

            }
        }
        public override string ToString()
        {
            string boundsX;
            string boundsY;
            if (DigitsX == EInteger.FromInt32(-1))
            {
                boundsX = $"{{{LowerBoundX}..{UpperBoundX}}}";
            }
            else
            {
                boundsX = DigitsX.ToString();
            }

            if (DigitsY == EInteger.FromInt32(-1))
            {
                boundsY = $"{{{LowerBoundY}..{UpperBoundY}}}";
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
            Regex regex = new(@"\s+");
            MatchCollection matches = regex.Matches(sessionDefinition);
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
                throw new FormatException("No valid digit code found! (Did you forget a decimal point?)");
            }



            GroupCollection groups = match.Groups;
            Group[] groupArray = groups.Cast<Group>().ToArray();
            Regex rangeRegex = new(@"\{([1-9]\d*)\.\.([1-9]\d*)\}");
            for (int i = 1; i < groupArray.Length; i++)
            {

                if (groupArray[i].ToString().Contains('{') && groupArray[i].ToString().Contains('}'))
                {
                    if (i == 1)
                    {
                        Match rangeMatch = rangeRegex.Match(groupArray[i].ToString());
                        dcLowerX = int.Parse(rangeMatch.Groups[1].ToString());
                        dcUpperX = int.Parse(rangeMatch.Groups[2].ToString());
                    }
                    else
                    {
                        Match rangeMatch = rangeRegex.Match(groupArray[i].ToString());
                        dcLowerY = int.Parse(rangeMatch.Groups[1].ToString());
                        dcUpperY = int.Parse(rangeMatch.Groups[2].ToString());
                    }
                }
                else if ("+-*/^R".Contains(groupArray[i].ToString()) && i != 4) //operator
                {
                    operation = groupArray[i].ToString()[0];
                }
                else if (int.TryParse(groupArray[i].ToString(), out int num))
                {
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