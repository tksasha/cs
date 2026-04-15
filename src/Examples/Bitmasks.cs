namespace Examples;

class Bitmasks
{
    public static void Run()
    {
        int Monday = 1;     // 0b0000001
        int Tuesday = 2;    // 0b0000010
        int Wednesday = 4;  // 0b0000100
        int Thursday = 8;   // 0b0001000
        int Friday = 16;    // 0b0010000
        int Saturday = 32;  // 0b0100000
        int Sunday = 64;    // 0b1000000

        int Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday;
        WriteLine($"{nameof(Weekdays)} = {Weekdays}");

        int Weekend = Saturday | Sunday;
        WriteLine($"{nameof(Weekend)} = {Weekend}");

        bool isWeekday(int day)
            => (Weekdays & day) != 0;

        bool isWeekend(int day)
            => (Weekend & day) != 0;

        WriteLine($"{nameof(Monday)} is weekday = {isWeekday(Monday)}, is weekend = {isWeekend(Monday)}");
        WriteLine($"{nameof(Friday)} is weekday = {isWeekday(Friday)}, is weekend = {isWeekend(Friday)}");
        WriteLine($"{nameof(Sunday)} is weekday = {isWeekday(Sunday)}, is weekend = {isWeekend(Sunday)}");
    }
}
