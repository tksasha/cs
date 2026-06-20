namespace Examples;

class PatternMatching
{
    abstract class Vehicle { }

    class Car : Vehicle
    {
        public int Passengers { get; set; }
    }

    class Truck : Vehicle
    { }

    public static void Run()
    {
        Log($"Car w/o passengers toll = {CalculateToll(new Car())}");
        Log($"Car w/ one passenger toll = {CalculateToll(new Car { Passengers = 1 })}");
        Log($"Car w/ three passenger toll = {CalculateToll(new Car { Passengers = 3 })}");
        Log($"Truck toll = {CalculateToll(new Truck())}");
    }

    static decimal CalculateToll(Vehicle vehicle)
    {
        return vehicle switch
        {
            Car { Passengers: 1 } => 2.0m,
            Car { Passengers: > 1 } car => car.Passengers * 1.5m,
            Car => 3.00m,
            Truck => 7.50m,
            _ => throw new ArgumentException("Unknown type of Vehicle: ", nameof(vehicle)),
        };
    }

    static void Log(string message)
    {
        WriteLine($"[{nameof(PatternMatching)}.{nameof(Run)}] {message}");
    }
}
