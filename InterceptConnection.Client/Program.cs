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
        try
        {

            Console.Clear();

            Console.WriteLine("Client");

            Console.WriteLine("Write a message:");
            var message = Console.ReadLine();

            var request = new SendMessageRequest() { TheMessage = message };

            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            var response = client.MessageCommunicator(request, deadline: deadline);


            Console.WriteLine("New Message Received:");
            Console.WriteLine(response.TheReturn);

            Console.ReadKey();
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.ReadKey();
        }
    }
}
finally
{
    channel.ShutdownAsync().Wait();
}