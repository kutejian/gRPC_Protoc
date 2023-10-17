using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using WebApplication1.Protos;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();




Console.WriteLine("�ͻ���");

//���ﴴ����һ�� gRPC ͨ����������ָ���� gRPC �����ַ�������ӣ�
//��������һ�� gRPC �ͻ��ˣ��Ա������������ͨ�š�
var channel = GrpcChannel.ForAddress("https://localhost:7264");
var client = new UserService.UserServiceClient(channel);
var user = new User() { Id = 6, Age = 18, Name = "С��", LastName = "����", Salary = 10 };
var AccessToken = "";



//�����ǵ�½ Ȼ�󷵻�token ������ʹ��
//��δ�������� GetAll ���񷽷�����ʹ����Ӧ���������������û���Ϣ����ӡ������
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


//��δ��봴����һ������Ԫ���ݣ�metadata���������а�����һ����ֵ�ԣ����ڴ��ݶ������Ϣ��
var md = new Metadata 
{
    { "username" ,"kute"},
    { "Authorization", "Bearer " + AccessToken } // ����� yourAccessToken ����ķ�������
};


//����ͨ�� gRPC �ͻ��˵����˷���� GetByUser ������
//������һ��������Ϣ��GetByUserRequest��������֮ǰ���õ�����Ԫ����һ�����͡�Ȼ�󣬽��������Ӧ��Ϣ�洢�� response �С�   
var response = client.GetByUser(new GetByUserRequest
{
    Id = 2
}, md);
Console.WriteLine(response);





//��������˷���� AddUser ����������������Ԫ���ݣ������������Ӧ��Ϣ�洢�� response3 �С�
//��Ҫע����ǣ����ﲢû�д����û���Ϣ��������ʹ���������������û����ݡ�
var response3 = client.AddUser(md);

//��δ����ȡ�� AddUser ��������������stramRequest����
//ʹ�� WriteAsync �������û���Ϣ��ӵ��������У�
//Ȼ��ͨ�� CompleteAsync �����������д�롣 �������Ӧ�� ��������Ӧ���������
var stramRequest = response3.RequestStream;
await stramRequest.WriteAsync(new AddUserRequest() { User= user });
await stramRequest.CompleteAsync();

//�����ȡ�� AddUser ��������Ӧ����stramResponse����
//ʹ�� MoveNext ��������������Ӧ��Ϣ������ӡÿ����Ϣ�� IsOk ���ԡ� ���ǽ��շ������Ӧ����������
var stramResponse = response3.ResponseStream;
while (await stramResponse.MoveNext())
{
    Console.WriteLine(stramResponse.Current.IsOk);
}



//��δ�������� GetAll ���񷽷�����ʹ����Ӧ���������������û���Ϣ����ӡ������
var response2 = client.GetAll(new GetAllRequest(),md);
var stram2 = response2.ResponseStream;
while (await stram2.MoveNext())
{
    Console.WriteLine(stram2.Current.User);
}














Console.ReadKey();
app.Run();
