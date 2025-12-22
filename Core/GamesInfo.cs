using FortiCrypts;
using Games_Launcher.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Games_Launcher.Core
{
    public static class GamesInfo
    {
        public readonly static string GAMESDATAFILE = "./games_data.dat";
        public readonly static string GAMESDATAFILEOLD = "./games_data_OLD.dat";
        public readonly static string GAMESDATAFILECRASH = "./games_data_CRASH.json";
        public const int CURRENTDATAVERSION = 3;

        private static AppModel _appData;
        public static ObservableCollection<GameModel> Games => _appData.Games;

        public static void LoadGamesData()
        {
            if (!File.Exists(GAMESDATAFILE))
            {
                CreateDefaultData();
                return;
            }
            string jsonEncrypted = File.ReadAllText(GAMESDATAFILE);
            string json = "";
            try { json = AES256.Decrypt(jsonEncrypted, CryptoUtils.defaultPassword); } catch { json = jsonEncrypted; }

            try
            {
                var jobject = JObject.Parse(json);
                int jsonVersion = jobject[nameof(AppModel.JsonDataVersion)].Value<int>();
                //Migrar datos si el JSON es viejo
                if (jsonVersion < CURRENTDATAVERSION)
                {
                    MigrarDatos(jobject, jsonVersion);
                    SaveGamesData();
                    return;
                }
                else if (jsonVersion > CURRENTDATAVERSION)
                {
                    DowngradeWarning();
                    return;
                }

                _appData = JsonConvert.DeserializeObject<AppModel>(json);
                if (_appData == null || _appData.Games == null)
                    throw new NullReferenceException();
            }
            catch
            {
                //Intentar cargar solo la lista de juegos
                try
                {
                    var gamesArray = JArray.Parse(json);
                    var jobj = new JObject
                    {
                        [nameof(AppModel.JsonDataVersion)] = 2,
                        [nameof(AppModel.Games)] = gamesArray
                    };

                    SaveGamesData(jobj.ToString(Formatting.Indented));
                    LoadGamesData();
                    return;
                }
                catch { ManageCorruptedFile(json); }
            }

        }
        public static void SaveGamesData()
        {
            if (_appData == null)
                return;

            if (File.Exists(GAMESDATAFILE))
                File.Copy(GAMESDATAFILE, GAMESDATAFILEOLD, true);

            _appData.JsonDataVersion = CURRENTDATAVERSION;

            string json = JsonConvert.SerializeObject(_appData, Formatting.Indented);
            string jsonEncrypted = AES256.Encrypt(json, CryptoUtils.defaultPassword);
            File.WriteAllText(GAMESDATAFILE, jsonEncrypted);
        }

        private static void SaveGamesData(object obj)
        {
            if (obj == null)
                return;

            if (File.Exists(GAMESDATAFILE))
                File.Copy(GAMESDATAFILE, GAMESDATAFILEOLD, true);

            string objEncrypted = AES256.Encrypt(obj.ToString(), CryptoUtils.defaultPassword);
            File.WriteAllText(GAMESDATAFILE, objEncrypted);
        }

        #region Migraciones
        private static void MigrarDatos(JObject jobj, int jsonDataVersion)
        {
            if (jsonDataVersion < 3)
                MigrarV2aV3(jobj);

            _appData = jobj.ToObject<AppModel>();
        }

        private static void MigrarV2aV3(JObject jobj)
        {
            var games = jobj[nameof(AppModel.Games)] as JArray;
            if (games == null)
                return;

            foreach (var item in games)
            {
                var game = item as JObject;
                if (game == null)
                    continue;

                TimeSpan playTime = TimeSpan.Zero;
                var ptGame = game[nameof(GameModel.PlayTime)];
                try
                {
                    var decrypted = AES256.Decrypt(ptGame.Value<string>(), CryptoUtils.defaultPassword);
                    TimeSpan.TryParse(decrypted, out playTime);
                }
                catch { try { TimeSpan.TryParse(ptGame.Value<string>(), out playTime); } catch { } }

                game[nameof(GameModel.PlayTime)] = playTime;
            }
        }
        #endregion

        public static void CheckFileNames()
        {
            string oldPath = Path.ChangeExtension(GAMESDATAFILE, ".json");
            if (File.Exists(oldPath) && !File.Exists(GAMESDATAFILE))
                File.Move(oldPath, GAMESDATAFILE);

            oldPath = Path.ChangeExtension(GAMESDATAFILEOLD, ".json");
            if (File.Exists(oldPath) && !File.Exists(GAMESDATAFILEOLD))
                File.Move(oldPath, GAMESDATAFILEOLD);
        }
        private static void CreateDefaultData()
        {
            _appData = new AppModel
            {
                JsonDataVersion = CURRENTDATAVERSION,
                Games = new ObservableCollection<GameModel>()
            };
        }

        private static void DowngradeWarning()
        {
            var msg = MessageBox.Show("El archivo de guardado fue creado con una versión más reciente de la aplicación.\n" +
                                              "Para usar la app actual, se requiere crear un nuevo archivo de guardado.\n" +
                                              "¿Desea borrar los datos y continuar?", "Versión de guardado incompatible", MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (msg == MessageBoxResult.Yes)
            {
                CreateDefaultData();
                SaveGamesData();
            }
            else
                Environment.Exit(0);
            
        }

        private static void ManageCorruptedFile(string json)
        {
            var msg = MessageBox.Show("El archivo de guardado esta corrrupto, se usara un archivo de respaldo, quiere guardar el archivo corrupto?", "Advertencia", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            if (msg == MessageBoxResult.Yes)
            {
                File.WriteAllText(GAMESDATAFILECRASH, json);
                Process.Start("explorer.exe", "/select,\"" + Path.GetFullPath(GAMESDATAFILECRASH) + "\"");
            }
            else if (msg == MessageBoxResult.Cancel)
                Environment.Exit(0);

            if (File.Exists(GAMESDATAFILEOLD))
            {
                File.Copy(GAMESDATAFILEOLD, GAMESDATAFILE, true);
                LoadGamesData();
            }
            else
            {
                MessageBox.Show("No se encontro archivo de respaldo, se creara un nuevo archivo de guardado.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                CreateDefaultData();
            }
        }

    }
}
