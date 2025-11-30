namespace AppHost;


public sealed class GroupResource(string name) : Resource(name)
{
}
public static class GroupResourceExtensions
{
    public static IResourceBuilder<GroupResource> AddGroup(
        this IDistributedApplicationBuilder builder,
        string name,
        string iconName = "Folder",
        string resourceState = "Active"
        )
    {
        var resource = new GroupResource(name);
        var group = builder.AddResource(resource)
            .WithIconName(iconName)
            .WithInitialState(new()
            {
                ResourceType = name + " Resource",
                State = resourceState,
                Properties = [
                    new(CustomResourceKnownProperties.Source, "Group")
                ]

            });
        return group;
    }
}
