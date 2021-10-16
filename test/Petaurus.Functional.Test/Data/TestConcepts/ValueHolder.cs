using System;
using System.Reflection;
using Petaurus.Functional.TypeLevel;
using Petaurus.Functional.Data;

namespace Petaurus.Functional.Test.Data.TestConcepts
{
    /// <summary>
    /// Okay, so this thing is horrible. xUnit has a bug that breaks type inference if it's passed
    /// a null, so instead we need to pass around a ValueHolder that holds the null. This makes reading the code
    /// unnecessarily complex, but I take advantage of the situation by giving it Left and Right factory methods. 
    /// </summary>
    public record ValueHolder<T>(T Value) : IValueHolder
    {
        public Either<T, TRight> Left<TRight>(Proxy<TRight> proxy = default) => new(Value);
        public Either<TLeft, T> Right<TLeft>(Proxy<TLeft> proxy = default) => new(Value);

        object? IValueHolder.Value => Value;
        
        private static readonly MethodInfo LeftMethod 
            = typeof(ValueHolder<T>).GetMethod("Left") 
              ?? throw new MissingMethodException("Can't find Left!"); 
        private static readonly MethodInfo RightMethod 
            = typeof(ValueHolder<T>).GetMethod("Right")
              ?? throw new MissingMethodException("Can't find Right!");
        public object ToEither(object proxy, bool isRight)
        {
            var type = proxy.GetType().GenericTypeArguments[0];
            return (isRight ? RightMethod : LeftMethod)
                .MakeGenericMethod(type)
                .Invoke(this, new object?[] {proxy})!;
        }
    }
}