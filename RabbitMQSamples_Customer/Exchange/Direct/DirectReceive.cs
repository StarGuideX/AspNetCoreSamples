using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Customer.Exchange.Direct
{
    public class DirectReceive
    {
        public static void ReceiveMessage()
        {
            var connection = RabbitMQHelper.GetConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("direct_exchange", "direct", durable: false, autoDelete: false, arguments: null);

            for (int i = 0; i < 3; i++)
            {
                string queueName = $"direct_exchange_{i + 1}";
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queueName, exchange: "direct_exchange", routingKey: $"direct_key_{i + 1}", arguments: null);
            }

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;
                Console.WriteLine("Direct Received {0} from {1}", message, routingKey);

                // 消费完成后需要手动签收消息，如果不写该代码就容易导致重复消费问题
                channel.BasicAck(ea.DeliveryTag, true); // 可以降低每次签收性能损耗
            };
            // 消息签收模式
            // 手动签收 保证正确消费，不会丢消息(基于客户端而已)
            // 自动签收 容易丢消息 
            // 签收：意味着消息从队列中删除
            channel.BasicConsume(queue: "direct_queue_1", autoAck: false, consumer: consumer);
        }
    }
}
