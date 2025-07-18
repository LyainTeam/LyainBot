using System.Diagnostics;
using System.Reflection;
using Autofac;
using TL;
using LyainBot.Command;
using LyainBot.Plugins;
using WTelegram;

namespace LyainBot;

internal static class Program
{
    internal static async Task Main(string[] args)
    {
        await LyainBotApp.Init();

        SharedData sharedData = new()
        {
            Client = LyainBotApp.Client,
            UpdateManager = LyainBotApp.UpdateManager,
            EventHandler = LyainBotApp.EventHandler,
            CommandManager = LyainBotApp.CommandManager,
            ClientConfig = LyainBotApp.ClientConfig,
            Me = LyainBotApp.Me
        };
        
        ContainerBuilder builder = new();
        builder.RegisterInstance(sharedData).SingleInstance();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            LyainBotApp.IsRunning = false;
            Console.WriteLine("Shutting down...");
        };

        LoadPlugins(builder);
        IContainer container = builder.Build();
        IEnumerable<IPlugin> plugins = container.Resolve<IEnumerable<IPlugin>>();
        
        foreach (IPlugin plugin in plugins)
        {
            plugin.Load();
        }

        await LyainBotApp.Loop();
        
        foreach (IPlugin plugin in plugins)
        {
            plugin.Unload();
        }
    }
    
    private static void LoadPlugins(ContainerBuilder builder)
    {
        string pluginDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        if (!Directory.Exists(pluginDirectory)) return;
        string[] pluginFiles = Directory.GetFiles(pluginDirectory, "*.dll");
        foreach (string file in pluginFiles)
        {
            Assembly assembly = Assembly.LoadFrom(file);
            IEnumerable<Type> pluginTypes = assembly.GetTypes()
                .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (Type type in pluginTypes)
            {
                builder.RegisterType(type).As<IPlugin>();
            }
        }
    }
}
