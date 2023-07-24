using Kimi.Logging;
using Newtonsoft.Json;

namespace Kimi.Services.Core
{
    public class KimiData
    {
        private Settings _settings;

        public KimiData()
        {
            _settings = new Settings();
        }

        public KimiData(Settings settings)
        {
            _settings = settings;
        }

        public Settings LoadSettings()
        {
            var serializer = new JsonSerializer();

            var path = $@"{Info.AppDataPath}\settings.kimi";

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Info.AppDataPath);
                using (StreamWriter sw = new StreamWriter(path))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    Log.Write("Settings file doesn't exist, creating default file...", Severity.Warning);
                    writer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, _settings);
                }
            }

            using (var sr = new StreamReader(path))
            {
                _settings = (Settings?)serializer.Deserialize(sr, typeof(Settings));
                Log.Write("Settings loaded!");
            }

            if(_settings == null)
                throw new ArgumentNullException(nameof(_settings));

            return _settings;
        }
    }
}
