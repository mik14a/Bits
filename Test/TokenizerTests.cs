using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Tokenization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.Text.Tokenization.Tests
{
    [TestClass()]
    public class TokenizerTests
    {
        [TestMethod()]
        public void Usage() {

            var message = "The quick brown fox jumps over the lazy dog";
            var word = TokenDefinition.Create("word", @"\w+");
            var space = TokenDefinition.Create("space", @"\s+");
            var definitions = new[] { word, space };
            var tokens = Tokenizer.Tokenize(definitions, message);
            var words = tokens.Where(t => t.Type == "word");

            var expected = message.Split(new[] { " " }, StringSplitOptions.None);
            var actual = words.Select(t => t.Value).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
