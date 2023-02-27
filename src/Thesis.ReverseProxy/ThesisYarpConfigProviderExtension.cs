using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Thesis.ReverseProxy;

public static class ThesisYarpConfigProviderExtension
{
    private static UriBuilder _uriBuilder = new(Uri.UriSchemeHttps);

    public static IReverseProxyBuilder UseThesisServices(this IReverseProxyBuilder builder, string publicUrl)
    {
        builder.Services.AddSingleton<IProxyConfigProvider, ThesisYarpConfigProvider>();
            
        return builder.AddTransforms(context =>
        {
            context.AddResponseTransform(async response =>
                await Task.Run(() =>
                {
                    if (response is null)
                        return;

                    if (response.ProxyResponse.StatusCode == System.Net.HttpStatusCode.Created &&
                        response.ProxyResponse.Headers.Location != null)
                    {
                        var publicLocation = new Uri($"{publicUrl}/api{response.ProxyResponse.Headers.Location.LocalPath}");
                        response.HttpContext.Response.Headers.Remove("Location");
                        response.HttpContext.Response.Headers.Add("Location", publicLocation.ToString());
                    }
                }));
        });
    }
}