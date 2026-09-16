using AutoMapper;
using Microsoft.Extensions.Logging;
using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

/// <summary>
/// Triển khai nghiệp vụ CRUD cho từ vựng (Vocabulary).
/// Che giấu hoàn toàn Entity, sử dụng AutoMapper, IUnitOfWork và ILogger (tích hợp NLog).
/// </summary>
public class VocabularyService : IVocabularyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VocabularyService> _logger;

    public VocabularyService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<VocabularyService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VocabularyDto?> GetVocabularyByIdAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.Vocabularies.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Không tìm thấy từ vựng với Id: {Id}", id);
                return null;
            }

            return _mapper.Map<VocabularyDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi lấy thông tin từ vựng với Id: {Id}", id);
            throw;
        }
    }

    public async Task<List<VocabularyDto>> GetAllVocabulariesAsync()
    {
        try
        {
            var entities = await _unitOfWork.Vocabularies.GetAllAsync();
            return _mapper.Map<List<VocabularyDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải danh sách toàn bộ từ vựng.");
            throw;
        }
    }

    public async Task<VocabularyDto?> CreateVocabularyAsync(VocabularyDto dto)
    {
        try
        {
            if (dto.Id == Guid.Empty)
            {
                dto.Id = Guid.NewGuid();
            }

            // Chuyển đổi DTO sang Domain Entity
            var entity = _mapper.Map<Vocabulary>(dto);

            // Thao tác Repository và commit giao dịch qua Unit of Work
            await _unitOfWork.Vocabularies.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã tạo mới thành công từ vựng: '{WordText}' (Id: {Id})", entity.WordText, entity.Id);

            return _mapper.Map<VocabularyDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tạo từ vựng mới: '{WordText}'", dto.WordText);
            throw;
        }
    }

    public async Task<bool> UpdateVocabularyAsync(VocabularyDto dto)
    {
        try
        {
            var existing = await _unitOfWork.Vocabularies.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                _logger.LogWarning("Cập nhật thất bại: Không tìm thấy từ vựng với Id: {Id}", dto.Id);
                return false;
            }

            // Đồng bộ dữ liệu từ DTO vào thực thể hiện có
            _mapper.Map(dto, existing);

            _unitOfWork.Vocabularies.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã cập nhật thành công từ vựng: '{WordText}' (Id: {Id})", existing.WordText, existing.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi cập nhật từ vựng với Id: {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteVocabularyAsync(Guid id)
    {
        try
        {
            var existing = await _unitOfWork.Vocabularies.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Xóa thất bại: Không tìm thấy từ vựng với Id: {Id}", id);
                return false;
            }

            _unitOfWork.Vocabularies.Delete(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã xóa thành công từ vựng với Id: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa từ vựng với Id: {Id}", id);
            throw;
        }
    }
}
