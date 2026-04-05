using BenchmarkDotNet.Running;
using Vali_Flow.Core.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
