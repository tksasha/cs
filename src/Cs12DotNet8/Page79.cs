namespace Cs12DotNet8;

public class Page79
{
    class Person
    {
        public required string FirstName;
        public int Age;
    }

    public static void Run()
    {
        string xml = """
            <person age="50">
                <first_name>Mark</first_name>
            </person>
            """;

        WriteLine(xml);

        Person person = new() { FirstName = "Alise", Age = 56 };

        string json = $$"""
            {
                "first_name": "{{person.FirstName}}",
                "age": "{{person.Age}}",
                "calculation": "{{{1 + 2}}}"
            }
            """;

        WriteLine(json);
    }
}
