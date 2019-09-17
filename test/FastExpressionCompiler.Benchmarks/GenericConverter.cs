using System;
using BenchmarkDotNet.Attributes;
using System.Linq.Expressions;
using L = FastExpressionCompiler.LightExpression.Expression;

namespace FastExpressionCompiler.Benchmarks
{
    public class GenericConverter
    {
        /*
|                      Method |  Job | Runtime |      Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 |  Gen 2 | Allocated |
|---------------------------- |----- |-------- |----------:|----------:|----------:|------:|--------:|-------:|-------:|-------:|----------:|
|                     Compile |  Clr |     Clr | 52.808 us | 0.5371 us | 0.5024 us |  9.15 |    0.09 | 0.9766 | 0.4883 |      - |   4.77 KB |
| CompileFast_LightExpression |  Clr |     Clr |  7.692 us | 0.0255 us | 0.0238 us |  1.33 |    0.01 | 0.4272 | 0.2136 | 0.0305 |   2.03 KB |
|                     Compile | Core |    Core | 60.547 us | 0.2227 us | 0.2083 us | 10.49 |    0.05 | 0.9155 | 0.4272 |      - |    4.4 KB |
| CompileFast_LightExpression | Core |    Core |  5.769 us | 0.0222 us | 0.0208 us |  1.00 |    0.00 | 0.4349 | 0.2136 | 0.0305 |   2.01 KB |

         */
        [ClrJob, CoreJob(baseline: true)]
        [MemoryDiagnoser]
        public class Compilation
        {
            [Benchmark]
            public Func<int, EnumX> Compile() => 
                GetConverter<int, EnumX>();

            // [Benchmark]
            public Func<int, EnumX> CompileFast() =>
                GetConverter_CompiledFast<int, EnumX>();

            [Benchmark(Baseline = true)]
            public Func<int, EnumX> CompileFast_LightExpression() =>
                GetConverter_CompiledFast_LightExpression<int, EnumX>();
        }

        [ClrJob, CoreJob(true)]
        [MemoryDiagnoser, DisassemblyDiagnoser(printIL: true, recursiveDepth: 2)]
        public class Invocation
        {
            /*
|                            Method |     Mean |     Error |    StdDev |   Median | Ratio | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------------- |---------:|----------:|----------:|---------:|------:|------:|------:|------:|----------:|
|                   Invoke_Compiled | 3.048 ns | 0.0024 ns | 0.0048 ns | 3.047 ns |  1.72 |     - |     - |     - |         - |
|                Invoke_CompileFast | 1.773 ns | 0.0013 ns | 0.0023 ns | 1.772 ns |  1.00 |     - |     - |     - |         - |
| Invoke_CompileFast_WithoutClosure | 1.774 ns | 0.0026 ns | 0.0051 ns | 1.772 ns |  1.00 |     - |     - |     - |         - |
             */

            private static readonly Func<int, EnumX> _compiled     = GetConverter<int, EnumX>();
            private static readonly Func<int, EnumX> _compiledFast = GetConverter_CompiledFast<int, EnumX>();
            private static readonly Func<int, EnumX> _compiledFast_LightExpression = GetConverter_CompiledFast_LightExpression<int, EnumX>();


            [Benchmark]
            public EnumX Invoke_CompiledFast() =>
                _compiledFast(3);

            //[Benchmark]
            public EnumX Invoke_Compiled() => 
                _compiled(3);

            [Benchmark(Baseline = true)]
            public EnumX Invoke_CompiledFast_LightExpression() =>
                _compiledFast_LightExpression(3);
        }

        public static Func<TFrom, TTo> GetConverter<TFrom, TTo>()
        {
            var fromParam = Expression.Parameter(typeof(TFrom));
            var expr = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(fromParam, typeof(TTo)), fromParam);
            return expr.Compile();
        }

        public static Func<TFrom, TTo> GetConverter_CompiledFast<TFrom, TTo>()
        {
            var fromParam = Expression.Parameter(typeof(TFrom));
            var expr = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(fromParam, typeof(TTo)), fromParam);
            return expr.CompileFast(true);
        }

        public static Func<TFrom, TTo> GetConverter_CompiledFast_LightExpression<TFrom, TTo>()
        {
            var fromParam = L.Parameter(typeof(TFrom));
            var expr = L.Lambda<Func<TFrom, TTo>>(L.Convert<TTo>(fromParam), fromParam);
            return LightExpression.ExpressionCompiler.CompileFast(expr, true);
        }

        public enum EnumX { A, B, D, E, F };
    }
}
