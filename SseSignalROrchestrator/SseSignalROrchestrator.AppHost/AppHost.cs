var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

builder.AddProject<Projects.BlazorDemoThirdPart>("blazordemothirdpart");
var sseApi = builder.AddProject<Projects.SseApi>("sseapi")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);
var signalRApi = builder.AddProject<Projects.SignalRApi>("signalrapi")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.EventProducerApi>("eventproducerapi")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq)
    .WithReference(sseApi)
    .WithReference(signalRApi);

builder.Build().Run();
