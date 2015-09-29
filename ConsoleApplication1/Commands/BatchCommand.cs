using System.Collections.Generic;

namespace CoreClrBuilder.Commands
{
    class BatchCommand : ICommand
    {
        public bool IsBatchOfIndependedCommands { get; set; }
        List<ICommand> commands;

        public IList<ICommand> Commands { get { return commands; } }

        public BatchCommand(params ICommand[] commands) : this(false, commands)
        {
        }
        public BatchCommand(bool isIndependedCommands, params ICommand[] commands)
        {
            IsBatchOfIndependedCommands = isIndependedCommands;
            this.commands = new List<ICommand>(commands);
        }

        public void Add(ICommand command)
        {
            commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in Commands)
            {
                command.Execute();
            }
        }
    }
}
