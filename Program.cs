using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApiContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);


var app = builder.Build();


app.UseAuthorization();
app.MapControllers();

app.Run();