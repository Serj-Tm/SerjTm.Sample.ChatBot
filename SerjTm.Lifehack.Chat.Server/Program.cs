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
        public static IChatCommand[] Commands(Users users)
        {
            return new IChatCommand[]
            {
                new SimpleCommand("Как дела?", "Хорошо"),
                new SimpleCommand("Как тебя зовут?", "Боб"),
                new SimpleCommand("Какая погода?", "Прохладно"),
                new SimpleCommand("1+1?", "2"),
                new UsersCommand(users),
                new ByeCommand(),
            };
        }

        static async Task Main(string[] args)
        {
            try
            {
                var cancel = new Cancel();

                await Task.WhenAll(ChatServer(cancel), CancelByAnyKey(cancel));

            }
            catch (Exception exc)
            {
                var aggregateExc = exc as AggregateException;
                if (!(aggregateExc?.InnerExceptions?.Count == 1 && aggregateExc?.InnerExceptions[0] is OperationCanceledException))
                    Console.Error.WriteLine(exc);
            }
        }
#pragma warning disable 1998

        static async Task CancelByAnyKey(Cancel cancel)
        {
            Console.WriteLine("Нажмите любую клавишу для останова сервера");
            Console.ReadKey(true);
            Console.WriteLine("Работа сервера прервана пользователем");
            cancel.TokenSource.Cancel();
        }

#pragma warning restore 1998


        static async Task ChatServer(Cancel cancel)
        {
            var listener = new TcpListener(IPAddress.Any, 8347);
            listener.Start();
            try
            {

                var sessions = new List<Task>();
                var users = new Users();
                var processor = new CommandProcessor(Commands(users));

                for (var id = 0;  !cancel.Token.IsCancellationRequested; id++)
                {
                    var client = await listener.AcceptTcpClientAsync().OrCancel(cancel);

                    var session = Task.Run(() => ChatSession(id, users, processor, client, cancel));
                    sessions.Add(session);

                    sessions.RemoveAll(_session => _session.IsCompleted);

                    Console.WriteLine($"Клиентов: {sessions.Count}");
                }
            }
            finally
            {
                listener.Stop();
            }
        }
        static async Task ChatSession(int id, Users users, CommandProcessor processor, TcpClient client, Cancel cancel)
        {
            try
            {
                using (client)
                {
                    Console.WriteLine($"{id}: Начата обработка");
                    var stream = client.GetStream();
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        writer.WriteLine("Представьтесь, пожалуйста");
                        writer.WriteLine();
                        var name = await reader.ReadLineAsync().OrCancel(cancel);

                        users.Add(id, name);
                        try
                        {

                            writer.WriteLine($"{name}, добро пожаловать!");
                            writer.WriteLine("Доступные команды:");
                            foreach (var command in processor.Commands)
                                writer.WriteLine($"  {command.Name}");
                            writer.WriteLine();

                            while (!cancel.Token.IsCancellationRequested)
                            {
                                var command = await reader.ReadLineAsync().OrCancel(cancel);
                                Console.WriteLine($"{id}: {command}");

                                var answer = processor.Process(command);
                                if (answer.answer != "")
                                    writer.WriteLine(answer.answer ?? "Неизвестная команда");
                                if (answer.isEnd)
                                    break;
                                writer.WriteLine();
                            }
                        }
                        finally
                        {
                            users.Remove(id);
                            Console.WriteLine($"{id}: Завершена обработка");
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                var aggregateExc = exc as AggregateException;
                if (!(aggregateExc?.InnerExceptions?.Count == 1 && aggregateExc?.InnerExceptions[0] is OperationCanceledException))
                    Console.Error.WriteLine(exc);
                throw;
            }
        }
    }
}
