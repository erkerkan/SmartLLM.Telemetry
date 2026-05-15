using BenchmarkDotNet.Attributes;
using SmartLLM.Telemetry.Tokenizer;

namespace SmartLLM.Telemetry.Benchmarks;

[MemoryDiagnoser]
public class TokenCounterBenchmark
{
    private readonly OfflineTokenCounter _counter = new();
    private readonly string _text = new('x', 4096);

    [Benchmark]
    public int CountTokens() => _counter.CountTokens("gpt-4o-mini", _text);
}
