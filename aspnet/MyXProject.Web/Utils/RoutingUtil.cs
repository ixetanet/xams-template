using System.Collections.Concurrent;

namespace MyXProject.Web.Utils;


/// <summary>
/// This routing util allows static NextJs site routing 
/// </summary>
public class RoutingUtil
{
    private static readonly ConcurrentDictionary<string, bool> FileExistsCache = new();
    public static async Task SetupRoutes(HttpContext context, Func<Task> next, WebApplication app)
    {
        var path = context.Request.Path.Value;
    
        // Skip processing for actual files (js, css, images, etc.)
        if (path == null || path.Contains('.') && !path.EndsWith(".html"))
        {
            await next();
            return;
        }
    
        // Check if the requested HTML file exists (without .html in the URL)
        var htmlFilePath = $"{path}.html".TrimStart('/');
        var physicalPath = Path.Combine(app.Environment.WebRootPath, htmlFilePath);
    
        // Use cache to check if file exists
        bool fileExists = FileExistsCache.GetOrAdd(physicalPath, File.Exists);
        
        if (fileExists)
        {
            // Rewrite to the actual .html file
            context.Request.Path = $"/{htmlFilePath}";
            await next();
            return;
        }
    
        // If the root path is requested, serve index.html
        if (path == "/" || string.IsNullOrEmpty(path))
        {
            context.Request.Path = "/index.html";
            await next();
            return;
        }
        
        await next();
    }
}