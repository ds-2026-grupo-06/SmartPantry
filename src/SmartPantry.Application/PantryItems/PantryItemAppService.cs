using Microsoft.AspNetCore.Authorization;
using SmartPantry.PantryItems;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using System.Collections.Generic;


namespace SmartPantry.PantryItems;

[ExposeServices(typeof(IPantryItemAppService), typeof(PantryItemAppService))]

[Authorize]
public class PantryItemAppService : CrudAppService<
        PantryItem,
        PantryItemDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdatePantryItemDto>,
    IPantryItemAppService
{
    private readonly IRepository<PantryItem, Guid> _repository;
    public PantryItemAppService(IRepository<PantryItem, Guid> repository) : base(repository)
    {
        _repository = repository;
    }
    public async Task<PantryItemDto> ActualizarEstadoAsync(Guid id, EstadoPantryItem estado)
    {
        var pantryItem = await _repository.GetAsync(id);
        pantryItem.ActualizarEstado(estado);
        await _repository.UpdateAsync(pantryItem);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }
    public async Task<PantryItemDto> ActualizarFechaVencimientoAsync(Guid id, DateTime fechaVencimiento)
    {
        var pantryItem = await _repository.GetAsync(id);
        pantryItem.ActualizarFechaVencimiento(fechaVencimiento);
        await _repository.UpdateAsync(pantryItem);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }
    public async Task<PantryItemDto> ActualizarCantidadAsync(Guid id, int cantidad)
    {
        var pantryItem = await _repository.GetAsync(id);
        pantryItem.ActualizarCantidad(cantidad);
        await _repository.UpdateAsync(pantryItem);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }
    public async Task<PantryItemDto>ActualizarUnidadAsync(Guid id, string unidad)
    {
        var pantryItem = await _repository.GetAsync(id);
        pantryItem.ActualizarUnidad(unidad);
        await _repository.UpdateAsync(pantryItem);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }
    public async Task<List<PantryItemDto>> ListarPorNombreAsync(string nombreProducto)
    {
        var pantryItems = await _repository.GetListAsync(p => p.NombreProducto.Contains(nombreProducto));
        return ObjectMapper.Map<List<PantryItem>, List<PantryItemDto>>(pantryItems);
    }
    public async Task EliminarItemAsync(Guid id)
    {
        var pantryItem = await _repository.GetAsync(id);
        await _repository.DeleteAsync(pantryItem);
    }
}
