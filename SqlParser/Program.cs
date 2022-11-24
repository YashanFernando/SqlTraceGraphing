using System.Text.RegularExpressions;
using Serilog;
using SqlParser.DataObjects;
using SqlParser.Parser;

namespace SqlParser;

class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Fatal() // Disable all output
            // .MinimumLevel.Error() // Capture all errors
            // .MinimumLevel.Debug() // Capture all
            .WriteTo.Console(
                outputTemplate: "{Level:u3} {Message:l}{NewLine}{Exception}{NewLine}{Properties}")
            .CreateLogger();

        var links = new List<TableLink>();

        var traceFilePath = "the path to the trace file goes here";
        var includeItemsRegex = new Regex("[tT].*");

        var queries = new SqlTraceReader().GetSqlStatements(traceFilePath);
        foreach (var query in queries)
        {
            var parser = new SqlStringParser();
            try
            {
                if (!parser.TryParse(query))
                    continue;

                links.AddRange(parser.GetTableLinks());
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception captured \n{Query}",
                        parser?.ToSqlString() ?? query);
            }
        }

        var diagram = new PlantUmlRenderer();
        diagram.WriteGraph(links
            .Where(l => includeItemsRegex.IsMatch(l.From.Table) && includeItemsRegex.IsMatch(l.To.Table))
            .Distinct());
    }
}