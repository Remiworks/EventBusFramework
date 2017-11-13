﻿namespace RabbitFramework
{
    public class BusOptions
    {
        public string Hostname { get; set; }
        public string ExchangeName { get; set; }
        public string VirtualHost { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}