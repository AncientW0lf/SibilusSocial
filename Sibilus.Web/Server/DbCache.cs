using System.Collections.Generic;
using System;
using Sibilus.Database;

namespace Sibilus.Web.Server
{
    internal static class DbCache
    {
        public const string Filename = "server.db";

        public static DatabaseClient DbClient = new DatabaseClient(Filename);

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
                "reactions", new[]
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
    }
}
