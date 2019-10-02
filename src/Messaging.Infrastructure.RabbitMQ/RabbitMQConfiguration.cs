﻿namespace Naos.Messaging.Infrastructure.RabbitMQ
{
    public class RabbitMQConfiguration
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5672;

        public string UserName { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public int RetryCount { get; set; } = 5;
    }
}
