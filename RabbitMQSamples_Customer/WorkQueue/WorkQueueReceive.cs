using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQSamples_Customer.WorkQueue
{
    public class WorkQueueReceive
    {
        public static void ReceiveMessage()
        {
            string queueName = "Worker_Queue";
            var connection = RabbitMQHelper.GetConnection();
            {
                var channel = connection.CreateModel();
                {
                    channel.QueueDeclare(queueName, false, false, false, null);
                    var consumer = new EventingBasicConsumer(channel);
                    //设置prefetchCount : 1来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时，不再分配任务。
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); //能者多劳
                    consumer.Received += (model, ea) => {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        Console.WriteLine(" Worker Queue Received => {0}", message);
                    };
                    channel.BasicConsume(queueName, true, consumer);
                }
            }
        }
    }
}
