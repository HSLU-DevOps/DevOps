namespace WebAPI.Features;

public static class FeatureFlags
{
    //Allows user only to read todos
    public const string Read = "Read";

    //Allows user to create todos
    public const string Create = "Create";

    //Allows special users to mutate todos: So update and delete
    public const string Mutate = "Mutate";
}