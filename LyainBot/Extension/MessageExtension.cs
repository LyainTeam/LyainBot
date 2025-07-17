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
            entities = Program.Client.MarkdownToEntities(ref message);
        }
        Task<UpdatesBase> task = Program.Client.Messages_EditMessage(
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
}
