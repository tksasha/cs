namespace Examples.Classes;

class Person(string name, int age)
{
    public string Name { get; } = name;
    public int Age { get; } = age;

    public Person() : this(name: "Unknown", age: 0)
    { }

    public static void Run()
    {
        var anonymous = new Person();

        WriteLine($"{anonymous.Name}, {anonymous.Age}");

        var batman = new Person(name: "Bruce Wayne", age: 42);

        WriteLine($"{batman.Name}, {batman.Age}");
    }
}
