using Greet;
using Grpc.Core;



Channel channel = new Channel("localhost", 50052, ChannelCredentials.Insecure);// channelCredentials);

await channel.ConnectAsync().ContinueWith((task) =>
{
    if (task.Status == TaskStatus.RanToCompletion)
        Console.WriteLine("The client connected successfully");
});

var client = new MessageService.MessageServiceClient(channel);

try
{
    string newMessage = string.Empty;
    while (newMessage != "exit")
    {
        Console.Clear();

        Console.WriteLine("Interceptor");

        await EnableConnection(client, true);

        var request = new StartInterptingRequest() { Enable = true };

        string messageIntercepted = await InterceptMessage(client, request);

        Console.WriteLine("Message Intercepted: " + messageIntercepted);
        Console.WriteLine("Modify the intercepted message:");
        newMessage = Console.ReadLine();

        await SendTheInterceptMessageBackToServer(client, messageIntercepted, newMessage);
    }

    Console.WriteLine("Interceptor OFF");
}
finally
{
    await EnableConnection(client, false);
    channel.ShutdownAsync().Wait();
}



static async Task EnableConnection(MessageService.MessageServiceClient client, bool enable)
{
    ConnectionManagerRequest connectionManagerRequest = new ConnectionManagerRequest();
    connectionManagerRequest.Enable = true;

    await client.ConnectionManagerAsync(connectionManagerRequest);
}

static async Task<string> InterceptMessage(MessageService.MessageServiceClient client, StartInterptingRequest request)
{
    var response = client.MessageInterceptor(request);
    string message = string.Empty;

    while (string.IsNullOrEmpty(message) && await response.ResponseStream.MoveNext())
    {
        message = response.ResponseStream.Current.TheInterceptedMessage;      
        Console.WriteLine(response.ResponseStream.Current.TheInterceptedMessage);
    }

    return message;
}

static async Task SendTheInterceptMessageBackToServer(MessageService.MessageServiceClient client, string messageIntercepted, string? newMessage)
{
    InjectorMessageRequest injectorMessageRequest = new InjectorMessageRequest();
    injectorMessageRequest.InjectedMessage = !string.IsNullOrEmpty(newMessage) ? newMessage : messageIntercepted;

    await client.MessageInjectorAsync(injectorMessageRequest);
}