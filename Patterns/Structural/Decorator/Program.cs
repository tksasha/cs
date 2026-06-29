namespace Patterns.Structural.Decorator;

interface IUser
{
    string Name { get; }
    int Age { get; }
}

sealed record class User(string Name, int Age) : IUser;

abstract class UserDecoratorBase(IUser user) : IUser
{
    public virtual string Name => user.Name;

    public virtual int Age => user.Age;
}

sealed class UserDecorator(IUser user) : UserDecoratorBase(user)
{
    public override string Name => $"decorated {base.Name}";
}

sealed class ImprovedUserDecorator(IUser user) : UserDecoratorBase(user)
{
    public override string Name => $"improved {base.Name}";
}

internal static class Program
{
    public static void Run()
    {
        var user = new User(Name: "Val Kilmer", Age: 38);

        ShowUserName(user);

        var decorated = new UserDecorator(user);

        ShowUserName(decorated);

        var improved = new ImprovedUserDecorator(decorated);

        ShowUserName(improved);
    }

    private static void ShowUserName(IUser user)
        => WriteLine(user.Name);
}
