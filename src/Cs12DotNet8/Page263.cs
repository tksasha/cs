namespace Cs12DotNet8;

public class Page263
{
    public static void Run()
    {
        int a = 10;
        int b = 20;
        int c = 30;
        int d = 40;

        WriteLine($"Before:\na = {a}, b = {b}, c = {c}, d = {d}");

        PassingParameters(a, b, ref c, out d);

        WriteLine($"After:\na = {a}, b = {b}, c = {c}, d = {d}");
    }

    public static void Run2()
    {
        Person person = new() { Name = "John McClane" };

        ChangeName(person, "Bruce Wayne");

        WriteLine($"person.Name = {person.Name}");

        ChangeName2(person, "Clark Kent");

        WriteLine($"person.Name = {person.Name}");
    }

    static void PassingParameters(int a, in int b, ref int c, out int d)
    {
        a++;

        // b++; // because it is readonly

        c++;

        d = default;

        d++;

        WriteLine($"In the method:\na = {a}, b = {b}, c = {c}, d = {d}");
    }

    class Person
    {
        public required string Name { get; set; }
    }

    static void ChangeName(Person person, string name)
    {
        person.Name = name;
    }

    static void ChangeName2(in Person person, string name)
    {
        // person = new Person { Name = name };
        var person2 = new Person { Name = name };
    }
}
