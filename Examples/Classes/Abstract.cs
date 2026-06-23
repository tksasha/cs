namespace Examples.Classes;

abstract class Shape
{
    public abstract decimal Area();

    public void Describe()
    {
        WriteLine($"Area() = {Area():F2}");
    }
}

sealed class Circle(decimal radius) : Shape
{
    const decimal Pi = 3.14m;

    public override decimal Area()
        => Pi * radius * radius;
}

sealed class Rectangle(decimal width, decimal height) : Shape
{
    public override decimal Area()
        => width * height;
}

static class Abstract
{
    public static void Run()
    {
        var circle = new Circle(radius: 10);

        circle.Describe();

        var rectangle = new Rectangle(width: 20, height: 30);

        rectangle.Describe();
    }
}
