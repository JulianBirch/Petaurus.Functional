using System;

namespace Petaurus.Functional.Test.Data.TestConcepts
{
    public interface IOperation
    {
        Type InitialType { get; }
        Type ExpectedType { get; }
    }
}