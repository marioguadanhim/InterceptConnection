using System;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace client
{
    class Program
    {
        public static string CurrentMessage { get; set; } = string.Empty;

        static async Task Main(string[] args)
        {
            while (true)
            {

                Channel channel = new Channel("localhost", 50052, ChannelCredentials.Insecure);

                await channel.ConnectAsync().ContinueWith((task) =>
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                        Console.WriteLine("The client connected successfully");
                    else
                        Console.WriteLine("Failed to connect");
                });

                var client = new MessageService.MessageServiceClient(channel);

                try
                {
                    string newMessage = string.Empty;
                    while (newMessage != "exit")
                    {
                        try
                        {
                            Console.Clear();

                            Console.WriteLine("Interceptor");

                            await EnableConnection(client, true);

                            var request = new StartInterptingRequest() { Enable = true };

                            await InterceptMessage(client, request);

                            Console.WriteLine("Message Intercepted: " + CurrentMessage);
                            Console.WriteLine("Modify the intercepted message:");
                            newMessage = Console.ReadLine();

                            await SendTheInterceptMessageBackToServer(client, newMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            Console.ReadKey();
                        }
                    }
                }
                finally
                {
                    await EnableConnection(client, false);
                    await channel.ShutdownAsync();
                }

                Console.WriteLine("Interceptor OFF...");
                Console.WriteLine("Press any key to start again...");
                Console.ReadKey();
            }
        }

        static async Task EnableConnection(MessageService.MessageServiceClient client, bool enable)
        {
            var connectionManagerRequest = new ConnectionManagerRequest { Enable = enable };

            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            await client.ConnectionManagerAsync(connectionManagerRequest, deadline: deadline);
        }

        static async Task InterceptMessage(MessageService.MessageServiceClient client, StartInterptingRequest request)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            using var response = client.MessageInterceptor(request, deadline: deadline);

            string message = string.Empty;
            while ((string.IsNullOrEmpty(message) || message == CurrentMessage) && await response.ResponseStream.MoveNext())
            {
                message = response.ResponseStream.Current.TheInterceptedMessage;
                Console.WriteLine(response.ResponseStream.Current.TheInterceptedMessage);
            }
            CurrentMessage = message;
        }

        static async Task SendTheInterceptMessageBackToServer(MessageService.MessageServiceClient client, string newMessage)
        {
            var injectorMessageRequest = new InjectorMessageRequest
            {
                InjectedMessage = !string.IsNullOrEmpty(newMessage) ? newMessage : CurrentMessage
            };

            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            await client.MessageInjectorAsync(injectorMessageRequest, deadline: deadline);
        }
    }
}
