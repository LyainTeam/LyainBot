using LyainBot.Command;
using TL;
using WTelegram;

namespace LyainBot;

public class SharedData
{
    public Client Client { get; set; }
    public UpdateManager UpdateManager { get; set; }
    public EventHandler EventHandler { get; set; }
    public CommandManager CommandManager { get; set; }
    public ClientConfig ClientConfig { get; set; }
    public User Me { get; set; }
}