using RabbitMQSamples_Customer.Exchange.Direct;
using RabbitMQSamples_Customer.Exchange.Fanout;
using RabbitMQSamples_Customer.Exchange.Topic;
using RabbitMQSamples_Customer.Simplest;
using RabbitMQSamples_Customer.WorkQueue;

namespace RabbitMQSamples_Customer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 简单队列
            //SimplestReceive.ReceiveMessage();
            // 工作队列
            //WorkQueueReceive.ReceiveMessage();
            // 扇形队列
            //FanoutReceive.ReceiveMessage();
            // 直接队列
            //DirectReceive.ReceiveMessage();
            // 模糊匹配
            //TopicReceive.ReceiveMessage();
            Console.ReadLine();
        }
    }
}