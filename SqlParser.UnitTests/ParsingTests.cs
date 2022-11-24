using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Serilog;
using SqlParser.DataObjects;
using SqlParser.Parser;

namespace SqlParser.UnitTests;

public class ParsingTests
{

    [SetUp]
    public void Setup()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug() // Capture all
            .WriteTo.Console(
                outputTemplate: "{Level:u3} {Message:l}{NewLine}{Exception}{NewLine}{Properties}")
            .CreateLogger();
    }

    [Test]
    public void SimpleJoin()
    {
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A
            JOIN Table_B ON Table_B.Field1 = Table_A.Field2";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(1));
        AssertLink(links.First(), "Table_B", "Field1", "Table_A", "Field2");
    }

    [Test]
    public void JoinWithAlias()
    {
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A A
            JOIN Table_B B ON B.Field1 = A.Field2";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(1));
        AssertLink(links.First(), "Table_B", "Field1", "Table_A", "Field2");
    }

    [Test]
    public void MultipleJoins()
    {
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A
            JOIN Table_B ON Table_B.Field1 = Table_A.Field2
            JOIN Table_C ON Table_C.Field3 = Table_A.Field4";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(2));
        AssertLink(links[1], "Table_B", "Field1", "Table_A", "Field2");
        AssertLink(links[0], "Table_C", "Field3", "Table_A", "Field4");
    }

    [Test]
    public void SubQuery()
    {
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A
            WHERE 100 <
                (SELECT Field3
                FROM Table_B
                JOIN Table_C ON Table_B.Field1 = Table_C.Field2)";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(1));
        AssertLink(links.First(), "Table_B", "Field1", "Table_C", "Field2");
    }

    [Test]
    public void SubQueryInWhereStatementWithSameAlias()
    {
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A ALIAS
            WHERE 100 <
                (SELECT Field3
                FROM Table_B ALIAS
                JOIN Table_C ON ALIAS.Field1 = Table_C.Field2)";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(1));
        Assert.That(links.First().From.Table, Is.EqualTo("Table_B"));
    }

    [Test]
    public void SubQueryInFromStatementWithSameAlias()
    {
        // The alias is only used to join in the inner query
        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A ALIAS
            JOIN (SELECT Field3
                FROM Table_B ALIAS
                JOIN Table_C ON ALIAS.Field1 = Table_C.Field2) Query2 ON ALIAS.Field2 = Query2.Field3";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(1));
        AssertLink(links.First(), "Table_B", "Field1", "Table_C", "Field2");
    }

    [Test]
    public void SubQueryInFromStatementWithSameAliasUsedToJoin()
    {
        // The alias is used in a join in the inner query and the outer query

        // Arrange
        var sql = @"
            SELECT *
            FROM Table_A
            JOIN (SELECT Field3
                FROM Table_B ALIAS
                JOIN Table_C ON ALIAS.Field1 = Table_C.Field2) Query2 ON Table_A.Field1 = Query2.Field3
            JOIN Table_D ALIAS ON ALIAS.Field2 = Table_A.Field3";

        // Act
        var links = Parse(sql);

        // Assert
        Assert.That(links.Count, Is.EqualTo(2));
        AssertLink(links[0], "Table_B", "Field1", "Table_C", "Field2");
        AssertLink(links[1], "Table_D", "Field2", "Table_A", "Field3");
    }

    private void AssertLink(TableLink link, string fromTable, string fromField, string toTable, string toField)
    {
        Assert.That(link.From.Table, Is.EqualTo(fromTable));
        Assert.That(link.From.Field, Is.EqualTo(fromField));
        Assert.That(link.To.Table, Is.EqualTo(toTable));
        Assert.That(link.To.Field, Is.EqualTo(toField));
    }

    private List<TableLink> Parse(string sql)
    {
        var parser = new SqlStringParser();
        Assert.That(parser.TryParse(sql), Is.True);

        return parser.GetTableLinks().ToList();
    }
}