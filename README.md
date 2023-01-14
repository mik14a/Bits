# E4

Single-file public domain libraries for [CLI](https://en.wikipedia.org/wiki/Common_Language_Infrastructure) to myself.
This little library of bits and pieces everybody wrote everytime, everywhere, everyplace.

## How to use

Copy and past from this repository.

## Libraries

| Name                                                         | Description                                  |
|--------------------------------------------------------------|----------------------------------------------|
| Microsoft.VisualStudio.TestTools.UnitTesting.BenchmarkRunner | Benchmark runner.                             |
| System.ComponentModel.Plugin                                 | Simple plugin support to application.        |
| System.Text.Humanize                                         | To human readable number with binary prefix. |
| System.Text.Tokenization.Tokenizer                           | Simple tokenizer.                            |
| System.Text.Tokenization.TokenizerFactory                    | Factory of tokenizer.                        |
| System.Scripting.Calculator                                  | Simple calculator.                            |

### Microsoft.VisualStudio.TestTools.UnitTesting.BenchmarkRunner

Source

```cs
[BenchmarkClass]
class RandomBenchmark
{
    [BenchmarkMethod]
    public void DefaultBenchmark() {
        var generator = new Random();
        _ = Enumerable.Repeat(generator, 100000).Select(r => r.Next()).ToArray();
    }
    [BenchmarkMethod]
    public void RNGCryptoBenchmark() {
        var generator = RandomNumberGenerator.Create("System.Security.Cryptography.RNGCryptoServiceProvider");
        var buffer = new byte[sizeof(int)];
        _ = Enumerable.Repeat(generator, 100000).Select(r => { r.GetBytes(buffer); return buffer; }).Select(b => BitConverter.ToInt32(b, 0)).ToArray();
    }
}

var benchmarkRunner = new BenchmarkRunner();
benchmarkRunner.Run(Assembly.GetEntryAssembly());
```

Output

```console
Start benchmark assembly [ Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null ].
  Start benchmark class [ RandomBenchmark ]: Instancing 636 Ticks.
    Start benchmark [ DefaultBenchmark ] for 100 times.
    End benchmark [ DefaultBenchmark ]: Total: 328.43 Milliseconds, Average: 3.28 Milliseconds, Min: 2.39 Milliseconds, Max: 5.74 Milliseconds.
    Start benchmark [ RNGCryptoBenchmark ] for 100 times.
    End benchmark [ RNGCryptoBenchmark ]: Total: 2.81 Seconds, Average: 28.09 Milliseconds, Min: 25.47 Milliseconds, Max: 33.08 Milliseconds.
  End benchmark: Total 3.14 Seconds.
End benchmark.
```

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
