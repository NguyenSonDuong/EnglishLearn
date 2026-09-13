using English.Repository.Data;
using English.Entity.Entities;
using English.Entity.Repositories;

namespace English.Repository.Repositories;

/// <summary>
/// Triển khai repository cho nhật ký mở khóa khẩn cấp (EmergencyLog).
/// </summary>
public class EmergencyLogRepository : Repository<EmergencyLog>, IEmergencyLogRepository
{
    public EmergencyLogRepository(AppDbContext context) : base(context) { }
}
