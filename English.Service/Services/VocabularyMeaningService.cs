using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

/// <summary>
/// Triển khai nghiệp vụ CRUD cho ngữ nghĩa từ vựng (VocabularyMeaning).
/// Che giấu hoàn toàn Entity, sử dụng AutoMapper và IUnitOfWork.
/// </summary>
public class VocabularyMeaningService : IVocabularyMeaningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VocabularyMeaningService> _logger;

    public VocabularyMeaningService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<VocabularyMeaningService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger ?? NullLogger<VocabularyMeaningService>.Instance;
    }

    public async Task<VocabularyMeaningDto?> GetMeaningByIdAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.Meanings.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Không tìm thấy ngữ nghĩa với Id: {Id}", id);
                return null;
            }

            return _mapper.Map<VocabularyMeaningDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi lấy thông tin ngữ nghĩa với Id: {Id}", id);
            throw;
        }
    }

    public async Task<List<VocabularyMeaningDto>> GetAllMeaningsAsync()
    {
        try
        {
            var entities = await _unitOfWork.Meanings.GetAllAsync();
            return _mapper.Map<List<VocabularyMeaningDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải danh sách toàn bộ ngữ nghĩa.");
            throw;
        }
    }

    public async Task<List<VocabularyMeaningDto>> GetMeaningsByVocabularyIdAsync(Guid vocabularyId)
    {
        try
        {
            var entities = await _unitOfWork.Meanings.FindAsync(m => m.VocabularyId == vocabularyId);
            return _mapper.Map<List<VocabularyMeaningDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải ngữ nghĩa theo VocabularyId: {VocabularyId}", vocabularyId);
            throw;
        }
    }

    public async Task<VocabularyMeaningDto?> CreateMeaningAsync(VocabularyMeaningDto dto)
    {
        try
        {
            if (dto.Id == Guid.Empty)
            {
                dto.Id = Guid.NewGuid();
            }

            var entity = _mapper.Map<VocabularyMeaning>(dto);

            await _unitOfWork.Meanings.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã tạo mới thành công ngữ nghĩa (Id: {Id}) cho VocabularyId: {VocabId}", entity.Id, entity.VocabularyId);
            return _mapper.Map<VocabularyMeaningDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tạo ngữ nghĩa mới cho từ vựng Id: {VocabId}", dto.VocabularyId);
            throw;
        }
    }

    public async Task<bool> UpdateMeaningAsync(VocabularyMeaningDto dto)
    {
        try
        {
            var existing = await _unitOfWork.Meanings.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                _logger.LogWarning("Cập nhật thất bại: Không tìm thấy ngữ nghĩa với Id: {Id}", dto.Id);
                return false;
            }

            _mapper.Map(dto, existing);

            _unitOfWork.Meanings.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã cập nhật thành công ngữ nghĩa với Id: {Id}", existing.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi cập nhật ngữ nghĩa với Id: {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteMeaningAsync(Guid id)
    {
        try
        {
            var existing = await _unitOfWork.Meanings.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Xóa thất bại: Không tìm thấy ngữ nghĩa với Id: {Id}", id);
                return false;
            }

            _unitOfWork.Meanings.Delete(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã xóa thành công ngữ nghĩa với Id: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa ngữ nghĩa với Id: {Id}", id);
            throw;
        }
    }
}
