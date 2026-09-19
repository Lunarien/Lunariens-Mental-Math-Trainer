namespace Lunariens_Mental_Math_Trainer
{
    internal static class Tokenizer
    {
        internal enum TokenType
        {
            Number,
            Plus,
            Minus,
            Star,
            Slash,
            Caret,
            Root,
            LParen,
            RParen,
            DotNumber,
            LCurly,
            RCurly,
            Range
        }

        internal class Token
        {
            internal readonly TokenType tokenType;
            internal readonly int? value;
            internal Token(TokenType tokenType, int? value = null)
            {
                this.tokenType = tokenType;
                this.value = value;
            }
        }

        internal static (int, int) ConsumeNumber(string inp, int index)
        {
            string number = "";
            for (int i = index; i < inp.Length; i++)
            {
                // Console.WriteLine($"DEBUG: {inp[i]} {i}");
                if (char.IsNumber(inp[i]))
                {
                    number += inp[i];
                    // Console.WriteLine($"DEBUG: successfully added {inp[i]} to number");
                }
                else
                {
                    // Console.WriteLine("else block: " + number);
                    return (int.Parse(number), i);
                }
            }
            return (int.Parse(number), inp.Length);
        }
        
        internal static List<Token> Tokenize(string inp)
        {
            List<Token> tokens = [];
            int i = 0;
            while (i < inp.Length)
            {
                char character = inp[i];
                switch (character)
                {
                    case '+':
                        tokens.Add(new Token(TokenType.Plus));
                        i += 1;
                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.Minus));
                        i += 1;
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.Star));
                        i += 1;
                        break;
                    case '/':
                        tokens.Add(new Token(TokenType.Slash));
                        i += 1;
                        break;
                    case '^':
                        tokens.Add(new Token(TokenType.Caret));
                        i += 1;
                        break;
                    case 'R':
                        tokens.Add(new Token(TokenType.Root));
                        i += 1;
                        break;
                    case '(':
                        tokens.Add(new Token(TokenType.LParen));
                        i += 1;
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RParen));
                        i += 1;
                        break;
                    case '{':
                        tokens.Add(new Token(TokenType.LCurly));
                        i += 1;
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RCurly));
                        i += 1;
                        break;
                    case var c when char.IsNumber(c):
                        {
                            (int number, int index) = ConsumeNumber(inp, i);
                            tokens.Add(new Token(TokenType.Number, number));
                            i = index;
                        }
                        break;
                    case '.':
                        if (i + 1 >= inp.Length)
                            throw new DigitCodeException("Digit code cannot end with a dot");

                        if (inp[i + 1] == '.')
                        {
                            tokens.Add(new Token(TokenType.Range));
                            i += 2;
                        }
                        else if (char.IsNumber(inp[i + 1]))
                        {
                            (int number, int index) = ConsumeNumber(inp, i + 1);
                            tokens.Add(new Token(TokenType.DotNumber, number));
                            i = index;
                        }
                        else
                        {
                            throw new DigitCodeException($"Invalid character found after dot: {inp[i + 1]}");
                        }
                        break;
                    default:
                        throw new DigitCodeException($"Invalid character found: {inp[i]}");
                }
            }
            return tokens;
        }
    }
}