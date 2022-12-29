/*
 * Copyright © 2017 mik14a <mik14a@gmail.com>
 * This work is free. You can redistribute it and/or modify it under the
 * terms of the Do What The Fuck You Want To Public License, Version 2,
 * as published by Sam Hocevar. See the COPYING file for more details.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Text.Tokenization
{
    /// <summary>
    /// Default factory of tokenizer.
    /// </summary>
    public class TokenizerFactory : TokenizerFactory<string>
    {
    }

    /// <summary>
    /// Factory of tokenizer.
    /// </summary>
    /// <typeparam name="TokenType">Type of token.</typeparam>
    public class TokenizerFactory<TokenType>
    {
        /// <summary>Regular expression options for token definition.</summary>
        public RegexOptions RegexOptions { get; }

        /// <summary>Collection of token definition.</summary>
        public IReadOnlyList<TokenDefinition<TokenType>> TokenDefinitions {
            get { return tokenDefinitions; }
        }

        /// <summary>
        /// Construct tokenizer factory.
        /// </summary>
        public TokenizerFactory() : this(RegexOptions.None) {
        }

        /// <summary>
        /// Construct tokenizer factory with regular expression options.
        /// </summary>
        /// <param name="regexOptions">Regular expression options of token definition.</param>
        public TokenizerFactory(RegexOptions regexOptions) {
            RegexOptions = regexOptions;
            tokenDefinitions = new TokenDefinitionCollection();
        }

        /// <summary>
        /// Define token definition from token type and pattern string.
        /// </summary>
        /// <param name="type">Type of token.</param>
        /// <param name="pattern">Pattern to match.</param>
        /// <returns>Instance of factory.</returns>
        public TokenizerFactory<TokenType> With(TokenType type, string pattern) {
            return With(type, pattern, RegexOptions);
        }

        /// <summary>
        /// Define token definition from token type and pattern string use special regex options.
        /// </summary>
        /// <param name="type">Type of token.</param>
        /// <param name="pattern">Pattern to match.</param>
        /// <returns>Instance of factory.</returns>
        public TokenizerFactory<TokenType> With(TokenType type, string pattern, RegexOptions regexOptions) {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern), $"{nameof(pattern)} is null.");

            tokenDefinitions.Add(new TokenDefinition<TokenType>(type, pattern, regexOptions));
            return this;
        }

        /// <summary>
        /// Create tokenizer.
        /// </summary>
        /// <param name="text">The string for tokenize.</param>
        /// <returns>Tokenizer that iterates a token.</returns>
        public IEnumerable<Token<TokenType>> Tokenize(string text) {
            if (text == null) throw new ArgumentNullException(nameof(text), $"{nameof(text)} is null.");

            return new Tokenizer<TokenType>(tokenDefinitions.ToArray(), text);
        }

        readonly TokenDefinitionCollection tokenDefinitions;

        class TokenDefinitionCollection : KeyedCollection<TokenType, TokenDefinition<TokenType>>
        {
            protected override TokenType GetKeyForItem(TokenDefinition<TokenType> item) {
                return item.Type;
            }
        }

    }
}
