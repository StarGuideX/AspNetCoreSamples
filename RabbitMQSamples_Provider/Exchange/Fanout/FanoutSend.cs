using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Provider.Exchange.Fanout
{
    public class FanoutSend
    {
        public static void SendMessage()
        {
            using (var connection = RabbitMQHelper.GetConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("fanout_exchange", "fanout", durable: false, autoDelete: false, arguments: null);
                for (int i = 0; i < 3; i++)
                {
                    string queueName = $"fanout_queue_{i + 1}";
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    channel.QueueBind(queue: queueName, exchange: "fanout_exchange", routingKey: "", arguments: null);
                }
                for (int i = 0; i < 10; i++)
                {
                    string message = $"Hello RabbitMQ Message {i}";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "fanout_exchange", routingKey: "", false, null, body);
                    Console.WriteLine($"Sended Fanout Message {i}");
                }
            }
        }
    }
}
