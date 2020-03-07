namespace Equ.Benchmark
{
    using System;

    using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Jobs;

    [SimpleJob(RuntimeMoniker.Net472, baseline: true)]
    //[SimpleJob(RuntimeMoniker.Net48)]
    //[SimpleJob(RuntimeMoniker.NetCoreApp21)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MemoryDiagnoser]
    public class BenchmarkHashCode
    {
        private MemberwiseEqualityComparer<Test> _equalityComparer;

        private Test _testArg1;

        private Test _testArg2;

        public enum EnumTest
        {
            Unset,
            ValA,
            ValB,
        }

        public class Test
        {
            public int A;

            public string B;

            public object C;

            public EnumTest D;

            public bool BaselineEquals(Test other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                if (this.GetType() != other.GetType()) return false;
                return A == other.A && B == other.B && Equals(C, other.C) && D == other.D;
            }

            public int BaselineGetHashCode()
            {
                unchecked
                {
                    var hashCode = 29;
                    hashCode = (hashCode * 486187739) ^ A;
                    hashCode = (hashCode * 486187739) ^ (B?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 486187739) ^ (C?.GetHashCode() ?? 0);
                    hashCode = (hashCode * 486187739) ^ (int)D;
                    return hashCode;
                }
            }
        }

        [GlobalSetup]
        public void Setup()
        {
            _equalityComparer = MemberwiseEqualityComparer<Test>.ByFields;
           
            _testArg1 = new Test {A = 4, B = "testing", C = "testing2", D = EnumTest.ValA };
            _testArg2 = new Test {A = 4, B = "testingA".Substring(0, 7), C = "testing23".Substring(0, 8), D = EnumTest.ValA };
        }

        [Benchmark]
        public void InvokeEquals()
        {
            _equalityComparer.Equals(_testArg1, _testArg2);
        }
        
        [Benchmark]
        public void InvokeBaselineEquals()
        {
            _testArg1.BaselineEquals(_testArg2);
        }
        
        [Benchmark]
        public void InvokeGetHashCode()
        {
            _equalityComparer.GetHashCode(_testArg1);
        }
        
        [Benchmark]
        public void InvokeBaselineGetHashCode()
        {
            _testArg1.BaselineGetHashCode();
        }

    }
}