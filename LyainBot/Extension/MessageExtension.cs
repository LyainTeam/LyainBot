using TL;
using LyainBot.Exceptions;

namespace LyainBot.Extension;

public static class MessageExtension
{
    public static UpdatesBase Edit(this Message origin, string? message = null, InputMedia? media = null, ReplyMarkup? replyMarkup = null, MessageEntity[]? entities = null, DateTime? scheduleDate = null, int? quickReplyShortcutId = null, bool noWebpage = false, bool invertMedia = false)
    {
        InputPeer inputPeer = origin.Peer.ToInputPeer();
        if (entities == null && message != null)
        {
            entities = LyainBotApp.Client.MarkdownToEntities(ref message);
        }
        Task<UpdatesBase> task = LyainBotApp.Client.Messages_EditMessage(
            inputPeer,
            origin.id,
            message,
            media,
            replyMarkup,
            entities,
            scheduleDate,
            quickReplyShortcutId,
            noWebpage,
            invertMedia
        );
        task.Wait();
        return task.Result;
    }

    public static async Task<ImageData> DownloadImageFromReply(this Message origin)
    {
        if (origin.reply_to is not MessageReplyHeader header) throw new ArgumentException("Message is not a reply to another message.", nameof(origin));
        MessageMedia? media = header?.reply_media;
        if (media is not MessageMediaPhoto mphoto || mphoto.photo is PhotoEmpty)
        {
            // try to find photo at document
            if (media is not MessageMediaDocument { document: DocumentEmpty } mdoc)
            {
                throw new ArgumentException("The replied message does not contain a valid photo or document.");
            }
            Document document = mdoc.document as Document ?? throw new InvalidOperationException();
            if (!document.mime_type.StartsWith("image/"))
            {
                throw new ArgumentException("The replied document is not an image.");
            }
            // download the document as image
            using MemoryStream _fileStream = new();
            await LyainBotApp.Client.DownloadFileAsync(document, _fileStream);
            Storage_FileType _type = document.mime_type switch
            {
                "image/jpeg" => Storage_FileType.jpeg,
                "image/gif" => Storage_FileType.gif,
                "image/png" => Storage_FileType.png,
                "image/webp" => Storage_FileType.webp,
                _ => Storage_FileType.unknown
            };
            return new ImageData(_fileStream.ToArray(), _type);
        }
        using MemoryStream fileStream = new();
        Storage_FileType type = await LyainBotApp.Client.DownloadFileAsync(mphoto.photo as Photo, fileStream);
        ImageData data = new(fileStream.ToArray(), type);
        return data;
    }
    
    public static async Task<DocumentData> DownloadDocumentFromReply(this Message origin) 
    {
        if (origin.reply_to is not MessageReplyHeader header) throw new ArgumentException("Message is not a reply to another message.", nameof(origin));
        MessageMedia? media = header?.reply_media;
        if (media is not MessageMediaDocument mdoc || mdoc.document is DocumentEmpty) throw new ArgumentException("The replied message does not contain a valid document.");
        using MemoryStream fileStream = new();
        await LyainBotApp.Client.DownloadFileAsync(mdoc.document as Document, fileStream);
        DocumentData data = new(fileStream.ToArray(), (mdoc.document as Document)!.mime_type);
        return data;
    }

    public static bool IsImage(this MessageMedia media)
    {
        return media is MessageMediaPhoto mphoto && mphoto.photo is not PhotoEmpty ||
               media is MessageMediaDocument mdoc && mdoc.document is Document doc && doc.mime_type.StartsWith("image/");
    }

    public class DocumentData(byte[] data, string mineType)
    {
        public byte[] Data { get; set; } = data;
        public string MimeType { get; set; } = mineType;
    }

    public class ImageData(byte[] data, Storage_FileType fileType)
    {
        public byte[] Data { get; set; } = data;
        public Storage_FileType FileType { get; set; } = fileType;
    }
}
