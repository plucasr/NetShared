using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace Shared.Database;

public class JsonParameter : SqlMapper.ICustomQueryParameter
{
    private readonly string _value;
    private readonly NpgsqlDbType _type;

    public JsonParameter(string value, NpgsqlDbType type = NpgsqlDbType.Json)
    {
        _value = value;
        _type = type;
    }

    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = new NpgsqlParameter(name, _type);
        parameter.Value = _value;

        command.Parameters.Add(parameter);
    }
}

public enum DefaultPgJsonType
{
    Object,
    Array,
}
