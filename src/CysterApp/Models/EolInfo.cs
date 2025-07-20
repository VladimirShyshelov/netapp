namespace CysterApp.Models;

public record EolInfo(
    string Os,
    string Version,
    int Build,
    DateTime HomePro,
    DateTime EnterpriseEdu);