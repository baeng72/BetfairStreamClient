using Microsoft.Extensions.Configuration;
using BetfairStreamClient.Auth;
using BetfairStreamClient.Betting;
using BetfairStreamClient.Stream;
using BetfairStreamClient.Logging;
using System.Threading.Channels;
using StreamClientConsole;
using System.Net.Mail;

try
{
    // 1. Build the Configuration
    IConfiguration config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables() // Allows overriding via env variables
        .Build();
    string appKey = config["settings:AppKey"]??string.Empty;
    string user = config["settings:Username"]??string.Empty;    
    string password = config["settings:Password"]??string.Empty;
    string certFile = config["settings:CertFile"]??string.Empty;
    if(string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(certFile))
    {
        Console.WriteLine("Configuration not set.");
        return;
    }
    using var cts = new CancellationTokenSource();
    var cancellationToken = cts.Token;
    Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };
    var sessionProvider = new AppKeyAndSessionProvider(AuthClient.DEFAULT_COM_AUS_BASEURL,appKey,user, password,certFile);
    var session = sessionProvider.GetOrCreateNewSession();
    if(session == null)
    {
        Console.WriteLine("Unable to get betfair session token.");
        return;

    }
    if(session.LoginStatus != "SUCCESS")
    {
        Console.WriteLine("Unable to loging.");
        return;
    }
    //list current horse racing markets in AU and NZ
    HttpClient _httpClient = new HttpClient();
    var betfairAsyncClient = new BetfairAsyncClient(_httpClient, session.AppKey, session.Token);
    string logDir = "C:/DATA/BF/BetfairStreamClient";
    Directory.CreateDirectory(logDir);
    await using var logger = new Logger();
    logger.Init(Path.Combine(logDir, $"strategy_initial-{DateTime.UtcNow.ToString("yyyy-MM-dd hh-mm-ss")}.csv"), cancellationToken);
    await using var streamDumper = new RawStreamDumper();
    streamDumper.Init(Path.Combine(logDir, $"raw_string_initial-{DateTime.UtcNow.ToString("yyyy-MM-dd hh-mm-ss")}.json"), cancellationToken);

    var streamClient = new StreamClient("stream-api.betfair.com", 443, session.AppKey, session.Token, logger, streamDumper);
    //var outboundOrderChannel = Channel.CreateUnbounded<OutboundCommand>();
    await streamClient.ConnectAndAuthenticateAsync(cancellationToken);

    SteamerService steamerService = new SteamerService(betfairAsyncClient, streamClient, logger, cts.Token, 1);
    await steamerService.Start();
    Task streamTask = Task.Run(() => streamClient.RunLoopAsync(cancellationToken));
    await Task.WhenAll(streamTask);
    

}
catch(Exception ex)
{
    Console.WriteLine(ex.Message);
}

