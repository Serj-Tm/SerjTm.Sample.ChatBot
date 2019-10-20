using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        Dictionary<int, string> users = new Dictionary<int, string>();

        public void Add(int id, string name)
        {
            lock(users)
            {
                users[id] = name;
            }
        }
        public void Remove(int id)
        {
            lock (users)
            {
                users.Remove(id);
            }
        }
        public string[] All()
        {
            lock (users)
                return users.Values.ToArray();
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
