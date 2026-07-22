using CultureLinkCRM.Core.Enums;

namespace CultureLinkCRM.Core.Entities;

public class Network : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public NetworkType NetworkType { get; set; }

    public int? ParentNetworkId { get; set; }
    public Network? ParentNetwork { get; set; }
    public ICollection<Network> ChildNetworks { get; set; } = [];

    public ICollection<PersonNetwork> PersonLinks { get; set; } = [];
    public ICollection<OrganizationNetwork> OrganizationLinks { get; set; } = [];
}

public class PersonNetwork
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
    public int NetworkId { get; set; }
    public Network? Network { get; set; }
}

public class OrganizationNetwork
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public int NetworkId { get; set; }
    public Network? Network { get; set; }
}
