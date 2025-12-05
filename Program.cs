using AspnetCoreMvcFull.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<GreenboDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GreenboConnectionDb")));

var app = builder.Build();

// Check DB connection
using (var scope = app.Services.CreateScope())
{
  var db = scope.ServiceProvider.GetRequiredService<GreenboDbContext>();
  try
  {
    db.Database.CanConnect();
    Console.WriteLine("Database connection OK");
  }
  catch (Exception ex)
  {
    Console.WriteLine("Database connection failed: " + ex);
  }
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboards}/{action=Index}/{id?}");

try
{
  app.Run();
}
catch (Exception ex)
{
  Console.WriteLine("App stopped: " + ex);
}
