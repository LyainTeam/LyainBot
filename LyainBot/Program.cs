using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
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
        
        // register a custom dependency resolver to resolve dependencies for plugins
        AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
        {
            string assemblyName = new AssemblyName(eventArgs.Name).Name?.ToLower() ?? string.Empty;
            string libsRootPath = Path.Combine("Libs", assemblyName);
            if (!Directory.Exists(libsRootPath))
            {
                Console.WriteLine("WARN: Libs directory does not exist, cannot resolve assembly.");
                return null;
            }
            string[] versions = Directory.GetDirectories(libsRootPath);
            if (versions.Length == 0)
            {
                Console.WriteLine($"No versions found for {assemblyName} in {libsRootPath}");
                return null;
            }
            string versionPath;
            string? latestVersion = versions.OrderByDescending(v => new Version(Path.GetFileName(v))).FirstOrDefault();
            if (latestVersion == null)
            {
                Console.WriteLine($"No valid version found for {assemblyName} in {libsRootPath}");
                return null;
            }
            versionPath = latestVersion;
            string assembliesPath = Path.Combine(versionPath, "lib");
            string[] netVersions = Directory.GetDirectories(assembliesPath);
            if (netVersions.Length == 0)
            {
                Console.WriteLine($"No .NET versions found in {assembliesPath} for {assemblyName}");
                return null;
            }
            string netVersion = netVersions.OrderBy(v => v).FirstOrDefault();
            if (netVersion == null)
            {
                Console.WriteLine($"No valid .NET version found in {assembliesPath} for {assemblyName}");
                return null;
            }
            // find all .dll files in the netVersion directory
            string[] assemblyFiles = Directory.GetFiles(netVersion, "*.dll");
            if (assemblyFiles.Length == 0)
            {
                Console.WriteLine($"No assemblies found for {assemblyName} in {netVersion}");
                return null;
            }
            // find the assembly that matches the requested name
            string? assemblyDll = assemblyFiles.FirstOrDefault(file => Path.GetFileNameWithoutExtension(file).ToLower() == assemblyName);
            if (string.IsNullOrEmpty(assemblyDll))
            {
                Console.WriteLine($"Assembly {assemblyName} not found in {netVersion}");
                return null;
            }
            if (!File.Exists(assemblyDll))
            {
                Console.WriteLine($"Assembly {assemblyName} not found in {assemblyDll}");
                return null;
            }
            try
            {
                return Assembly.LoadFrom(assemblyDll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load assembly {assemblyName} from {assemblyDll}: {ex.Message}");
                return null;
            }
        };
        
        LoadPlugins(builder);
        IContainer container = builder.Build();
        IEnumerable<IPlugin> plugins = container.Resolve<IEnumerable<IPlugin>>();
        
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
        string currentRuntime = RuntimeInformation.RuntimeIdentifier.ToLowerInvariant();
        Console.WriteLine($"Current runtime: {currentRuntime}");
        
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
                packageName.Equals("sixlabors.imagesharp", StringComparison.OrdinalIgnoreCase) ||
                packageName.Equals("nuget.protocol", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("nuget.common", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("nuget.configuration", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("nuget.frameworks", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("nuget.packaging", StringComparison.OrdinalIgnoreCase) || 
                packageName.Equals("nuget.versioning", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            string packagePath = Path.Combine(libsPath, packageName, packageVersion, $".nupkg.metadata");
            if (!File.Exists(packagePath))
            {
                Console.Write($"Downloading {packageName} {packageVersion}... ");
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
                        Console.WriteLine($"Failed");
                        continue;
                    }
                    Console.WriteLine($"Done.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to download: {ex.Message}");
                }
            }
            
            string runtimeDir = Path.Combine(libsPath, packageName, packageVersion, "runtimes", currentRuntime);
            if (!Directory.Exists(runtimeDir)) continue;
            string[] versions = Directory.GetDirectories(Path.Combine(libsPath, packageName, packageVersion, "lib"));
            string? latestVersion = versions.OrderBy(v => v).FirstOrDefault();
            if (latestVersion == null)
            {
                Console.WriteLine($"No versions found for {packageName} {packageVersion}");
                return;
            }
            // copy runtime files to the latest version directory
            string[] runtimeFiles = Directory.GetFiles(runtimeDir, "*", SearchOption.AllDirectories);
            foreach (string runtimeFile in runtimeFiles)
            {
                string destFile = Path.Combine(latestVersion, Path.GetFileName(runtimeFile));
                if (!File.Exists(destFile))
                {
                    File.Copy(runtimeFile, destFile);
                }
            }
        }
    }
}
