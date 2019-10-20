using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SerjTm.Lifehack.Chat.Server
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //var cancelTokenSource = new CancellationTokenSource();
            var cancel = new Cancel();

            await Task.WhenAll(ChatServer(cancel), CancelByAnyKey(cancel));

        }
        static async Task CancelByAnyKey(Cancel cancel)
        {
            Console.WriteLine("Press any key to stop");
            Console.ReadKey();
            cancel.TokenSource.Cancel();
        }
        static async Task ChatServer(Cancel cancel)
        {
            var listener = new TcpListener(IPAddress.Any, 8347);
            listener.Start();
            try
            {
                var sessions = new List<Task>();
                while (!cancel.Token.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync().OrCancel(cancel);
                    Console.WriteLine("Присоединился клиент");

                    var session = Task.Run(() => ChatSession(client, cancel));
                    sessions.Add(session);

                    foreach (var _session in sessions)
                        Console.WriteLine($"Id: {_session.Id}");
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        static async Task ChatSession(TcpClient client, Cancel cancel)
        {
            try
            {
                using (client)
                {
                    Console.WriteLine("Запуск обработки клиента");
                    var stream = client.GetStream();
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        writer.WriteLine("Представьтесь, пожалуйста");
                        writer.WriteLine();
                        var name = await reader.ReadLineAsync().OrCancel(cancel);
                        writer.WriteLine($"{name}, добро пожаловать!");
                        writer.WriteLine();
                        while (!cancel.Token.IsCancellationRequested)
                        {
                            var command = await reader.ReadLineAsync().OrCancel(cancel);
                            Console.WriteLine(command);
                            if (command == "Пока")
                                break;
                            var answer = ProcessCommand(command) ?? "Неизвестная команда";
                            writer.WriteLine(answer);
                            writer.WriteLine();
                        }
                        writer.WriteLine("Пока");
                        Console.WriteLine("Завершена обработка клиента"); 
                        //writer.WriteLine();
                    }
                }
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc);
                throw;
            }
        }
        static string ProcessCommand(string line)
        {
            switch (line)
            {
                case "Как дела?":
                    return "Хорошо";
            }
            return null;
        }
    }
}
