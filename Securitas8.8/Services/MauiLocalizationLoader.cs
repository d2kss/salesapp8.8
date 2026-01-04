using Securitas8._8.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Services
{
    public class MauiLocalizationLoader : ILocalizationLoader
    {
        public async Task<string> LoadJsonAsync(string language)
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(
                $"Localization/{language}.json");

            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }
}
