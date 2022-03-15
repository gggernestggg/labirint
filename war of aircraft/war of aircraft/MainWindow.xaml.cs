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
        InventoryPanel hppanel; // Панель предметов
        //InventoryPanel hppanel2; // Панель предметов
        //InventoryPanel Items1; // Панель предметов
        //InventoryPanel Items2; // Панель предметов
        GameObject player;
        TimerController timer = new TimerController();
        List<GameObject> bullets = new List<GameObject>();
        TextArea_Vertical Info; // Место для текстовой информации
        Random r = new Random();
        List<GameObject> misha = new List<GameObject>();
        MediaPlayer Audio = new MediaPlayer();
        List<GameObject> enemies = new List<GameObject>();
        List<GameObject> enemyBull = new List<GameObject>();//список снарядов враговp
        List<GameObject> heals = new List<GameObject>();//cпписок аптек
        List<GameObject> box = new List<GameObject>();//cпписок аптек


        int expEnemy;
        int expMine;

        public MainWindow()
        {
            #region Подготовка карты и панели меню
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
            hppanel = new InventoryPanel(Map.Library, 150);
            //hppanel2 = new InventoryPanel(Map.Library, 150);
            //Items1 = new InventoryPanel(Map.Library, 50);
            //Items2 = new InventoryPanel(Map.Library, 50);
            Lay.Attach(hppanel, 1);
            //Lay.Attach(Items1, 1);
            //Lay.Attach(hppanel2, 1);
            //Lay.Attach(Items2, 1);
            hppanel.SetBackground(Brushes.Wheat);

            // Создаем текстовую панель и размещаем ее в меню
            Info = new TextArea_Vertical();
            Lay.Attach(Info, 1);
            Info.AddTextBlock("expEnemy");
            Info.AddTextBlock("expMine");

            // определяем функцию, которая будет вызвана при нажатии на клавишу
            //Map.Keyboard.SetSingleKeyEventHandler(CheckKey);
            #endregion
            player = new GameObject();
            player.y = 50;
            player.speed = 3;
            player.reload = 100000;
            player.ammo = 40;
            player.hp = 10;
            player.InventoryPanel = hppanel;
            addPictures();
            //player.Name = 
            Map.SetMapBackground("map");
            Name Start = new Name();
            Start.player = player;
            Start.ShowDialog();
            timer.AddAction(BCE, 10);
            timer.AddAction(mishen, 30000);
            timer.AddAction(spaunEnemy, 3000);
            timer.AddAction(sudba, 60000);

            hppanel.AddItem("hp", "hp10");
            hppanel.AddItem("box", "box", player.ammo.ToString());
        }

        void addPictures()
        {
            Map.Library.AddPicture("map", "map.jpg");
            Map.Library.AddPicture("player", "player.png");
            Map.Library.AddContainer("player", "player", ContainerType.AutosizedSingleImage);
            player.container = "player";
            Map.ContainerSetMaxSide("player", 150);
            Map.ContainerSetCoordinate("player", player.x, player.y);
            Map.ContainerSetAngle("player", player.angle);
            Map.Library.AddPicture("bullet", "bullet.png");
            Map.Library.AddPicture("mishen", "misha.png");
            Map.Library.AddPicture("GO", "Game over.png");
            Map.Library.AddPicture("enemy", "enemy.png");
            Map.Library.AddPicture("enemyShoot", "enemyShoot.png");
            Map.Library.AddPicture("heal", "heal.png");
            Map.Library.AddPicture("box", "box.png");
            AnimationDefinition animation = new AnimationDefinition();
            for (int i = 0; i < 10; i++)
            {
                Map.Library.AddPicture("exp" + i, "exp" + i + ".png");
                animation.AddFrame(100, "exp" + i);
            }

            for (int i = 0; i < 11; i++)
            {
                Map.Library.AddPicture("hp" + i, "hp" + i + ".png");
            }
            Map.Library.AddAnimation("explosion", animation);
            Map.Library.AddPicture("0", "exp10.png");
            animation.AddFrame(10, "0");
            animation.LastFrame = "0";
        }

        void BCE()
        {
            chekKey();
            shoot();
            MoveShots();
            CheckPlayerHit();
            ReloadMine();
            enemyShoot();
            reloadEnemy();
            reloadPlayer();
            GO();
            getHeal();
            getAmmo();
        }

        void chekKey()
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
        }

        void GO()
        {
            if (player.hp <= 0)
            {
                Map.Library.AddContainer("GO", "GO");
                Map.ContainerSetCoordinate("GO", Map.XAbsolute / 2, Map.YAbsolute / 2);
                Map.ContainerSetSize("GO", Map.XAbsolute, Map.YAbsolute);
                timer.RemoveAction(BCE, 10);
                timer.RemoveAction(mishen, 30000);
            }
        }

    void reloadPlayer()
        {
            player.reload += 50;
        }

        void reloadEnemy()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].reload += 100;
            }
        }

        void shoot()
        {
            if (Map.Keyboard.IsKeyPressed(Key.Space) && player.reload >= 1000 && player.ammo > 0)
            {
                string FileName = "C:\\git\\labirint\\war of aircraft\\3ByKN\\shot.mp3";
                Audio.Open(new Uri(FileName));
                Audio.Play();
                player.ammo--;
                hppanel.SetText("box", player.ammo.ToString());

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

            for (int i = 0; i < enemyBull.Count; i++)
            {
                if (enemyBull[i].x >= 0 && enemyBull[i].y >= 0 && enemyBull[i].x <= 1900 && enemyBull[i].y <= 1000)
                {
                    enemyBull[i].x = enemyBull[i].x + enemyBull[i].dx;
                    enemyBull[i].y = enemyBull[i].y + enemyBull[i].dy;
                    Map.ContainerSetCoordinate(enemyBull[i].container, enemyBull[i].x, enemyBull[i].y);
                }

                else
                {
                    Map.ContainerErase(enemyBull[i].container);
                    enemyBull.RemoveAt(i);
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
                        player.getDamage("", 5);
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

        void CheckPlayerHit()
        {
            for(int i = 0; i < bullets.Count; i++)
            {
                for(int j = 0; j < misha.Count; j++)
                {
                    if (Map.CollisionContainers(bullets[i].container, misha[j].container))
                    {
                        Map.ContainerSetFrame(misha[j].container, "box");
                        Map.ContainerSetMaxSide(misha[j].container, 70);

                        box.Add(misha[j]);
                 
                        misha.RemoveAt(j);
                        Map.ContainerErase(bullets[i].container);
                        bullets.RemoveAt(i);
                        expMine = expMine + 1;
                        Info.SetText("expMine", "взрывов " + expMine);

                        break;
                    }
                }
            }

            for (int i = 0; i < enemyBull.Count; i++)
            {
                if (Map.CollisionContainers(enemyBull[i].container, player.container))
                {
                    Map.ContainerErase(enemyBull[i].container);
                    enemyBull.RemoveAt(i);
                    player.hp = player.hp - 1;

                    hppanel.SetImage("hp", "hp" + player.hp);

                    break;
                }
            }
            //ПОПАДАНИЕ ПО ВРАГУ

            for(int i = 0; i < enemies.Count; i++)
            {
                for (int j = 0; j < bullets.Count; j++)
                {
                    if (Map.CollisionContainers(bullets[j].container, enemies[i].container))//!!!
                    {
                        enemies[i].hp--;

                        if (enemies[i].hp <= 0)
                        {
                            Map.ContainerErase(enemies[i].container);
                            expEnemy = expEnemy + 1;
                            Info.SetText("expEnemy", "убийств " + expEnemy);
                            RepairKit(enemies[i].x, enemies[i].y);
                            enemies.RemoveAt(i);
                        }

                        Map.ContainerErase(bullets[j].container);
                        bullets.RemoveAt(j);

                        break;
                    }
                }
            }
        }
        
        void spaunEnemy()
        {
            GameObject enemy = new GameObject();

            enemy.unitType = "enemy";

            enemies.Add(enemy);

            int x, y; //в них рандом
            string container = "enemy" + counter.ToString(); //создание уникального имени контенера

            x = r.Next((int)(player.x) - 300, (int)(player.x) + 300); //получения рандом координат
            y = r.Next((int)(player.y) - 300, (int)(player.y) + 300); //получения рандом координат

            Map.Library.AddContainer(container, "enemy", ContainerType.AutosizedSingleImage); //
            Map.ContainerSetMaxSide(container, 150); //
            Map.ContainerSetCoordinate(container, x, y); //
            int angle = (int)(GameMath.GetAngleOfVector(player.x - x, player.y - y));
            Map.ContainerSetAngle(container, angle); //
            enemy.target = player;
            counter = counter + 1;
            enemy.x = x;
            enemy.y = y;
            enemy.angle = angle;
            enemy.hp = 1;
            enemy.container = container;
            
        }

        void enemyShoot()
        {
            for (int i = 0; enemies.Count > i; i++)
            {
                if (enemies[i].checkShot() && enemies[i].reload >= 10000)
                {
                    /*создать обект для выстреса врага*/
                    GameObject bull = new GameObject();
                    bull.x = enemies[i].x;
                    bull.y = enemies[i].y;
                    bull.angle = enemies[i].angle;
                    bull.container = "enemySoot" + counter;
                    bull.speed = player.speed + 10;

                    bull.dx = bull.speed * Math.Cos(GameMath.DegreesToRad(bull.angle));
                    bull.dy = bull.speed * Math.Sin(GameMath.DegreesToRad(bull.angle));
                    counter = counter + 1;
                    Map.Library.AddContainer(bull.container, "enemyShoot", ContainerType.AutosizedSingleImage);
                    Map.ContainerSetMaxSide(bull.container, 30);
                    Map.ContainerSetCoordinate(bull.container, enemies[i].x, enemies[i].y);
                    Map.ContainerSetAngle(bull.container, enemies[i].angle);

                    enemyBull.Add(bull);

                    enemies[i].reload = 0;
                }
            }
        }

        void sudba()
        {
            player.getDamage("", 1);
        }

        void RepairKit(double x, double y)
        {
            //создаем аптеку
            GameObject heal = new GameObject();
            heal.x = x;
            heal.y = y;
            heal.angle = 0;
            heal.container = "heal" + counter;

            counter = counter + 1;
            Map.Library.AddContainer(heal.container, "heal", ContainerType.AutosizedSingleImage);
            Map.ContainerSetMaxSide(heal.container, 200);
            Map.ContainerSetCoordinate(heal.container, x, y);

            heals.Add(heal);
        }

        void getHeal()
        {
            for (int i = 0; i < heals.Count; i++)
            {
                if (Map.CollisionContainers(player.container, heals[i].container))//!!!
                {
                    player.getDamage("", -2);
                    Map.ContainerErase(heals[i].container);
                    heals.RemoveAt(i);
                }
            }
        }

        void getAmmo()
        {
            for (int i = 0; i < box.Count; i++)
            {
                if (Map.CollisionContainers(player.container, box[i].container))
                {
                    player.ammo = player.ammo + 50;
                    hppanel.SetText("box", player.ammo.ToString());
                    Map.ContainerErase(box[i].container);
                    box.RemoveAt(i);
                }
            }
        }


    }
    public class GameObject
    {
        public string unitType;
        public string Name;
        public double x;
        public double y;
        public int angle;
        public double speed;
        public double dx;
        public double dy;
        public string container;
        public int reload;
        public int hp;
        public int ammo;
        public InventoryPanel InventoryPanel;
        public GameObject target;
        /*
         * проверка наведения врага на игрока
         * 1 вычесляем угол вектора от врага к играку
         * 2 
         */
        public void getDamage(string damageType, int damage)
        {
            hp = hp - damage;
            if (hp <= 0)
            {
                hp = 0;
            }

            if (InventoryPanel != null)
            {
                if(hp > 10)
                {
                    hp = 5;
                }
                InventoryPanel.SetImage("hp", "hp" + hp);
            }
        }

        public bool checkShot()
        {
            bool isAimed = false;
            double angleToTarget = GameMath.GetAngleOfVector(target.x - x, target.y - y);
            if (Math.Abs(angleToTarget - angle) < 5)
            {
                isAimed = true;
            }
            return isAimed;
        }
    }
}
