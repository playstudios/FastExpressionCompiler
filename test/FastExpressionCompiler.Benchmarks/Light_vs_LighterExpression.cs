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


            Method |     Mean |     Error |    StdDev | Ratio | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
    -------------- |---------:|----------:|----------:|------:|------------:|------------:|------------:|--------------------:|
       CreateLight | 388.4 ns | 1.2407 ns | 1.1606 ns |  1.00 |      0.1712 |           - |           - |               808 B |
     CreateLighter | 372.7 ns | 0.8424 ns | 0.7468 ns |  0.96 |      0.1369 |           - |           - |               648 B |
      CreateSystem | 979.2 ns | 3.1278 ns | 2.9257 ns |  2.52 |      0.1106 |           - |           - |               528 B |
    */

    [MemoryDiagnoser]
    public class Light_vs_LighterExpression
    {
        private static readonly ConstructorInfo _blahCtor = typeof(Blah).GetTypeInfo().DeclaredConstructors.First();

        [Benchmark(Baseline = true)]
        public object CreateLight()
        {
            var sParam = L.Parameter(typeof(string), "s");
            return L.Lambda<Func<string, Blah>>(
                L.New(_blahCtor,
                    sParam, L.Constant(42)),
                sParam);
        }

        [Benchmark]
        public object CreateLighter()
        {
            var sParam = M.Parameter(typeof(string), "s");
            return M.Lambda(typeof(Func<string, Blah>),
                M.New(_blahCtor,
                    sParam, M.Constant(42)),
                sParam);
        }

        [Benchmark]
        public object CreateSystem()
        {
            var sParam = S.Parameter(typeof(string), "s");
            return S.Lambda<Func<string, Blah>>(
                S.New(_blahCtor,
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
