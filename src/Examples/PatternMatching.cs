using Microsoft.Extensions.Logging;

namespace Examples;

class PatternMatching(ILogger<PatternMatching> logger)
{
    abstract class Vehicle { }

    class Car : Vehicle
    {
        public int Passengers { get; set; }
    }

    class Truck : Vehicle
    { }

    public void Run()
    {
        using (logger.BeginScope("Run"))
        {
            logger.LogInformation("Car w/o passengers toll = {Toll}", CalculateToll(new Car()));
            logger.LogInformation("Car w/ one passenger toll = {Toll}", CalculateToll(new Car { Passengers = 1 }));
            logger.LogInformation("Car w/ three passenger toll = {Toll}", CalculateToll(new Car { Passengers = 3 }));
            logger.LogInformation("Truck toll = {Toll}", CalculateToll(new Truck()));
        }
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
}
