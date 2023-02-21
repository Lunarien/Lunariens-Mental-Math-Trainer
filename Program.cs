using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;


namespace Lunarien_s_Mental_Math_Trainer
{
    internal class Program
    {
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

                Console.WriteLine("Enter a digit code. A digit code specifies what problem type you'd like to train. Enter \"help\" to get more info.");

                while (true)
                {
                    string usrDigitCode = Console.ReadLine();
                    if (usrDigitCode.Length == 3 && !usrDigitCode.Contains('/') && !usrDigitCode.Contains('R')) //then check if the first and the third symbol are numbers and the second is an operation.
                    {
                        if ((numbers.Contains(usrDigitCode[0])) & (operations.Contains(usrDigitCode[1])) & (numbers.Contains(usrDigitCode[2])))
                        {
                            DigitsX = int.Parse(usrDigitCode[0].ToString());
                            Operation = usrDigitCode[1];
                            DigitsY = int.Parse(usrDigitCode[2].ToString());
                            return;
                        }
                        else
                            Console.WriteLine("Invalid digit code.");
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
        public static ulong RandomUlong(ulong bottom, ulong top)
        {
            Random randomness = new();
            ulong rangeSize = top - bottom;

            byte[] buf = new byte[8];
            randomness.NextBytes(buf);
            ulong result = (BitConverter.ToUInt64(buf, 0)%rangeSize)+bottom;
            return result;
        }
        static void Main(string[] args)
        {
            IFormatProvider ifp;
            ifp = new CultureInfo("en-US");
            Console.ForegroundColor = ConsoleColor.White;
            // 1) get digit code
            DigitCode usrDC = new();
            usrDC.Get();
            Console.Clear();
            while (true)
            {
                // 2) make random numbers corresponding to the number of digits in the digit code
                ulong xRangeBottom = Convert.ToUInt64(Math.Round(Math.Pow(10, usrDC.DigitsX - 1), 0)); //the bottom of the range for X. it is 10^(DigitsX - 1).
                ulong xRangeTop = Convert.ToUInt64(Math.Round(Math.Pow(10, usrDC.DigitsX), 0)); //the top of the range for X. it is 10^DigitsX
                ulong x = RandomUlong(xRangeBottom, xRangeTop);

                ulong yRangeBottom = Convert.ToUInt64(Math.Round(Math.Pow(10, usrDC.DigitsY - 1), 0));
                ulong yRangeTop = Convert.ToUInt64(Math.Round(Math.Pow(10, usrDC.DigitsY), 0));
                ulong y = RandomUlong(yRangeBottom, yRangeTop);

                // 3) make a problem with the random numbers and the operation
                string problem;
                if (usrDC.Operation == '^')
                {
                    problem = x.ToString() + usrDC.Operation + usrDC.DigitsY.ToString();
                }
                else if (usrDC.Operation == 'R')
                {
                    if (usrDC.DigitsX == 2)
                    {
                        problem = '√' + y.ToString();
                    }
                    else
                    {
                        problem = usrDC.DigitsX.ToString() + '√' + y.ToString();
                    }
                    
                }
                else 
                {
                    problem = x.ToString() + usrDC.Operation + y.ToString();
                }
                // 3.5) precompute the correct solution
                ulong? intResult = null;
                decimal? decResult = null;
                switch (usrDC.Operation)
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
                        decResult = Math.Round((decimal)(Convert.ToDecimal(x) / Convert.ToDecimal(y)), usrDC.Decimals, MidpointRounding.ToZero);
                        break;
                    case '^':
                        intResult = (ulong)Math.Pow(x, usrDC.DigitsY);
                        break;
                    case 'R':
                        decResult = Math.Round((decimal)Math.Pow(Convert.ToDouble(y), 1 / Convert.ToDouble(usrDC.DigitsX)), usrDC.Decimals, MidpointRounding.ToZero);
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
                if (intResult != null && usrResult != "") //if the result is an int. in other words, if there is a result that is of type int.
                {
                    if (ulong.TryParse(usrResult, out ulong _))
                    {
                        if (ulong.Parse(usrResult) == intResult)
                        {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Correct");
                        Console.ForegroundColor = ConsoleColor.White;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine("Wrong, correct was: " + intResult.ToString());
                        }
                    }
                    
                }
                else if (decResult != null && usrResult != "") //if the result is a decimal
                {
                    
                    
                    if (decResult == decimal.Parse(usrResult, CultureInfo.InvariantCulture))
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Correct");
                        Console.ForegroundColor = ConsoleColor.White;
                    }  
                    else
                    {
                        Console.Clear();
                        Console.WriteLine("Wrong, correct was: " + decResult?.ToString(ifp));
                    }
                        
                }
                else
                {
                    Console.Clear();
                    Console.WriteLine("Invalid result.");
                }
                // 5) repeat 2-4 until the user quits (to be implemented)
            }
        }
    }
}
