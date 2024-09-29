namespace SimpleUDPServer
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static List<IPEndPoint> clients = new List<IPEndPoint>();
        static bool running = true;

        static async Task Main(string[] args)
        {
            UdpClient udpServer = new UdpClient(11000);
            Console.WriteLine("UDP сервер запущен и ожидает сообщений...");

            // Запускаем отдельный поток для обработки команд с консоли
            Task.Run(() => HandleConsoleCommands());

            while (running)
            {
                var receivedResults = await udpServer.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(receivedResults.Buffer);

                Console.WriteLine($"Получено сообщение: {receivedMessage} от {receivedResults.RemoteEndPoint}");

                if (!clients.Contains(receivedResults.RemoteEndPoint))
                {
                    clients.Add(receivedResults.RemoteEndPoint);
                }

                string responseMessage = "Сообщение получено";
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
                await udpServer.SendAsync(responseBytes, responseBytes.Length, receivedResults.RemoteEndPoint);

                // Пересылаем сообщение всем клиентам
                foreach (var client in clients)
                {
                    if (!client.Equals(receivedResults.RemoteEndPoint))
                    {
                        await udpServer.SendAsync(receivedResults.Buffer, receivedResults.Buffer.Length, client);
                    }
                }
            }

            udpServer.Close();
            Console.WriteLine("Сервер остановлен.");
        }

        static void HandleConsoleCommands()
        {
            while (running)
            {
                string command = Console.ReadLine();
                if (command.Trim().ToLower() == "stop")
                {
                    Console.WriteLine("Команда 'stop' получена. Завершение работы сервера...");
                    running = false;
                }
                else if (command.Trim().ToLower() == "count")
                {
                    Console.WriteLine($"Количество подключенных клиентов: {clients.Count}");
                }
            }
        }
    }

}