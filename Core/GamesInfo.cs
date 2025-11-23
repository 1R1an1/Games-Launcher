using Games_Launcher.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Games_Launcher.Core
{
    public static class GamesInfo
    {
        public readonly static string GAMESDATAFILE = "./games_data.json";
        public readonly static string GAMESDATAFILEOLD = "./games_data_OLD.json";
        public readonly static string GAMESDATAFILECRASH = "./games_data_CRASH.json";
        public static ObservableCollection<Game> Games = new ObservableCollection<Game>();

        public static void LoadGamesData()
        {
            if (File.Exists(GAMESDATAFILE))
            {
                //string encryptedJson = File.ReadAllText(GAMESDATAFILE);
                string json = File.ReadAllText(GAMESDATAFILE);
                //string json = AES256.Decrypt(encryptedJson, CryptoUtils.defaultPassword);
                ObservableCollection<Game> deJson;
                try { deJson = JsonConvert.DeserializeObject<ObservableCollection<Game>>(json); }
                catch { deJson = null; }
                if (deJson == null)
                {
                    if (MessageBox.Show("El archivo de guardado esta corrrupto, se usara un archivo antiguo como respaldo, quiere guardar el archivo corrupto?", "Advertencia", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        File.WriteAllText(GAMESDATAFILECRASH, json);
                        Process.Start("explorer.exe", "/select,\"" + Path.GetFullPath(GAMESDATAFILECRASH) + "\"");
                    }
                    json = File.ReadAllText(GAMESDATAFILEOLD);
                    File.WriteAllText(GAMESDATAFILE, json);
                    Games = JsonConvert.DeserializeObject<ObservableCollection<Game>>(json);

                }
                else
                    Games = deJson;

            }
        }
        

        public static void SaveGamesData()
        {
            string oldContent = File.ReadAllText(GAMESDATAFILE);
            File.WriteAllText(GAMESDATAFILEOLD, oldContent);

            string json = JsonConvert.SerializeObject(Games, Formatting.Indented);
            File.WriteAllText(GAMESDATAFILE, json);
            //string encryptedJson = AES256.Encrypt(json, CryptoUtils.defaultPassword);
        }
    }
}
