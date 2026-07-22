using CultureLinkCRM.Core.Dtos;
using CultureLinkCRM.Core.Entities;
using CultureLinkCRM.Infrastructure.Services;

namespace CultureLinkCRM.Tests.Unit;

/// <summary>Direct unit tests for the shared Household-level de-dup routine (Ref: FR-10, FR-11).</summary>
public class HouseholdDedupBuilderTests
{
    [Fact]
    public void TwoPersonsInSameHousehold_CollapseToOneRow()
    {
        var household = new Household { Id = 1, HouseholdName = "The Ragan Family" };
        var personA = new Person { Id = 1, FirstName = "Alice", LastName = "Ragan", HouseholdId = 1, Household = household };
        var personB = new Person { Id = 2, FirstName = "Bob", LastName = "Ragan", HouseholdId = 1, Household = household };

        var rows = HouseholdDedupBuilder.BuildRows([personA, personB], []);

        var row = Assert.Single(rows);
        Assert.Equal(AudienceRowKind.Household, row.Kind);
        Assert.Equal("The Ragan Family", row.DisplayName);
    }

    [Fact]
    public void PersonWithNoHousehold_ProducesIndividualRow()
    {
        var person = new Person { Id = 1, FirstName = "Carol", LastName = "Smith" };

        var rows = HouseholdDedupBuilder.BuildRows([person], []);

        var row = Assert.Single(rows);
        Assert.Equal(AudienceRowKind.Person, row.Kind);
        Assert.Equal("Carol Smith", row.DisplayName);
    }

    [Fact]
    public void DifferentHouseholds_ProduceSeparateRows()
    {
        var householdOne = new Household { Id = 1, HouseholdName = "Household One" };
        var householdTwo = new Household { Id = 2, HouseholdName = "Household Two" };
        var personA = new Person { Id = 1, FirstName = "A", LastName = "A", HouseholdId = 1, Household = householdOne };
        var personB = new Person { Id = 2, FirstName = "B", LastName = "B", HouseholdId = 2, Household = householdTwo };

        var rows = HouseholdDedupBuilder.BuildRows([personA, personB], []);

        Assert.Equal(2, rows.Count);
    }

    [Fact]
    public void Organizations_ProduceOneRowEach()
    {
        var org = new Organization { Id = 1, Name = "First Baptist" };

        var rows = HouseholdDedupBuilder.BuildRows([], [org]);

        var row = Assert.Single(rows);
        Assert.Equal(AudienceRowKind.Organization, row.Kind);
        Assert.Equal("First Baptist", row.DisplayName);
    }
}
