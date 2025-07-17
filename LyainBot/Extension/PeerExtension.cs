using TL;

namespace LyainBot.Extension;

public static class PeerExtension
{
    public static InputPeer ToInputPeer(this Peer peer)
    {
        IPeerInfo peerInfo = Program.UpdateManager.UserOrChat(peer);
        return peerInfo.ToInputPeer();
    }
}
