using Microsoft.SqlServer.TransactSql.ScriptDom;
using Serilog;
using SqlParser.DataObjects;

namespace SqlParser.Parser;

public class SqlStringParser
{
    private TSqlFragment _sqlFragment;

    public bool TryParse(string sql)
    {
        TSqlParser parser = new TSql120Parser(true);
        IList<ParseError> parseErrors;
        _sqlFragment = parser.Parse(new StringReader(sql), out parseErrors);

        if (parseErrors.Count > 0)
        {
            Log.Error("Failed to parse \n Errors: {Errors} \n{Query}",
                parseErrors.Select(e => e.Message),
                sql);
            return false;
        }

        return true;
    }

    public IEnumerable<TableLink> GetTableLinks()
    {
        var visitor = new SqlDomVisitor();
        _sqlFragment.Accept(visitor);
        return visitor.GetLinks();
    }

    public string? ToSqlString() => _sqlFragment?.ToSqlString();
}