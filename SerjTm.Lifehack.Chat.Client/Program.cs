using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SerjTm.Lifehack.Chat.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync("localhost", 8347);
                var stream = client.GetStream();
                using (var reader = new StreamReader(stream))
                using (var writer = new StreamWriter(stream) { AutoFlush = true })
                {
                    var aliveTask = Task.Run(() => CheckConnection(client));
                    for (; ; )
                    {
                        if (!ReadAll(reader))
                            break;
                        await Task.WhenAny(Task.Run(() => ProcessUserCommand(writer)), aliveTask);
                    }
                }
            }
        }
        static void ProcessUserCommand(StreamWriter writer)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            writer.WriteLine(line);
        }

        static bool ReadAll(StreamReader reader)
        {
            for (; ; )
            {
                var line = reader.ReadLine();
                if (line == null)
                    return false;
                if (line == "")
                    break;
                Console.WriteLine(line);
            }
            return true;
        }

        static async Task CheckConnection(TcpClient client)
        {
            for (; ; )
            {
                if (GetState(client) != TcpState.Established)
                    throw new Exception("Прервано соединение с сервером");
                await Task.Delay(200);
            }
        }
        public static TcpState GetState(TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint)
                 && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint)
              );

            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
