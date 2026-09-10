using System;

namespace SmartPantry.Authors;

public class AuthorExcelDto
{
    public required string Name { get; set; }

    public DateTime BirthDate { get; set; }

    public string? ShortBio { get; set; }
}
