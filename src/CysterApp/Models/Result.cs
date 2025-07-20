namespace CysterApp.Models;

public record Result(
    bool Pass,
    string Step,
    string Value,
    string Comment = "");