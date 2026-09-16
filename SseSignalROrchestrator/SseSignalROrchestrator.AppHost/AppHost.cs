var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlazorDemoThirdPart>("blazordemothirdpart");
var sseApi = builder.AddProject<Projects.SseApi>("sseapi");
var signalRApi = builder.AddProject<Projects.SignalRApi>("signalrapi");

builder.AddProject<Projects.EventProducerApi>("eventproducerapi")
    .WithReference(sseApi)
    .WithReference(signalRApi);

builder.Build().Run();
