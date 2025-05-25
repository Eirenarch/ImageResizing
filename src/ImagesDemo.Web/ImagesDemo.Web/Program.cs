namespace ImagesDemo.Web;

using ImagesDemo.Business;
using ImagesDemo.Business.Account;
using ImagesDemo.Business.Images;
using ImagesDemo.Web.Components;
using ImagesDemo.Web.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContextPool<ImagesDemoContext>(optionsBuilder => optionsBuilder
        .UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            options =>
            {
                options.EnableRetryOnFailure();
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            })
        #if DEBUG
        .EnableSensitiveDataLogging()
        #endif
        );

        builder.Services.AddScoped<DbContext>(context => context.GetService<ImagesDemoContext>()!);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents()
            .AddAuthenticationStateSerialization();

        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
        builder.Services.AddScoped<IImageBlobStorageClient, FileSystemBlobStorageClient>();
        builder.Services.AddScoped<IImageService, ImageService>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddIdentityCookies();

        
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddIdentityCore<User>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ImagesDemoContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.Map("/Images", config =>
        {
            config.UseImageHandlerMiddleware();
        });

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>();

        using (IServiceScope serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            ImagesDemoContext context = serviceScope.ServiceProvider.GetRequiredService<ImagesDemoContext>();
            context.Database.EnsureCreated();
        }

        app.Run();
    }
}
