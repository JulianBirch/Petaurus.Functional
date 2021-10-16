using Petaurus.Functional.Data;
using Petaurus.Functional.TypeLevel;
using System.Diagnostics;

namespace Petaurus.Functional.Test.Data.TestConcepts
{
    [DebuggerDisplay("Initial:{Initial},{Expected} {Operation}")]
    public record Expectation<TInitial, TExpected>(
        TInitial Initial,
        TExpected Expected,
        Operation<TInitial, TExpected> Operation
    ) : IExpectation
    {
        public Either<TInitial, VoidResult> InitialLeft() => InitialLeft<VoidResult>();

        IOperation IExpectation.Operation => Operation;
        
        public Either<VoidResult, TInitial> InitialRight() => InitialRight<VoidResult>();
        
        public Either<TInitial, T> InitialLeft<T>() => Either.Left<TInitial, T>(Initial);
        
        public Either<T, TInitial> InitialRight<T>() => Either.Right<T, TInitial>(Initial);
    }
}