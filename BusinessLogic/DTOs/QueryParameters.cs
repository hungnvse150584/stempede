using BusinessLogic.Constants;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.DTOs
{
    public class QueryParameters
    {
        private const int MaxPageSize = PaginationDefaults.MaxPageSize;
        private int _pageSize = PaginationDefaults.DefaultPageSize;

        /// <summary>
        /// The page number to retrieve. Must be greater than 0.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
        public int PageNumber { get; set; } = PaginationDefaults.DefaultPageNumber;

        /// <summary>
        /// The number of items per page. Maximum allowed is 100.
        /// </summary>
        [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
    }
}
