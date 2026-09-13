namespace English.Entity.DTOs;

public class EmergencyLogDto
{
    public Guid Id { get; set; }
    public DateTime BypassedAt { get; set; }
    public string? Reason { get; set; }
    public bool IsDeducted { get; set; }
}
