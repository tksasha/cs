namespace Cs12DotNet8;

public class Page247
{
    public static void Run()
    {
        Person person = new();

        person.VisitedWonders =
        WondersOfTheAncientWorld.GreatPyramidOfGiza |
        WondersOfTheAncientWorld.StatueOfZeusAtOlympia;

        WriteLine($"VisitedWonders = {person.VisitedWonders}");
    }

    [Flags]
    enum WondersOfTheAncientWorld : byte
    {
        None = 0b_0000_0000, // 0
        GreatPyramidOfGiza = 0b_0000_0001, // 1
        HangingGardensOfBabylon = 0b_0000_0010, // 2
        StatueOfZeusAtOlympia = 0b_0000_0100, // 4
        TempleOfArtemisAtEphesus = 0b_0000_1000, // 8
        MausoleumAtHalicarnassus = 0b_0001_0000, // 16
        ColossusOfRhodes = 0b_0010_0000, // 32
        LightHouseOfAlexandria = 0b_0100_0000, // 64
    }

    class Person
    {
        public WondersOfTheAncientWorld VisitedWonders;
    }
}
