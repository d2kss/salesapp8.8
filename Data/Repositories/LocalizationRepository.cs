using Securitas8._8.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data.Repositories
{
    public class LocalizationRepository : IlocalizationRepository
    {

        public string? LoadJson(string languageCode)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT JsonPayload
            FROM LocalizationJson
            WHERE LanguageCode = @lang";
                cmd.Parameters.AddWithValue("@lang", languageCode);

            return cmd.ExecuteScalar()?.ToString();
        }
    }
}
