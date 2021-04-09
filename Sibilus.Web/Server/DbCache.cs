using System.Collections.Generic;
using System;
using Sibilus.Database;
using System.Threading.Tasks;

namespace Sibilus.Web.Server
{
    internal static class DbCache
    {
        public const string Filename = "server.db";

        private static DatabaseClient _dbClient;

        public static DatabaseClient DbClient
        {
            get
            {
                if (_dbClient == null)
                    InitializeDb().GetAwaiter().GetResult();

                return _dbClient;
            }
        }

        public static readonly IReadOnlyDictionary<string, DbColumn[]> DbTables = new Dictionary<string, DbColumn[]>
        {
            {
                "posts", new[]
                {
                    new DbColumn("id", DbDatatype.INT, true, false),
                    new DbColumn("content", DbDatatype.TEXT, "[EMPTY POST]", false, false),
                    new DbColumn("authorId", DbDatatype.INT, (-1).ToString(), false, false),
                    new DbColumn("createdAt", DbDatatype.INT, 0.ToString(), false, false),
                    new DbColumn("tags", DbDatatype.TEXT, false)
                }
            },
            //TODO: Multiple primary keys need a separate syntax
            {
                "postreactions", new[]
                {
                    new DbColumn("reaction", DbDatatype.TEXT, true, false),
                    new DbColumn("byUserId", DbDatatype.INT, (-1).ToString(), true, false),
                    new DbColumn("postId", DbDatatype.INT, false, false)
                }
            },
            {
                "users", new[]
                {
                    new DbColumn("id", DbDatatype.INT, true, false),
                    new DbColumn("emailHash", DbDatatype.TEXT, true, false),
                    new DbColumn("passwordHash", DbDatatype.TEXT, false, false),
                    new DbColumn("username", DbDatatype.TEXT, false, false),
                    new DbColumn("displayname", DbDatatype.TEXT, false, false),
                    new DbColumn("bio", DbDatatype.TEXT, "No information given."),
                    new DbColumn("createdAt", DbDatatype.INT, 0.ToString(), false, false)
                }
            }
        };

        private static async Task InitializeDb()
        {
            if (_dbClient != null)
                return;

            _dbClient = new DatabaseClient(Filename);

            if (!await _dbClient.TestConnectionAsync())
                throw new Exception("Could not connect to the database.");

            foreach (var table in DbTables)
                if (!await _dbClient.TableExistsAsync(table.Key))
                    await _dbClient.CreateTableAsync(table.Key, table.Value);
        }
    }
}
