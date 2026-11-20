using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using Volo.Abp.Validation;

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

    private async Task<PantryItem> ObtenerItemPropioAsync(Guid id)
    {
        var currentUserId = CurrentUser.GetId();
        var pantryItem = await _repository.GetAsync(id);

        if (pantryItem.IdPropietario != currentUserId)
        {
            throw new EntityNotFoundException(typeof(PantryItem), id);
        }

        return pantryItem;
    }

    protected override async Task<IQueryable<PantryItem>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        var currentUserId = CurrentUser.GetId();
        var query = await base.CreateFilteredQueryAsync(input);
        return query.Where(p => p.IdPropietario == currentUserId);
    }

    public override async Task<PantryItemDto> GetAsync(Guid id)
    {
        var pantryItem = await ObtenerItemPropioAsync(id);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public override async Task<PantryItemDto> CreateAsync(CreateUpdatePantryItemDto input)
    {
        var idPropietario = CurrentUser.Id ?? input.IdPropietario;
        var pantryItem = new PantryItem(
            GuidGenerator.Create(),
            idPropietario,
            input.NombreProducto,
            input.Estado,
            input.FechaVencimiento,
            input.Cantidad,
            input.Unidad
        );

        await _repository.InsertAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public override async Task<PantryItemDto> UpdateAsync(Guid id, CreateUpdatePantryItemDto input)
    {
        var pantryItem = await ObtenerItemPropioAsync(id);

        pantryItem.NombreProducto = input.NombreProducto;
        pantryItem.ActualizarEstado(input.Estado);
        pantryItem.ActualizarFechaVencimiento(input.FechaVencimiento);
        pantryItem.ActualizarCantidad(input.Cantidad);
        pantryItem.ActualizarUnidad(input.Unidad);

        await _repository.UpdateAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public override async Task DeleteAsync(Guid id)
    {
        var pantryItem = await ObtenerItemPropioAsync(id);
        await _repository.DeleteAsync(pantryItem, autoSave: true);
    }

    public async Task<PantryItemDto> ActualizarEstadoAsync(Guid id, EstadoPantryItem estado)
    {
        var pantryItem = await ObtenerItemPropioAsync(id);
        pantryItem.ActualizarEstado(estado);
        await _repository.UpdateAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public async Task<PantryItemDto> ActualizarFechaVencimientoAsync(Guid id, DateTime fechaVencimiento)
    {
        if (fechaVencimiento == default)
        {
            throw new AbpValidationException(
                "La fecha de vencimiento no es válida.",
                new List<ValidationResult> { new("La fecha de vencimiento no es válida.", new[] { nameof(fechaVencimiento) }) }
            );
        }

        var pantryItem = await ObtenerItemPropioAsync(id);
        pantryItem.ActualizarFechaVencimiento(fechaVencimiento);
        await _repository.UpdateAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public async Task<PantryItemDto> ActualizarCantidadAsync(Guid id, int cantidad)
    {
        if (cantidad < 0)
        {
            throw new AbpValidationException(
                "La cantidad no puede ser negativa.",
                new List<ValidationResult> { new("La cantidad no puede ser negativa.", new[] { nameof(cantidad) }) }
            );
        }

        var pantryItem = await ObtenerItemPropioAsync(id);
        pantryItem.ActualizarCantidad(cantidad);
        await _repository.UpdateAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public async Task<PantryItemDto> ActualizarUnidadAsync(Guid id, string unidad)
    {
        if (unidad != null && unidad.Length > 32)
        {
            throw new AbpValidationException(
                "La unidad supera la longitud máxima permitida.",
                new List<ValidationResult> { new("La unidad supera la longitud máxima permitida.", new[] { nameof(unidad) }) }
            );
        }

        var pantryItem = await ObtenerItemPropioAsync(id);
        pantryItem.ActualizarUnidad(unidad);
        await _repository.UpdateAsync(pantryItem, autoSave: true);
        return ObjectMapper.Map<PantryItem, PantryItemDto>(pantryItem);
    }

    public async Task<List<PantryItemDto>> ListarPorNombreAsync(string nombreProducto)
    {
        var currentUserId = CurrentUser.GetId();
        var pantryItems = await _repository.GetListAsync(p =>
            p.IdPropietario == currentUserId && p.NombreProducto.Contains(nombreProducto));
        return ObjectMapper.Map<List<PantryItem>, List<PantryItemDto>>(pantryItems);
    }

    public async Task EliminarItemAsync(Guid id)
    {
        var pantryItem = await ObtenerItemPropioAsync(id);
        await _repository.DeleteAsync(pantryItem, autoSave: true);
    }
}
