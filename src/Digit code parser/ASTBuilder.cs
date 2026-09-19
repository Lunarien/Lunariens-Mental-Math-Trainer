using static Lunariens_Mental_Math_Trainer.Tokenizer;

namespace Lunariens_Mental_Math_Trainer
{
    internal static class ASTBuilder
    {
        internal enum UnaryOp
        {
            Negate
        }
        internal enum BinaryOp
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Power,
            Root
        }
        internal abstract record Expression
        {
            internal record Number(int Value) : Expression;
            internal record Unary(UnaryOp Op, Expression Operand) : Expression;
            internal record Binary(BinaryOp Op, Expression Left, Expression Right, int? Precision = null) : Expression;
            internal record Range(int Start, int End) : Expression;
        }

        private static string StringTokenValues(TokenType[] tokenTypes, bool quotes = true)
        {
            Dictionary<TokenType, string> tokenValues = [];
            tokenValues.Add(TokenType.Number, "<Number>");
            tokenValues.Add(TokenType.Plus, "+");
            tokenValues.Add(TokenType.Minus, "-");
            tokenValues.Add(TokenType.Star, "*");
            tokenValues.Add(TokenType.Slash, "/");
            tokenValues.Add(TokenType.Caret, "^");
            tokenValues.Add(TokenType.Root, "R");
            tokenValues.Add(TokenType.LParen, "(");
            tokenValues.Add(TokenType.RParen, ")");
            tokenValues.Add(TokenType.DotNumber, ".<Number>");
            tokenValues.Add(TokenType.LCurly, "{");
            tokenValues.Add(TokenType.RCurly, "}");
            tokenValues.Add(TokenType.Range, "..");

            string s = "";
            foreach (TokenType type in tokenTypes)
            {
                if (quotes)
                    s += $"\"{tokenValues[type]}\", ";
                else
                    s += $"{tokenValues[type]}, ";
            }
            s = s.Trim([',', ' ']);
            return s;
        }

