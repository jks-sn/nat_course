// Manager/Options/RabbitOptions.cs

namespace Manager.Options;

public class RabbitOptions
{
    public string Host { get; set; } = "rabbitmq";
    public string HostName
    {
        get => Host;
        set => Host = value;
    }
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string TaskQueue { get; set; } = "crack.tasks";
    public string ResultQueue { get; set; } = "crack.results";
}