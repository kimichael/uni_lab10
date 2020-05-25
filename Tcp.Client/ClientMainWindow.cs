using System;
using System.Windows.Forms;
using SomeProject.Library.Client;
using SomeProject.Library;
using System.IO;
using System.Security;

namespace SomeProject.TcpClient
{
    public partial class ClientMainWindow : Form
    {

        private OpenFileDialog openFileDialog;

        public ClientMainWindow()
        {
            InitializeComponent();
            openFileDialog = new OpenFileDialog();
        }

        private void OnMsgBtnClick(object sender, EventArgs e)
        {
            Client client = new Client();
            Result res = client.SendMessageToServer(textBox.Text).Result;
            if(res == Result.OK)
            {
                textBox.Text = "";
                labelRes.Text = "Message was sent succefully!";
            }
            else
            {
                labelRes.Text = "Cannot send the message to the server.";
            }
            timer.Interval = 2000;
            timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            labelRes.Text = "";
            timer.Stop();
        }

        private void sendFileBtn_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Client client = new Client();
                Result res = client.SendFileToServer(openFileDialog.FileName).Result;
                if (res == Result.OK)
                {
                    textBox.Text = "";
                    labelRes.Text = "Message was sent succefully!";
                }
                else
                {
                    labelRes.Text = "Cannot send the message to the server.";
                }
                timer.Interval = 2000;
                timer.Start();
            }
        }
    }
}
