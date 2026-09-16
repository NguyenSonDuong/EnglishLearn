using AutoMapper;
using English.Entity.DTOs;
using English.Entity.Entities;

namespace English.Service.Mappings;

/// <summary>
/// Cấu hình ánh xạ hai chiều giữa Domain Entities và Data Transfer Objects (DTOs).
/// </summary>
public class AppMapperProfile : Profile
{
    public AppMapperProfile()
    {
        CreateMap<Vocabulary, VocabularyDto>().ReverseMap();
        CreateMap<VocabularyMeaning, VocabularyMeaningDto>().ReverseMap();
        CreateMap<MeaningExample, MeaningExampleDto>().ReverseMap();
    }
}
