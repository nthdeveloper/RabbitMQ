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
        SimpleRabbitClient rabbitClient;

        public Form1()
        {
            InitializeComponent();
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

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
            }
            catch(Exception ex)
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
