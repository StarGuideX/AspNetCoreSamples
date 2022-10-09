using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Provider.Exchange.Direct
{
    public class DirectSend
    {
        public static void SendMessage()
        {
            using (var connection = RabbitMQHelper.GetConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare("direct_exchange", "direct", durable: false, autoDelete: false, arguments: null);
                for (int i = 0; i < 3; i++)
                {
                    string queueName = $"direct_queue_{i + 1}";
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    // 发送消息的时候需要指定routingKey发送
                    channel.QueueBind(queue: queueName, exchange: "direct_exchange", routingKey: $"direct_key_{i + 1}", arguments: null);
                }
                // 分别往direct_key_1、direct_key_2、direct_key_3放入10条
                for (int i = 0; i < 3; i++)
                {
                    var tempRoutingKey = $"direct_key_{i + 1}";
                    for (int j = 0; j < 10; j++)
                    {
                        string message = $"Hello RabbitMQ Message {j} to direct_key_{i + 1}";
                        var body = Encoding.UTF8.GetBytes(message);
                        // 发送消息的时候需要指定routingKey发送
                        channel.BasicPublish(exchange: "direct_exchange", routingKey: tempRoutingKey, false, null, body);
                        Console.WriteLine($"Sended Message {j} to direct_key_{i + 1}");
                    }
                }
                
            }
        }
    }
}
