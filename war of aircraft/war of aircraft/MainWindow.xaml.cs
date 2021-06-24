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
using GameMaps;
using GameMaps.Layouts;

namespace war_of_aircraft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int counter = 0;

        IGameScreenLayout Lay; // Разметка окна (определяет, как именно разместятся карта и область меню)
        UniversalMap_Wpf Map; // Карта
        CellMapInfo MapInfo; // Параметры карты
        InventoryPanel hppanel1; // Панель предметов
        InventoryPanel hppanel2; // Панель предметов
        InventoryPanel Items1; // Панель предметов
        InventoryPanel Items2; // Панель предметов
        GameObject player;
        TimerController timer = new TimerController();
        List<GameObject> bullets = new List<GameObject>();
        TextArea_Vertical Info; // Место для текстовой информации
        Random r = new Random();
        List<GameObject> misha = new List<GameObject>();
        MediaPlayer Audio = new MediaPlayer();
        public MainWindow()
        {
            #region Подготовка карты и панели меню
            InitializeComponent();
            // Определяем вид разметки окна: область карты слева и меню справа
            Lay = LayoutsFactory.GetLayout(LayoutType.Vertical, this.Content);

            // Определяем параметры карты: количество клеток по горизонтали и вертикали, размер клетки, 
            // ширина декоративной рамки вокруг карты
            MapInfo = new CellMapInfo(38, 20, 50, 0);

            // Создаем карту и размещаем его в окне программы
            Map = MapCreator.GetUniversalMap(this, MapInfo);
            Lay.Attach(Map, 0);
            //Map.DrawGrid(); // выводим сетку

            // Указываем путь к папке с картинками 
            Map.Library.ImagesFolder = new PathInfo { Path = "..\\..\\..\\..\\i", Type = PathType.Relative };
            // Создаем панель инвентаря и размещаем ее в меню
            hppanel1 = new InventoryPanel(Map.Library, 150);
            hppanel2 = new InventoryPanel(Map.Library, 150);
            Items1 = new InventoryPanel(Map.Library, 50);
            Items2 = new InventoryPanel(Map.Library, 50);
            Lay.Attach(hppanel1, 1);
            Lay.Attach(Items1, 1);
            Lay.Attach(hppanel2, 1);
            Lay.Attach(Items2, 1);
            hppanel1.SetBackground(Brushes.Wheat);

            // Создаем текстовую панель и размещаем ее в меню
            Info = new TextArea_Vertical();
            Lay.Attach(Info, 1);

            // определяем функцию, которая будет вызвана при нажатии на клавишу
            //Map.Keyboard.SetSingleKeyEventHandler(CheckKey);
            #endregion
            player = new GameObject();
            player.angle = 45;
            player.x = 50;
            player.y = 50;
            player.speed = 3;
            player.reload = 1000;
            player.hp = 10;
            addPictures();
            //player.Name = 
            Map.SetMapBackground("map");
            Name Start = new Name();
            Start.player = player;
            Start.ShowDialog();
            timer.AddAction(BCE, 10);
            timer.AddAction(mishen, 30000);
        }

        void addPictures()
        {
            Map.Library.AddPicture("map", "map.png");
            Map.Library.AddPicture("player", "player.png");
            Map.Library.AddContainer("player", "player", ContainerType.AutosizedSingleImage);
            player.container = "player";
            Map.ContainerSetMaxSide("player", 150);
            Map.ContainerSetCoordinate("player", player.x, player.y);
            Map.ContainerSetAngle("player", player.angle);
            Map.Library.AddPicture("bullet", "bullet.png");
            Map.Library.AddPicture("mishen", "misha.png");
            Map.Library.AddPicture("GO", "Game over.png");
            AnimationDefinition animation = new AnimationDefinition();
            for (int i = 0; i < 10; i++)
            {
                Map.Library.AddPicture("exp" + i, "exp" + i + ".png");
                animation.AddFrame(100, "exp" + i);
            }
            Map.Library.AddAnimation("explosion", animation);
            Map.Library.AddPicture("0", "exp10.png");
            animation.AddFrame(10, "0");
            animation.LastFrame = "0";
        }

        void BCE()
        {
            if (Map.Keyboard.IsKeyPressed(Key.A))
            {
                player.angle = player.angle - 2;
                Map.ContainerSetAngle("player", player.angle);
                player.dx = player.speed * Math.Cos(GameMath.DegreesToRad(player.angle));
                player.dy = player.speed * Math.Sin(GameMath.DegreesToRad(player.angle));
            }

            if (Map.Keyboard.IsKeyPressed(Key.D))
            {
                player.angle = player.angle + 2;
                Map.ContainerSetAngle("player", player.angle);
                player.dx = player.speed * Math.Cos(GameMath.DegreesToRad(player.angle));
                player.dy = player.speed * Math.Sin(GameMath.DegreesToRad(player.angle));
            }

            if (Map.Keyboard.IsKeyPressed(Key.W))
            {
                player.x += player.dx;
                player.y += player.dy;
                Map.ContainerSetCoordinate("player", player.x, player.y);
            }

            if (Map.Keyboard.IsKeyPressed(Key.S))
            {
                player.x -= player.dx / 3;
                player.y -= player.dy / 3;
                Map.ContainerSetCoordinate("player", player.x, player.y);
            }

            if (player.hp <= 0)
            {
                Map.Library.AddContainer("GO", "GO");
                Map.ContainerSetCoordinate("GO", Map.XAbsolute / 2, Map.YAbsolute / 2);
                Map.ContainerSetSize("GO", Map.XAbsolute, Map.YAbsolute);
                timer.RemoveAction(BCE, 10);
                timer.RemoveAction(mishen, 30000);
            }

            shoot();
            MoveShots();
            CheckHit();
            ReloadMine();

            player.reload += 50;
        }

        void shoot()
        {
            if (Map.Keyboard.IsKeyPressed(Key.Space) && player.reload >= 1000)
            {
                GameObject bullet = new GameObject();
                bullet.x = player.x;
                bullet.y = player.y;
                bullet.angle = player.angle;
                bullet.container = "bullet" + counter;
                bullet.speed = player.speed * 5;

                bullet.dx = bullet.speed * Math.Cos(GameMath.DegreesToRad(bullet.angle));
                bullet.dy = bullet.speed * Math.Sin(GameMath.DegreesToRad(bullet.angle));

                Map.Library.AddContainer(bullet.container, "bullet", ContainerType.AutosizedSingleImage);
                Map.ContainerSetMaxSide(bullet.container, 30);
                Map.ContainerSetCoordinate(bullet.container, player.x, player.y);
                Map.ContainerSetAngle(bullet.container, player.angle);
                counter = counter + 1;

                bullets.Add(bullet);

                player.reload = 0;
            }
        }
        void MoveShots()
        {
            for(int i = 0; i < bullets.Count; i++)
            {
                if (bullets[i].x >= 0 && bullets[i].y >= 0 && bullets[i].x <= 1900 && bullets[i].y <= 1000)
                {
                    bullets[i].x = bullets[i].x + bullets[i].dx;
                    bullets[i].y = bullets[i].y + bullets[i].dy;
                    Map.ContainerSetCoordinate(bullets[i].container, bullets[i].x, bullets[i].y);
                }

                else
                {
                    Map.ContainerErase(bullets[i].container);
                    bullets.RemoveAt(i);
                }
            }
        }

        void mishen()
        {
            int x, y;
            string container = "target" + counter.ToString();

            x = r.Next(0, Map.XAbsolute);
            y = r.Next(0, Map.YAbsolute);

            Map.Library.AddContainer(container, "mishen", ContainerType.AutosizedSingleImage);
            Map.ContainerSetMaxSide(container, 50);
            Map.ContainerSetCoordinate(container, player.x, player.y);
            Map.ContainerSetAngle(container, player.angle + 90);
            counter = counter + 1;
            GameObject mine = new GameObject();
            mine.x = player.x;
            mine.y = player.y;
            mine.container = container;
            misha.Add(mine);
        }

        void ReloadMine()
        {
            for(int i = 0; i < misha.Count; i++)
            {
                if (misha[i].reload >= 3000)
                {
                    if (Map.CollisionContainers(player.container, misha[i].container))
                    {
                        Map.ContainerSetMaxSide(misha[i].container, 150);
                        Map.AnimationStart(misha[i].container, "explosion", 1);
                        misha[i].reload = -1000000000;
                        string FileName = "C:\\git\\labirint\\war of aircraft\\3ByKN\\BUM.flac";
                        Audio.Open(new Uri(FileName));
                        Audio.Play();
                        misha.RemoveAt(i);
                        player.hp = player.hp - 5.0;
                    }
                }

                else
                {
                    misha[i].reload = misha[i].reload + 10;
                }
            }

            /*
             * 1 с помощю фор смотрим мины
             * 2 в цыкле
             *   1проверяем вкл каждую мину
             *   2вкл =
             *     1взаимодействие с игроком
             *       1бах
             *   3+ заряд
             */
        }

        void CheckHit()
        {
            for(int i = 0; i < bullets.Count; i++)
            {
                for(int j = 0; j < misha.Count; j++)
                {
                    if (Map.CollisionContainers(bullets[i].container, misha[j].container))
                    {
                        Map.ContainerErase(misha[j].container);
                        misha.RemoveAt(j);
                        Map.ContainerErase(bullets[i].container);
                        bullets.RemoveAt(i);

                        break;
                    }
                }
            }
        }
    }
    public class GameObject
    {
        public string Name;
        public double x;
        public double y;
        public int angle;
        public double speed;
        public double dx;
        public double dy;
        public string container;
        public int reload;
        public double hp;
        public void getDamage(string damageType, double damage)
        {
            //hp = hp - 
        }
    }
}
