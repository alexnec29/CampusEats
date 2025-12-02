using CampusEats.Api.Models;
using CampusEats.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampusEats.Api.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly CampusEatsDbContext _context;

    public PaymentRepository(CampusEatsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment entity)
    {
        await _context.Payments.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Payments.FindAsync(id);
        if (entity != null)
        {
            _context.Payments.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IList<Payment>> GetAllAsync() =>
        await _context.Payments.ToListAsync();

    public async Task<Payment?> GetByIdAsync(int id) =>
        await _context.Payments.FindAsync(id);

    public async Task UpdateAsync(Payment entity)
    {
        _context.Payments.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IList<Payment>> GetPaymentsByStatusAsync(PaymentStatus status) =>
        await _context.Payments.Where(p => p.Status == status).ToListAsync();

    public async Task<Payment?> GetByTransactionIdAsync(string transactionId) =>
        await _context.Payments.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
}