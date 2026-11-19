using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry;

public interface IItemDespensaAppService : IApplicationService
{
    Task<PagedResultDto<ItemDespensaDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<ItemDespensaDto> UpdateAsync(Guid id, UpdateItemDespensaDto input);

    Task<ItemDespensaDto> MarkAsConsumedAsync(Guid id);

    Task DeleteAsync(Guid id);
}