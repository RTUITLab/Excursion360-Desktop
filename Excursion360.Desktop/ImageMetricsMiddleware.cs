namespace Excursion360.Desktop;

public class ImageMetricsMiddleware(RequestDelegate next, ILogger<ImageMetricsMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var pathString = context.Request.Path.ToString();
        if (pathString.Contains("state_") && pathString.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Hey! {Path}", pathString);
        }
        await next(context);
    }
}
