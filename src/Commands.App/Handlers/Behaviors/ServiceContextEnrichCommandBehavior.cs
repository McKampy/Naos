﻿//namespace Naos.Commands.Domain
//{
//    using System.Threading.Tasks;
//    using EnsureThat;

//    public class ServiceContextEnrichCommandBehavior : ICommandBehavior
//    {
//        private readonly ServiceContext serviceContext;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="ServiceContextEnrichCommandBehavior"/> class.
//        /// </summary>
//        /// <param name="serviceContext">The service context.</param>
//        public ServiceContextEnrichCommandBehavior(ServiceContext serviceContext) // TODO: accessor
//        {
//            EnsureArg.IsNotNull(serviceContext);

//            this.serviceContext = serviceContext;
//        }

//        /// <summary>
//        /// Executes this behavior for the specified command, adds service context information to command
//        /// </summary>
//        /// <typeparam name="TResponse">The type of the response.</typeparam>
//        /// <param name="request">The command.</param>
//        /// <returns></returns>
//        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
//        {
//            EnsureArg.IsNotNull(request);

//            //command.Update(this.serviceContext.RequestId, this.serviceContext.CorrelationId);

//            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
//        }
//    }
//}
