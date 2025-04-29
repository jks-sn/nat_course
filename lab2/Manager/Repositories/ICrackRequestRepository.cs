// Manager/Repositories/ICrackRequestRepository.cs

using Manager.Models;

namespace Manager.Repositories;

public interface ICrackRequestRepository
{
    Task<CrackRequest?> GetAsync(Guid requestId, CancellationToken ct = default);
    Task AddAsync(CrackRequest request, CancellationToken ct = default);
    Task<bool> SaveChangesAsync(CancellationToken ct = default);
}