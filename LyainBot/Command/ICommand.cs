using TL;
using WTelegram;

namespace LyainBot.Command;

public interface ICommand
{
    public void OnInvoke(Message message, UpdateManager manager, Client client, string[] args);
}
