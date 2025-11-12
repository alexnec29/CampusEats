using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;

namespace CampusEats.Api.Infrastructure.Repositories;

public interface IPaymentRepository : IRepository<Payment, int>
{
    Task<IList<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
    Task<Payment?> GetByTransactionIdAsync(string transactionId);
}