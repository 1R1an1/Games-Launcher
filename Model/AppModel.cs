using System.Collections.ObjectModel;

namespace Games_Launcher.Model
{
    public class AppModel
    {
        public int JsonDataVersion;
        public ObservableCollection<GameModel> Games;
    }
}
