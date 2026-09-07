using LaborRisk.Domain;
using LaborRisk.LegalEngine;
using LaborRisk.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký HttpClient để gọi Ollama
builder.Services.AddHttpClient();

// 2. Đăng ký các dịch vụ nghiệp vụ
builder.Services.AddScoped<DocumentReaderService>();
builder.Services.AddScoped<ContractParserAgent>();
builder.Services.AddScoped<LegalRuleEngine>();
builder.Services.AddScoped<SimulationService>();

// 3. Đăng ký Blazor Server Interactivity
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// 4. Bắt buộc kích hoạt Render Mode cho App
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();