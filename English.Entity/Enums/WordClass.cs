using System.ComponentModel;

namespace English.Entity.Enums;

/// <summary>
/// Từ loại trong tiếng Anh (Part of Speech / Word Class).
/// </summary>
public enum WordClass
{
    [Description("Danh từ")]
    Noun = 1,
    [Description("Động từ")]
    Verb = 2,
    [Description("Tính từ")]
    Adjective = 3,
    [Description("Trạng từ")]
    Adverb = 4,
    [Description("Đại từ")]
    Pronoun = 5,
    [Description("Giới từ")] 
    Preposition = 6,
    [Description("Liên từ")] 
    Conjunction = 7,
    [Description("Thán từ")] 
    Interjection = 8,
    [Description("Cụm động từ")] 
    PhrasalVerb = 9,
    [Description("Thành ngữ")] 
    Idiom = 10
}
