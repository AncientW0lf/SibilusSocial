namespace Sibilus.Database
{
    /// <summary>
    /// A database column that contains all relevant information for creating, reading, writing and altering database tables.
    /// </summary>
    public readonly struct DbColumn
    {
        public string Name { get; }

        public DbDatatype Datatype { get; }

        public string DefaultValue { get; }

        public bool IsPrimary { get; }

        public bool CanBeNull { get; }

        public DbColumn(string name, DbDatatype datatype, string defaultValue = null, bool isPrimary = false, bool canBeNull = true)
        {
            Name = name;
            Datatype = datatype;
            DefaultValue = defaultValue;
            IsPrimary = isPrimary;
            CanBeNull = canBeNull;
        }

        public DbColumn(string name, DbDatatype datatype, bool isPrimary = false, bool canBeNull = true)
        {
            Name = name;
            Datatype = datatype;
            DefaultValue = null;
            IsPrimary = isPrimary;
            CanBeNull = canBeNull;
        }

        /// <summary>
        /// Converts this object to its string representation used in database table creation.
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Converts this object to its string representation used in database table creation.
        /// </summary>
        public string ToString(bool noPrimary)
        {
            string primarystr = IsPrimary && !noPrimary ? " PRIMARY KEY" : string.Empty;
            string nullstr = CanBeNull ? string.Empty : " NOT NULL";
            string defaultstr = DefaultValue != null ? $" DEFAULT('{DefaultValue}')" : string.Empty;

            return $"{Name} {Datatype}{primarystr}{nullstr}{defaultstr}";
        }
    }
}