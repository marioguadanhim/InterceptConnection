using Greet;
using Grpc.Core;

Console.WriteLine("Client");

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
        Console.WriteLine("Write a message:");
        var message = Console.ReadLine();

        var request = new SendMessageRequest() { TheMessage = message };

        var response = client.MessageCommunicator(request);

        Console.WriteLine(response.TheReturn);

        //while (await response.ResponseStream.MoveNext())
        //{
        //    Console.WriteLine(response.ResponseStream.Current.Result);
        //    await Task.Delay(200);
        //}
    }
}
finally
{

    channel.ShutdownAsync().Wait();
    
}