using System.Collections.Generic;

namespace CoreClrBuilder.Commands {
    public interface IBatchCommand : ICommand {
        IList<ICommand> Commands { get; }
        bool IsBatchOfIndependedCommands { get; set; }

        void Add(ICommand command);
    }
}