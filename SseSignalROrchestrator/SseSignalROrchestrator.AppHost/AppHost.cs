var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.SseApi>("sseapi");

builder.AddProject<Projects.SignalRApi>("signalrapi");

builder.Build().Run();
