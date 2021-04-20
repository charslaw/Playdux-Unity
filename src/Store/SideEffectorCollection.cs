#nullable enable
using System;
using System.Collections.Generic;

namespace Playdux.src.Store
{
    public class SideEffectorCollection<TRootState>
    {
        private readonly Dictionary<Guid, ISideEffector<TRootState>> table = new();
        private readonly List<ISideEffector<TRootState>> priority = new();

        private readonly IComparer<ISideEffector<TRootState>> comparer = new SideEffectorPriorityComparer<TRootState>();

        public IEnumerable<ISideEffector<TRootState>> ByPriority => priority;

        public Guid Register(ISideEffector<TRootState> sideEffector)
        {
            var id = Guid.NewGuid();
            table.Add(id, sideEffector);

            var index = priority.BinarySearch(sideEffector, comparer);

            if (index < 0)
            {
                // a side effector with the same priority was not found, so this one should be inserted at the bitwise complement of the returned index.
                // See <https://docs.microsoft.com/en-us/dotnet/api/System.Collections.Generic.List-1.BinarySearch>
                index = ~index;
            }
            else
            {
                // a side effector with the same priority already exists in the list, insert this one after the existing one.
                while (++index < priority.Count && priority[index].Priority == sideEffector.Priority) { }
            }

            priority.Insert(index, sideEffector);

            return id;
        }

        public void Unregister(Guid id)
        {
            if (!table.TryGetValue(id, out var sideEffector)) throw new ArgumentException("Given ID does not correspond with a known side effector", nameof(id));
            table.Remove(id);
            
            var index = priority.FindIndex(other => other == sideEffector);

            priority.RemoveAt(index);
        }
    }
}