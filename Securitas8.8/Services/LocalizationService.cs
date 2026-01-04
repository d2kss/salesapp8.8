using Securitas8._8.Services.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Securitas8._8.Services
{
    public class LocalizationService
    {
        private readonly ILocalizationLoader _loader;
        private Dictionary<string, string> _strings = new();
        public event Action? OnChange;
        public CultureInfo Culture { get; private set; }
            = CultureInfo.InvariantCulture;

        public LocalizationService(ILocalizationLoader loader)
        {
            _loader = loader;
        }

        public string this[string key]
            => _strings.TryGetValue(key, out var v) ? v : key;

        public async Task LoadAsync(string language)
        {
            var json = await _loader.LoadJsonAsync(language);

            _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;

            Culture = language switch
            {
                "fr" => new CultureInfo("fr-FR"),
                _ => new CultureInfo("en-US")
            };

            CultureInfo.DefaultThreadCurrentCulture = Culture;
            CultureInfo.DefaultThreadCurrentUICulture = Culture;

            NotifyStateChanged();
        }
        public void NotifyStateChanged()
        {
            try
            {
                // Safely invoke OnChange on the UI thread
                var syncContext = SynchronizationContext.Current;
                if (syncContext != null)
                {
                    syncContext.Post(_ => OnChange?.Invoke(), null);
                }
                else
                {
                    // fallback if no synchronization context (e.g., during refresh)
                    OnChange?.Invoke();
                }
            }
            catch
            {
                // optionally ignore errors when component disposed
            }
        }

        public string Currency(decimal value)
            => string.Format(Culture, "{0:C}", value);

        public string Number(decimal value)
            => value.ToString("N", Culture);

        public string Date(DateTime value)
            => value.ToString("d", Culture);
    }
}
