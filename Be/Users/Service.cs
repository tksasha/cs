using Be.Data;

namespace Be.Users;

public class Service(
    IRepository<User> repository,
    CreateRequestValidator createRequestValidator,
    ILogger<Service> logger) : IService
{
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public async Task<bool> CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var result = createRequestValidator.Validate(request);

        if (!result.IsValid)
        {
            return false; // TODO: use result pattern here
        }

        var user = new User
        {
            Name = request.Name,
            ValidFrom = request.ValidFrom.UtcDateTime,
            ValidTo = request.ValidTo?.UtcDateTime ?? DateTimeOffset.MaxValue,
        };

        logger.LogInformation("user.ValidFrom = {ValidFrom}, user.ValidTo = {ValidTo}", user.ValidFrom, user.ValidTo);

        await repository.InsertAsync(user, cancellationToken);

        return true;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetCurrentAsync(id, cancellationToken);

    public async Task UpdateAsync(Guid id, UpdateRequest request, CancellationToken cancellationToken)
    {
        User? user = await repository.GetByIdAsync(id, cancellationToken);

        if (user is null)
        {
            return; // TODO: use Result or Expected to return an error
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            user.Name = request.Name; // TODO: use validation
        }

        await repository.UpdateAsync(cancellationToken);
    }
}
