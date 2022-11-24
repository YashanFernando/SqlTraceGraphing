using System.Text;

namespace SqlParser;

public static class StringHelpers
{
    public static string Indent(this string source, int numberOfSpaces)
    {
        string indent = new string (' ', numberOfSpaces);
        return indent + source.Replace("\n", "\n" + indent);
    }
        
    public static string Multiply(this string source, int multiplier)
    {
        StringBuilder stringBuilder = new StringBuilder(multiplier * source.Length);
        for (int i = 0; i < multiplier; i++)
        {
            stringBuilder.Append(source);
        }
        return stringBuilder.ToString();
    }
}