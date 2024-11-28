using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Lab;
using BusinessLogic.Services.Interfaces;
using BusinessLogic.Utils.Implementation;
using AutoMapper.QueryableExtensions;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class LabService : ILabService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LabService> _logger;
        private readonly IMapper _mapper;

        public LabService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<LabService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ReadLabSimpleDto>> GetAllLabsAsync()
        {
            var labs = await _unitOfWork.GetRepository<Lab>()
                .GetAllAsync();
            return _mapper.Map<IEnumerable<ReadLabSimpleDto>>(labs);
        }

        public async Task<PaginatedList<ReadLabSimpleDto>> GetLabsAsync(QueryParameters queryParameters)
        {
            var labsQuery = _unitOfWork.GetRepository<Lab>()
                .GetAllQueryable();

            var mappedQuery = labsQuery.ProjectTo<ReadLabSimpleDto>(_mapper.ConfigurationProvider);

            var paginatedList = await PaginatedList<ReadLabSimpleDto>.CreateAsync(
                mappedQuery,
                queryParameters.PageNumber,
                queryParameters.PageSize
            );

            return paginatedList;
        }

        public async Task<ReadLabSimpleDto> GetLabByIdAsync(int labId)
        {
            var lab = await _unitOfWork.GetRepository<Lab>()
                .GetAsync(l => l.LabId == labId);

            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found.", labId);
                return null;
            }

            return _mapper.Map<ReadLabSimpleDto>(lab);
        }

        public async Task<ReadLabSimpleDto> CreateLabAsync(CreateLabDto createLabDto)
        {
            var lab = _mapper.Map<Lab>(createLabDto);
            var labRepository = _unitOfWork.GetRepository<Lab>();
            await labRepository.AddAsync(lab);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Lab created with ID {LabId}.", lab.LabId);

            return _mapper.Map<ReadLabSimpleDto>(lab);
        }

        public async Task<bool> UpdateLabAsync(int labId, UpdateLabDto updateLabDto)
        {
            var lab = await _unitOfWork.GetRepository<Lab>().GetByIdAsync(labId);
            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found for update.", labId);
                return false;
            }

            _mapper.Map(updateLabDto, lab);
            _unitOfWork.GetRepository<Lab>().Update(lab);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteLabAsync(int labId)
        {
            var lab = await _unitOfWork.GetRepository<Lab>().GetByIdAsync(labId);
            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found for deletion.", labId);
                return false;
            }

            _unitOfWork.GetRepository<Lab>().Delete(lab);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
