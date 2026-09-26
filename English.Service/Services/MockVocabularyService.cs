using English.Entity.DTOs;
using English.Entity.Enums;
using English.Entity.Services;

namespace English.Service.Services;

/// <summary>
/// Triển khai giả lập (Mock) của IVocabularyGeneratorService.
/// Cung cấp dữ liệu mẫu theo cấp độ CEFR kèm độ trễ giả lập cuộc gọi API bất đồng bộ.
/// Sẵn sàng để thay thế bằng service AI hoặc API khác trong tương lai qua Dependency Injection.
/// </summary>
public class MockVocabularyService : IVocabularyGeneratorService
{
    private readonly Random _random = new();

    public async Task<VocabularyDto?> GenerateVocabularyAsync(CEFRLevel level)
    {
        // Mô phỏng độ trễ mạng khi gọi API
        await Task.Delay(500);

        var list = GetMockListByLevel(level);
        if (list.Count == 0)
        {
            return CreateDefaultVocabulary(level);
        }

        var chosen = list[_random.Next(list.Count)];
        return CloneWithNewGuids(chosen);
    }

    private static VocabularyDto CloneWithNewGuids(VocabularyDto template)
    {
        var dtoId = Guid.NewGuid();
        return new VocabularyDto
        {
            Id = dtoId,
            WordText = template.WordText,
            Description = template.Description,
            Phonetic_UK = template.Phonetic_UK,
            Phonetic_US = template.Phonetic_US,
            AudioPath_UK = template.AudioPath_UK,
            AudioPath_US = template.AudioPath_US,
            Level = template.Level,
            WordFamily = new List<string>(template.WordFamily),
            Meanings = template.Meanings.Select(m =>
            {
                var meaningId = Guid.NewGuid();
                return new VocabularyMeaningDto
                {
                    Id = meaningId,
                    VocabularyId = dtoId,
                    WordClass = m.WordClass,
                    Definition_EN = m.Definition_EN,
                    Definition_VI = m.Definition_VI,
                    Context = m.Context,
                    Synonyms = new List<string>(m.Synonyms),
                    Antonyms = new List<string>(m.Antonyms),
                    Examples = m.Examples.Select(e => new MeaningExampleDto
                    {
                        Id = Guid.NewGuid(),
                        MeaningId = meaningId,
                        Sentence_EN = e.Sentence_EN,
                        Sentence_VI = e.Sentence_VI,
                        HighlightedTarget = e.HighlightedTarget
                    }).ToList()
                };
            }).ToList()
        };
    }

