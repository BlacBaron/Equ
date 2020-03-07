using Xunit;

namespace Equ.Test
{
    public class HashCodeTest
    {
        [Fact]
        public void TestA()
        {
            var v1 = new SomeClassA { A = 3, B = ValueTypeWithHashCode.TestA, C = ValueTypeWithoutHashCode.TestA, D = EnumType.Value1, E = EnumTypeB.ValueA };
            var v2 = new SomeClassA { A = 3, B = ValueTypeWithHashCode.TestB, C = ValueTypeWithoutHashCode.TestB, D = EnumType.Value2, E = EnumTypeB.ValueB };

            var hashCode1 = v1.GetHashCode();
            var hashCode2 = v2.GetHashCode();
            
            Assert.NotEqual(hashCode1, hashCode2);
        }

        struct ValueTypeWithHashCode
        {
            public int Value;
            
            public static ValueTypeWithHashCode TestA => new ValueTypeWithHashCode(1);
            public static ValueTypeWithHashCode TestB => new ValueTypeWithHashCode(2);

            public ValueTypeWithHashCode(int value)
            {
                this.Value = value;
            }

            public override int GetHashCode()
            {
                return Value;
            }
        }

        struct ValueTypeWithoutHashCode
        {
            public int Value;
            
            public static ValueTypeWithoutHashCode TestA => new ValueTypeWithoutHashCode(1);
            public static ValueTypeWithoutHashCode TestB => new ValueTypeWithoutHashCode(2);

            public ValueTypeWithoutHashCode(int value)
            {
                this.Value = value;
            }
        }

        enum EnumType
        {
            Value1,
            Value2
        }
        
        enum EnumTypeB : short
        {
            ValueA,
            ValueB
        }

        class SomeClassA : MemberwiseEquatable<SomeClassA>
        {
            public int A;

            public ValueTypeWithHashCode B;

            public ValueTypeWithoutHashCode C;

            public EnumType D;
            
            public EnumTypeB E;

        }
    }
}