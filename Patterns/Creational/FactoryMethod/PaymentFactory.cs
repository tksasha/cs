using Microsoft.Extensions.DependencyInjection;

namespace Patterns.Creational.FactoryMethod;

sealed class PaymentFactory(IServiceProvider serviceProvider)
{
    public IPayment Create(PaymentMethod paymentMethod)
        => serviceProvider.GetRequiredKeyedService<IPayment>(paymentMethod);
}
