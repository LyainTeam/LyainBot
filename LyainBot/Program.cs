using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using Autofac;
using TL;
using LyainBot.Command;
using LyainBot.Plugins;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
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
        
        // register a custom dependency resolver to resolve dependencies for plugins
        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
        {
            string? assemblyName = new AssemblyName(eventArgs.Name).Name;
            Version? assemblyVersion = new AssemblyName(eventArgs.Name).Version;
            string libsRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs", assemblyName);
            if (!Directory.Exists(libsRootPath)) return null;
            string[] versions = Directory.GetDirectories(libsRootPath);
            if (versions.Length == 0) return null;
            string versionPath;
            if (assemblyVersion == null)
            {
                string? latestVersion = versions.OrderByDescending(v => new Version(Path.GetFileName(v))).FirstOrDefault();
                if (latestVersion == null) return null;
                versionPath = Path.Combine(libsRootPath, latestVersion);
            }
            else
            {
                string? nearestVersion = versions
                    .Select(v => new Version(Path.GetFileName(v)))
                    .Where(v => v >= assemblyVersion)
                    .OrderBy(v => v)
                    .FirstOrDefault()?.ToString();
                if (nearestVersion == null) return null;
                versionPath = Path.Combine(libsRootPath, nearestVersion);
            }
            string assembliesPath = Path.Combine(versionPath, "lib");
            string[] netVersions = Directory.GetDirectories(assembliesPath);
            if (netVersions.Length == 0) return null;
            string netVersion = netVersions.OrderByDescending(v => v).FirstOrDefault();
            if (netVersion == null) return null;
            string assemblyPath = Path.Combine(netVersion, $"{assemblyName}.dll");
            if (!File.Exists(assemblyPath))
            {
                Console.WriteLine($"Assembly {assemblyName} not found in {assemblyPath}");
                return null;
            }
            try
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {assemblyName} from {assemblyPath}: {ex.Message}");
                return null;
            }
        };
        
        foreach (IPlugin plugin in plugins)
        {
            plugin.Load();
        }

        Console.WriteLine("Plugins loaded successfully.");
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
        string[] pluginFiles = Directory.GetFiles(pluginDirectory, "*.lyplugin");
        foreach (string file in pluginFiles)
        {
            try
            {
                string assemblyName = Path.GetFileNameWithoutExtension(file);
                using FileStream fs = new(file, FileMode.Open, FileAccess.Read);
                using ZipArchive archive = new(fs, ZipArchiveMode.Read);
                ZipArchiveEntry? entry = archive.GetEntry($"{assemblyName}.deps.json");
                if (entry == null)
                {
                    Console.WriteLine($"No deps.json found in {file}, skipping.");
                    continue;
                }
                using StreamReader reader = new(entry.Open());
                string depsJson = reader.ReadToEnd();
                ParseDepsJsonAndDownload(depsJson);
                entry = archive.GetEntry($"{assemblyName}.dll");
                if (entry == null)
                {
                    Console.WriteLine($"No {assemblyName}.dll found in {file}, skipping.");
                    continue;
                }
                using Stream dllStream = entry.Open();
                using MemoryStream ms = new();
                dllStream.CopyTo(ms);
                byte[] dllBytes = ms.ToArray();
                Assembly pluginAssembly = Assembly.Load(dllBytes);
                Type? pluginType = pluginAssembly.GetTypes().FirstOrDefault(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                if (pluginType == null)
                {
                    Console.WriteLine($"No valid plugin type found in {file}, skipping.");
                    continue;
                }
                builder.RegisterType(pluginType).As<IPlugin>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load plugin from {file}: ");
                Console.WriteLine(ex);
            }
        }
    }
    
    private static void ParseDepsJsonAndDownload(string depsJson)
    {
        string libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Libs");
        if (!Directory.Exists(libsPath))
        {
            Directory.CreateDirectory(libsPath);
        }
        List<KeyValuePair<string, string>> dependencies = new();
        using JsonDocument doc = JsonDocument.Parse(depsJson);
        if (!doc.RootElement.TryGetProperty("libraries", out JsonElement libraries)) return;
        // 修正遍历方式，遍历对象属性
        foreach (JsonProperty libraryProp in libraries.EnumerateObject())
        {
            JsonElement library = libraryProp.Value;
            if (library.TryGetProperty("type", out JsonElement value) && value.GetString() == "project")
            {
                continue;
            }
            
            string[] parts = libraryProp.Name.Split('/');
            if (parts.Length < 2) continue;
            string name = parts[0];
            string version = parts[1];
            dependencies.Add(new KeyValuePair<string, string>(name, version));
        }
        
        PackageSource packageSource = new("https://api.nuget.org/v3/index.json");
        SourceRepository sourceRepository = new(packageSource, Repository.Provider.GetCoreV3());
        DownloadResource downloadResource = sourceRepository.GetResourceAsync<DownloadResource>().Result;
        SourceCacheContext sourceCacheContext = new()
        {
            NoCache = false,
            DirectDownload = true
        };
        PackageDownloadContext downloadContext = new(sourceCacheContext);
        
        foreach (KeyValuePair<string, string> dependency in dependencies)
        {
            string packageName = dependency.Key.ToLower();
            string packageVersion = dependency.Value.ToLower();
            
            if (packageName.Equals("lyainbot", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("log4net", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("autofac", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("microsoft.netcore.platforms", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.configuration.configurationmanager", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.diagnostics.diagnosticsource", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.security.accesscontrol", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.security.cryptography.protecteddata", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.security.permissions", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("system.security.principal.windows", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("wtelegramclient", StringComparison.OrdinalIgnoreCase) || 
                packageName.StartsWith("nuget.protocol", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            string packagePath = Path.Combine(libsPath, packageName, packageVersion, $".nupkg.metadata");
            if (!File.Exists(packagePath))
            {
                Console.WriteLine($"Downloading {packageName} {packageVersion}...");
                try
                {
                    PackageIdentity packageIdentity = new(packageName, NuGet.Versioning.NuGetVersion.Parse(packageVersion));
                    PackageMetadataResource metadataResource = sourceRepository.GetResourceAsync<PackageMetadataResource>().Result;
                    IPackageSearchMetadata? package = metadataResource.GetMetadataAsync(packageIdentity, sourceCacheContext, new NullLogger(), CancellationToken.None).Result;
                    if (package == null)
                    {
                        Console.WriteLine($"Package {packageName} {packageVersion} not found.");
                        continue;
                    }
                    
                    DownloadResourceResult result = downloadResource.GetDownloadResourceResultAsync(packageIdentity, downloadContext, libsPath, new NullLogger(), CancellationToken.None).Result;
                    if (result.Status != DownloadResourceResultStatus.Available)
                    {
                        Console.WriteLine($"Failed to download {packageName} {packageVersion}: {result.Status}");
                        continue;
                    }
                    Console.WriteLine($"Downloaded {packageName} {packageVersion}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download {packageName} {packageVersion}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"{packageName} {packageVersion} already exists.");
            }
        }
    }
}
