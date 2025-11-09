using Games_Launcher.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Games_Launcher.Core
{
    public static class GamesInfo
    {
        public readonly static string GAMESDATAFILE = "./games_data.json";
        public static List<Game> Games = new List<Game>();

        public static void LoadGamesData()
        {
            if (File.Exists(GAMESDATAFILE))
            {
                //string encryptedJson = File.ReadAllText(GAMESDATAFILE);
                string json = File.ReadAllText(GAMESDATAFILE);
                //string json = AES256.Decrypt(encryptedJson, CryptoUtils.defaultPassword);
                Games = JsonConvert.DeserializeObject<List<Game>>(json);
            }
        }

        public static void SaveGamesData()
        {
            string json = JsonConvert.SerializeObject(Games, Formatting.Indented);
            //string encryptedJson = AES256.Encrypt(json, CryptoUtils.defaultPassword);
            File.WriteAllText(GAMESDATAFILE, json);
        }
    }
}
