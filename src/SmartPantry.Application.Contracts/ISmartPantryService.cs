using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry;

public interface ISmartPantryService : IApplicationService
{
    Task<List<ProductDto>> GetListAsync();
    Task<ProductDto> CreateAsync(string barcode);
    Task<ProductDto> CreateAsync(string name);
    Task<ProductDto> CreateAsync(string brand);
    Task<ProductDto> CreateAsync(string nutriScore);
    Task<ProductDto> CreateAsync(int novaGroup);
    Task DeleteAsync(Guid id);
}

