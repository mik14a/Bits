/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace System.Text.Tokenization
{
    /// <summary>
    /// TokenDefinition factories.
    /// </summary>
    public static class TokenDefinition
    {
        /// <summary>
        /// Create TokenDefinition.
        /// </summary>
        /// <typeparam name="TokenType">Type of token.</typeparam>
        /// <param name="type">Token type of definition.</param>
        /// <param name="pattern">Pattern to match.</param>
        /// <returns>New instance of TokenDefinition.</returns>
        public static TokenDefinition<TokenType> Create<TokenType>(TokenType type, string pattern)
            => Create(type, pattern, RegexOptions.None);

        /// <summary>
        /// Create TokenDefinition.
        /// </summary>
        /// <typeparam name="TokenType">Type of token.</typeparam>
        /// <param name="type">Token type of definition.</param>
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="regexOptions">A bitwise options the regular expression.</param>
        /// <returns>New instance of TokenDefinition.</returns>
        public static TokenDefinition<TokenType> Create<TokenType>(TokenType type, string pattern, RegexOptions regexOptions)
            => new TokenDefinition<TokenType>(type, pattern, regexOptions);
    }


    /// <summary>
    /// Tokenizer factory.
    /// </summary>
    /// <typeparam name="TokenType">Type of token.</typeparam>
    public static class Tokenizer
    {
        /// <summary>
        /// Create tokenizer.
        /// </summary>
        /// <typeparam name="TokenType">Type of token.</typeparam>
        /// <param name="definitions">Token definition for tokenize.</param>
        /// <param name="text">Tokenize source input.</param>
        /// <returns>New instance of tokenizer.</returns>
        public static IEnumerable<Token<TokenType>> Tokenize<TokenType>(TokenDefinition<TokenType>[] definitions, string text)
            => new Tokenizer<TokenType>(definitions, text);
    }


    /// <summary>
    /// Definition of token.
    /// </summary>
    /// <typeparam name="TokenType">Type of token.</typeparam>
    public class TokenDefinition<TokenType>
    {
        /// <summary>Definition type.</summary>
        public TokenType Type { get; }
        /// <summary>Definition pattern.</summary>
        public string Pattern { get; }
        /// <summary>Regex pattern.</summary>
        public Regex Regex { get; }

        /// <summary>
        /// Construct token definition with token type and regular expression pattern.
        /// </summary>
        /// <param name="type">Token type of definition.</param>
        /// <param name="pattern">Pattern to match.</param>
        /// <param name="regexOptions">A bitwise options the regular expression.</param>
        public TokenDefinition(TokenType type, string pattern, RegexOptions regexOptions) {
            if (type == null) throw new ArgumentNullException(nameof(type), $"{nameof(type)} is null.");
            if (pattern == null) throw new ArgumentNullException(nameof(pattern), $"{nameof(pattern)} is null.");

            Type = type;
            Pattern = pattern;
            Regex = new Regex(pattern, RegexOptions.Compiled | regexOptions);
        }

        /// <summary>
        /// Get matched value at the specified starting position in the string.
        /// </summary>
        /// <param name="input">The string to match.</param>
        /// <param name="startat">The zero-based character position at which to start the search.</param>
        /// <returns>Matched string at a start position.</returns>
        public string Match(string input, int startat) {
            if (input == null) throw new ArgumentNullException(nameof(input), $"{nameof(input)} is null.");

            var match = Regex.Match(input, startat);
            var success = match.Success && match.Index == startat;
            return success ? match.Value : null;
        }
    }


    /// <summary>
    /// Tokenizer.
    /// </summary>
    /// <typeparam name="TokenType">Type of token.</typeparam>
    public class Tokenizer<TokenType> : IEnumerable<Token<TokenType>>, IEnumerator<Token<TokenType>>
    {
        /// <summary>Current token.</summary>
        public Token<TokenType> Current { get; set; }

        /// <summary>
        /// Construct tokenizer with token definition and target string.
        /// </summary>
        /// <param name="definitions">Token definition for tokenize.</param>
        /// <param name="text">Tokenize source input.</param>
        public Tokenizer(TokenDefinition<TokenType>[] definitions, string text) {
            if (definitions == null || definitions.Length == 0) throw new ArgumentException($"{nameof(definitions)} is null or empty.", nameof(definitions));
            if (text == null) throw new ArgumentNullException(nameof(text), $"{nameof(text)} is null.");

            this.definitions = definitions;
            this.text = text;
            index = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Token<TokenType>> GetEnumerator() => this;

        /// <summary>
        /// Performs associated with freeing resources.
        /// </summary>
        public void Dispose() {
            definitions = null;
            text = null;
            index = -1;
            Current = null;
        }

        /// <summary>
        /// Advances the enumerator to the next token of the collection.
        /// </summary>
        /// <returns>
        /// true if the enumerator was successfully advanced to the next token;
        /// false if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext() {
            if (index == -1 || definitions == null || text == null) {
                return false;
            }

            foreach (var definition in definitions) {
                var value = definition.Match(text, index);
                if (value != null) {
                    Current = new Token<TokenType>(index, definition.Type, value);
                    index += value.Length;
                    return true;
                }
            }
            index = -1;
            return false;
        }

        /// <summary>
        /// Sets the enumerator to its initial position,
        /// which is before the first token in the collection.
        /// </summary>
        public void Reset() {
            index = 0;
            Current = null;
        }

        object IEnumerator.Current => Current;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        TokenDefinition<TokenType>[] definitions;
        string text;
        int index;
    }


    /// <summary>
    /// Token.
    /// </summary>
    /// <typeparam name="TokenType"></typeparam>
    [DebuggerDisplay("Index = {Index}, Type = {Type}, Value = {Value}")]
    public class Token<TokenType>
    {
        /// <summary>Index of token.</summary>
        public int Index { get; }
        /// <summary>Type of token.</summary>
        public TokenType Type { get; }
        /// <summary>Value of token.</summary>
        public string Value { get; }

        /// <summary>
        /// Construct token.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="type">Type</param>
        /// <param name="value">Value.</param>
        public Token(int index, TokenType type, string value) {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"{nameof(value)} is null or empty.", nameof(value));

            Index = index;
            Type = type;
            Value = value;
        }
    }

}
