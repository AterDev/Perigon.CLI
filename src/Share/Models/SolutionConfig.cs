namespace Share.Models;

public class SolutionConfig
{
    public ICollection<string> UserIdKeys { get; set; } = ["UserId"];
    public string SystemModName { get; set; } = "SystemMod";
}
