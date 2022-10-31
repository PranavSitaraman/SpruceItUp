using LinqToDB.Mapping;
using SpruceItUp.Shared.Models;
namespace SpruceItUp.Backend.Models
{
    [Table("comments")]
    [Column("id", nameof(Id))]
    [Column("author", nameof(Author))]
    [Column("pin", nameof(Pin))]
    [Column("created", nameof(Created))]
    [Column("text", nameof(Text))]
    public record DbComment : Comment { }
}