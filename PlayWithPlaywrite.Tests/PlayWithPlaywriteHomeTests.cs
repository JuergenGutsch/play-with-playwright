using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace PlayWithPlaywrite.Tests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class PlayWithPlaywriteHomeTests : SelfHostedPageTest<Program>
{
    public PlayWithPlaywriteHomeTests() :
        base(services =>
        {
			// configure needed services, like mocked db access, fake mail service, etc.
        })
    { }

    [Test]
    public async Task TestWithWebApplicationFactory()
    {
        var serverAddress = GetServerAddress();

        await Page.GotoAsync(serverAddress);
        await Expect(Page).ToHaveTitleAsync(new Regex("Home Page - PlayWithPlaywrite"));

        Assert.Pass();
    }
}

public abstract class SelfHostedPageTest<TEntryPoint> : PageTest where TEntryPoint : class
{
    private readonly CustomWebApplicationFactory<TEntryPoint> _webApplicationFactory;

    public SelfHostedPageTest(Action<IServiceCollection> configureServices)
    {
        _webApplicationFactory = new CustomWebApplicationFactory<TEntryPoint>(configureServices);
        _webApplicationFactory.CreateClient();
    }

    protected string GetServerAddress() => _webApplicationFactory.ServerAddress;
}

internal class CustomWebApplicationFactory<TEntryPoint> :
   WebApplicationFactory<TEntryPoint> where TEntryPoint : class
{
    private readonly Action<IServiceCollection> _configureServices;
    private readonly string _environment;

    public CustomWebApplicationFactory(
        Action<IServiceCollection> configureServices,
        string environment = "Development")
    {
        _configureServices = configureServices;
        _environment = environment;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_environment);
        base.ConfigureWebHost(builder);

        // Add mock/test services to the builder here
        if (_configureServices is not null)
        {
            builder.ConfigureServices(_configureServices);
        }
    }

    private IHost? _host;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Create the host for TestServer now before we  
        // modify the builder to use Kestrel instead.    
        var testHost = builder.Build();

        // Modify the host builder to use Kestrel instead  
        // of TestServer so we can listen on a real address.
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        // Create and start the Kestrel server before the test server,  
        // otherwise due to the way the deferred host builder works    
        // for minimal hosting, the server will not get "initialized    
        // enough" for the address it is listening on to be available.    
        // See https://github.com/dotnet/aspnetcore/issues/33846.
        _host = builder.Build();
        _host.Start();

        // Extract the selected dynamic port out of the Kestrel server  
        // and assign it onto the client options for convenience so it    
        // "just works" as otherwise it'll be the default http://localhost    
        // URL, which won't route to the Kestrel-hosted HTTP server.
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        // Return the host that uses TestServer, rather than the real one.  
        // Otherwise the internals will complain about the host's server    
        // not being an instance of the concrete type TestServer.    
        // See https://github.com/dotnet/aspnetcore/pull/34702.
        testHost.Start();
        return testHost;
    }

    public string ServerAddress
    {
        get
        {
            EnsureServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    private void EnsureServer()
    {
        if (_host is null)
        {
            // This forces WebApplicationFactory to bootstrap the server  
            using var _ = CreateDefaultClient();
        }
    }
}