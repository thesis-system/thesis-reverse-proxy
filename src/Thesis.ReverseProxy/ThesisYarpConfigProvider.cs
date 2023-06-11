using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Thesis.ReverseProxy;

public class ThesisYarpConfigProvider : IProxyConfigProvider
{
    private readonly ThesisServices _thesisServices;

    public ThesisYarpConfigProvider(IOptions<ThesisServices> options)
    {
        _thesisServices = options.Value;
    }

    public IProxyConfig GetConfig() => new ThesisYarpConfigConfig(_thesisServices);
    
    private class ThesisYarpConfigConfig : IProxyConfig
    {
        private const string AuthCluster = "auth_cluster";
        private const string RequestsCluster = "requests_cluster";
        private const string ImagesCluster = "images_cluster";
        private const string AssetsCluster = "assets_cluster";

        private static readonly List<(string service, string[] controllers, string prefix)> ServicesMetadata = new()
        {
            (AuthCluster, new[] { "auth", "users" }, "auth"),
            (RequestsCluster, new[] { "requests" }, "requests"),
            (ImagesCluster, new[] { "images" }, "images"),
            (AssetsCluster, new[] { "assets" }, "assets"),
        };
        
        private readonly CancellationTokenSource _cts = new();

        private readonly RouteConfig[] _routes;
        private readonly ClusterConfig[] _clusters;

        public ThesisYarpConfigConfig(ThesisServices thesisServices)
        {
            ChangeToken = new CancellationChangeToken(_cts.Token);
            
            _routes = ServicesMetadata
                .SelectMany(meta => meta.controllers.Select(controllerName =>
                    (controller: $"api/{controllerName}", service: meta.service)))
                .Select(MakeRoute)
                .Union(ServicesMetadata.Select(x => MakeSwaggerRoute(x.prefix, x.service)))
                .ToArray();

            _clusters = new[]
            {
                MakeClusterConfig(AuthCluster, thesisServices.AuthUrl),
                MakeClusterConfig(RequestsCluster, thesisServices.RequestsUrl),
                MakeClusterConfig(ImagesCluster, thesisServices.ImagesUrl),
            };
        }
        
        private RouteConfig MakeSwaggerRoute(string prefix, string service) =>
            new RouteConfig
                {
                    RouteId = $"{prefix}-route",
                    ClusterId = service,
                    Match = new RouteMatch { Path = $"/swagger/{prefix}/{{**catch-all}}" },
                }
                .WithTransformPathRemovePrefix(prefix: $"/swagger/{prefix}")
                .WithTransformPathPrefix(prefix: "/swagger");

        private RouteConfig MakeRoute((string controller, string service) description) =>
            new RouteConfig
            {
                RouteId = $"{description.service}-{description.controller}-route",
                ClusterId = description.service,
                Match = new RouteMatch {Path = $"/{description.controller}/{{**catch-all}}"},
            };

        private ClusterConfig MakeClusterConfig(string clusterName, string url) => new ClusterConfig
        {
            ClusterId = clusterName,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                { "default", new DestinationConfig{ Address = url } }
            }
        };

        public IReadOnlyList<RouteConfig> Routes => _routes;

        public IReadOnlyList<ClusterConfig> Clusters => _clusters;
        
        public IChangeToken ChangeToken { get; }
    }
}