using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RabbitMQSamples_Provider.Simplest
{
    public class SimplestSend
    {
        public static void SendMessage()
        {
            string queueName = "simplest";

            using (var connection = RabbitMQHelper.GetConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                    while (true)
                    {
                        string message = "Hello RabbitMQ Message";
                        var body = Encoding.UTF8.GetBytes(message);
                        // 如果(AMQP default)
                        // 发送消息到rabbitmq,使用rabbitmq中默认提供交换机路由,默认的路由Key和队列名称完全一致
                        channel.BasicPublish(exchange: "", routingKey: queueName, null, body);
                        Thread.Sleep(1000);
                        Console.WriteLine("Send Simplest message");
                    }
                }
            }
        }
    }
}
