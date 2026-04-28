using System.Reflection;

using People.Endpoints;

namespace People;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication MapEndpoints(Assembly assembly)
        {
            var endpointGroupTypes = assembly.GetExportedTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false }
                    && t.IsAssignableTo(typeof(IEndpointGroup)));

            foreach (var type in endpointGroupTypes)
            {
                var groupName = type.Name;

                var route = groupName.ToLower();

                var group = app.MapGroup(route).WithTags(groupName);

                type.GetMethod(nameof(IEndpointGroup.Map))!.Invoke(null, [group]);
            }

            return app;
        }
    }
}
