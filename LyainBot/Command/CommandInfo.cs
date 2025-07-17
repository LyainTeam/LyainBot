namespace LyainBot.Command;

[AttributeUsage(AttributeTargets.Class)]
public class CommandInfo : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public bool Edit { get; }
    public string[] Aliases { get; }

    public CommandInfo(string name, string description, bool edit, params string[]? aliases)
    {
        Name = name;
        Description = description;
        Edit = edit;
        Aliases = aliases ?? [];
    }
}
