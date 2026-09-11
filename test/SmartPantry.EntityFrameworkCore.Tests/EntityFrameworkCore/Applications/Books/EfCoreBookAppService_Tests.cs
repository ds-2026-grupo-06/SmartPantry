using SmartPantry.Books;
using System;
using System.Threading.Tasks;
using SmartPantry.Authors;
using Xunit;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.EntityFrameworkCore.Applications.Books;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
    private readonly IRepository<Author, Guid> _authorRepository;

    public EfCoreBookAppService_Tests()
    {
        _authorRepository = GetRequiredService<IRepository<Author, Guid>>();
    }

    public override async Task Should_Create_A_Valid_Book()
    {
        // Primero crear un Author válido
        var author = new Author
        {
            Id = Guid.NewGuid(),
            Name = "Test Author"
        };

        await _authorRepository.InsertAsync(author, autoSave: true);

        // Luego crear el Book con el AuthorId válido
        var createUpdateBookDto = new CreateUpdateBookDto
        {
            Name = "Test Book",
            AuthorId = author.Id,
            Type = BookType.Novel,
            PublishDate = DateTime.Now,
            Price = 29.99f
        };

        var result = await BookAppService.CreateAsync(createUpdateBookDto);

        result.Id.ShouldNotBeEmpty();
        result.Name.ShouldBe("Test Book");
    }
}