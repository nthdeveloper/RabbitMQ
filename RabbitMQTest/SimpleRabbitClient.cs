using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQTest
{
    //https://www.rabbitmq.com/dotnet-api-guide.html
    //https://www.rabbitmq.com/dotnet.html

    class QueueMessageEventArgs
    {
        public string QueueName { get; set; }
        public byte[] MessageData { get; set; }
        public bool IsProcessed { get; set; }
    }

    delegate void QueueMessageHandler(SimpleRabbitClient sender, QueueMessageEventArgs e);

    class SimpleRabbitClient
    {
        IConnection conn;
        IModel channel;
        Dictionary<string, EventingBasicConsumer> m_ConsumerList = new Dictionary<string, EventingBasicConsumer>();

        public event QueueMessageHandler MessageReceived;

        public void Connect(string hostName, string virtualHost, int portNo, string userName, string password)
        {
            ConnectionFactory factory = new ConnectionFactory();
            // "guest"/"guest" by default, limited to localhost connections
            factory.HostName = hostName;
            factory.VirtualHost = virtualHost;
            factory.Port = portNo;
            factory.UserName = userName;
            factory.Password = password;

            conn = factory.CreateConnection();
            channel = conn.CreateModel();
        }

        private void Disconnect()
        {
            foreach (var consumerPair in m_ConsumerList)
            {
                channel.BasicCancel(consumerPair.Value.ConsumerTag);
            }

            m_ConsumerList.Clear();

            if (channel != null)
                channel.Close();

            if (conn != null)
                conn.Close();

            channel = null;
            conn = null;
        }

        public void SendMessage(string exchangeName, string routingKey, string msg)
        {
            if (channel != null)
                throw new Exception("Not connected");

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(msg);

            channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
        }

        public void SendMessageAdvanced(string exchangeName, string routingKey, string msg)
        {
            if (channel != null)
                throw new Exception("Not connected");

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            IBasicProperties props = channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            //props.Headers = new Dictionary<string, object>();
            //props.Headers.Add("latitude", 51.5252949);
            //props.Headers.Add("longitude", -0.0905493);
            //props.Expiration = "36000000";

            channel.BasicPublish(exchangeName,
                               routingKey,
                               props,
                               messageBodyBytes);
        }

        public string ReadFromQueue(string queueName)
        {
            if (channel != null)
                throw new Exception("Not connected");

            bool noAck = false;
            BasicGetResult result = channel.BasicGet(queueName, noAck);
            if (result == null)
            {
                // No message available at this time.
                return null;
            }
            else
            {
                IBasicProperties props = result.BasicProperties;
                byte[] body = result.Body;

                return System.Text.Encoding.UTF8.GetString(body);
            }
        }

        public void SubscribeToQueue(string queueName)
        {
            if (channel != null)
                throw new Exception("Not connected");

            if (m_ConsumerList.ContainsKey(queueName))
                return;

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
            {
                //Process message
                QueueMessageEventArgs _e = new QueueMessageEventArgs()
                {
                    QueueName = queueName,
                    MessageData = ea.Body
                };

                MessageReceived?.Invoke(this, _e);

                if (_e.IsProcessed)
                    channel.BasicAck(ea.DeliveryTag, false);
            };

            String consumerTag = channel.BasicConsume(queueName, false, consumer);

            m_ConsumerList.Add(queueName, consumer);
        }

        public void UnsubscribeFromQueue(string queueName)
        {
            if (m_ConsumerList.ContainsKey(queueName))
            {
                EventingBasicConsumer _basicConsumer = m_ConsumerList[queueName];
                channel.BasicCancel(_basicConsumer.ConsumerTag);

                m_ConsumerList.Remove(queueName);
            }
        }
    }
}
