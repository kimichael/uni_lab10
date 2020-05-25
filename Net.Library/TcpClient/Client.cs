using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace SomeProject.Library.Client
{
    public class Client
    {
        private const string fileHeader = "file:";

        private const string messageHeader = "messageHeader:";

        public TcpClient tcpClient;

        public OperationResult ReceiveMessageFromServer()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                StringBuilder recievedMessage = new StringBuilder();
                byte[] data = new byte[256];
                NetworkStream stream = tcpClient.GetStream();
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    recievedMessage.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);
                stream.Close();
                tcpClient.Close();

                return new OperationResult(Result.OK, recievedMessage.ToString());
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.ToString());
            }
        }

        public OperationResult SendMessageToServer(string message)
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"{messageHeader};{message}");
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "") ;
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        public OperationResult SendFileToServer(string fileName) {
            try
            {
                var sr = new StreamReader(fileName);
                var file = File.ReadAllText(fileName);
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"{fileHeader};{Path.GetExtension(fileName)};{file}");

                tcpClient = new TcpClient("127.0.0.1", 8080);
                NetworkStream stream = tcpClient.GetStream();
            
                stream.Write(data, 0, data.Length);
                stream.Close();
                tcpClient.Close();
                return new OperationResult(Result.OK, "");
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            finally { 
            
            }
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }
    }
}
