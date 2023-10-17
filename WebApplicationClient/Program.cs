using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using WebApplication1.Protos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();




Console.WriteLine("客户端");

//这里创建了一个 gRPC 通道，用于与指定的 gRPC 服务地址建立连接，
//并创建了一个 gRPC 客户端，以便后续与服务进行通信。
var channel = GrpcChannel.ForAddress("https://localhost:7264");
var client = new UserService.UserServiceClient(channel);
var user = new User() { Id = 6, Age = 18, Name = "小妙", LastName = "妙妙", Salary = 10 };
var AccessToken = "";



//这里是登陆 然后返回token 给后面使用
//这段代码调用了 GetAll 服务方法，并使用响应流迭代接收所有用户信息并打印出来。
var response4 = client.LoginUser();

var stramRequest2 = response4.RequestStream;
var user1 = new User() { Id = 6, Age = 18, Name = "yaokang", LastName = "kang", Salary = 10 };
await stramRequest2.WriteAsync(new AddUserRequest() { User = user1 });
await stramRequest2.CompleteAsync();


var stramResponse2 = response4.ResponseStream;
while (await stramResponse2.MoveNext())
{
    AccessToken = stramResponse2.Current.Token;
    Console.WriteLine(AccessToken);
}


//这段代码创建了一个请求元数据（metadata）对象，其中包含了一个键值对，用于传递额外的信息。
var md = new Metadata 
{
    { "username" ,"kute"},
    { "Authorization", "Bearer " + AccessToken } // 这里的 yourAccessToken 是你的访问令牌
};


//这里通过 gRPC 客户端调用了服务的 GetByUser 方法，
//传递了一个请求消息（GetByUserRequest），并将之前设置的请求元数据一并发送。然后，将服务的响应消息存储在 response 中。   
var response = client.GetByUser(new GetByUserRequest
{
    Id = 2
}, md);
Console.WriteLine(response);





//这里调用了服务的 AddUser 方法，传递了请求元数据，并将服务的响应消息存储在 response3 中。
//需要注意的是，这里并没有传递用户信息，后续将使用请求流来传输用户数据。
var response3 = client.AddUser(md);

//这段代码获取了 AddUser 方法的请求流（stramRequest），
//使用 WriteAsync 方法将用户信息添加到请求流中，
//然后通过 CompleteAsync 完成请求流的写入。 这就是响应流 把数据响应传给服务端
var stramRequest = response3.RequestStream;
await stramRequest.WriteAsync(new AddUserRequest() { User= user });
await stramRequest.CompleteAsync();

//这里获取了 AddUser 方法的响应流（stramResponse），
//使用 MoveNext 方法迭代接收响应消息，并打印每条消息的 IsOk 属性。 就是接收服务端响应回来的数据
var stramResponse = response3.ResponseStream;
while (await stramResponse.MoveNext())
{
    Console.WriteLine(stramResponse.Current.IsOk);
}



//这段代码调用了 GetAll 服务方法，并使用响应流迭代接收所有用户信息并打印出来。
var response2 = client.GetAll(new GetAllRequest(),md);
var stram2 = response2.ResponseStream;
while (await stram2.MoveNext())
{
    Console.WriteLine(stram2.Current.User);
}














Console.ReadKey();
app.Run();
