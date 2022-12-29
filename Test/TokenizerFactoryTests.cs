using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Text.Tokenization.Tests
{
    [TestClass]
    public class TokenizerFactoryTests
    {
        [TestMethod]
        public void Usage() {
            var message = "The quick brown fox jumps over the lazy dog";
            var tokenizer = new TokenizerFactory<int>()
                .With(0, @"[a-z]+", RegexOptions.IgnoreCase)
                .With(1, @"\s+");
            var tokens = tokenizer.Tokenize(message);
            var words = tokens.Where(t => t.Type == 0);

            var expected = message.Split(new[] { " " }, StringSplitOptions.None);
            var actual = words.Select(t => t.Value).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
