namespace Cs12DotNet8;

public class Page131
{
    class Animal
    {
        public required string Name;
        public DateTime Born;
        public byte Legs;
    }

    class Cat : Animal
    {
        public bool IsDomestic;
    }

    class Spider : Animal
    {
        public bool IsPoisonous;
    }

    public static void Run()
    {
        Animal?[] animals =
        [
            new Cat {Name = "Karen", Born = new(year: 2022, month: 8, day: 23), Legs = 4, IsDomestic = true},
            null,
            new Cat {Name= "Mufasa", Born = new(year: 1994, month: 6, day: 12)},
            new Spider {Name = "Sid Vicious", Born = DateTime.Today, IsPoisonous = true},
            new Spider {Name = "Captain Furry", Born = DateTime.Today},
        ];

        foreach (Animal? animal in animals)
        {
            string message = animal switch
            {
                Cat fourLeggedCat when fourLeggedCat.Legs == 4
                    => $"The cat named {fourLeggedCat.Name} has four legs.",
                Cat wildCat when wildCat.IsDomestic == false
                    => $"The non-domestic cat is named {wildCat.Name}",
                Cat cat
                    => $"The cat is named {cat.Name}",
                Spider spider when spider.IsPoisonous
                    => $"The {spider.Name} spider is poisonous. Run!",
                null
                    => "The animal is null",
                _
                    => $"The {animal.Name} is {animal.GetType().Name}",

            };

            WriteLine($"switch expression: {message}");
        }
    }
}
