using FortiCrypts;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Games_Launcher.Model
{
    public class GameModel
    {
        public string Name;
        public string ProcessName;
        public string Parameters;
        public string Path;
        public DateTime LastPlayed;


        [JsonProperty("PlayTime")]
        private string _playTimeEncrypted;

        private TimeSpan _playTime;

        [JsonIgnore]
        public TimeSpan PlayTime
        {
            get => _playTime;
            set
            {
                _playTime = value;

                _playTimeEncrypted = AES256.Encrypt(value.ToString(), CryptoUtils.defaultPassword);
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            if (!string.IsNullOrWhiteSpace(_playTimeEncrypted))
            {
                var t = TimeSpan.Zero;
                if (TimeSpan.TryParse(_playTimeEncrypted, out t))
                {
                    _playTimeEncrypted = AES256.Encrypt(t.ToString(), CryptoUtils.defaultPassword);
                    _playTime = t;
                    return;
                }
                else if (TimeSpan.TryParse(AES256.Decrypt(_playTimeEncrypted, CryptoUtils.defaultPassword), out t))
                {
                    _playTime = t;
                    return;
                }
                _playTime = t;
            }
        }
    }
}
