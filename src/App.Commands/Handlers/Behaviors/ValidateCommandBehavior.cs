﻿namespace Naos.Core.App.Commands
{
    using System.Threading.Tasks;
    using EnsureThat;
    using FluentValidation;

    public class ValidateCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> command)
        {
            EnsureArg.IsNotNull(command);

            var result = command.Validate();
            if (!result.IsValid)
            {
                // instead of cancel, throw an exception
                // TODO: log validation errors
                throw new ValidationException($"{command.GetType().Name} has validation errors", result.Errors);
            }

            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
        }
    }
}
