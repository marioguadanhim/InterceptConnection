using Greet;
using Grpc.Core;
using InterceptConnection.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Greet.MessageService;

namespace InterceptConnection.Server
{
    public class MessageServiceImpl : MessageServiceBase
    {

        public override async Task<ConnectionManagerResponse> ConnectionManager(ConnectionManagerRequest request, ServerCallContext context)
        {
            TheUltimateEventManager.Enabled = request.Enable;

            var connectionManagerResponse = new ConnectionManagerResponse();
            await Task.Run(() => connectionManagerResponse.Enable = request.Enable);

            return connectionManagerResponse;
        }

        public override async Task<ReturnMessageResponse> MessageCommunicator(SendMessageRequest request, ServerCallContext context)
        {       
            string result = request.TheMessage;

            if (TheUltimateEventManager.Enabled)
            {
                Console.WriteLine($"Enqueue: {request.TheMessage}");
                TheUltimateEventManager.messagesIn.Enqueue(result);
                bool hasMessageArrived = false;
                while (!hasMessageArrived)
                {
                    hasMessageArrived = TheUltimateEventManager.messagesOut.TryDequeue(out result!);
                    await Task.Delay(500);
                }
            }
            else
            {
                Console.WriteLine($"QA Tool not connected, received: {request.TheMessage}");
            }

            var resultobjectresponse = new ReturnMessageResponse();
            resultobjectresponse.TheReturn = result;
            return resultobjectresponse;
        }

        public override async Task MessageInterceptor(StartInterptingRequest request, IServerStreamWriter<MessageInterceptedResponse> responseStream, ServerCallContext context)
        {
            string? previousResult = null;
            while (TheUltimateEventManager.Enabled)
            {
                await Task.Delay(500);
                string? resultToBeModified = null;
                TheUltimateEventManager.messagesIn.TryDequeue(out resultToBeModified);
                if (!string.IsNullOrEmpty(resultToBeModified))
                    previousResult = resultToBeModified;

                if (!string.IsNullOrEmpty(previousResult))
                {
                    Console.WriteLine($"Dequeue: {previousResult}");
                    await responseStream.WriteAsync(new MessageInterceptedResponse() { TheInterceptedMessage = previousResult });
                }
            }
        }

        public override async Task<InjectorMessageResponse> MessageInjector(InjectorMessageRequest request, ServerCallContext context)
        {
            bool success = true;
            try
            {
                TheUltimateEventManager.messagesOut.Enqueue(request.InjectedMessage);
            }
            catch (Exception ex)
            {
                success = false;
            }

            InjectorMessageResponse injectorMessageResponse = new InjectorMessageResponse();
            await Task.Run(() => injectorMessageResponse.Confirmation = success);

            return injectorMessageResponse;
        }

    }
}
