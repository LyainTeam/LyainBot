using System.Text.Json.Serialization;
using System.Text.Json;

namespace LyainBot;

public class ClientConfig
{
    public string? AppId { get; set; }
    public string? AppHash { get; set; }
    public string? CommandPrefix { get; set; } = ",";
    public string? Phone { get; set; }
    public string? MTProxyUrl { get; set; }

    public static ClientConfig GetConfig()
    {
        FileInfo file = new("client_config.json");
        if (!file.Exists)
        {
            // create a new file with default values if it doesn't exist
            ClientConfig defaultConfig = new()
            {
                AppId = "your_app_id_here",
                AppHash = "your_app_hash_here",
                CommandPrefix = ",",
                Phone = null,
                MTProxyUrl = null
            };
            string serialize = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(file.FullName, serialize);
            Console.WriteLine("client_config.json created with default values. Please fill in your AppId and AppHash.");
            Console.WriteLine("After filling in the values, press Enter to continue.");
            Console.ReadLine();
        }
        string json = File.ReadAllText(file.FullName);
        return JsonSerializer.Deserialize<ClientConfig>(json)
               ?? throw new InvalidOperationException("Failed to deserialize client configuration.");
    }

    public string? ConfigCallback(string what)
    {
        switch (what)
        {
            case "api_id":
                return AppId;
            case "api_hash":
                return AppHash;
            case "phone_number":
                if (Phone == null)
                {
                    Console.Write("Enter your phone number: ");
                    Phone = Console.ReadLine()?.Trim();
                    FileInfo file = new("client_config.json");
                    string serialize = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(file.FullName, serialize);
                }
                return Phone;
            case "verification_code":
                Console.Write("Enter the verification code you received: ");
                return Console.ReadLine()?.Trim();
            case "first_name":
                Console.Write("Enter your first name: ");
                return Console.ReadLine()?.Trim();
            case "last_name":
                Console.Write("Enter your last name: ");
                return Console.ReadLine()?.Trim();
            case "password":
                Console.Write("Enter your 2fa password: ");
                return Console.ReadLine()?.Trim();
            case "device_model":
                return "Android 12";
            case "app_version":
                return "11.13.0";
            case "system_version":
                return "12 (31)";
            default:
                return null;
        }
    }
}
