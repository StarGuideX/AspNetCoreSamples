using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Customer.Simplest
{
    public class SimplestReceive
    {
        public static void ReceiveMessage()
        {
            string queueName = "simplest";

            var connection = RabbitMQHelper.GetConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine("Simplest Received {0}", message);
            };
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }
    }
}
