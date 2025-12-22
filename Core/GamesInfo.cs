using Games_Launcher.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System;

namespace Games_Launcher.Core
{
    public static class GamesInfo
    {
        public readonly static string GAMESDATAFILE = "./games_data.json";
        public readonly static string GAMESDATAFILEOLD = "./games_data_OLD.json";
        public readonly static string GAMESDATAFILECRASH = "./games_data_CRASH.json";
        public const int CURRENTDATAVERSION = 2;

        private static AppModel _appData;
        public static ObservableCollection<GameModel> Games => _appData.Games;

        public static void LoadGamesData()
        {
            if (!File.Exists(GAMESDATAFILE))
            {
                CreateDefaultData();
                return;
            }
            string json = File.ReadAllText(GAMESDATAFILE);

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
                    var oldGames = JsonConvert.DeserializeObject<ObservableCollection<GameModel>>(json);
                    if (oldGames == null)
                        throw new NullReferenceException();

                    _appData = new AppModel
                    {
                        JsonDataVersion = CURRENTDATAVERSION,
                        Games = oldGames
                    };

                    SaveGamesData();
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
            File.WriteAllText(GAMESDATAFILE, json);
        }

        #region Migraciones
        private static void MigrarDatos(JObject jobj, int versionActual)
        {
            //if (versionActual < 1)
            //    MigrarV0aV1();

            //if (versionActual < 2)
            //    MigrarV1aV2();
        }
        #endregion

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
