using TL;
using LyainBot.Command.Impls;
using WTelegram;

namespace LyainBot.Command;

public class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new();
    private readonly Dictionary<string, ICommand> _commandAliases = new();
    private readonly Dictionary<string, CommandInfo> _commandInfos = new();

    public void Init()
    {
        LyainBotApp.EventHandler.OnUpdateNewMessage += message => HandleMessage(message.message);
        LyainBotApp.EventHandler.OnUpdateEditMessage += message => HandleMessage(message.message, true);
        LyainBotApp.EventHandler.OnUpdateNewChannelMessage += message => HandleMessage(message.message);
        LyainBotApp.EventHandler.OnUpdateEditChannelMessage += message => HandleMessage(message.message, true);
        RegisterCommand(new HelpCommand());
    }

    private void HandleMessage(MessageBase messageBase, bool edit = false)
    {
        if (messageBase is not Message message) return;
        if (!message.flags.HasFlag(Message.Flags.out_)) return;

        if (string.IsNullOrEmpty(message.message)) return;
        string messageString = message.message;
        if (LyainBotApp.ClientConfig.CommandPrefix == null)
        {
            Console.WriteLine("Warn: Invalid command prefix found.");
            return;
        }

        if (!messageString.StartsWith(LyainBotApp.ClientConfig.CommandPrefix)) return;
        string[] commandArgs = messageString.Split(" ").Select(arg => arg.Trim()).ToArray();
        string commandName = commandArgs[0][LyainBotApp.ClientConfig.CommandPrefix.Length..].Trim();
        TryInvokeCommand(commandName, message, LyainBotApp.UpdateManager, LyainBotApp.Client, edit, commandArgs[1..]);
    }

    public void RegisterCommand(ICommand command)
    {
        if (command.GetType().GetCustomAttributes(typeof(CommandInfo), false)
                .FirstOrDefault() is not CommandInfo info) throw new ArgumentException("Command must have CommandInfo attribute");
        if (_commands.ContainsKey(info.Name)) throw new ArgumentException($"Command with name {info.Name} already exists");
        
        _commands[info.Name] = command;
        
        foreach (string aliasName in info.Aliases)
        {
            if (_commandAliases.TryGetValue(aliasName, out ICommand? alias))
                throw new ArgumentException($"Alias {aliasName} already exists for Command {alias.GetType().Name}");
            _commandAliases[aliasName] = command;
        }
        
        _commandInfos[info.Name] = info;
    }

    private bool TryInvokeCommand(string commandName, Message message, UpdateManager manager, Client client, bool edit, string[] args)
    {
        if (TryGetCommandInfo(commandName)?.Edit == false && edit) return false;
        if (!_commands.TryGetValue(commandName, out ICommand? command))
        {
            if (!_commandAliases.TryGetValue(commandName, out command))
            {
                return false; // Command not found
            }
        }

        try
        {
            command.OnInvoke(message, manager, client, args);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return true;
    }

    public string[] GetAllCommandName()
    {
        string[] result = new string[_commands.Count];
        _commands.Keys.CopyTo(result, 0);
        return result;
    }

    public CommandInfo? TryGetCommandInfo(string commandName)
    {
        return _commandInfos.GetValueOrDefault(commandName) ?? _commandInfos.Values.FirstOrDefault(commandInfo => commandInfo.Aliases.Contains(commandName));
    }
}
