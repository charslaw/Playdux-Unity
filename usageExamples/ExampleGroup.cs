using System;
using System.Collections.Generic;
using AReSSO.CopyUtils;

namespace AReSSO.UsageExamples
{
    /// <summary>
    /// An example state object which demonstrates best practices for state objects.
    /// State objects *must* implement IEquatable.
    ///
    /// It would be very possible to statically generate an implementation of this given an interface with something like
    /// Fody (https://github.com/Fody/Home) or another similar framework.
    /// </summary>
    public class ExampleGroup : IEquatable<ExampleGroup>
    {
        // All properties of state objects *must* be readonly (no setters).
        // ImmutableList would be preferred to IReadOnlyList. Get it on nuget.
        public IReadOnlyList<ExamplePerson> People { get; }
        
        
        public ExampleGroup(IReadOnlyList<ExamplePerson> people)
        {
            People = people;
        }

        public ExampleGroup Copy(PropertyChange<IReadOnlyList<ExamplePerson>> people = default)
        {
            return new ExampleGroup(people.Else(People));
        }

        public bool Equals(ExampleGroup other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(People, other.People);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExampleGroup) obj);
        }

        public override int GetHashCode()
        {
            return (People != null ? People.GetHashCode() : 0);
        }
    }
}