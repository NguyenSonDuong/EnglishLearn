using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using English.Entity.DTOs;
using English.Entity.Entities;
using English.Entity.Repositories;
using English.Entity.Services;

namespace English.Service.Services;

/// <summary>
/// Triển khai nghiệp vụ CRUD cho câu ví dụ minh họa (MeaningExample).
/// Che giấu hoàn toàn Entity, sử dụng AutoMapper và IUnitOfWork.
/// </summary>
public class MeaningExampleService : IMeaningExampleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MeaningExampleService> _logger;

    public MeaningExampleService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MeaningExampleService>? logger = null)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger ?? NullLogger<MeaningExampleService>.Instance;
    }

    public async Task<MeaningExampleDto?> GetExampleByIdAsync(Guid id)
    {
        try
        {
            var entity = await _unitOfWork.Examples.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Không tìm thấy câu ví dụ với Id: {Id}", id);
                return null;
            }

            return _mapper.Map<MeaningExampleDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi lấy câu ví dụ với Id: {Id}", id);
            throw;
        }
    }

    public async Task<List<MeaningExampleDto>> GetAllExamplesAsync()
    {
        try
        {
            var entities = await _unitOfWork.Examples.GetAllAsync();
            return _mapper.Map<List<MeaningExampleDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải danh sách toàn bộ câu ví dụ.");
            throw;
        }
    }

    public async Task<List<MeaningExampleDto>> GetExamplesByMeaningIdAsync(Guid meaningId)
    {
        try
        {
            var entities = await _unitOfWork.Examples.FindAsync(e => e.MeaningId == meaningId);
            return _mapper.Map<List<MeaningExampleDto>>(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải câu ví dụ theo MeaningId: {MeaningId}", meaningId);
            throw;
        }
    }

    public async Task<MeaningExampleDto?> CreateExampleAsync(MeaningExampleDto dto)
    {
        try
        {
            if (dto.Id == Guid.Empty)
            {
                dto.Id = Guid.NewGuid();
            }

            var entity = _mapper.Map<MeaningExample>(dto);

            await _unitOfWork.Examples.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã tạo mới thành công câu ví dụ (Id: {Id}) cho MeaningId: {MeaningId}", entity.Id, entity.MeaningId);
            return _mapper.Map<MeaningExampleDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tạo câu ví dụ mới cho MeaningId: {MeaningId}", dto.MeaningId);
            throw;
        }
    }

    public async Task<bool> UpdateExampleAsync(MeaningExampleDto dto)
    {
        try
        {
            var existing = await _unitOfWork.Examples.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                _logger.LogWarning("Cập nhật thất bại: Không tìm thấy câu ví dụ với Id: {Id}", dto.Id);
                return false;
            }

            _mapper.Map(dto, existing);

            _unitOfWork.Examples.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã cập nhật thành công câu ví dụ với Id: {Id}", existing.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi cập nhật câu ví dụ với Id: {Id}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteExampleAsync(Guid id)
    {
        try
        {
            var existing = await _unitOfWork.Examples.GetByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Xóa thất bại: Không tìm thấy câu ví dụ với Id: {Id}", id);
                return false;
            }

            _unitOfWork.Examples.Delete(existing);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Đã xóa thành công câu ví dụ với Id: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa câu ví dụ với Id: {Id}", id);
            throw;
        }
    }
}
