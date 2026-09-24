using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;


namespace SmartPantry;

public interface IProductAppService : IApplicationService
{
    Task<PagedResultDto<ProductDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ProductDto> CreateAsync(ProductDto product);
  Task DeleteAsync(Guid id);
  Task<ProductDto> GetAsync(Guid id);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto product);
    
}
