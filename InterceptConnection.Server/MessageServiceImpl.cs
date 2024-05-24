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

        public override async Task<ReturnMessageResponse> MessageCommunicator(SendMessageRequest request, ServerCallContext context)
        {
            Console.WriteLine("The server received the request : ");
            Console.WriteLine(request.ToString());

            string result = String.Format("hello {0}", request.TheMessage);

            if (TheUltimateEventManager.Enabled)
            {
                TheUltimateEventManager.messagesIn.Enqueue(result);
                bool hasMessageArrived = false;
                string modifiedResult = null;
                //while (!hasMessageArrived)
                //    hasMessageArrived = TheUltimateEventManager.messagesOut.TryDequeue(out modifiedResult);
            }

            var resultobjectresponse = new ReturnMessageResponse();
            resultobjectresponse.TheReturn = result;
            return resultobjectresponse;

        }

        public override async Task MessageInterceptor(StartInterptingRequest request, IServerStreamWriter<MessageInterceptedResponse> responseStream, ServerCallContext context)
        {
            TheUltimateEventManager.Enabled = true;

            while (TheUltimateEventManager.Enabled)
            {
                string resultToBeModified = null;
                var isNewMessage = TheUltimateEventManager.messagesOut.TryDequeue(out resultToBeModified);
                if (isNewMessage)
                {
                    await responseStream.WriteAsync(new MessageInterceptedResponse() { TheInterceptedMessage = resultToBeModified });
                }
            }

        }

    }
}
