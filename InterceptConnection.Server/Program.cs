// See https://aka.ms/new-console-template for more information
using Greet;
using Grpc.Core;
using InterceptConnection.Server;

Console.WriteLine("Server");

Server server = null;
MessageServiceImpl grpcServer = new MessageServiceImpl();


try
{

    server = new Server()
    {
        Services = { MessageService.BindService(grpcServer) },
        Ports = { new ServerPort("localhost", 50052, ServerCredentials.Insecure) }// credentials) }
    };

    server.Start();
    Console.WriteLine("The server is listening on the port : " + 50052);
    Console.ReadKey();
}
catch (IOException e)
{
    Console.WriteLine("The server failed to start : " + e.Message);
    throw;
}
finally
{
    if (server != null)
        server.ShutdownAsync().Wait();
}