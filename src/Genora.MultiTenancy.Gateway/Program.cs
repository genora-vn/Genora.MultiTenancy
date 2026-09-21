using Genora.MultiTenancy.Gateway;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTenantGateway(builder.Configuration);
var app = builder.Build();
app.UseTenantGateway();
app.Run();
