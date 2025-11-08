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
        private static readonly object _lock = new Object();
        private static bool _stopRequested = false;
        public static event Action ReadyToClose;
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

            if (!_isRunning)
            {
                StartLoop();
            }
        }

        public static void Unregister(GameView view)
        {
            lock (_lock)
            {
                _views.Remove(view);
            }
        }

        public static void RequestStop()
        {
            _stopRequested = true;
        }

        private static void StartLoop()
        {

            _ = Task.Run(async () =>
            {
                _isRunning = true;

                while (true)
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
                                view.BTNJugar.Margin = new Thickness(0, 0, 12, 0);
                                view.BTNJugar.Content = "DETENER";
                                view.BTNJugar.Tag = view.FindResource("DownloadColorNormal");
                                view.BTNJugar.BorderBrush = (Brush)view.FindResource("DownloadColorMouseOver");
                            });

                            runingGames.Add(view);
                            view.IsRunning = true;
                            view.starterTime = DateTime.Now;
                        }
                        else if (!currentlyRunning && view.IsRunning)
                        {
                            view.Dispatcher.Invoke(() =>
                            {
                                view.BTNJugar.Margin = new Thickness(0, 0, 23, 0);
                                view.BTNJugar.Content = "JUGAR";
                                view.BTNJugar.Tag = view.FindResource("JugarColorNormal");
                                view.BTNJugar.BorderBrush = (Brush)view.FindResource("JugarColorMouseOver");
                            });

                            runingGames.Remove(view);
                            view.IsRunning = false;
                            TimeSpan duration = DateTime.Now - view.starterTime;
                            view.thisGame.PlayTime += duration;

                            view.Dispatcher.Invoke(() => view.LBLTimeOppend.Content = GameFunctions.ConvertTime(view.thisGame.PlayTime));
                        }
                        //if (_stopRequested && (currentlyRunning && view.IsRunning))
                        //{
                        //    runingGames--;
                        //    view.thisGame.PlayTime += (DateTime.Now - view.starterTime);
                        //    //ReadyToClose?.Invoke();
                        //    //_isRunning = false;
                        //}
                        //if (_stopRequested && runingGames == 0)
                        //{
                        //    _isRunning = false;
                        //    ReadyToClose?.Invoke();
                        //}
                    }

                    
                    await Task.Delay(3000);
                }

                //ReadyToClose?.Invoke();
            });
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (_stopRequested && runingGames.Count == 0)
                    {
                        _isRunning = false;
                        ReadyToClose?.Invoke();
                        break;
                    }
                    else if (_stopRequested && runingGames.Count > 0)
                    {
                        foreach (var item in runingGames)
                        {
                            item.thisGame.PlayTime += (DateTime.Now - item.starterTime);
                        }
                        runingGames.Clear();
                    }
                    await Task.Delay(500);
                }
            });
        }
    }
}
