using System.Timers;
using System.Collections.Generic;
using System;
using Sibilus.Database;
using System.Threading.Tasks;

namespace Sibilus.Web.Server
{
    internal static class DbCache
    {
        public const string Filename = "server.db";

        internal static readonly IReadOnlyDictionary<string, DbColumn[]> DbTables = new Dictionary<string, DbColumn[]>
        {
            {
                "posts", new[]
                {
                    new DbColumn("id", DbDatatype.INT, true, false),
                    new DbColumn("content", DbDatatype.TEXT, "[EMPTY POST]", false, false),
                    new DbColumn("userId", DbDatatype.INT, (-1).ToString(), false, false),
                    new DbColumn("createdAt", DbDatatype.INT, 0.ToString(), false, false),
                    new DbColumn("tags", DbDatatype.TEXT, false)
                }
            },
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
            },
            {
                "sessions", new[]
                {
                    new DbColumn("id", DbDatatype.TEXT, true, false),
                    new DbColumn("userId", DbDatatype.INT, (-1).ToString(), false, false),
                    new DbColumn("expires", DbDatatype.INT, 0.ToString())
                }
            }
        };

        public static DatabaseClient DbClient = InitializeDb().GetAwaiter().GetResult();

        internal static readonly Timer SessionClearer = new Timer
        {
            AutoReset = true,
            Interval = TimeSpan.FromMinutes(5).TotalMilliseconds
        };

        private static readonly TimeSpan SessionLifespan = TimeSpan.FromDays(7);

        private static async Task<DatabaseClient> InitializeDb()
        {
            var client = new DatabaseClient(Filename);

            if (!await client.TestConnectionAsync())
                throw new Exception("Could not connect to the database.");

            foreach (var table in DbTables)
                if (!await client.TableExistsAsync(table.Key))
                    await client.CreateTableAsync(table.Key, table.Value);

            SessionClearer.Elapsed += async (_, _) => await ClearExpiredSessions();
            SessionClearer.Start();

            return client;
        }

        private static async Task ClearExpiredSessions()
        {
            int deleted = await DbClient.DeleteAsync("sessions", $"(expires + {SessionLifespan.Ticks}) < {DateTime.UtcNow.Ticks}");

            if (deleted > 0)
                Console.WriteLine($"Deleted {deleted} expired sessions");
        }
    }
}
