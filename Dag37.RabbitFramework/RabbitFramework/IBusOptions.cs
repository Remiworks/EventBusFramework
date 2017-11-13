namespace RabbitFramework
{
    public interface IBusOptions
    {
        string Hostname { get; }
        string ExchangeName { get; }
        string VirtualHost { get; }
        int Port { get; }
        string UserName { get; }
        string Password { get; }
        IBusProvider Provider { get; }
    }
}