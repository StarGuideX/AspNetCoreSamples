using RabbitMQ.Client;
using RabbitMQSamples_Provider.Exchange.Direct;
using RabbitMQSamples_Provider.Exchange.Fanout;
using RabbitMQSamples_Provider.Exchange.Topic;
using RabbitMQSamples_Provider.Simplest;
using RabbitMQSamples_Provider.WorkQueue;

namespace RabbitMQSamples_Provider
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 简单队列
            //SimplestSend.SendMessage();
            // 工作队列
            //WorkQueueSend.SendMessage();
            // 扇形队列
            // FanoutSend.SendMessage();
            // 直接队列
            //DirectSend.SendMessage();
            // 模糊匹配
            //TopicSend.SendMessage();
        }
    }
}