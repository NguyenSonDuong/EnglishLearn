namespace English.Entity.DTOs;

public class StudyRecordDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public int RepetitionCount { get; set; }
    public double EasinessFactor { get; set; }
    public double Interval { get; set; }
    public DateTime NextReviewTime { get; set; }
    public DateTime LastTestedAt { get; set; }
}
