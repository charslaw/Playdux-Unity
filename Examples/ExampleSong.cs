using System;

namespace AReSSO.Examples
{
    /// <summary>
    /// A song is an example of something that won't change, so it doesn't need a Copy method or copy constructor.
    /// Since this state object won't change, it doesn't need to subclass StateNode, however it should implement
    /// Equals and GetHashCode correctly. In this case I have added IEquatable to make that explicit.
    /// </summary>
    public class ExampleSong : IEquatable<ExampleSong>
    {
        public string SongName { get; }
        public string AlbumName { get; }
        public string ArtistName { get; }

        public ExampleSong(string songName, string albumName, string artistName)
        {
            SongName = songName;
            AlbumName = albumName;
            ArtistName = artistName;
        }
        
        #region Equals Implementations

        public bool Equals(ExampleSong other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SongName == other.SongName && AlbumName == other.AlbumName && ArtistName == other.ArtistName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExampleSong) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (SongName != null ? SongName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AlbumName != null ? AlbumName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ArtistName != null ? ArtistName.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}