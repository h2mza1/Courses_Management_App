using Corses_App.Models;
using Corses_App.Repostory;
using Courses_App.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Corses_App.Data;
using Corses_App.Data.Repostory;
using Corses_App.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("connectionStr")
    ?? throw new InvalidOperationException("Connection string 'connectionStr' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddScoped<IStudentRepostory, StudentRepostory>();
builder.Services.AddScoped<ICourseRepostory, CourseRepostory>();
builder.Services.AddScoped<IInstructorRepostory, InstructorRepostory>();
builder.Services.AddScoped<IVideoReostory, VideoRepostory>();
builder.Services.AddScoped<IEnreollmentRepostory, EnreollmentRepostory>();
builder.Services.AddScoped<ICategeoryRepostory, CategeoryRepostory>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<User,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();
builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(Program));


var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var s = scope.ServiceProvider;
    await Seeder.SeedRolesAsync(s);
    await Seeder.SeedFoldersAsync(s);
    await Seeder.SeedAdminAsync(s);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
