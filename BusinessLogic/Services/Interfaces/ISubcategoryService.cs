using BusinessLogic.DTOs.Subcategory;

namespace BusinessLogic.Services.Interfaces
{
    public interface ISubcategoryService
    {
        Task<IEnumerable<ReadSubcategoryDto>> GetAllSubcategoriesAsync();
    }
}
