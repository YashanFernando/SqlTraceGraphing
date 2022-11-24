using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SqlParser;

public class SqlTraceReader
{
    private readonly XmlNamespaceManager _ns;

    public SqlTraceReader()
    {
        _ns = new XmlNamespaceManager(new NameTable());
        _ns.AddNamespace("tp", "http://tempuri.org/TracePersistence.xsd");
    }

    public IEnumerable<string> GetSqlStatements(string traceFilePath)
    {
        var filter =
            "/tp:TraceData/tp:Events/tp:Event[@name='SQL:BatchCompleted' or @name='RPC:Completed']/tp:Column[@name='TextData']";
        return XDocument.Load(traceFilePath)
            .XPathSelectElements(filter, _ns)
            .Select(node => node.Value)
            .Select(ExtractSqlFromProc)
            .Distinct();
    }

    private static string ExtractSqlFromProc(string sql)
    {
        if (sql.StartsWith("exec sp_executesql N'"))
        {
            // Extract the SQL string from executed stored procs and pretend they are just sql statements
            var cleanedSql = Regex.Match(sql, "N'((.|\n)*)',N").Groups[1].Value;
            cleanedSql = cleanedSql.Replace("''", "'");
            return cleanedSql;
        }

        return sql;
    }
}