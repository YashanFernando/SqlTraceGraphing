namespace SqlParser.DataObjects;

public class TableLink : IEquatable<TableLink>
{
    public FieldReference From { get; init; }
    public FieldReference To { get; init; }

    public override string ToString()
    {
        return $"{From.Table}.{From.Field} -> {To.Table}.{To.Field}";
    }

    public bool Equals(TableLink? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return From.Equals(other.From) && To.Equals(other.To);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((TableLink) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }
}