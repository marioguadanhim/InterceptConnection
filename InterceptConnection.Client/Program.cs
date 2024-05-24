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

    while (true)
    {
        Console.Clear();

        Console.WriteLine("Client");

        Console.WriteLine("Write a message:");
        var message = Console.ReadLine();

        var request = new SendMessageRequest() { TheMessage = message };

        var response = client.MessageCommunicator(request);


        Console.WriteLine("New Message Received:");
        Console.WriteLine(response.TheReturn);

        Console.ReadKey();
    }
}
finally
{
    channel.ShutdownAsync().Wait();
}