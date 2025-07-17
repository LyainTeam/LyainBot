namespace LyainBot.Exceptions;

public class TelegramException : Exception
{
    public TelegramException(string message) : base(message)
    {
    }

    public TelegramException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
