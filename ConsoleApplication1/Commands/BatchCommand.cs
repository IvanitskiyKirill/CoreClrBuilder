using System.Collections.Generic;

namespace CoreClrBuilder.Commands
{
    class BatchCommand : ICommand
    {

        List<ICommand> commands;

        public IEnumerable<ICommand> Commands { get { return commands; } }

        public BatchCommand(params ICommand[] commands)
        {
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
