// Install-Package Microsoft.SemanticKernel.Connectors.Google -Version 1.67.1-alpha
// Install-Package Microsoft.SemanticKernel.Yaml

// https://aistudio.google.com/

using Microsoft.SemanticKernel;
using MyProject.AI.Processes;
using MyProject.Hubs;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

//-- Razor Pages
builder.Services.AddRazorPages();

// GENERATE KEY ->> https://hoven.in/cs-lang/gemini-keys-for-sk.html

string? GEMINIAPIKEY = Environment.GetEnvironmentVariable("GEMINIAPIKEY");

Debug.Assert(!string.IsNullOrEmpty(GEMINIAPIKEY), $"Please set the environment variable '{GEMINIAPIKEY}' with your Gemini API key.");

//--- Register AI Services
// get key here: https://hoven.in/cs-lang/gemini-keys-for-sk.html
builder.Services.AddGoogleAIGeminiChatCompletion(
modelId: "gemini-2.5-flash",
apiKey: GEMINIAPIKEY
);

builder.Services.AddTransient((sp) => new Kernel(sp));

builder.Services.AddTransient((sp) => ProcessBuilderFactory.CreateFirstProcess());

builder.Services.AddSignalR();

var app = builder.Build();

// prefer app.MapStaticAssets for version 10 or later
app.UseStaticFiles();

app.MapRazorPages();

app.MapHub<ProgressHub>("/progressHub");

app.Run();
