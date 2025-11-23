using Games_Launcher.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Games_Launcher.Core
{
    public static class GameMonitor
    {
        private static readonly List<GameView> _views = new List<GameView>();
        private static bool _isRunning = false;
        private static readonly object _lock = new object();
        private static List<GameView> runingGames = new List<GameView>();

        /// <summary>
        /// Agrega un nuevo GameView a la lista de monitoreo.
        /// </summary>
        public static void Register(GameView view)
        {
            lock (_lock)
            {
                _views.Add(view);
            }
        }

        public static void Unregister(GameView view)
        {
            lock (_lock)
            {
                _views.Remove(view);
            }
        }

        public static void StartLoop()
        {

            _ = Task.Run(async () =>
            {
                _isRunning = true;

                while (_isRunning)
                {
                    GameView[] currentViews;

                    // Evitamos modificar la lista mientras se recorre
                    lock (_lock)
                    {
                        currentViews = _views.ToArray();
                    }

                    foreach (var view in currentViews)
                    {
                        view._gameProcess = Process.GetProcessesByName(view.thisGame.ProcessName);
                        bool currentlyRunning = view._gameProcess.Length > 0;

                        if (currentlyRunning && !view.IsRunning)
                        {
                            view.Dispatcher.Invoke(() =>
                            {
                                view.BTNJugar.Margin = new Thickness(22.3, 0, 11.2, 0);
                                view.BTNJugar.Content = "DETENER";
                                view.BTNJugar.Tag = view.FindResource("DownloadColorNormal");
                                view.BTNJugar.BorderBrush = (Brush)view.FindResource("DownloadColorMouseOver");
                            });

                            runingGames.Add(view);
                            view.IsRunning = true;
                            view.thisGame.LastPlayed = DateTime.Now;
                            view.Dispatcher.Invoke(() => view.LBLLastOppend.Content = GameFunctions.UltimaVezJugado(view.thisGame.LastPlayed));
                        }
                        else if (!currentlyRunning && view.IsRunning)
                        {
                            view.Dispatcher.Invoke(() =>
                            {
                                view.BTNJugar.Margin = new Thickness(31.65, 0, 20.55, 0);
                                view.BTNJugar.Content = "JUGAR";
                                view.BTNJugar.Tag = view.FindResource("JugarColorNormal");
                                view.BTNJugar.BorderBrush = (Brush)view.FindResource("JugarColorMouseOver");
                            });

                            runingGames.Remove(view);
                            view.IsRunning = false;
                            TimeSpan duration = DateTime.Now - view.thisGame.LastPlayed;
                            view.thisGame.PlayTime += duration;

                            view.Dispatcher.Invoke(() => view.LBLTimeOppend.Content = GameFunctions.ConvertTime(view.thisGame.PlayTime));
                        }
                    }


                    await Task.Delay(3000);
                }
            });

            App.window.CloseEvent += CloseEvent;
        }

        private static void CloseEvent()
        {
            if (runingGames.Count > 0)
            {
                _isRunning = false;
                foreach (var item in runingGames)
                {
                    item.thisGame.PlayTime += (DateTime.Now - item.thisGame.LastPlayed);
                }
                runingGames.Clear();
            }
        }
    }
}
