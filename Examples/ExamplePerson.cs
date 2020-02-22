using System;
using System.Collections.Generic;

namespace AReSSO.Examples
{
    /// <summary>
    /// An example StateNode which demonstrates best practices for subclassing StateNode.
    ///
    /// It would be very possible to statically generate an implementation of this given an interface with something like
    /// Fody (https://github.com/Fody/Home) or another similar framework.
    /// </summary>
    public class ExamplePerson : StateNode
    {
        // All properties of StateNodes *must* be readonly (no setters).
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
        /// This is a private copy constructor. It is used to create a copy when Copy is called.
        /// Note the call to the protected base copy constructor. That is very important.
        /// </summary>
        /// <remarks>
        /// This method is basically entirely boilerplate and could easily be automated.
        /// </remarks>
        private ExamplePerson(ExamplePerson old,
            PropertyChange<string> newName,
            PropertyChange<DateTime> newBirthday,
            PropertyChange<IReadOnlyList<ExampleSong>> newFavoriteSongs) : base(old)
        {
            Name = newName.Else(old.Name);
            Birthday = newBirthday.Else(old.Birthday);
            FavoriteSongs = newFavoriteSongs.Else(old.FavoriteSongs);
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
            PropertyChange<IReadOnlyList<ExampleSong>> favoriteSongs = default) =>
            new ExamplePerson(this, name, birthday, favoriteSongs);

        #region Equals Implementations
        // Generic implementations of Equals and GetHashCode.
        // Prime candidates for code generation; in fact these implementations were auto-implemented by Rider.
        
        protected bool Equals(ExamplePerson other)
        {
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