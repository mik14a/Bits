/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Tokenization;

namespace System.Scripting
{
    /// <summary>
    /// Calculator using Reverse Polish Notation.
    /// </summary>
    public class Calculator
    {
        /// <summary>
        /// Calculate math expression.
        /// </summary>
        /// <param name="expression">Expression string.</param>
        /// <returns>Calculation result.</returns>
        public static int Calc(string expression) {
            var notation = Rpn.Parse(expression);
            return Rpn.Calc(notation);
        }

        public class Rpn
        {
            public enum TokenType
            {
                Num, Add, Sub, Mul, Div, Sp
            }

            public static int Calc(IEnumerable<Token<TokenType>> notation) {
                var stack = new Stack<int>();
                foreach (var token in notation) {
                    if (token.Type == TokenType.Num) {
                        stack.Push(Convert.ToInt32(token.Value));
                    } else {
                        stack.Push(_operator[token.Type](stack.Pop(), stack.Pop()));
                    }
                }
                Debug.Assert(stack.Count == 0 || stack.Count == 1);
                return 0 < stack.Count ? stack.Pop() : 0;
            }

            public static IEnumerable<Token<TokenType>> Parse(string expression) {
                var stack = new Stack<Token<TokenType>>();
                var tokens = _tokenizer.Tokenize(expression).Where(t => t.Type != TokenType.Sp);
                foreach (var token in tokens) {
                    if (token.Type == TokenType.Num) {
                        yield return token;
                    } else if (stack.Count == 0) {
                        stack.Push(token);
                    } else if (stack.Peek().Type < token.Type) {
                        stack.Push(token);
                    } else {
                        while (0 < stack.Count && token.Type < stack.Peek().Type) {
                            yield return stack.Pop();
                        }
                        stack.Push(token);
                    }
                }
                while (0 < stack.Count) {
                    yield return stack.Pop();
                }
            }

            /// <summary>Tokenizer factory.</summary>
            static readonly TokenizerFactory<TokenType> _tokenizer
                = new TokenizerFactory<TokenType>(RegexOptions.None)
                    .With(TokenType.Num, @"[1-9][0-9]*")
                    .With(TokenType.Add, @"[+]")
                    .With(TokenType.Sub, @"[-]")
                    .With(TokenType.Mul, @"[*]")
                    .With(TokenType.Div, @"[/]")
                    .With(TokenType.Sp, @"\s+");

            /// <summary>Numeric operator.</summary>
            static readonly Dictionary<TokenType, Func<int, int, int>> _operator
                = new Dictionary<TokenType, Func<int, int, int>>() {
                    { TokenType.Add, (m, n) => n + m },
                    { TokenType.Sub, (m, n) => n - m },
                    { TokenType.Mul, (m, n) => n * m },
                    { TokenType.Div, (m, n) => n / m },
                };
        }

    }
}
