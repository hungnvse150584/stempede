using BusinessLogic.DTOs.Product;
using AutoMapper;
using BusinessLogic.Services.Interfaces;
using AutoMapper.QueryableExtensions;
using BusinessLogic.Utils.Implementation;
using BusinessLogic.DTOs;
using DataAccess.Data;
using DataAccess.Entities;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<ProductService> logger)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ReadProductDto?> GetProductByIdAsync(int productId)
        {
            var product = await _unitOfWork.GetRepository<Product>().GetAsync(
                p => p.ProductId == productId,
                includeProperties: "Lab,Subcategory");

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found.", productId);
                return null;
            }

            return _mapper.Map<ReadProductDto>(product);
        }

        public async Task<PaginatedList<ReadProductDto>> GetAllProductsAsync(QueryParameters queryParameters)
        {
            var productsQuery = _unitOfWork.GetRepository<Product>().GetAllQueryable(includeProperties: "Lab,Subcategory");

            var mappedQuery = productsQuery.ProjectTo<ReadProductDto>(_mapper.ConfigurationProvider);

            // Create paginated list
            var paginatedList = await PaginatedList<ReadProductDto>.CreateAsync(
                mappedQuery,
                queryParameters.PageNumber,
                queryParameters.PageSize
            );

            return paginatedList;
        }

        public async Task<ReadProductDto> CreateProductAsync(CreateProductDto createDto)
        {
            // Validate LabId
            var lab = await _unitOfWork.GetRepository<Lab>().GetAsync(l => l.LabId == createDto.LabId);
            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found.", createDto.LabId);
                throw new ArgumentException("Invalid LabId.");
            }

            // Validate SubcategoryId
            var subcategory = await _unitOfWork.GetRepository<Subcategory>().GetAsync(s => s.SubcategoryId == createDto.SubcategoryId);
            if (subcategory == null)
            {
                _logger.LogWarning("Subcategory with ID {SubcategoryId} not found.", createDto.SubcategoryId);
                throw new ArgumentException("Invalid SubcategoryId.");
            }

            var product = _mapper.Map<Product>(createDto);
            await _unitOfWork.GetRepository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product created with ID {ProductId}.", product.ProductId);

            // Fetch the product with related entities to include in the response
            var createdProduct = await _unitOfWork.GetRepository<Product>().GetAsync(
                p => p.ProductId == product.ProductId,
                includeProperties: "Lab,Subcategory");

            return _mapper.Map<ReadProductDto>(createdProduct);
        }

        public async Task<bool> UpdateProductAsync(int productId, UpdateProductDto updateDto)
        {
            var product = await _unitOfWork.GetRepository<Product>().GetAsync(
                p => p.ProductId == productId,
                includeProperties: "");

            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update.", productId);
                return false;
            }

            // Validate LabId
            var lab = await _unitOfWork.GetRepository<Lab>().GetAsync(l => l.LabId == updateDto.LabId);
            if (lab == null)
            {
                _logger.LogWarning("Lab with ID {LabId} not found.", updateDto.LabId);
                throw new ArgumentException("Invalid LabId.");
            }

            // Validate SubcategoryId
            var subcategory = await _unitOfWork.GetRepository<Subcategory>().GetAsync(s => s.SubcategoryId == updateDto.SubcategoryId);
            if (subcategory == null)
            {
                _logger.LogWarning("Subcategory with ID {SubcategoryId} not found.", updateDto.SubcategoryId);
                throw new ArgumentException("Invalid SubcategoryId.");
            }

            // Map the updated fields
            _mapper.Map(updateDto, product);
            _unitOfWork.GetRepository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product with ID {ProductId} updated successfully.", productId);

            return true;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _unitOfWork.GetRepository<Product>().GetAsync(p => p.ProductId == productId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion.", productId);
                return false;
            }

            _unitOfWork.GetRepository<Product>().Delete(product);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Product with ID {ProductId} deleted successfully.", productId);

            return true;
        }
    }
}
