using BenchmarkDotNet.Running;
using OpenAutoMapper.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(FlatMappingBenchmarks).Assembly).Run(args);
