using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace SomeProject.Library.Client
{
    /// <summary>
    /// Класс, инкапсулирующий поведение клиента
    /// </summary>
    public class Client
    {
        private const string fileHeader = "file:";

        private const string messageHeader = "messageHeader:";

        public TcpClient tcpClient;

        /// <summary>
        /// Метод, ждущий и принимающий сигнал от сервера
        /// </summary>
        /// <returns>Возвращает результат операции</returns>
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

        /// <summary>
        /// Метод, отправляющий сообщение на сервер
        /// </summary>
        /// <returns>Возвращает результат операции</returns>
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

        /// <summary>
        /// Метод, отправляющий файл на сервер
        /// </summary>
        /// <returns>Возвращает результат операции</returns>
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
}
