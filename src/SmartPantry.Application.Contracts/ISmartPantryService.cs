using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry;

public interface ISmartPantryService : IApplicationService
{
    Task<List<ProductDto>> GetListAsync();
    Task<ProductDto> CreateAsync(string barcode, string name, string brand, string nutriScore, int novaGroup);
    Task DeleteAsync(Guid id);
}

