namespace Equ.Test
{
    using System;

    using Equ;

    using Xunit;

    public class PropertywiseEquatableTest
    {
        [Fact]
        public void Self_equality_is_sane()
        {
            var x = new ValueType("asdf", 42);

#pragma warning disable 1718
            // ReSharper disable EqualExpressionComparison
            Assert.True(x.Equals(x));
            Assert.True(x == x);
            Assert.False(x != x);
            Assert.False(x.Equals(null));
            Assert.False(Equals(null, x));

            // ReSharper disable once HeuristicUnreachableCode
            Assert.Equal(x.GetHashCode(), x.GetHashCode());
            // ReSharper restore EqualExpressionComparison
#pragma warning restore 1718
        }

        [Fact]
        public void Value_objects_with_equal_members_are_equal()
        {
            var x = new ValueType("asdf", 42);
            var y = new ValueType("asdf", 42);

            Assert.True(x.Equals(y));
            Assert.True(y.Equals(x));
            Assert.True(x == y);
            Assert.False(x != y);

            Assert.Equal(x.GetHashCode(), y.GetHashCode());
        }

        [Fact]
        public void Value_objects_with_one_differing_member_are_unequal()
        {
            var x = new ValueType("asdf", 100);
            var y = new ValueType("asdf", 42);

            Assert.False(x.Equals(y));
            Assert.False(y.Equals(x));
            Assert.False(x == y);
            Assert.True(x != y);

            Assert.NotEqual(x.GetHashCode(), y.GetHashCode());
        }

        [Fact]
        public void Ignore_index_properties()
        {
            var v1 = new IndexedValueType(15);
            var v2 = new IndexedValueType(15);

            Assert.Equal(v1, v2);
        }

        private class ValueType : PropertywiseEquatable<ValueType>
        {
            public ValueType(string x, int y)
            {
                X = x;
                Y = y;
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string X { get; private set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public int Y { get; private set; }
        }

        private class IndexedValueType : PropertywiseEquatable<IndexedValueType>
        {
            public IndexedValueType(int value)
            {
                Value = value;
            }

            public int Value { get; private set; }

            public string this[string val]
            {
                get
                {
                    return val;
                }
            }
        }
        
        private class MyBadClass : PropertywiseEquatable<string>
        {
        }
        
        [Fact]
        public void Assert_badly_constructed_class_throws()
        {
            Assert.Throws<TypeInitializationException>(
                () =>
                {
                    var badClass = new MyBadClass();
                });
        }
        
        [Fact]
        public void Check_basic_equals_checks()
        {
            var testValue = new ValueType("test", 23);
            Assert.False(testValue.Equals((object)null));
            Assert.True(testValue.Equals((object)testValue));
            Assert.False(testValue.Equals("fred"));
        }
    }
}
