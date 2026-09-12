using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;
using SmartPantry.Authors;
using SmartPantry.Books;
using SmartPantry.Application.Contracts.Books;

namespace SmartPantry.Books;

public abstract class BookAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IBookAppService _bookAppService;

    protected BookAppService_Tests()
    {
        _bookAppService = GetRequiredService<IBookAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Books()
    {
        //Act
        var result = await _bookAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "1984");
    }

    [Fact]
    public async Task Should_Create_A_Valid_Book()
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

    [Fact]
    public async Task Should_Not_Create_A_Book_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _bookAppService.CreateAsync(
                new CreateUpdateBookDto
                {
                    Name = "",
                    Price = 10,
                    PublishDate = DateTime.Now,
                    Type = BookType.ScienceFiction
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }
}