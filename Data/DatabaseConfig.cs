using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data
{
    public static class DatabaseConfig
    {
        public static string DbPath =>
            Path.Combine(FileSystem.AppDataDirectory, "securitas.db");

        public static SqliteConnection GetConnection() =>
            new($"Data Source={DbPath}");
    }
}
