using System;

namespace SmartPantry.Books;

public class BookExcelDto
{
    public required string Name { get; set; }

    public required string AuthorName { get; set; }

    public BookType Type { get; set; }

    public DateTime PublishDate { get; set; }

    public float Price { get; set; }
}
