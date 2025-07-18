using System.Diagnostics;
using LyainBot.Command;
using TL;
using WTelegram;

namespace LyainBot;

public class LyainBotApp
{
    public static Client Client;
    public static UpdateManager UpdateManager;
    public static EventHandler EventHandler;
    public static CommandManager CommandManager;
    public static ClientConfig ClientConfig;
    public static User Me;
    public static bool IsRunning = true;

    internal static async Task Init()
    {
        Helpers.Log = (l, s) => Debug.WriteLine(s);
        ClientConfig = ClientConfig.GetConfig();
        EventHandler = new EventHandler();
        CommandManager = new CommandManager();
        CommandManager.Init();
        Client = new Client(ClientConfig.ConfigCallback);
        if (ClientConfig.MTProxyUrl != null)
        {
            Client.MTProxyUrl = ClientConfig.MTProxyUrl;
        }

        Me = await Client.LoginUserIfNeeded();
        Console.WriteLine($"Logged as user {Me.first_name}");
        UpdateManager = Client.WithUpdateManager(OnUpdate);
        Messages_Dialogs dialogs = await Client.Messages_GetAllDialogs();
        await UpdateManager.LoadDialogs(dialogs);
    }

    internal static async Task Loop()
    {
        while (IsRunning)
        {
            await Task.Delay(100);
        }
    }

    private static Task OnUpdate(Update update)
    {
        EventHandler.CallEvent(update);
        return Task.CompletedTask;
    }
}