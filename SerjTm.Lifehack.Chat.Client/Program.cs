using System;
using System.IO;
using System.Net.Sockets;
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
                    for (;; )
                    {
                        if (!ReadAll(reader))
                            break;
                        Console.Write("> ");
                        var line = Console.ReadLine();
                        writer.WriteLine(line);
                    }
                }
            }
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
    }
}
