namespace English.Entity.DTOs;

public class StudyHistoryDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int ResponseTimeMs { get; set; }
    public DateTime TestedAt { get; set; }
}
