namespace CampusEats.Api.Utils.PaymentUtil;

public class PaymentProviderFactory(IEnumerable<IPaymentService> strategies)
{
    public IPaymentService? GetProvider(string providerName)
    {
        return strategies.FirstOrDefault(s => s.Name.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }
}