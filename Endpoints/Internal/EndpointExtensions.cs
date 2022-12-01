using Microsoft.Extensions.Configuration;

namespace LibraryMinimumAPI.Endpoints.Internal
{
    public static class EndpointExtensions
    {
        /* Scans everything in a given assembly and scan every class that implements
         the IEndpoints Interface and dynamically call the AddServices and DefineEndpoints Methods*/
        public static void AddEndpoints<TMarker>(this IServiceCollection services, IConfiguration configuration)
        {
            AddEndpoints(services, typeof(TMarker), configuration);
        }

        public static void AddEndpoints(this IServiceCollection services, Type typeMarker, IConfiguration configuration)
        {
            var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);

            foreach (var endpointType in endpointTypes)
            {
                endpointType.GetMethod(nameof(IEndpoints.AddServices))!.Invoke(null, new object[] { services, configuration });
            }
        }

        public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
        {
            UseEndpoints(app, typeof(TMarker));
        }
        public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
        {
            var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);
            
            // 2. - iterates through endpoints and invokes those that are defined in the DefineEndpoints method.
            foreach (var endpointType in endpointTypes)
            {
                endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!.Invoke(null, new object[] { app });
            }
        }
        private static IEnumerable<System.Reflection.TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
        {
            // 1. - Grabs endpoints where definedTypes are not abstract and not interfaces, that are using the IEndpoints interface.
            var endpointTypes = typeMarker.Assembly.DefinedTypes.Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x));
            return endpointTypes;
        }
    }
}
