using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Provider.Exchange.Topic
{
    public class TopicSend
    {
        public static void SendMessage()
        {
            using (var connection = RabbitMQHelper.GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("topic_exchange", "topic");

                    // 创建队列
                    string queueName1 = "topic_queue1";
                    channel.QueueDeclare(queueName1, false, false, false, null);
                    string queueName2 = "topic_queue2";
                    channel.QueueDeclare(queueName2, false, false, false, null);
                    string queueName3 = "topic_queue3";
                    channel.QueueDeclare(queueName3, false, false, false, null);

                    // 绑定到交互机
                    channel.QueueBind(queue: queueName1, exchange: "topic_exchange", routingKey: "user.data.#");
                    channel.QueueBind(queue: queueName2, exchange: "topic_exchange", routingKey: "user.data.delete");
                    channel.QueueBind(queue: queueName3, exchange: "topic_exchange", routingKey: "user.data.update");

                    // routingKey:user.data.abc
                    for (int i = 0; i < 10; i++)
                    {
                        string message = $"RabbitMQ Topic {i + 1} abc Message";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("topic_exchange", "user.data.abc", null, body);
                        Console.WriteLine($"Sended Topic abc Message {i}");
                    }
                    // routingKey:user.data.abc
                    for (int i = 0; i < 10; i++)
                    {
                        string message = $"RabbitMQ Topic {i + 1} delete Message";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("topic_exchange", "user.data.delete", null, body);
                        Console.WriteLine($"Sended Topic delete Message {i}");
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        string message = $"RabbitMQ Topic {i + 1} update Message";
                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("topic_exchange", "user.data.update", null, body);
                        Console.WriteLine($"Sended Topic update Message {i}");
                    }
                }
            }
        }
    }
}
