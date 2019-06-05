using System;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using S = System.Linq.Expressions.Expression;
using L = FastExpressionCompiler.LightExpression.Expression;
using M = FastExpressionCompiler.LighterExpression.LighterExpression;

namespace FastExpressionCompiler.Benchmarks
{
    /*
    ## Initial results: 80% win for memory usage without performance degradation

    **Note:** System Expression uses static cache in .Net Core - so its memory is not particularly comparable here.

     BenchmarkDotNet=v0.11.3, OS=Windows 10.0.17134.765 (1803/April2018Update/Redstone4)
    Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
    Frequency=2156250 Hz, Resolution=463.7681 ns, Timer=TSC
    .NET Core SDK=3.0.100-preview3-010431
      [Host]     : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT
      DefaultJob : .NET Core 2.1.11 (CoreCLR 4.6.27617.04, CoreFX 4.6.27617.02), 64bit RyuJIT


            Method |       Mean |     Error |    StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
    -------------- |-----------:|----------:|----------:|------:|--------:|------------:|------------:|------------:|--------------------:|
       CreateLight |   512.3 ns |  2.038 ns |  1.806 ns |  1.00 |    0.00 |      0.1774 |           - |           - |               840 B |
     CreateLighter |   491.7 ns |  2.611 ns |  2.442 ns |  0.96 |    0.01 |      0.1440 |           - |           - |               680 B |
      CreateSystem | 1,185.4 ns | 22.740 ns | 23.352 ns |  2.30 |    0.04 |      0.1183 |           - |           - |               560 B |
    */

    [MemoryDiagnoser]
    public class Light_vs_LighterExpression
    {
        [Benchmark(Baseline = true)]
        public object CreateLight()
        {
            var sParam = L.Parameter(typeof(string), "s");
            return L.Lambda<Func<string, Blah>>(
                L.New(typeof(Blah).GetTypeInfo().DeclaredConstructors.First(),
                    sParam, L.Constant(42)),
                sParam);
        }

        [Benchmark]
        public object CreateLighter()
        {
            var sParam = M.Parameter(typeof(string), "s");
            return M.Lambda(typeof(Func<string, Blah>),
                M.New(typeof(Blah).GetTypeInfo().DeclaredConstructors.First(),
                    sParam, M.Constant(42)),
                sParam);
        }

        [Benchmark]
        public object CreateSystem()
        {
            var sParam = S.Parameter(typeof(string), "s");
            return S.Lambda<Func<string, Blah>>(
                S.New(typeof(Blah).GetTypeInfo().DeclaredConstructors.First(),
                    sParam, S.Constant(42)),
                sParam);
        }

        public class Blah
        {
            public string S { get; }
            public int I { get; }

            public Blah(string s, int i)
            {
                S = s;
                I = i;
            }
        }
    }
}