    private static VocabularyDto CreateDefaultVocabulary(CEFRLevel level)
    {
        var id = Guid.NewGuid();
        var meaningId = Guid.NewGuid();
        return new VocabularyDto
        {
            Id = id,
            WordText = "example",
            Description = "Một sự vật, sự việc cụ thể được nêu ra để làm minh chứng hoặc giải thích cho một quy tắc chung.",
            Phonetic_UK = "/ɪɡˈzɑːm.pəl/",
            Phonetic_US = "/ɪɡˈzæm.pəl/",
            Level = level,
            WordFamily = new List<string> { "exemplary", "exemplify" },
            Meanings = new List<VocabularyMeaningDto>
            {
                new()
                {
                    Id = meaningId,
                    VocabularyId = id,
                    WordClass = WordClass.Noun,
                    Definition_EN = "Something that serves to illustrate a rule or typical instance.",
                    Definition_VI = "Ví dụ, mẫu vật minh họa cho quy tắc hoặc trường hợp điển hình.",
                    Context = ContextTag.General,
                    Synonyms = new List<string> { "instance", "sample", "model" },
                    Antonyms = new List<string>(),
                    Examples = new List<MeaningExampleDto>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            MeaningId = meaningId,
                            Sentence_EN = "This is a good example of modern architecture.",
                            Sentence_VI = "Đây là một ví dụ điển hình của kiến trúc hiện đại.",
                            HighlightedTarget = "example"
                        }
                    }
                }
            }
        };
    }

    private static List<VocabularyDto> GetMockListByLevel(CEFRLevel level) => level switch
    {
        CEFRLevel.A1 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "happy",
                Phonetic_UK = "/ˈhæp.i/",
                Phonetic_US = "/ˈhæp.i/",
                Level = CEFRLevel.A1,
                WordFamily = new List<string> { "happiness", "happily", "unhappy" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Feeling, showing, or causing pleasure or satisfaction.",
                        Definition_VI = "Cảm thấy hoặc thể hiện sự vui vẻ, hạnh phúc, hài lòng.",
                        Context = ContextTag.General,
                        Synonyms = new List<string> { "cheerful", "glad", "joyful" },
                        Antonyms = new List<string> { "sad", "unhappy" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "They are a very happy family.",
                                Sentence_VI = "Họ là một gia đình rất hạnh phúc.",
                                HighlightedTarget = "happy"
                            }
                        }
                    }
                }
            },
            new()
            {
                WordText = "learn",
                Phonetic_UK = "/lɜːn/",
                Phonetic_US = "/lɝːn/",
                Level = CEFRLevel.A1,
                WordFamily = new List<string> { "learner", "learning", "learned" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Verb,
                        Definition_EN = "To get knowledge or skill in a new subject or activity.",
                        Definition_VI = "Học tập, tiếp thu kiến thức hoặc kỹ năng mới.",
                        Context = ContextTag.General,
                        Synonyms = new List<string> { "study", "acquire", "master" },
                        Antonyms = new List<string> { "forget", "unlearn" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "I want to learn English every day.",
                                Sentence_VI = "Tôi muốn học tiếng Anh mỗi ngày.",
                                HighlightedTarget = "learn"
                            }
                        }
                    }
                }
            }
        },

        CEFRLevel.A2 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "travel",
                Phonetic_UK = "/ˈtræv.əl/",
                Phonetic_US = "/ˈtræv.əl/",
                Level = CEFRLevel.A2,
                WordFamily = new List<string> { "traveler", "travelling" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Verb,
                        Definition_EN = "To make a journey, usually over a long distance.",
                        Definition_VI = "Đi lại, du lịch, di chuyển qua một khoảng cách xa.",
                        Context = ContextTag.General,
                        Synonyms = new List<string> { "journey", "trip", "tour" },
                        Antonyms = new List<string> { "stay" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "She loves to travel around the world.",
                                Sentence_VI = "Cô ấy thích đi du lịch vòng quanh thế giới.",
                                HighlightedTarget = "travel"
                            }
                        }
                    }
                }
            }
        },

        CEFRLevel.B1 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "convenient",
                Phonetic_UK = "/kənˈviː.ni.ənt/",
                Phonetic_US = "/kənˈviː.ni.ənt/",
                Level = CEFRLevel.B1,
                WordFamily = new List<string> { "convenience", "conveniently", "inconvenient" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Suitable for your purposes and needs and causing the least difficulty.",
                        Definition_VI = "Tiện lợi, thuận tiện cho mục đích và không gây khó khăn.",
                        Context = ContextTag.General,
                        Synonyms = new List<string> { "handy", "suitable", "accessible" },
                        Antonyms = new List<string> { "inconvenient", "awkward" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "The house is very convenient for several schools.",
                                Sentence_VI = "Ngôi nhà rất thuận tiện để đi đến nhiều trường học.",
                                HighlightedTarget = "convenient"
                            }
                        }
                    }
                }
            }
        },

        CEFRLevel.B2 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "resilient",
                Phonetic_UK = "/rɪˈzɪl.i.ənt/",
                Phonetic_US = "/rɪˈzɪl.jənt/",
                Level = CEFRLevel.B2,
                WordFamily = new List<string> { "resilience", "resiliently", "resiliency" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Able to be happy, successful, etc. again after something difficult.",
                        Definition_VI = "Kiên cường, có khả năng phục hồi nhanh chóng sau khó khăn.",
                        Context = ContextTag.General,
                        Synonyms = new List<string> { "tough", "hardy", "adaptable", "strong" },
                        Antonyms = new List<string> { "fragile", "vulnerable", "weak" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "She showed a resilient spirit throughout the hardship.",
                                Sentence_VI = "Cô ấy đã thể hiện tinh thần kiên cường suốt thời kỳ khó khăn.",
                                HighlightedTarget = "resilient"
                            },
                            new()
                            {
                                Sentence_EN = "The local economy has proven to be remarkably resilient.",
                                Sentence_VI = "Nền kinh tế địa phương đã chứng minh khả năng phục hồi đáng kinh ngạc.",
                                HighlightedTarget = "resilient"
                            }
                        }
                    }
                }
            },
            new()
            {
                WordText = "sustainable",
                Phonetic_UK = "/səˈsteɪ.nə.bəl/",
                Phonetic_US = "/səˈsteɪ.nə.bəl/",
                Level = CEFRLevel.B2,
                WordFamily = new List<string> { "sustain", "sustainability", "sustainably" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Able to continue over a period of time without causing damage.",
                        Definition_VI = "Bền vững, có thể duy trì lâu dài mà không gây tổn hại môi trường.",
                        Context = ContextTag.Business,
                        Synonyms = new List<string> { "viable", "enduring", "renewable" },
                        Antonyms = new List<string> { "unsustainable", "exhaustible" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "We must encourage sustainable economic growth.",
                                Sentence_VI = "Chúng ta phải khuyến khích sự tăng trưởng kinh tế bền vững.",
                                HighlightedTarget = "sustainable"
                            }
                        }
                    }
                }
            }
        },

        CEFRLevel.C1 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "meticulous",
                Phonetic_UK = "/məˈtɪk.jə.ləs/",
                Phonetic_US = "/məˈtɪk.jə.ləs/",
                Level = CEFRLevel.C1,
                WordFamily = new List<string> { "meticulously", "meticulousness" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Very careful and with great attention to every detail.",
                        Definition_VI = "Tỉ mỉ, cẩn thận kỹ lưỡng tới từng chi tiết nhỏ.",
                        Context = ContextTag.Formal,
                        Synonyms = new List<string> { "thorough", "painstaking", "scrupulous" },
                        Antonyms = new List<string> { "careless", "sloppy" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "She was meticulous in her research preparation.",
                                Sentence_VI = "Cô ấy vô cùng tỉ mỉ trong việc chuẩn bị nghiên cứu.",
                                HighlightedTarget = "meticulous"
                            }
                        }
                    }
                }
            }
        },

        CEFRLevel.C2 => new List<VocabularyDto>
        {
            new()
            {
                WordText = "ubiquitous",
                Phonetic_UK = "/juːˈbɪk.wɪ.təs/",
                Phonetic_US = "/juːˈbɪk.wə.t̬əs/",
                Level = CEFRLevel.C2,
                WordFamily = new List<string> { "ubiquity", "ubiquitously" },
                Meanings = new List<VocabularyMeaningDto>
                {
                    new()
                    {
                        WordClass = WordClass.Adjective,
                        Definition_EN = "Seeming to be everywhere at the same time.",
                        Definition_VI = "Có mặt ở khắp mọi nơi cùng lúc, phổ biến ở khắp chốn.",
                        Context = ContextTag.Formal,
                        Synonyms = new List<string> { "omnipresent", "pervasive", "universal" },
                        Antonyms = new List<string> { "rare", "scarce" },
                        Examples = new List<MeaningExampleDto>
                        {
                            new()
                            {
                                Sentence_EN = "Smartphones have become ubiquitous in everyday life.",
                                Sentence_VI = "Điện thoại thông minh đã trở nên hiện diện ở khắp mọi nơi trong đời sống thường nhật.",
                                HighlightedTarget = "ubiquitous"
                            }
                        }
                    }
                }
            }
        },

        _ => new List<VocabularyDto>()
    };
}
