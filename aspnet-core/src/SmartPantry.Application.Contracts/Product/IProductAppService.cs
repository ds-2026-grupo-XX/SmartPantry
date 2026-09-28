using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;


namespace SmartPantry;

public interface IProductAppService :
    ICrudAppService<ProductDto, 
        Guid, 
        PagedAndSortedResultRequestDto,
        CreateUpdateProductDto>
{
    
}
