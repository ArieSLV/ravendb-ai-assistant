namespace RavenDBAIAssistant
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder);

            var app = builder.Build();
            
            var azureEndpoint = app.Configuration[AppConstants.AzureEndpointKey];
            if (string.IsNullOrEmpty(azureEndpoint))
                throw new InvalidOperationException($"The {AppConstants.AzureEndpointKey} is not set. Please set it in your appsettings.json or as an environment variable.");

            ConfigureMiddleware(app);
            ConfigureRoutes(app);

            var port = app.Configuration.GetValue(AppConstants.PortKey, int.Parse(AppConstants.DefaultPort));
            app.Run($"http://0.0.0.0:{port}");
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(AppConstants.CorsPolicy, policyBuilder =>
                {
                    policyBuilder.WithOrigins(builder.Configuration[AppConstants.AllowedOriginsKey] ?? "*")
                                 .AllowAnyMethod()
                                 .AllowAnyHeader();
                });
            });

            builder.Services.AddHttpClient();
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
            });
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors(AppConstants.CorsPolicy);

            if (app.Environment.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Received {Method} request for {Path}", context.Request.Method, context.Request.Path);
                await next();
            });
        }

        private static void ConfigureRoutes(WebApplication app)
        {
            app.MapGet("/", () => Results.Ok("Welcome to the chat server. Use POST /chat to send messages."));

            app.MapPost("/chat", async (HttpContext context, IHttpClientFactory clientFactory, ILogger<Program> logger, IConfiguration config) =>
            {
                logger.LogInformation("Processing POST request to /chat");
                try 
                {
                    var request = await context.Request.ReadFromJsonAsync<ChatRequest>();
                    if (request?.Message == null)
                        return Results.BadRequest(new { error = "Invalid request. Message is required." });

                    var client = clientFactory.CreateClient();
                    var azureEndpoint = config[AppConstants.AzureEndpointKey];
                    if (string.IsNullOrEmpty(azureEndpoint))
                    {
                        logger.LogError("Azure Promptflow endpoint is not configured");
                        return Results.StatusCode(StatusCodes.Status500InternalServerError);
                    }

                    var response = await client.PostAsJsonAsync(azureEndpoint, request);
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadFromJsonAsync<ChatResponse>();

                    logger.LogInformation("Successfully processed chat request");
                    return Results.Ok(result);
                }
                catch (HttpRequestException e)
                {
                    logger.LogError(e, "Error communicating with Azure Promptflow");
                    return Results.StatusCode(StatusCodes.Status502BadGateway);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An unexpected error occurred");
                    return Results.StatusCode(StatusCodes.Status500InternalServerError);
                }
            });
        }
    }
    

    public static class AppConstants
    {
        public const string CorsPolicy = "CorsPolicy";
        public const string AzureEndpointKey = "AzurePromptflowEndpoint";
        public const string AllowedOriginsKey = "AllowedOrigins";
        public const string PortKey = "Port";
        public const string DefaultPort = "5000";
    }

    public record ChatRequest(string Message);
    public record ChatResponse(string Response);
}