using TL;

namespace LyainBot.Extension;

public static class PeerExtension
{
    public static InputPeer ToInputPeer(this Peer peer)
    {
        IPeerInfo peerInfo = LyainBotApp.UpdateManager.UserOrChat(peer);
        return peerInfo.ToInputPeer();
    }
}
