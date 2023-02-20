using System.Text;

namespace MovieLibrary;

public sealed record Movie(
    int Id,
    string Name,
    string[] Genres
) {
    public int Id {get;} = Id;
    public string Name {get;} = Name;
    public string[] Genres {get;} = Genres;
    
    private int? _hash;

    private string GenreString() {
        StringBuilder sb = new();

        foreach(string genre in Genres) {
            sb.Append($"{genre}|");
        }

        sb.Length--;

        return sb.ToString();
    }

    public bool Equals(Movie? other) {
        return
            other is not null &&
            this.GetHashCode() == other.GetHashCode(); //Compare hashes.
    }

    public override int GetHashCode() { //Get a hash of the record, based on only the name and string array.
        if(_hash.HasValue)
            return _hash.Value;
        
        HashCode hc = new();
        hc.Add(Name);

        foreach(string genre in Genres) {
            hc.Add(genre);
        }

        _hash = hc.ToHashCode();
        
        return _hash.Value;
    }

    public override string ToString() {
        return Name.Contains(',') ?
            $"{Id},\"{Name}\",{GenreString()}" :
            $"{Id},{Name},{GenreString()}";
    }

    public string UserToString() {
        return $"ID: {Id}, \"{Name}\", {string.Join(", ", Genres)}";
    }
}