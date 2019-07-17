namespace Equ
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// This class is able to produce compiled Equals() and GetHashCode() functions.
    /// </summary>
    public class EqualityFunctionGenerator
    {
        private static readonly MethodInfo _objectEqualsMethod = new Func<object, object, bool>(Equals).GetMethodInfo();

        private readonly Type _type;

        private readonly Func<Type, IEnumerable<FieldInfo>> _fieldSelector;

        private readonly Func<Type, IEnumerable<PropertyInfo>> _propertySelector;

        /// <summary>
        /// Creates a generator for type <paramref name="type"/>. The generated functions will consider all fields
        /// and properties of <paramref name="type"/> that are returned by <paramref name="fieldSelector"/> and 
        /// <paramref name="propertySelector"/>, respectively.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fieldSelector"></param>
        /// <param name="propertySelector"></param>
        public EqualityFunctionGenerator(Type type, Func<Type, IEnumerable<FieldInfo>> fieldSelector, Func<Type, IEnumerable<PropertyInfo>> propertySelector)
        {
            _type = type;
            _fieldSelector = fieldSelector;
            _propertySelector = propertySelector;
        }

        /// <summary>
        /// Generates a GetHashCode() function. The generated function should be cached at class-level scope, so that
        /// the generation only takes place once per type. This can be achieved e.g. by storing the function in a static
        /// readonly field.
        /// </summary>
        public Func<ObjectType, int> MakeGetHashCodeMethod<ObjectType>()
        {
            var objRaw = Expression.Parameter(typeof(ObjectType), "obj");

            // compound XOR expression
            var getHashCodeExprs = GetIncludedMembers(_type).Select(mi => MakeGetHashCodeExpression(mi, objRaw));
            var xorChainExpr = getHashCodeExprs.Aggregate((Expression)Expression.Constant(29), LinkHashCodeExpression);
            
            return Expression.Lambda<Func<ObjectType, int>>(xorChainExpr, objRaw).Compile();
        }

        /// <summary>
        /// Generates a Equals() function. The generated function should be cached at class-level scope, so that
        /// the generation only takes place once per type. This can be achieved e.g. by storing the function in a static
        /// readonly field.
        /// </summary>
        public Func<ObjectType, ObjectType, bool> MakeEqualsMethod<ObjectType>()
        {
            var leftRaw = Expression.Parameter(typeof(ObjectType), "left");
            var rightRaw = Expression.Parameter(typeof(ObjectType), "right");

            // AND expression using short-circuit evaluation
            var equalsExprs = GetIncludedMembers(_type).Select(mi => MakeEqualsExpression(mi, leftRaw, rightRaw));
            var andChainExpr = equalsExprs.Aggregate((Expression)Expression.Constant(true), Expression.AndAlso);

            // call Object.Equals if second parameter doesn't match type
            var objectEqualsExpr = Expression.Equal(leftRaw, rightRaw);

            return Expression.Lambda<Func<ObjectType, ObjectType, bool>>(andChainExpr, leftRaw, rightRaw).Compile();
        }

        private IEnumerable<MemberInfo> GetIncludedMembers(Type type)
        {
            return _fieldSelector(type).Cast<MemberInfo>().Concat(_propertySelector(type));
        }

        private static Expression LinkHashCodeExpression(Expression left, Expression right)
        {
            var leftMultiplied = Expression.Multiply(left, Expression.Constant(486187739));
            return Expression.ExclusiveOr(leftMultiplied, right);
        }

        private static Expression MakeEqualsExpression(MemberInfo member, Expression left, Expression right)
        {
            var leftMemberExpr = Expression.MakeMemberAccess(left, member);
            var rightMemberExpr = Expression.MakeMemberAccess(right, member);

            var memberType = leftMemberExpr.Type;

            var leftTypeInfo = leftMemberExpr.Type.GetTypeInfo();
            if (leftTypeInfo.IsValueType)
            {
                if (leftTypeInfo.IsPrimitive || leftTypeInfo.IsEnum || leftTypeInfo.GetMethod("op_Equality") != null)
                {
                    return Expression.Equal(leftMemberExpr, rightMemberExpr);
                }
                else
                {
                    var boxedLeftMemberExpr = Expression.Convert(leftMemberExpr, typeof(object));
                    var boxedRightMemberExpr = Expression.Convert(rightMemberExpr, typeof(object));
                    return MakeReferenceTypeEqualExpression(boxedLeftMemberExpr, boxedRightMemberExpr);
                }
            }
            if (IsSequenceType(memberType))
            {
                return MakeSequenceTypeEqualExpression(leftMemberExpr, rightMemberExpr, memberType);
            }
            return MakeReferenceTypeEqualExpression(leftMemberExpr, rightMemberExpr);
        }

        private static Expression MakeSequenceTypeEqualExpression(Expression left, Expression right, Type enumerableType)
        {
            return MakeCallOnSequenceEqualityComparerExpression("Equals", enumerableType, left, right);
        }

        private static Expression MakeReferenceTypeEqualExpression(Expression left, Expression right)
        {
            return Expression.Call(_objectEqualsMethod, left, right);
        }

        private static Expression MakeGetHashCodeExpression(MemberInfo member, Expression obj)
        {
            var memberAccessExpr = Expression.MakeMemberAccess(obj, member);
            var memberAccessAsObjExpr = Expression.Convert(memberAccessExpr, typeof(object));

            var memberType = memberAccessExpr.Type;

            var getHashCodeExpr = IsSequenceType(memberType)
                ? MakeCallOnSequenceEqualityComparerExpression("GetHashCode", memberType, memberAccessExpr)
                : Expression.Call(memberAccessAsObjExpr, "GetHashCode", Type.EmptyTypes);

            return Expression.Condition(
                Expression.ReferenceEqual(Expression.Constant(null), memberAccessAsObjExpr), // If member is null
                Expression.Constant(0), // Return 0
                getHashCodeExpr); // Return the actual getHashCode call
        }

        private static Expression MakeCallOnSequenceEqualityComparerExpression(string methodName, Type enumerableType, params Expression[] parameterExpressions)
        {
            var comparerType = typeof(ElementwiseSequenceEqualityComparer<>).MakeGenericType(enumerableType);
            var comparerInstance = comparerType.GetTypeInfo().GetProperty("Default", BindingFlags.Static | BindingFlags.Public).GetValue(null);
            var comparerExpr = Expression.Constant(comparerInstance);

            return Expression.Call(comparerExpr, methodName, Type.EmptyTypes, parameterExpressions);
        }

        private static bool IsSequenceType(Type type)
        {
            return typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(type) && type != typeof(string);
        }
    }
}