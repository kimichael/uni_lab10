using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace SomeProject.Library.Server
{
    /// <summary>
    /// Класс, инкапсулирующий поведение сервера
    /// </summary>
    public class Server
    {

        private const string fileHeader = "file:";

        private const string messageHeader = "messageHeader:";

        private int receivedFileNumber = 0;

        TcpListener serverListener;

        const int MAXIMUM_CONNECTION_COUNT = 2;

        private int currentConnections = 0;

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Loopback, 8080);
        }

        public bool TurnOffListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Stop();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn off listener: " + e.Message);
                return false;
            }
        }

        public async Task TurnOnListener()
        {
            try
            {
                if (serverListener != null)
                    serverListener.Start();
                while (true)
                {
                    if (currentConnections >= MAXIMUM_CONNECTION_COUNT) {
                        Console.WriteLine("Too many connections!");
                    }
                    OperationResult result = await ReceiveMessageFromClient();
                    if (result.Result == Result.Fail)
                        Console.WriteLine("Unexpected error: " + result.Message);
                    else
                        Console.WriteLine("New message from client: " + result.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot turn on listener: " + e.Message);
            }
        }

        /// <summary>
        /// Метод, ждущий и принимающий сигнал от клиента
        /// </summary>
        /// <returns>Возвращает операцию</returns>
        public async Task<OperationResult> ReceiveMessageFromClient()
        {
            try
            {
                Interlocked.Increment(ref currentConnections);
                Console.WriteLine("Waiting for connections...");
                StringBuilder receivedCommand = new StringBuilder();
                String str = "";
                
                TcpClient client = serverListener.AcceptTcpClient();

                byte[] data = new byte[256];
                NetworkStream stream = client.GetStream();
                int firstBytes = stream.Read(data, 0, data.Length);
                string firstMessage = Encoding.UTF8.GetString(data, 0, firstBytes);

                var res = parseCommand(firstMessage, receivedCommand.ToString(), stream);

                while (stream.DataAvailable);
                stream.Close();
                client.Close();
                Interlocked.Decrement(ref currentConnections);
                return new OperationResult(Result.OK, res);
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
        }

        /// <summary>
        /// Метод, который идентифицирует полученное сообщение и обрабатывает его
        /// </summary>
        /// <returns>Возвращает строку в которой результат обработки сигнала</returns>
        private string parseCommand(string headerMessage, string command, NetworkStream stream) {
            var tokens = headerMessage.Split(';');
            var header = tokens[0];
            if (header == fileHeader)
            {
                return ReceiveFileFromClient(tokens[1], tokens[2], stream);
            }
            else if (header == messageHeader) {
                return ReceiveMessageFromClient(tokens[1], stream);
            }
            return $"Unknown command: {command}";
        }
        
        /// <summary>
        /// Метод, который обрабатывает сообщение от клиента
        /// </summary>
        /// <returns>Возвращает строку в которой результат обработки сигнала</returns>
        public string ReceiveMessageFromClient(string firstMessage, NetworkStream stream) {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(firstMessage);
            byte[] data = new byte[256];

            do
            {
                int bytes = stream.Read(data, 0, data.Length);
                string message = Encoding.UTF8.GetString(data, 0, bytes);
                stringBuilder.Append(message);
            } while (stream.DataAvailable);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Метод, который обрабатывает файл от клиента
        /// </summary>
        /// <returns>Возвращает строку в которой результат обработки сигнала</returns>
        public string ReceiveFileFromClient(string extension, string firstChunk, NetworkStream stream)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            Interlocked.Increment(ref receivedFileNumber);
            var filename = $"File{receivedFileNumber}{extension}";
            var newDir = Directory.CreateDirectory(Path.Combine(currentDirectory, DateTime.Now.ToString("yyyy-MM-dd")));
            var newFile = File.Create(Path.Combine(newDir.FullName, filename));
            newFile.Close();

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(newFile.Name))
            {
                byte[] data = new byte[256];
                file.Write(firstChunk);
                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    string message = Encoding.UTF8.GetString(data, 0, bytes);
                    Console.WriteLine(message);
                    file.Write(message);
                } while (stream.DataAvailable);
            }
            return $"File was successfully written to {newFile.Name}";
        }

        /// <summary>
        /// Метод, который отправляет сообщение клиенту
        /// </summary>
        /// <returns>Возвращает строку в которой результат обработки сигнала</returns>
        public OperationResult SendMessageToClient(string message)
        {
            try
            {
                TcpClient client = serverListener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                return new OperationResult(Result.Fail, e.Message);
            }
            return new OperationResult(Result.OK, "");
        }
    }
}