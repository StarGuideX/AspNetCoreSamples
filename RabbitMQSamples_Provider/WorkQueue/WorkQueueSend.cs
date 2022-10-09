using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Provider.WorkQueue
{
    public class WorkQueueSend
    {
        public static void SendMessage()
        {
            string queueName = "Worker_Queue";

            using (var connection = RabbitMQHelper.GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    for (int i = 0; i < 20; i++)
                    {
                        string message = $"Hello RabbitMQ Message {i}";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "", routingKey: queueName, null, body);
                        Console.WriteLine($"Sended Message {i}");
                    }
                }
            }
        }
    }
}
