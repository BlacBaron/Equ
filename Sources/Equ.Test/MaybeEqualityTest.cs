// ReSharper disable NotAccessedField.Local

using System;

namespace Equ.Test
{
    using Equ;

    using MayBee;

    using Xunit;

    public class MaybeEqualityTest
    {
        [Fact]
        public void Equality_on_maybe_propagates_to_inner_value()
        {
            var v1 = new SomeValue1(Maybe.Is(new SomeValue2("asdf")));
            var v2 = new SomeValue1(Maybe.Is(new SomeValue2("asdf")));

            //Assert.True(v1.TheValue == v2.TheValue);
            //Assert.True(v1 == v2);
            
            Assert.Equal(v1, v2);
        }

        [Fact]
        public void Equality_on_value_type_doesnt_box()
        {
            var v1 = new SomeValue3(new EvilValueType(3));
            var v2 = new SomeValue3(new EvilValueType(3));
            
            Assert.Equal(v1, v2);
        }
        
        [Fact]
        public void Equality_on_value_type_doesnt_box2()
        {
            var v1 = new SomeValue4(new EvilValueType2(3));
            var v2 = new SomeValue4(new EvilValueType2(3));
            
            Assert.Equal(v1, v2);
        }

        [Fact]
        public void Empty_values_are_considered_equal()
        {
            var v1 = new SomeValue1(Maybe.Empty<SomeValue2>());
            var v2 = new SomeValue1(Maybe.Empty<SomeValue2>());

            Assert.Equal(v1, v2);
        }

        [Fact]
        public void Empty_values_are_not_equal_to_existing()
        {
            var v1 = new SomeValue1(Maybe.Empty<SomeValue2>());
            var v2 = new SomeValue1(Maybe.Is(new SomeValue2("asdf")));

            Assert.NotEqual(v1, v2);
        }

        private class SomeValue1 : MemberwiseEquatable<SomeValue1>
        {
            private readonly Maybe<SomeValue2> _theValue;
            public Maybe<SomeValue2> TheValue => _theValue;
            public SomeValue1(Maybe<SomeValue2> theValue)
            {
                _theValue = theValue;
            }
        }

        private class SomeValue2 : MemberwiseEquatable<SomeValue2>
        {
            private readonly string _val;

            public SomeValue2(string val)
            {
                _val = val;
            }
        }

        private struct EvilValueType : IEquatable<EvilValueType>
        {
            private readonly int _x;
            
            public EvilValueType(int x)
            {
                _x = x;
            }

            public bool Equals(EvilValueType other)
            {
                return _x == other._x;
            }

            public override bool Equals(object obj)
            {
                throw new Exception("You boxed me");
            }

            public override int GetHashCode()
            {
                return _x;
            }

            public static bool operator ==(EvilValueType left, EvilValueType right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(EvilValueType left, EvilValueType right)
            {
                return !left.Equals(right);
            }
        }

        private class SomeValue3 : MemberwiseEquatable<SomeValue3>
        {
            private readonly EvilValueType _val;

            public SomeValue3(EvilValueType val)
            {
                _val = val;
            }
        }
        
        private struct EvilValueType2 
        {
            private readonly int _x;
            
            public EvilValueType2(int x)
            {
                _x = x;
            }

            public bool Equals(EvilValueType2 other)
            {
                return _x == other._x;
            }

            public override bool Equals(object obj)
            {
                throw new Exception("You boxed me");
            }

            public override int GetHashCode()
            {
                return _x;
            }
        }

        private class SomeValue4 : MemberwiseEquatable<SomeValue4>
        {
            private readonly EvilValueType2 _val;

            public SomeValue4(EvilValueType2 val)
            {
                _val = val;
            }
        }
    }
}
