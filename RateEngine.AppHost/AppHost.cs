var builder = DistributedApplication.CreateBuilder(args);

// 1. Define a SQL Server Container
// This will pull the official SQL Server Docker image and run it.
var sql = builder.AddSqlServer("sql")
                 .WithDataVolume()
                 .AddDatabase("rateenginedb");

// 2. Create RabbitMQ (New!)
var rabbit = builder.AddRabbitMQ("messaging")
                    .WithManagementPlugin(); // Adds a UI to see queues

var redis = builder.AddRedis("cache");

// 2. Pass the reference to the API
// The API project now knows where "rateenginedb" is located.
builder.AddProject<Projects.RateEngine_ApiService>("apiservice")
       .WithReference(sql)
       .WithReference(rabbit)
       .WithReference(redis);

builder.AddProject<Projects.RateEngine_Worker>("worker")
       .WithReference(sql)     // Needs DB to save data
       .WithReference(rabbit); // Needs Queue to read data

builder.Build().Run();
//var apiService = builder.AddProject<Projects.RateEngine_ApiService>("apiservice")
//    .WithHttpHealthCheck("/health");

//builder.AddProject<Projects.RateEngine_Web>("webfrontend")
//    .WithExternalHttpEndpoints()
//    .WithHttpHealthCheck("/health")
//    .WithReference(apiService)
//    .WaitFor(apiService);

builder.Build().Run();
