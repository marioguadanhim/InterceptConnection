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
            Console.WriteLine("The server received the request : ");
            Console.WriteLine(request.TheMessage);

            string result = request.TheMessage;

            if (TheUltimateEventManager.Enabled)
            {
                TheUltimateEventManager.messagesIn.Enqueue(result);
                bool hasMessageArrived = false;
                while (!hasMessageArrived)
                {
                    hasMessageArrived = TheUltimateEventManager.messagesOut.TryDequeue(out result!);
                    await Task.Delay(500);
                }
            }

            var resultobjectresponse = new ReturnMessageResponse();
            resultobjectresponse.TheReturn = result;
            return resultobjectresponse;
        }

        public override async Task MessageInterceptor(StartInterptingRequest request, IServerStreamWriter<MessageInterceptedResponse> responseStream, ServerCallContext context)
        {
            while (TheUltimateEventManager.Enabled)
            {
                string resultToBeModified = null;
                var isNewMessage = TheUltimateEventManager.messagesIn.TryDequeue(out resultToBeModified);
                if (isNewMessage)
                {
                    await responseStream.WriteAsync(new MessageInterceptedResponse() { TheInterceptedMessage = resultToBeModified });
                }

                await Task.Delay(2000);
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
