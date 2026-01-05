using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Services.Interface
{
    public interface ILocalizationLoader
    {
        Task<string> LoadJsonAsync(string language);
    }
}
