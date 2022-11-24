namespace SqlParser.DataObjects;

public class FieldReference : IEquatable<FieldReference>
{
    public string Table { get; init; }
    public string Field { get; init; }

    public bool Equals(FieldReference? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Table, other.Table, StringComparison.InvariantCultureIgnoreCase)
               && string.Equals(Field, other.Field, StringComparison.InvariantCultureIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((FieldReference) obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Table, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(Field, StringComparer.InvariantCultureIgnoreCase);
        return hashCode.ToHashCode();
    }
}