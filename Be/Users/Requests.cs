namespace Be.Users;

public record class CreateRequest(string Name, DateTimeOffset ValidFrom, DateTimeOffset? ValidTo);

public record class UpdateRequest(string Name, int Fact);