        private static Token NextToken(ref List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                throw new DigitCodeException("Unexpected end of digit code");
            }
            Token token = tokens[0];
            tokens.RemoveAt(0);
            return token;
        }

        private static Token NextTokenFiltered(ref List<Token> tokens, TokenType[] filter)
        {
            Token token = NextToken(ref tokens);
            if (!filter.Contains(token.tokenType))
                throw new DigitCodeException($"Expected one of: {StringTokenValues(filter)}; found {StringTokenValues([token.tokenType])} instead.");
            return token;
        }
        private static Expression.Range ParseRange(ref List<Token> tokens)
        {
            // NextTokenFiltered(ref tokens, [TokenType.LCurly]);
            int rangeMin = NextTokenFiltered(ref tokens, [TokenType.Number]).value!.Value;
            Token next = NextTokenFiltered(ref tokens, [TokenType.Range, TokenType.RCurly]);
            if (next.tokenType == TokenType.RCurly) //other option is TokenType.Range
            {
                return new Expression.Range(rangeMin, rangeMin);
            }
            int rangeMax = NextTokenFiltered(ref tokens, [TokenType.Number]).value!.Value;
            NextTokenFiltered(ref tokens, [TokenType.RCurly]);
            return new Expression.Range(rangeMin, rangeMax);
        }

        //TODO: have the parsing methods report back the position of the errors (exceptions)! do so by using the index from the end (based on the remaining tokens.Count)
        private static Expression ParseNumber(ref List<Token> tokens)
        {
            // Console.WriteLine("DEBUG NUMBER: " + string.Join(", ", tokens.Select((t) => $"[Type: {t.tokenType}, Val: {t.value}]")));
            Token token = NextTokenFiltered(ref tokens, [TokenType.Number, TokenType.LCurly]);
            if (token.tokenType == TokenType.Number)
            {
                return new Expression.Number(token.value!.Value);
            }
            return ParseRange(ref tokens);
        }

        private static int? ParseDotNumber(ref List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                return null;
            }
            Token token = tokens[0];
            if (token.tokenType != TokenType.DotNumber)
            {
                return null;
            }
            tokens.RemoveAt(0);
            return token.value!.Value;
        }
        private static Expression ParseExponentiation(ref List<Token> tokens)
        {
            Expression left = ParseParentheses(ref tokens);
            while (tokens.Count > 0 && (tokens[0].tokenType == TokenType.Caret || tokens[0].tokenType == TokenType.Root))
            {
                BinaryOp op = (NextToken(ref tokens).tokenType == TokenType.Caret) ? BinaryOp.Power : BinaryOp.Root;

                Expression right = ParseParentheses(ref tokens);
                int? precision = ParseDotNumber(ref tokens);

                if (op == BinaryOp.Power)
                {
                    left = precision == null ? new Expression.Binary(op, left, right) : new Expression.Binary(op, left, right, precision);
                }
                else
                {
                    if (precision == null)
                        throw new DigitCodeException("Expected to find a precision indicator (.N)");
                    left = new Expression.Binary(op, left, right, precision);
                }
            }
            return left;
        }

        private static Expression ParseNegation(ref List<Token> tokens)
        {
            if (tokens.Count > 0 && tokens[0].tokenType == TokenType.Minus)
            {
                Token leftPrefix = NextToken(ref tokens);
                Expression left = ParseExponentiation(ref tokens);
                if (leftPrefix.tokenType == TokenType.Minus)
                {
                    left = new Expression.Unary(UnaryOp.Negate, left);
                }
                return left;
            }
            else
            {
                return ParseExponentiation(ref tokens);
            }
        }

        private static Expression ParseMultiplication(ref List<Token> tokens)
        {
            // Console.WriteLine("DEBUG MULTIPLICATION: " + string.Join(", ", tokens.Select((t) => $"[Type: {t.tokenType}, Val: {t.value}]")));
            Expression left = ParseNegation(ref tokens);
            while (tokens.Count > 0 && (tokens[0].tokenType == TokenType.Star || tokens[0].tokenType == TokenType.Slash))
            {
                BinaryOp op = (NextToken(ref tokens).tokenType == TokenType.Star) ? BinaryOp.Multiply : BinaryOp.Divide;

                Expression right = ParseNegation(ref tokens);
                int? precision = ParseDotNumber(ref tokens);

                if (op == BinaryOp.Multiply)
                {
                    left = precision == null ? new Expression.Binary(op, left, right) : new Expression.Binary(op, left, right, precision);
                }
                else
                {
                    if (precision == null)
                        throw new DigitCodeException("Expected to find a precision indicator (.N)");
                    left = new Expression.Binary(op, left, right, precision);
                }
            }
            return left;
        }
        private static Expression ParseAddition(ref List<Token> tokens) //TODO: make private after testing, then make a Build method that calls this
        {
            // Console.WriteLine("DEBUG ADDITION: " + string.Join(", ", tokens.Select((t) => $"[Type: {t.tokenType}, Val: {t.value}]")));
            Expression left = ParseMultiplication(ref tokens);
            while (tokens.Count > 0 && (tokens[0].tokenType == TokenType.Plus || tokens[0].tokenType == TokenType.Minus))
            {
                BinaryOp op = (NextToken(ref tokens).tokenType == TokenType.Plus) ? BinaryOp.Add : BinaryOp.Subtract;
                Expression right = ParseMultiplication(ref tokens);
                int? precision = ParseDotNumber(ref tokens);

                left = precision == null ? new Expression.Binary(op, left, right) : new Expression.Binary(op, left, right, precision);
            }
            return left;
        }
        private static Expression ParseParentheses(ref List<Token> tokens)
        {
            if (tokens.Count == 0)
                throw new DigitCodeException("Unexpected end of digit code");
            if (tokens[0].tokenType == TokenType.LParen)
            {
                NextToken(ref tokens);
                Expression innerExpression = ParseAddition(ref tokens);
                NextTokenFiltered(ref tokens, [TokenType.RParen]);
                return innerExpression;
            }
            else
            {
                return ParseNumber(ref tokens);
            }
        }
        
        private static string[] TokensToValues(List<Token> tokens)
        {
            List<string> tokenValues = [];
            foreach (Token token in tokens)
            {
                if (token.tokenType == TokenType.Number)
                    tokenValues.Add(token.value!.Value.ToString());
                else if (token.tokenType == TokenType.DotNumber)
                    tokenValues.Add($".{token.value!.Value}");
                else
                    tokenValues.Add(StringTokenValues([token.tokenType], false));
            }
            return tokenValues.ToArray();
        }

        internal static Expression Build(ref List<Token> tokens)
        {
            Expression expr = ParseAddition(ref tokens);
            if (tokens.Count > 0)
            {
                string[] values = TokensToValues(tokens);
                string valuesString = "";
                foreach (string value in values)
                {
                    valuesString += value;
                }
                throw new DigitCodeException($"Error: Failed to parse the following characters: {valuesString}");
            }
            return expr;
        }
    }
}