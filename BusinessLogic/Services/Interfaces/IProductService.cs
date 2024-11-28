using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Product;
using BusinessLogic.Utils.Implementation;

namespace BusinessLogic.Services.Interfaces
{
    public interface IProductService
    {
        Task<ReadProductDto?> GetProductByIdAsync(int productId);
        Task<PaginatedList<ReadProductDto>> GetAllProductsAsync(QueryParameters queryParameters);
        Task<ReadProductDto> CreateProductAsync(CreateProductDto createDto);
        Task<bool> UpdateProductAsync(int productId, UpdateProductDto updateDto);
        Task<bool> DeleteProductAsync(int productId);
    }
}
