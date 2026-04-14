namespace Examples.Classes;

interface IAnimal
{
    string Speak();
    string Move();

    string Describe()
        => $"{Speak()} and {Move()}";
}

class Dog : IAnimal
{
    public string Speak()
        => "Wof-wof!";

    public string Move()
        => "Running";
}

class Snake : IAnimal
{
    public string Speak()
        => "Sh-h-h!";

    public string Move()
        => "Slithering";

}

class Animal
{
    public static void Run()
    {
        List<IAnimal> animals = [new Dog(), new Snake()];

        foreach (var animal in animals)
        {
            WriteLine(animal.Describe());
        }
    }

}
