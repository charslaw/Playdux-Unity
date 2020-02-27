using System;
using AReSSO.Store;

namespace AReSSO.UsageExamples
{
    public class ExampleStoreBehaviour : StoreBehaviour<ExampleGroup>
    {
        private readonly ExamplePerson[] initialPeople = new[]
        {
            new ExamplePerson("Alice", new DateTime(1980, 01, 01), new ExampleSong[] { }),
            new ExamplePerson("Bob", new DateTime(1800, 12, 31), new ExampleSong[] { }),
            new ExamplePerson("Charlie", new DateTime(0, 0, 0), new ExampleSong[] { })
        };
        
        // Override InitializeStore to set the initial state and root reducer.
        protected override Store<ExampleGroup> InitializeStore() =>
            new Store<ExampleGroup>(new ExampleGroup(initialPeople), RootReducer);

        // This is the most basic reducer, the identity reducer. It doesn't do anything.
        private static ExampleGroup RootReducer(ExampleGroup state, IAction action) => state;
    }
}