﻿namespace Naos.Core.Commands.App.Web
{
    using System;

    public class CommandRequestWrapper
    {
        public string Id { get; set; }

        public string CorrelationId { get; set; }

        public string ParentSpanId { get; set; }

        public CommandRequestStatus Status { get; set; }

        public string StatusDescription { get; set; } // Finished datetime/ Failure message

        public DateTimeOffset Created { get; set; }

        public DateTimeOffset? Started { get; set; }

        public DateTimeOffset? Completed { get; set; }

        public Command Command { get; set; }

        public object Response { get; set; }

        //public CommandWrapper SetCommand<TCommand, TResponse>(TCommand command)
        //    where TCommand : Command<TResponse>
        //{
        //    this.Command = command;
        //    return this;
        //}

        public CommandRequestWrapper SetCommand(Command command)
        {
            if (command != null)
            {
                this.Id = command.Id;
                this.Created = DateTime.UtcNow;
                this.CorrelationId = command.CorrelationId;
                //this.ParentSpanId = command.s
                this.Command = command;
            }

            return this;
        }

        //public CommandWrapper SetCommand<TCommand>(TCommand command)
        //    where TCommand : Command<object>
        //{
        //    this.Command = command;
        //    return this;
        //}
    }
}
