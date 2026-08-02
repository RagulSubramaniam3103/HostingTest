/*
using EWOMS_ClassLibrary.DataControlled;
using EWOMS_ClassLibrary.DataIntegration;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        var connectionString = hostContext.Configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options => 
            options.UseSqlServer(connectionString));
    }).Build();

using (var scope = builder.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "vishnupriya@gmail.com");
    if (user != null)
    {
        Console.WriteLine($"USER_FOUND: Id={user.Id}, FullName={user.FullName}, UserName={user.UserName}, IsPrivate={user.IsPrivate}");
    }
    else
    {
        Console.WriteLine("USER_NOT_FOUND");
    }
}
*/
