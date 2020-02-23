using System;

namespace AReSSO.UsageExamples
{
    /// <summary>
    /// A song is an example of something that won't change, so it doesn't need a Copy method or copy constructor,
    /// however it should still implement IEquatable.
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