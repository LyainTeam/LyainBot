using TL;
using LyainBot.Extension;
using WTelegram;

namespace LyainBot.Command.Impls;

[CommandInfo("help", "Show command info", true, "h")]
public class HelpCommand : ICommand
{
    public void OnInvoke(Message message, UpdateManager manager, Client client, string[] args)
    {
        if (args.Length == 0)
        {
            string result = Program.CommandManager.GetAllCommandName().Aggregate("指令列表: \n", (current, se) => current + $"`{se}`, ");
            result = result[..^2];
            message.Edit(result);
        }
        else
        {
            CommandInfo? commandInfo = Program.CommandManager.TryGetCommandInfo(args[0]);
            string? aliases = commandInfo?.Aliases.Aggregate("", (current, alias) => current + (alias + ", "));
            message.Edit(commandInfo == null ? "Can't find target command." : $"Command: {commandInfo.Name}\nDescription: {commandInfo.Description}\nAliases: {aliases![..^2]}");
        }
    }
}
