namespace ACP.Data.Identity;

public static class RoleNames
{
    public const string SuperAdministrator =
        "SuperAdministrator";

    public const string OperationsAdministrator =
        "OperationsAdministrator";

    public const string ComplianceAuditor =
        "ComplianceAuditor";

    public const string CollectionManager =
        "CollectionManager";

    public const string CollectionAgent =
        "CollectionAgent";

    public const string ClientAdministrator =
        "ClientAdministrator";

    public const string ClientUser =
        "ClientUser";

    public const string Consumer =
        "Consumer";

    public static readonly string[] All =
    [
        SuperAdministrator,
        OperationsAdministrator,
        ComplianceAuditor,
        CollectionManager,
        CollectionAgent,
        ClientAdministrator,
        ClientUser,
        Consumer
    ];
}