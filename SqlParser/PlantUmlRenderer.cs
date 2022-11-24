using SqlParser.DataObjects;

namespace SqlParser;

public class PlantUmlRenderer
{
    public void WriteGraph(IEnumerable<TableLink> tableLinks)
    {
        var links = tableLinks.ToList();
        var tables = new Dictionary<string, HashSet<string>>();
        foreach (var link in links)
        {
            AddTableReferences(tables, link.From);
            AddTableReferences(tables, link.To);
        }

        Write("@startuml");
        foreach (var table in tables.OrderBy(t => t.Key))
        {
            Write($"Class {table.Key} {{");
            foreach (var field in table.Value)
            {
                Write($"  + {field}");
            }
            Write("}\n");
        }

        foreach (var link in links.OrderBy(l => l.From.Table))
        {
            Write($"{link.From.Table}::{link.From.Field.ToLowerInvariant()} --> {link.To.Table}::{link.To.Field.ToLowerInvariant()}");
        }
        Write("@enduml");
    }

    private void Write(string data) => Console.WriteLine(data);

    private static void AddTableReferences(Dictionary<string, HashSet<string>> classes, FieldReference reference)
    {
        if (!classes.TryGetValue(reference.Table, out HashSet<string>? fields))
        {
            fields = new HashSet<string>();
            classes.Add(reference.Table, fields);
        }

        fields.Add(reference.Field.ToLowerInvariant());
    }
}