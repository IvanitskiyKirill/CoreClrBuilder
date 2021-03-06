﻿using System;

namespace CoreClrBuilder.Commands
{
    class ActionCommand : ICommand
    {
        Action action;
        public string Name { get; private set; }
        public ActionCommand(string name, Action action)
        {
            this.Name = name;
            this.action = action;
        }
        public void Execute()
        {
            action();
        }
    }
}
