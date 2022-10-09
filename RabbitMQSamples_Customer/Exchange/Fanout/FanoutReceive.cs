using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Customer.Exchange.Fanout
{
    public class FanoutReceive
    {
        public static void ReceiveMessage()
        {
            var connection = RabbitMQHelper.GetConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("fanout_exchange", "fanout", durable: false, autoDelete: false, arguments: null);

            for (int i = 0; i < 3; i++)
            {
                string queueName = $"fanout_queue_{i + 1}";
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queueName, exchange: "fanout_exchange", routingKey: "", arguments: null);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                Console.WriteLine("Fanout Received {0}", message);
            };
            channel.BasicConsume("fanout_queue_1", autoAck: true, consumer);
        }
    }
}
