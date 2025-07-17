using System.Reflection;
using TL;
#pragma warning disable CS0067 // Event is never used

namespace LyainBot;

public class EventHandler
{
    public event Action<UpdateNewMessage>? OnUpdateNewMessage;
    public event Action<UpdateMessageID>? OnUpdateMessageId;
    public event Action<UpdateDeleteMessages>? OnUpdateDeleteMessages;
    public event Action<UpdateUserTyping>? OnUpdateUserTyping;
    public event Action<UpdateChatUserTyping>? OnUpdateChatUserTyping;
    public event Action<UpdateChatParticipants>? OnUpdateChatParticipants;
    public event Action<UpdateUserStatus>? OnUpdateUserStatus;
    public event Action<UpdateUserName>? OnUpdateUserName;
    public event Action<UpdateNewAuthorization>? OnUpdateNewAuthorization;
    public event Action<UpdateNewEncryptedMessage>? OnUpdateNewEncryptedMessage;
    public event Action<UpdateEncryptedChatTyping>? OnUpdateEncryptedChatTyping;
    public event Action<UpdateEncryption>? OnUpdateEncryption;
    public event Action<UpdateEncryptedMessagesRead>? OnUpdateEncryptedMessagesRead;
    public event Action<UpdateChatParticipantAdd>? OnUpdateChatParticipantAdd;
    public event Action<UpdateChatParticipantDelete>? OnUpdateChatParticipantDelete;
    public event Action<UpdateDcOptions>? OnUpdateDcOptions;
    public event Action<UpdateNotifySettings>? OnUpdateNotifySettings;
    public event Action<UpdateServiceNotification>? OnUpdateServiceNotification;
    public event Action<UpdatePrivacy>? OnUpdatePrivacy;
    public event Action<UpdateUserPhone>? OnUpdateUserPhone;
    public event Action<UpdateReadHistoryInbox>? OnUpdateReadHistoryInbox;
    public event Action<UpdateReadHistoryOutbox>? OnUpdateReadHistoryOutbox;
    public event Action<UpdateWebPage>? OnUpdateWebPage;
    public event Action<UpdateReadMessagesContents>? OnUpdateReadMessagesContents;
    public event Action<UpdateChannelTooLong>? OnUpdateChannelTooLong;
    public event Action<UpdateChannel>? OnUpdateChannel;
    public event Action<UpdateNewChannelMessage>? OnUpdateNewChannelMessage;
    public event Action<UpdateReadChannelInbox>? OnUpdateReadChannelInbox;
    public event Action<UpdateDeleteChannelMessages>? OnUpdateDeleteChannelMessages;
    public event Action<UpdateChannelMessageViews>? OnUpdateChannelMessageViews;
    public event Action<UpdateChatParticipantAdmin>? OnUpdateChatParticipantAdmin;
    public event Action<UpdateNewStickerSet>? OnUpdateNewStickerSet;
    public event Action<UpdateStickerSetsOrder>? OnUpdateStickerSetsOrder;
    public event Action<UpdateStickerSets>? OnUpdateStickerSets;
    public event Action<UpdateSavedGifs>? OnUpdateSavedGifs;
    public event Action<UpdateBotInlineQuery>? OnUpdateBotInlineQuery;
    public event Action<UpdateBotInlineSend>? OnUpdateBotInlineSend;
    public event Action<UpdateEditChannelMessage>? OnUpdateEditChannelMessage;
    public event Action<UpdateBotCallbackQuery>? OnUpdateBotCallbackQuery;
    public event Action<UpdateEditMessage>? OnUpdateEditMessage;
    public event Action<UpdateInlineBotCallbackQuery>? OnUpdateInlineBotCallbackQuery;
    public event Action<UpdateReadChannelOutbox>? OnUpdateReadChannelOutbox;
    public event Action<UpdateDraftMessage>? OnUpdateDraftMessage;
    public event Action<UpdateReadFeaturedStickers>? OnUpdateReadFeaturedStickers;
    public event Action<UpdateRecentStickers>? OnUpdateRecentStickers;
    public event Action<UpdateConfig>? OnUpdateConfig;
    public event Action<UpdatePtsChanged>? OnUpdatePtsChanged;
    public event Action<UpdateChannelWebPage>? OnUpdateChannelWebPage;
    public event Action<UpdateDialogPinned>? OnUpdateDialogPinned;
    public event Action<UpdatePinnedDialogs>? OnUpdatePinnedDialogs;
    public event Action<UpdateBotWebhookJSON>? OnUpdateBotWebhookJson;
    public event Action<UpdateBotWebhookJSONQuery>? OnUpdateBotWebhookJsonQuery;
    public event Action<UpdateBotShippingQuery>? OnUpdateBotShippingQuery;
    // ReSharper disable once IdentifierTypo
    public event Action<UpdateBotPrecheckoutQuery>? OnUpdateBotPrecheckoutQuery;
    public event Action<UpdatePhoneCall>? OnUpdatePhoneCall;
    public event Action<UpdateLangPackTooLong>? OnUpdateLangPackTooLong;
    public event Action<UpdateLangPack>? OnUpdateLangPack;
    // ReSharper disable once IdentifierTypo
    public event Action<UpdateFavedStickers>? OnUpdateFavedStickers;
    public event Action<UpdateChannelReadMessagesContents>? OnUpdateChannelReadMessagesContents;
    public event Action<UpdateContactsReset>? OnUpdateContactsReset;
    public event Action<UpdateChannelAvailableMessages>? OnUpdateChannelAvailableMessages;
    public event Action<UpdateDialogUnreadMark>? OnUpdateDialogUnreadMark;
    public event Action<UpdateMessagePoll>? OnUpdateMessagePoll;
    public event Action<UpdateChatDefaultBannedRights>? OnUpdateChatDefaultBannedRights;
    public event Action<UpdateFolderPeers>? OnUpdateFolderPeers;
    public event Action<UpdatePeerSettings>? OnUpdatePeerSettings;
    public event Action<UpdatePeerLocated>? OnUpdatePeerLocated;
    public event Action<UpdateNewScheduledMessage>? OnUpdateNewScheduledMessage;
    public event Action<UpdateDeleteScheduledMessages>? OnUpdateDeleteScheduledMessages;
    public event Action<UpdateTheme>? OnUpdateTheme;
    public event Action<UpdateGeoLiveViewed>? OnUpdateGeoLiveViewed;
    public event Action<UpdateLoginToken>? OnUpdateLoginToken;
    public event Action<UpdateMessagePollVote>? OnUpdateMessagePollVote;
    public event Action<UpdateDialogFilter>? OnUpdateDialogFilter;
    public event Action<UpdateDialogFilterOrder>? OnUpdateDialogFilterOrder;
    public event Action<UpdateDialogFilters>? OnUpdateDialogFilters;
    public event Action<UpdatePhoneCallSignalingData>? OnUpdatePhoneCallSignalingData;
    public event Action<UpdateChannelMessageForwards>? OnUpdateChannelMessageForwards;
    public event Action<UpdateReadChannelDiscussionInbox>? OnUpdateReadChannelDiscussionInbox;
    public event Action<UpdateReadChannelDiscussionOutbox>? OnUpdateReadChannelDiscussionOutbox;
    public event Action<UpdatePeerBlocked>? OnUpdatePeerBlocked;
    public event Action<UpdateChannelUserTyping>? OnUpdateChannelUserTyping;
    public event Action<UpdatePinnedMessages>? OnUpdatePinnedMessages;
    public event Action<UpdatePinnedChannelMessages>? OnUpdatePinnedChannelMessages;
    public event Action<UpdateChat>? OnUpdateChat;
    public event Action<UpdateGroupCallParticipants>? OnUpdateGroupCallParticipants;
    public event Action<UpdateGroupCall>? OnUpdateGroupCall;
    public event Action<UpdatePeerHistoryTTL>? OnUpdatePeerHistoryTtl;
    public event Action<UpdateChatParticipant>? OnUpdateChatParticipant;
    public event Action<UpdateChannelParticipant>? OnUpdateChannelParticipant;
    public event Action<UpdateBotStopped>? OnUpdateBotStopped;
    public event Action<UpdateGroupCallConnection>? OnUpdateGroupCallConnection;
    public event Action<UpdateBotCommands>? OnUpdateBotCommands;
    public event Action<UpdatePendingJoinRequests>? OnUpdatePendingJoinRequests;
    public event Action<UpdateBotChatInviteRequester>? OnUpdateBotChatInviteRequester;
    public event Action<UpdateMessageReactions>? OnUpdateMessageReactions;
    public event Action<UpdateAttachMenuBots>? OnUpdateAttachMenuBots;
    public event Action<UpdateWebViewResultSent>? OnUpdateWebViewResultSent;
    public event Action<UpdateBotMenuButton>? OnUpdateBotMenuButton;
    public event Action<UpdateSavedRingtones>? OnUpdateSavedRingtones;
    public event Action<UpdateTranscribedAudio>? OnUpdateTranscribedAudio;
    public event Action<UpdateReadFeaturedEmojiStickers>? OnUpdateReadFeaturedEmojiStickers;
    public event Action<UpdateUserEmojiStatus>? OnUpdateUserEmojiStatus;
    public event Action<UpdateRecentEmojiStatuses>? OnUpdateRecentEmojiStatuses;
    public event Action<UpdateRecentReactions>? OnUpdateRecentReactions;
    public event Action<UpdateMoveStickerSetToTop>? OnUpdateMoveStickerSetToTop;
    public event Action<UpdateMessageExtendedMedia>? OnUpdateMessageExtendedMedia;
    public event Action<UpdateChannelPinnedTopic>? OnUpdateChannelPinnedTopic;
    public event Action<UpdateChannelPinnedTopics>? OnUpdateChannelPinnedTopics;
    public event Action<UpdateUser>? OnUpdateUser;
    public event Action<UpdateAutoSaveSettings>? OnUpdateAutoSaveSettings;
    public event Action<UpdateStory>? OnUpdateStory;
    public event Action<UpdateReadStories>? OnUpdateReadStories;
    public event Action<UpdateStoryID>? OnUpdateStoryId;
    public event Action<UpdateStoriesStealthMode>? OnUpdateStoriesStealthMode;
    public event Action<UpdateSentStoryReaction>? OnUpdateSentStoryReaction;
    public event Action<UpdateBotChatBoost>? OnUpdateBotChatBoost;
    public event Action<UpdateChannelViewForumAsMessages>? OnUpdateChannelViewForumAsMessages;
    public event Action<UpdatePeerWallpaper>? OnUpdatePeerWallpaper;
    public event Action<UpdateBotMessageReaction>? OnUpdateBotMessageReaction;
    public event Action<UpdateBotMessageReactions>? OnUpdateBotMessageReactions;
    public event Action<UpdateSavedDialogPinned>? OnUpdateSavedDialogPinned;
    public event Action<UpdatePinnedSavedDialogs>? OnUpdatePinnedSavedDialogs;
    public event Action<UpdateSavedReactionTags>? OnUpdateSavedReactionTags;
    public event Action<UpdateSmsJob>? OnUpdateSmsJob;
    public event Action<UpdateQuickReplies>? OnUpdateQuickReplies;
    public event Action<UpdateNewQuickReply>? OnUpdateNewQuickReply;
    public event Action<UpdateDeleteQuickReply>? OnUpdateDeleteQuickReply;
    public event Action<UpdateQuickReplyMessage>? OnUpdateQuickReplyMessage;
    public event Action<UpdateDeleteQuickReplyMessages>? OnUpdateDeleteQuickReplyMessages;
    public event Action<UpdateBotBusinessConnect>? OnUpdateBotBusinessConnect;
    public event Action<UpdateBotNewBusinessMessage>? OnUpdateBotNewBusinessMessage;
    public event Action<UpdateBotEditBusinessMessage>? OnUpdateBotEditBusinessMessage;
    public event Action<UpdateBotDeleteBusinessMessage>? OnUpdateBotDeleteBusinessMessage;
    public event Action<UpdateNewStoryReaction>? OnUpdateNewStoryReaction;
    public event Action<UpdateStarsBalance>? OnUpdateStarsBalance;
    public event Action<UpdateBusinessBotCallbackQuery>? OnUpdateBusinessBotCallbackQuery;
    public event Action<UpdateStarsRevenueStatus>? OnUpdateStarsRevenueStatus;
    public event Action<UpdateBotPurchasedPaidMedia>? OnUpdateBotPurchasedPaidMedia;
    public event Action<UpdatePaidReactionPrivacy>? OnUpdatePaidReactionPrivacy;
    public event Action<UpdateSentPhoneCode>? OnUpdateSentPhoneCode;
    public event Action<UpdateGroupCallChainBlocks>? OnUpdateGroupCallChainBlocks;
    public event Action<UpdateReadMonoForumInbox>? OnUpdateReadMonoForumInbox;
    public event Action<UpdateReadMonoForumOutbox>? OnUpdateReadMonoForumOutbox;

    public void CallEvent(Update update)
    {
        string eventName = "On" + update.GetType().Name;
        EventInfo? eventInfo = GetType().GetEvent(eventName);
        if (eventInfo == null)
        {
            return;
        }
        FieldInfo? field = GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            return;
        }
        MulticastDelegate? eventDelegate = field.GetValue(this) as MulticastDelegate;
        if (eventDelegate == null) return;
        foreach (Delegate handler in eventDelegate.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(update);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error invoking event {eventName}: {e.Message}");
            }
        }
    }
}
