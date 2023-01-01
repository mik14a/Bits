# E4

Single-file public domain libraries for [CLI](https://en.wikipedia.org/wiki/Common_Language_Infrastructure) to myself.
This little library of bits and pieces everybody wrote everytime, everywhere, everyplace.

## How to use

Copy and past from this repository.

## Libraries

| Name                                      | Description                                  |
|-------------------------------------------|----------------------------------------------|
| System.ComponentModel.Plugin              | Simple plugin support to application.        |
| System.Text.Humanize                      | To human readable number with binary prefix. |
| System.Text.Tokenization.Tokenizer        | Simple tokenizer.                            |
| System.Text.Tokenization.TokenizerFactory | Factory of tokenizer.                        |
| System.Scripting.Calculator               | Simple calculator                            |

### System.Text.Tokenization.Tokenizer

```cs
var message = "The quick brown fox jumps over the lazy dog";
var word = TokenDefinition.Create("word", @"\w+");
var space = TokenDefinition.Create("space", @"\s+");
var definitions = new[] { word, space };
var tokens = Tokenizer.Tokenize(definitions, message);
var words = tokens.Where(t => t.Type == "word");
```

### System.Text.Tokenization.TokenizerFactory

It depends on the Tokenizer.

```cs
var message = "The quick brown fox jumps over the lazy dog";
var tokenizer = new TokenizerFactory<int>()
    .With(0, @"[a-z]+", RegexOptions.IgnoreCase)
    .With(1, @"\s+");
var tokens = tokenizer.Tokenize(message);
var words = tokens.Where(t => t.Type == 0);
```

### System.Scripting.Calculator

```cs
Console.WriteLine(Calculator.Calc("1 + 2 + 3 + 4"));
```

## License

[WTFPL](http://www.wtfpl.net/)

## Author

[mik14a](https://github.com/mik14a)
