using Microsoft.EntityFrameworkCore;
using EduConnect.Components;
using EduConnect.Data;
using EduConnect.Interfaces;
using EduConnect.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── EF Core DbContext ────────────────────────────────────────
builder.Services.AddDbContext<EduConnectDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Custom services
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IGradeService, GradeService>();
builder.Services.AddScoped<FacultyService>();

var app = builder.Build();

// ── Ensure database is created & seeded ──────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EduConnectDbContext>();
    db.Database.Migrate();

    // Run EF Core LINQ queries if --run-queries flag is passed
    if (args.Contains("--run-queries"))
    {
        EfCoreQueryRunner.RunAllQueries(db);
        return; // Exit after printing results — don't start the web server
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
