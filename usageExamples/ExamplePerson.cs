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
    public class ExamplePerson : IEquatable<ExamplePerson>
    {
        // All properties of state objects *must* be readonly (no setters).
        public string Name { get; }
        public DateTime Birthday { get; }
        
        // This *should* be an ImmutableList from System.Collections.Immutable. Import via NuGet.
        public IReadOnlyList<ExampleSong> FavoriteSongs { get; }

        // Since the properties must be read only, you must provide a constructor which initializes all of them.
        public ExamplePerson(string name, DateTime birthday, IReadOnlyList<ExampleSong> favoriteSongs)
        {
            Name = name;
            Birthday = birthday;
            FavoriteSongs = favoriteSongs;
        }
        
        /// <summary>
        /// The copy function is a pure function that returns a copy of a state.
        /// Note the usage of optional arguments. PropertyDelta defaults to Changed = false, so a property will only
        /// change for properties that are specified.
        ///
        /// Usage: <code>newState = oldState.Copy(Prop1: "newValue", Prop3: 42);</code>
        /// In this case, Prop2 is not specified in the call to Copy, so it retains the same value in the newState.
        /// </summary>
        /// 
        /// <remarks>
        /// This method is basically entirely boilerplate and could easily be automated.
        /// </remarks>
        public ExamplePerson Copy(
            PropertyChange<string> name = default,
            PropertyChange<DateTime> birthday = default,
            PropertyChange<IReadOnlyList<ExampleSong>> favoriteSongs = default)
        {
            if (name.Changed || birthday.Changed || favoriteSongs.Changed)
            {
                return new ExamplePerson(name.Else(Name), birthday.Else(Birthday), favoriteSongs.Else(FavoriteSongs));
            }

            return this;
        }

        #region Equals Implementations
        // Generic implementations of Equals and GetHashCode.
        // Prime candidates for code generation; in fact these implementations were auto-implemented by Rider.
        
        public bool Equals(ExamplePerson other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Birthday.Equals(other.Birthday) && Equals(FavoriteSongs, other.FavoriteSongs);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExamplePerson) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Birthday.GetHashCode();
                hashCode = (hashCode * 397) ^ (FavoriteSongs != null ? FavoriteSongs.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}