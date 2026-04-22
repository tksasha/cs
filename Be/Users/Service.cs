using Be.Data;

namespace Be.Users;

public class Service(IRepository<User> repository) : IService
{
    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public async Task CreateAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Name = request.Name,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.MaxValue,
            RecordedFrom = DateTime.UtcNow,
            RecordedTo = DateTime.MaxValue,
            Fact = 1,
        };

        await repository.CreateAsync(user, cancellationToken);
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => await repository.GetByIdAsync(id, cancellationToken);

    public async Task UpdateAsync(int id, UpdateRequest request, CancellationToken cancellationToken)
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

        user.Fact = request.Fact;

        await repository.UpdateAsync(cancellationToken);
    }
}
