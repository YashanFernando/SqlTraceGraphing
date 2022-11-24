using System.Collections.Concurrent;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Serilog;
using SqlParser.DataObjects;

namespace SqlParser.Parser;

/**
 * Find all the joins between tables
 *
 * Visit all the from clauses from the outermost to the innermost.
 * For each FOR clause,
 * - Visit each TableReference
 * -- If it's a named join capture the name and optionally alias
 * -- If it's a join, recursively visit it as a TableReference
 * -- If it's a subquery, ignore it. We'll visit as the next FOR clause
 * -- When you see a direct comparision, capture the two tables on either side. Replace the alias with actual table.
 */
class SqlDomVisitor : TSqlConcreteFragmentVisitor
{
    private readonly ConcurrentDictionary<string, string> _aliasMap = new();
    private readonly ConcurrentBag<string> _tableNames = new();
    private readonly ConcurrentBag<TableLink> _links = new();

    public override void Visit(FromClause node)
    {
        _aliasMap.Clear();
        _tableNames.Clear();

        foreach (var tableReference in node.TableReferences)
        {
            WalkTheTree(tableReference);
        }

        base.Visit(node);
    }

    public IReadOnlyCollection<TableLink> GetLinks() => _links;

    private void WalkTheTree(TableReference reference)
    {
        if (reference is NamedTableReference namedTableReference)
        {
            var tableName = namedTableReference.SchemaObject.BaseIdentifier.Value!;
            _tableNames.Add(tableName.ToLowerInvariant());

            if (namedTableReference.Alias != null)
            {
                if (!_aliasMap.TryAdd(namedTableReference.Alias.Value.ToLowerInvariant(), tableName))
                {
                    // This should never really happen.
                    // We can't have duplicate aliases within a FOR clause.
                    Log.Warning("Adding alias failed \n {Query}", reference.ToSqlString());
                }
            }
        }

        if (reference is QualifiedJoin qualifiedJoin)
        {
            WalkTheTree(qualifiedJoin.FirstTableReference);
            WalkTheTree(qualifiedJoin.SecondTableReference);
            Handle(qualifiedJoin.SearchCondition);
        }

        // If it's not a qualified join, it's not a join
    }

    private void Handle(BooleanExpression expression)
    {
        if (expression is BooleanComparisonExpression comparisonExpression)
        {
            Handle(comparisonExpression);
        }
        else if (expression is BooleanBinaryExpression binaryExpression)
        {
            Handle(binaryExpression.FirstExpression);
            Handle(binaryExpression.SecondExpression);
        }
        else
        {
            Log.Warning("Not a comparison or a binary expression: \n{Expression}",
            expression.ToSqlString());
        }
    }

    private void Handle(BooleanComparisonExpression expression)
    {
        try
        {
            var firstExpression = expression.FirstExpression as ColumnReferenceExpression;
            var secondExpression = expression.SecondExpression as ColumnReferenceExpression;

            if (firstExpression is null || secondExpression is null)
            {
                Log.Warning("Skipping unsupported comparison: \n{Expression}",
                expression.ToSqlString());
                return;
            }

            _links.Add(new TableLink
            {
                From = GetExpressionComponents(firstExpression),
                To = GetExpressionComponents(secondExpression)
            });
        }
        catch (Exception e)
        {
            // This usually fails when joining to a subquery
            // The subquery as another FromClause that'll be processed separately
            Log.Warning("Couldn't parse the link: {Error} \n {Query}",
                e.Message,
                expression.ToSqlString());
        }
    }

    private FieldReference GetExpressionComponents(ColumnReferenceExpression expression)
    {
        var nameComponents = expression.MultiPartIdentifier.Identifiers;
        var table = nameComponents[^2].Value;
        var field = nameComponents[^1].Value;

        if (!_tableNames.Contains(table.ToLowerInvariant()))
        {
            table = _aliasMap[table.ToLowerInvariant()];
        }

        return new FieldReference
        {
            Table = table,
            Field = field
        };
    }
}