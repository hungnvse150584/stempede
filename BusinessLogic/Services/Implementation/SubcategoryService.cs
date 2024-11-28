using AutoMapper;
using DataAccess.Data;
using DataAccess.Entities;
using BusinessLogic.DTOs.Subcategory;
using BusinessLogic.Services.Interfaces;

namespace BusinessLogic.Services.Implementation
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubcategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all subcategories from the database.
        /// </summary>
        /// <returns>List of subcategories.</returns>
        public async Task<IEnumerable<ReadSubcategoryDto>> GetAllSubcategoriesAsync()
        {
            var subcategories = await _unitOfWork.GetRepository<Subcategory>().GetAllAsync();
            return _mapper.Map<IEnumerable<ReadSubcategoryDto>>(subcategories);
        }
    }
}
