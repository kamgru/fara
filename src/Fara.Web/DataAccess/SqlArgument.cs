using System.Data;

namespace Fara.Web.DataAccess;

public record SqlArgument(string Name, object? Value, DbType DbType);