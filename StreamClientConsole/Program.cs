using Microsoft.Extensions.Configuration;
using BetfairStreamClient.Auth;


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
var sessionProvider = new AppKeyAndSessionProvider(AppKeyAndSessionProvider.SSO_HOST_AU,appKey,user, password,certFile);
