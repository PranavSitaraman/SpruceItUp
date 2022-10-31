using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using SpruceItUp.Backend.Models;
using UserManagement.Models;
using SpruceItUp.Shared.Models;
namespace SpruceItUp.Backend
{
    public class AppDataConnection : DataConnection
    {
        public ITable<Pin> Pins => GetTable<DbPin>();
        public ITable<DbComment> Comments => GetTable<DbComment>();
        public ITable<Loc> Locs => GetTable<DbLoc>();
        public AppDataConnection(LinqToDbConnectionOptions<AppDataConnection> options) : base(options) { }
    }
}