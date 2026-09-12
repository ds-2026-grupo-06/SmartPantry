using SmartPantry.Books;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications.Books;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task Should_Create_A_Valid_Book()
    {
        // Arrange: Crear un Author primero
        var authorRepository = GetRequiredService<IRepository<Author, Guid>>();
        var author = new Author
        {
            Name = "Test Author"
        };
        await authorRepository.InsertAsync(author);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Act: Crear el Book con el Author válido
        var input = new CreateBookDto
        {
            Title = "Test Book",
            AuthorId = author.Id
        };

        var result = await BookAppService.CreateAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("Test Book");
        result.AuthorId.ShouldBe(author.Id);
    }
}