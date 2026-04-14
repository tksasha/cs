namespace Examples.Classes;

class Person(string name, int age)
{
    public string Name { get; } = name;
    public int Age { get; } = age;

    public Person() : this(name: "Unknown", age: 0)
    { }

    override public string ToString()
        => $"{Name}";
}

class Employee(string name, int age, string company) : Person(name, age)
{
    public string Company { get; } = company;

    override public string ToString()
        => $"{base.ToString()} from {Company}";
}

class Test
{
    public static void Run()
    {
        var anonymous = new Person();

        WriteLine($"{anonymous.Name}, {anonymous.Age}");

        var batman = new Person(name: "Bruce Wayne", age: 42);

        WriteLine($"{batman.Name}, {batman.Age}");

        var employee = new Employee(name: "John Smith", age: 37, company: "Microsoft");

        WriteLine(employee);
    }
}
