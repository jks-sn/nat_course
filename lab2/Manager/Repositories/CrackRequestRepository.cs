// Manager/Repositories/CrackRequestRepository.cs

using Manager.Data;
using Manager.Models;
using Microsoft.EntityFrameworkCore;

namespace Manager.Repositories;

public class CrackRequestRepository(CrackDbContext db) : ICrackRequestRepository
{
    public async Task<CrackRequest?> GetAsync(Guid requestId, CancellationToken ct = default)
        => await db.Requests.Include(r => r.FoundWords).Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.Id == requestId, ct);

    public async Task AddAsync(CrackRequest request, CancellationToken ct = default)
    {
        db.Requests.Add(request);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
        => (await db.SaveChangesAsync(ct)) > 0;
}