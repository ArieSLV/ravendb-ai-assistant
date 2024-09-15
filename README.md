# RavenDB AI Assistant

RavenDB AI Assistant is a simple HTTP server that acts as a middleware between a web client and Azure Promptflow. It's built using ASP.NET Core and designed to be easily deployable and configurable.

## Features

- Secure handling of sensitive information through appsettings.json and environment variables
- CORS support for cross-origin requests
- Logging for better debugging and monitoring
- Error handling and appropriate status codes
- Easy configuration for different environments

## Prerequisites

- .NET 6.0 SDK or later
- An Azure account with Promptflow set up

## Setup

1. Clone the repository:
   ```
   git clone https://github.com/yourusername/ravendb-ai-assistant.git
   cd ravendb-ai-assistant
   ```

2. Restore the NuGet packages:
   ```
   dotnet restore
   ```

3. Configure your application by editing the `appsettings.json` file in the root directory of your project:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "AzurePromptflowEndpoint": "https://your-azure-endpoint.com",
     "AllowedOrigins": "*",
     "Port": 5000
   }
   ```
   Replace `https://your-azure-endpoint.com` with your actual Azure Promptflow endpoint.

## Running the server

1. Make sure you're in the project directory.

2. Run the following command:
   ```
   dotnet run
   ```

3. The server will start and listen on the port specified in your appsettings.json (default is 5000).

## Configuration

You can override the settings in `appsettings.json` using environment variables. For example:

```
set AzurePromptflowEndpoint=https://your-new-endpoint.com
set Port=5001
```

## Troubleshooting

If you encounter an error saying "Azure Promptflow endpoint is not configured", make sure you've correctly set the `AzurePromptflowEndpoint` in your appsettings.json or as an environment variable.

## Usage

Once the server is running, you can send POST requests to the `/chat` endpoint with a JSON body in the following format:

```json
{
  "Message": "Your message here"
}
```

The server will forward this message to Azure Promptflow and return the response.

## Development

To run the server in development mode with hot reloading:

```
dotnet watch run
```