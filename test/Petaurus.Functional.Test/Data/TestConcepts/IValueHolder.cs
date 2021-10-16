namespace Petaurus.Functional.Test.Data.TestConcepts
{
    public interface IValueHolder
    {
        object ToEither(object proxy, bool isRight);
        
        object? Value { get; }
    }
}