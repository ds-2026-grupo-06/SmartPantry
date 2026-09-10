using System;
using Volo.Abp.Application.Dtos;
namespace SmartPantry.Books;
public class BookDto : AuditedEntityDto<Guid>
{
    public required string Name { get; set; }
    public Guid AuthorId { get; set; }
    public required string AuthorName { get; set; }
    public BookType Type { get; set; }
    public DateTime PublishDate { get; set; }
    public float Price { get; set; }
}