using System;
using System.Threading.Tasks;
using SmartPantry.Authors;
using SmartPantry.Books;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Application.Tests.Books;

public abstract class BookAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : class
{
    protected IRepository<Author, Guid> AuthorRepository { get; set; }
    protected IRepository<Book, Guid> BookRepository { get; set; }
    protected IBookAppService BookAppService { get; set; }

    protected virtual async Task Should_Create_A_Valid_Book()
    {
        // Arrange: Crear un Author
        var author = new Author
        {
            Name = "Test Author"
        };
        await AuthorRepository.InsertAsync(author);

        // Act: Crear el Book con Author válido
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