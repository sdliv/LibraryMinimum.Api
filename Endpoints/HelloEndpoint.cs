using LibraryMinimumAPI.Endpoints.Internal;

namespace LibraryMinimumAPI.Endpoints
{
    public class HelloEndpoint : IEndpoints
    {
        public static void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            
        }

        public static void DefineEndpoints(IEndpointRouteBuilder app)
        {
            app.MapGet("HelloEndpoint", () => "Hello World!");
        }
    }
}
