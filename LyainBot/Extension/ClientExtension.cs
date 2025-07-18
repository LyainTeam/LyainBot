using LyainBot.Utils;
using TL;
using WTelegram;
// ReSharper disable UnusedType.Global

namespace LyainBot.Extension;

public static class ClientExtension
{
    // ReSharper disable InconsistentNaming
    private const long MIN_CHANNEL_ID = -1002147483647;
    private const long MAX_CHANNEL_ID = -1000000000000;
    private const long MIN_CHAT_ID = -2147483647;
    private const long MAX_USER_ID = 9999999999;
    // ReSharper restore InconsistentNaming
    
    public static async Task SendImageAsSticker(this Client client, string usernameOrId, byte[] imageData)
    {
        byte[] processedImage = ImageProcessor.ProcessImage(imageData);
        using MemoryStream stream = new(processedImage);
        InputFileBase baseFile = await client.UploadFileAsync(stream, "sticker.webp");
        InputMediaUploadedDocument inputDocument = new()
        {
            mime_type = "image/webp",
            file = baseFile,
            attributes =
            [
                new DocumentAttributeFilename()
                {
                    file_name = "sticker.webp",
                }
            ]
        };
        await client.Messages_SendMedia(await client.ResolvePeer(usernameOrId), inputDocument, "", 0);
    }

    public static async Task<InputPeer> ResolvePeer(this Client client, string usernameOrId)
    {
        if (usernameOrId is "me" or "self")
        {
            return InputPeer.Self;
        }
        
        if (long.TryParse(usernameOrId, out long id))
        {
            string type = GuessPeerType(id);
            
            switch (type)
            {
                case "user":
                {
                    UserBase[] users = await client.Users_GetUsers([new InputUser(id, 0)]);
                    if (users.Length == 0) throw new ArgumentException("User not found.", nameof(usernameOrId));
                    return users[0].ToInputPeer();
                }
                case "chat":
                {
                    Messages_Chats chats = await client.Messages_GetChats([id]);
                    if (chats.chats.Count == 0) throw new ArgumentException("Chat not found.", nameof(usernameOrId));
                    return chats.chats[id].ToInputPeer();
                }
                case "channel":
                {
                    Messages_Chats channels = await client.Channels_GetChannels([new InputChannel(id, 0)]);
                    if (channels.chats.Count == 0) throw new ArgumentException("Channel not found.", nameof(usernameOrId));
                    return channels.chats[id].ToInputPeer();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(usernameOrId), "The provided ID does not match any known peer type.");
            }
        }

        Contacts_ResolvedPeer peer = await client.Contacts_ResolveUsername(usernameOrId);
        if (peer.peer == null) throw new ArgumentException("Cannot resolve the provided username.", nameof(usernameOrId));
        return peer.peer.ToInputPeer();
    }

    public static string GuessPeerType(long id)
    {
        if (id < 0)
        {
            if (id >= MIN_CHAT_ID) return "chat";
            if (id >= MIN_CHANNEL_ID && id < MAX_CHANNEL_ID) return "channel";
        } 
        else if (id <= MAX_USER_ID)
        {
            return "user";
        }
        throw new ArgumentOutOfRangeException(nameof(id), "The provided ID does not match any known peer type.");
    }
}