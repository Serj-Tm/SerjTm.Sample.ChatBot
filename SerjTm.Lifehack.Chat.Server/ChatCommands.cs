using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace SerjTm.Lifehack.Chat
{
    public interface IChatCommand
    {
        string Name { get; }
        (string answer, bool isEnd) Process(string command);
    }
    public class SimpleCommand: IChatCommand
    {
        public SimpleCommand(string name, string answer)
        {
            this.Name = name;
            this.Answer = answer;
        }
        public string Name { get; private set; }
        public readonly string Answer;

        public (string answer, bool isEnd) Process(string command)
        {
            return (this.Answer, false);
        }
    }
    public class ByeCommand : IChatCommand
    {
        public string Name { get; private set; } = "Пока";

        public (string answer, bool isEnd) Process(string command)
        {
            return ("Пока", true);
        }
    }
    public class UsersCommand : IChatCommand
    {
        public UsersCommand(Users users)
        {
            this.Users = users;
        }
        public readonly Users Users;

        public string Name { get; private set; } = "Кто здесь?";

        public (string answer, bool isEnd) Process(string command)
        {
            return (string.Join(System.Environment.NewLine, Users.All()), false);
        }
    }
    public class Users
    {
        ImmutableDictionary<int, string> users = ImmutableDictionary<int, string>.Empty;

        public void Add(int id, string name)
        {
            for (; ; )
            {
                var currentUsers = users;
                var newUsers = currentUsers.Add(id, name);
                if (Interlocked.Exchange(ref users, newUsers) == currentUsers)
                    break;
            }
        }
        public void Remove(int id)
        {
            for (; ; )
            {
                var currentUsers = users;
                var newUsers = currentUsers.Remove(id);
                if (Interlocked.Exchange(ref users, newUsers) == currentUsers)
                    break;
            }
        }
        public IEnumerable<string> All()
        {
           return users.Values;
        }
    }

    public class CommandProcessor
    {
        public CommandProcessor(IChatCommand[] commands)
        {
            this.Commands = commands;
            this.CommandIndex = this.Commands.ToDictionary(command => command.Name, command => command);
        }
        public readonly IChatCommand[] Commands;
        readonly Dictionary<string, IChatCommand> CommandIndex;

        public (string answer, bool isEnd) Process(string command)
        {
            if (CommandIndex.TryGetValue(command, out var chatCommand))
                return chatCommand.Process(command);
            return (null, false);
        }
    }
}
