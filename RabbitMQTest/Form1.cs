using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RabbitMQTest
{
    public partial class Form1 : Form
    {
        class ReceivedMessageItem
        {
            public string Queue { get; set; }
            public string Message { get; set; }
        }

        SimpleRabbitClient rabbitClient;

        BindingList<ReceivedMessageItem> receivedMessages;

        public Form1()
        {
            InitializeComponent();

            receivedMessages = new BindingList<ReceivedMessageItem>();

            dgvMessages.AutoGenerateColumns = false;
            dgvMessages.DataSource = receivedMessages;
        }

        private void addToReceivedMessages(string queueName,  string message)
        {
            receivedMessages.Add(new ReceivedMessageItem()
            {
                Queue = queueName,
                Message = message
            });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (rabbitClient == null)
                rabbitClient = new SimpleRabbitClient();

            try
            {
                rabbitClient.Connect(
                    txtHostName.Text,
                    txtVirtualHost.Text,
                    (int)numPortNo.Value,
                    txtUserName.Text,
                    txtPassword.Text
                    );

                rabbitClient.MessageReceived += RabbitClient_MessageReceived;

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
        }        

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                rabbitClient.Disconnect();

                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                rabbitClient.SendMessage(
                    txtExchangeName.Text, 
                    txtRoutingKey.Text, 
                    txtMessage.Text);

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
        }        

        private void btnSubscribe_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtSubscribeQueueName.Text))
                return;

            try
            {
                rabbitClient.SubscribeToQueue(txtSubscribeQueueName.Text);

                lbxQueues.Items.Add(txtSubscribeQueueName.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
        }

        private void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            if(lbxQueues.SelectedIndex > -1)
            {
                try
                {
                    rabbitClient.UnsubscribeFromQueue((string)lbxQueues.SelectedItem);

                    lbxQueues.Items.RemoveAt(lbxQueues.SelectedIndex);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error:{ex.Message}");
                }
            }
        }

        private void btnReadMessage_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtReadQueueName.Text))
                return;

            try
            {
                string messageContent = rabbitClient.ReadFromQueue(txtReadQueueName.Text);

                if(String.IsNullOrEmpty(messageContent))
                {
                    MessageBox.Show("There is no message in this queue.");
                }
                else
                {
                    addToReceivedMessages(txtReadQueueName.Text, messageContent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
        }

        private void lbxQueues_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnUnsubscribe.Enabled = lbxQueues.SelectedIndex > -1;
        }

        private void RabbitClient_MessageReceived(SimpleRabbitClient sender, QueueMessageEventArgs e)
        {            
            this.BeginInvoke(new Action(() =>
            {
                addToReceivedMessages(e.QueueName, e.MessageData);
            }));

            e.IsProcessed = true;
        }

        private void dgvMessages_SelectionChanged(object sender, EventArgs e)
        {
            txtReceivedMessageContent.Text = String.Empty;

            if (dgvMessages.SelectedRows.Count > 0)
            {
                ReceivedMessageItem _messageItem = (ReceivedMessageItem)dgvMessages.SelectedRows[0].DataBoundItem;

                txtReceivedMessageContent.Text = _messageItem.Message;
            }
        }
    }
}
