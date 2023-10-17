using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;
using WebApplication1.Entity;
using WebApplication1.JwtToken;
using WebApplication1.Protos;

namespace WebApplication1.Services
{
    //继承 你写的proto文件写的 server方法的 base类 这样就可以重写里面的方法和使用里面的属性
    public class ProtosServices:UserService.UserServiceBase
    {
        private readonly JwtAuthManager _JwtAuthManager;
        public ProtosServices(JwtAuthManager jwtTokenConfig) {

            _JwtAuthManager = jwtTokenConfig;
            Console.WriteLine("服务端");
        }
        //重写 server方法里创建的 方法
        //它接受一个 GetByUserRequest 类型的请求消息和一个 ServerCallContext 上下文对象，并返回一个 UserResponse 类型的响应消息。
        public override Task<UserResponse> GetByUser(GetByUserRequest request, ServerCallContext context)
        {
            //这行代码获取了请求的元数据（metadata），这是包含有关请求的信息的对象。您可以使用元数据来查看请求头信息等。
            var md = context.RequestHeaders;

            //遍历请求头的键值对 一般可以用于传数据 但是不能传重要数据
            foreach(var kv in md)
            {
                Console.WriteLine($"{kv.Key}"+ $"{kv.Value}");
            }
            //根据客户端请求中的request.Id 查找用户信息。UserData.UserAll 可能是您应用程序中的用户数据集合。
            var user = UserData.UserAll.SingleOrDefault(x => x.Id == request.Id);
            //创建了一个 UserResponse 对象，并将用户信息设置到响应中，然后通过 Task.FromResult(response) 返回响应。这表示请求成功并返回了用户信息
            if (user != null)
            {
                var response = new UserResponse()
                {
                    User = user
                };
                
                return Task.FromResult(response);
            }
            return base.GetByUser(request, context);
        }
        [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        //它接受一个 GetAllRequest 类型的请求消息，一个 IServerStreamWriter<UserResponse> 类型的响应流，以及一个 ServerCallContext 上下文对象。
        public override async Task GetAll(GetAllRequest request, IServerStreamWriter<UserResponse> responseStream, ServerCallContext context)
        {
            foreach(var kv in UserData.UserAll)
            {
                //向响应流中写入了一个 UserResponse 对象，其中包含一个用户信息。这将使用户信息逐一发送给客户端。

                //这是 流的响应方式
                await responseStream.WriteAsync(new UserResponse
                {
                    User = kv
                });
            }
        }
        //它接受一个 IAsyncStreamReader<AddUserRequest> 类型的请求流、一个 IServerStreamWriter<AddUserResponse> 类型的响应流，以及一个 ServerCallContext 上下文对象。
        public override async Task AddUser(IAsyncStreamReader<AddUserRequest> requestStream, IServerStreamWriter<AddUserResponse> responseStream, ServerCallContext context)
        {
            //这行代码获取了请求的元数据（metadata），这是包含有关请求的信息的对象。您可以使用元数据来查看请求头信息等。
            var md = context.RequestHeaders;

            //遍历请求头的键值对 一般可以用于传数据 但是不能传重要数据
            foreach (var kv in md)
            {
                Console.WriteLine($"{kv.Key}" + $"{kv.Value}");
            }
            //它在不断地等待请求流中的下一个消息。每个消息都代表一个用户信息，因此它会将用户信息添加到 UserData.UserAll 中，并在控制台上打印消息以表示已添加一个用户。
            //读取请求流里的数据
            while (await requestStream.MoveNext())
            {
                UserData.UserAll.Add(requestStream.Current.User);
                Console.WriteLine("添加了一个用户"+requestStream.Current.User);
            }
            if (requestStream.MoveNext()!=null)
                //往响应流里传数据 到时候客户端可以接收
                await responseStream.WriteAsync(new AddUserResponse() { IsOk = true });
            else
                await responseStream.WriteAsync(new AddUserResponse() { IsOk = false });
        }
        //jwt登录
        public override async Task LoginUser(IAsyncStreamReader<AddUserRequest> requestStream, IServerStreamWriter<LoginUserResponse> responseStream, ServerCallContext context)
        {
            User user = null;
            while (await requestStream.MoveNext())
            {
                 user =  UserData.UserAll.SingleOrDefault(x => x.Name == requestStream.Current.User.Name);
                Console.WriteLine("登陆验证" + requestStream.Current.User);
            }
            if (user != null)
            {
                var claims = new[]{
                    new Claim(ClaimTypes.Name,user.Name)
                };

                var jwtResult = _JwtAuthManager.GenerateTokens(claims, DateTime.Now);
                await responseStream.WriteAsync(new LoginUserResponse() { Token = jwtResult.AccessToken });
            }

            else
                await responseStream.WriteAsync(new LoginUserResponse() { Token = "" });
        }
       
    }
}
