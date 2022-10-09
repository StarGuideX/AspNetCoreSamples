using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Customer.Exchange.Topic
{
    public class TopicReceive
    {
        public static void ReceiveMessage()
        {
            var queueName = "topic_queue1";
         
            var connection = RabbitMQHelper.GetConnection();
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("topic_exchange", "topic", durable: false, autoDelete: false, arguments: null);

            channel.QueueDeclare(queueName, false, false, false, null);
            channel.QueueBind(queue: queueName, exchange: "topic_exchange", routingKey: "user.data.insert");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var routingKey = ea.RoutingKey;
                Console.WriteLine("Topic Received {0} from {1}", message, routingKey);
            };
            // 消费是基于队列，与routingKey无关
            channel.BasicConsume(queueName, autoAck: true, consumer);
        }
    }
}
