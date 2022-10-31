using LinqToDB.Mapping;
using SpruceItUp.Shared.Models;
namespace SpruceItUp.Backend.Models
{
    [Table("locs")]
    [Column("id", nameof(Id))]
    [Column("author", nameof(Author))]
    [Column("title", nameof(Title))]
    [Column("lat", nameof(Lat))]
    [Column("lng", nameof(Lng))]
    [Column("kind", nameof(Kind))]
    [Column("expires", nameof(Expires))]
    [Column("created", nameof(Created))]
    [Column("image", nameof(Image))]
    [Column("description", nameof(Description))]
    public record DbLoc : Loc { }
}