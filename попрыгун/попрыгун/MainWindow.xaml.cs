using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameMaps.Layouts;
using GameMaps;

namespace попрыгун
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IGameScreenLayout Lay; // Разметка окна (определяет, как именно разместятся карта и область меню)
        UniversalMap_Wpf Map; // Карта
        CellMapInfo MapInfo; // Параметры карты
        List<Platform> Platforms = new List<Platform>();
        TimerController timer = new TimerController();
        double speed = -1;
        int playerX, playerY;
        public MainWindow()
        {

            InitializeComponent();
            // Определяем вид разметки окна: область карты слева и меню справа
            Lay = LayoutsFactory.GetLayout(LayoutType.Vertical, this.Content);

            // Определяем параметры карты: количество клеток по горизонтали и вертикали, размер клетки, 
            // ширина декоративной рамки вокруг карты
            MapInfo = new CellMapInfo(35, 20, 50, 0);

            // Создаем карту и размещаем его в окне программы
            Map = MapCreator.GetUniversalMap(this, MapInfo);
            Lay.Attach(Map, 0);
            //Map.DrawGrid(); // выводим сетку

            // Указываем путь к папке с картинками 
            Map.Library.ImagesFolder = new PathInfo { Path = "..\\..\\..\\..\\i", Type = PathType.Relative };
            // Создаем панель инвентаря и размещаем ее в меню

            addPictures();
            Map.Keyboard.SetSingleKeyEventHandler(checkKey);

            Map.Library.AddContainer("player", "player", ContainerType.AutosizedSingleImage);
            Map.ContainerSetMaxSide("player", 100);
            Map.ContainerSetCoordinate("player", 960, 540);
            playerX = 960;
            playerY = 540;
            Platform.Map = Map;
            Platform platform = new Platform();
            platform.SetCoordinate(960, 600);
            Platforms.Add(platform);

            timer.AddAction(gameCycle, 10);
        }
        void addPictures()
        {
            Map.Library.AddPicture("A", "A.png");
            Map.Library.AddPicture("player", "player.png");
        }

        void gameCycle()
        {
            for (int i = 0; i < Platforms.Count; i++)
            {
                Platforms[i].SetCoordinate(0, (int)speed);
            }

            if(speed > -1)
            {
                speed = speed - 0.05;
            }

            checkPlatform();
        }

        bool checkPlatform ()
        {
            bool onPlatform = false;
            for(int i = 0; i < Platforms.Count;i++)
            {
                if (playerX > Platforms[i].x - 250 && playerX < Platforms[i].x + 250 &&
                    Platforms[i].y - playerY > 58 && Platforms[i].y - playerY < 62)
                {
                    speed = 0;
                }
            }
            return onPlatform;
        }

        void checkKey(Key k)
        {
            if (k == Key.Space)
            {
                speed = 5;
            }

            if (k == Key.Right)
            {
                movePlayer(playerX + 5, playerY);
            }

            if (k == Key.Left)
            {
                movePlayer(playerX - 5, playerY);
            }

            /*if (k == Key.LeftAlt)
            {
                for(int i = 0; i < Platforms.Count; i++)
                {
                    Platforms[i].SetCoordinate(20, 0);
                }
            }
            if (k == Key.RightAlt)
            {
                speed = 5;
            }*/
        }

        void movePlayer (int newX, int newY)
        {
            playerX = newX;
            playerY = newY;

            Map.ContainerSetCoordinate("player", newX, newY);

        }


    }
    class Platform
    {
        public int x;
        public int y;
        public string name;
        static public UniversalMap_Wpf Map;
        static int counter = 0;
        public Platform()
        {
            //создать контейнер и его размер и нерисооооооовать егооооооооооооооооооооо0p
            name = "Platform" + counter;
            counter++;
            Map.Library.AddContainer(name, "A", ContainerType.AutosizedSingleImage);
            Map.ContainerSetMaxSide(name, 500);
            
        }

        public void SetCoordinate(int dx, int dy)
        {
            x = x + dx;
            y = y + dy;
            Map.ContainerSetCoordinate(name, x, y);
        }
    }
}
