using System;
using System.Data.Common;
using System.Globalization;
using PeterO.Numbers;


namespace Lunarien_s_Mental_Math_Trainer
{
    internal class Program
    {
        public class DigitCode
        {
            //variables that store digit amounts of X and Y, the operation and optionally the decimal digits
            public int digitsX;
            public int digitsY;
            public char operation;
            public int decimals;
            public void Get()
            {                
                char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
                char[] operations = { '+', '-', '*', '/', '^', 'R' };

                Console.WriteLine("Enter a digit code. A digit code specifies what problem type you'd like to train. Enter \"help\" to get more info.");

                while (true)
                {
                    string usrDigitCode = Console.ReadLine();
                    if (usrDigitCode.Length == 3) //then check if the first and the third symbol are numbers and the second is an operation.
                    {
                        if ((numbers.Contains(usrDigitCode[0])) & (operations.Contains(usrDigitCode[1])) & (numbers.Contains(usrDigitCode[2])))
                        {
                            digitsX = int.Parse(usrDigitCode[0].ToString());
                            operation = usrDigitCode[1];
                            digitsY = int.Parse(usrDigitCode[2].ToString());
                            return;
                        }
                        else
                            Console.WriteLine("Invalid digit code.");
                    } //if decimals are specified, check if fifth symbol is a valid number and whether there is a dot symbol before it.
                    else if (usrDigitCode.Length == 5)
                    {
                        if (numbers.Contains(usrDigitCode[0]) & operations.Contains(usrDigitCode[1]) & numbers.Contains(usrDigitCode[2]) & usrDigitCode[3] == '.' & numbers.Contains(usrDigitCode[4]))
                        {
                            digitsX = int.Parse(usrDigitCode[0].ToString());
                            operation = usrDigitCode[1];
                            digitsY = int.Parse(usrDigitCode[2].ToString());
                            decimals = int.Parse(usrDigitCode[4].ToString());
                            return;
                        }
                        else
                            Console.WriteLine("Invalid digit code.");
                    }
                    else if (usrDigitCode == "help")
                    {
                        Console.WriteLine("A digit code looks like the following:");
                        Console.WriteLine("XopY.Z");
                        Console.WriteLine("X and Y represent the number of digits for the first and second random number.");
                        Console.WriteLine("op means operation, and can be one of the following: + - * / ^ R");
                        Console.WriteLine("the ^ operator makes Y, the second number, represent the actual typed number.\nThis has to be a whole number.");
                        Console.WriteLine("So if it was 2, then it is 2 and not a random 2 digit number.");
                        Console.WriteLine("The R operator means root, and makes X represent the base of the root.");
                        Console.WriteLine("So if X was 3, then the operation is a cube root.");
                        Console.WriteLine("The last two symbols are required for / and R operations.");
                        Console.WriteLine("They represent the amount of decimal digits needed in the result.");
                        Console.WriteLine("The dot is required if you choose to limit the decimal digits. Z is the amount of decimals.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid digit code format");
                    }
                }
            }
            
        }
        static void Main(string[] args)
        {
            // 1) get digit code
            DigitCode usrDC = new();
            usrDC.Get();
            Console.Clear();
            // 2) make random numbers corresponding to the number of digits in the digit code
            Random randomness = new();
            while (true)
            {
                int xRangeBottom = (int)Math.Round(Math.Pow(10, usrDC.digitsX - 1), 0); //the bottom of the range for X. it is 10^(DigitsX - 1).
                int xRangeTop = (int)Math.Round(Math.Pow(10, usrDC.digitsX), 0); //the top of the range for X. it is 10^DigitsX
                int x = randomness.Next(xRangeBottom, xRangeTop);

                int yRangeBottom = (int)Math.Round(Math.Pow(10, usrDC.digitsY - 1), 0);
                int yRangeTop = (int)Math.Round(Math.Pow(10, usrDC.digitsY), 0);
                int y = randomness.Next(yRangeBottom, yRangeTop);
                // 3) make a problem with the random numbers and the operation
                string problem = x.ToString() + usrDC.operation + y.ToString();
                // 3.5) precompute the correct solution
                int? intResult = null;
                decimal? decResult = null;
                switch (usrDC.operation)
                {
                    case '+':
                        intResult = x + y;
                        break;
                    case '-':
                        if (x < y)
                            intResult = y - x;
                        else
                            intResult = x - y;
                        break;
                    case '*':
                        intResult = x * y;
                        break;
                    case '/':
                        decResult = Math.Round((decimal)(x / y), usrDC.decimals, MidpointRounding.ToZero);
                        break;
                    case '^':
                        intResult = (int)Math.Pow(x, y);
                        break;
                    case 'R':
                        decResult = Math.Round((decimal)Math.Pow(x, 1 / y), usrDC.decimals, MidpointRounding.ToZero);
                        break;
                    default:
                        intResult = 0;
                        decResult = 0.0m;
                        break;
                }
                // 4) ask the user to solve the problem. verify answer. give evaluation (correct/wrong).
                Console.WriteLine(problem);
                Console.Write("Your result: ");
                
                string usrResult = Console.ReadLine();
                if (intResult != null) //if the result is an int. in other words, if there is a result that is of type int.
                {
                    if (int.TryParse(usrResult, out int _))
                    {
                        if (int.Parse(usrResult) == intResult)
                            Console.WriteLine("Correct");
                        else
                            Console.WriteLine("Wrong, correct was: " + intResult.ToString());
                    } 
                    
                }
                else if (decResult != null) //if the result is a decimal
                {
                    decimal decUsrResult; //its a fix for something
                    decimal.TryParse(usrResult, out decUsrResult);
                    if (decUsrResult == decResult)
                        Console.WriteLine("Correct");
                    else
                        Console.WriteLine("Wrong, correct was: " + decResult.ToString());
                }
                // 5) repeat 2-4 until the user quits
            }
        }
    }
}
