//тренировочный проект, с помощью которого я улучшал свои знания в языке программирования C#
//программа разрабатывалась на VS2022 Community NET 6.0
//разрешаю использовать эту программу в любых целях, возможно она кому-нибудь пригодится:)
//DSV 2022,Odessa
using System.Diagnostics;
using System.Security.Cryptography;
using System.Timers;
using static System.Console;


namespace Tetris
{
    
    internal class Program
    {
        private const int MapWidth = 17;//размеры консольного окна
        private const int MapHeight = 22;
        private const char fullBlock = '█';//из этого рисуем фигуры тетриса
        public const char blank = (char)32;//пробел
        public static string stroke0 = "║          ║";//рисуем игровое поле
        public static string stroke1 = "╚══════════╝";//║═╚╝
        public static int tick = 800;//период срабатывания таймера
        public static int speed = 0;
        public static int farbe_figure = 10;//цвет фигуры на игровом поле, которой сейчас управляет игрок
        static int figure = 55;//0...18 обычные фигуры, 55-показывает, что это 1й запуск
        static int figureNext = 55;
        static int[,] tet=new int[4,4];//с помощью этого массива рисуется фигура
        static int[,] te = new int[4, 4];//тут фигура повёрнутая на 90гр.массив нужен для проверки коллизий при повороте фигуры
        static int[,] field = new int[12, 21];//игровой "стакан"
        static int x = 5;//координаты верхнего левого угла массива фигуры
        static int y = 0;//
        static string line0FigureNext = "";
        static string line1FigureNext = "";
        static int numCountTick=0;//для понимания: 1й тик таймера или нет
        static bool inMenu = true;//с ним делаем так, чтобы не сработал тик таймера когда он может помешать
        static int score = 0;
        static int lines = 0;
        static int level = 0;
        static int[]temp = new int[20];
        static bool exitToMenu = false;
        static bool gameOver = false;
        private static System.Timers.Timer timerLoopGame;
        private static System.Timers.Timer timerAbout;
        private static System.Timers.Timer timerMenu;
        //  0    1   2    3    4   5      6   7    8    9   10  11  12  13  14  15   16   17   18
        //████  ██  ██    ██   █   █      █  ███  ███  ███  █    █  ██  ██  █    █    █   █    █
        //      ██   ██  ██   ███  ███  ███   █     █  █    ██  ██   █  █   █    █   ██   ██   █
        //                                                  █    █   █  █   ██  ██   █     █   █
        //                                                                                     █
        static void Main(string[] args)
        {
            Console.Title = "Tetris";
            SetWindowSize(MapWidth, MapHeight);
            SetBufferSize(MapWidth, MapHeight);
            CursorVisible = false;
            ConsoleHelper.SetCurrentFont("Modern DOS 8x8", 24);
            Menu();//рисуем меню
            timerLoopGame = new System.Timers.Timer(tick);
            timerLoopGame.Elapsed += Count;
            timerLoopGame.AutoReset = true;
            timerLoopGame.Enabled = true;
            ConsoleKeyInfo btn;    //Переменная для чтения нажатия клавиши
            do
            {
                //if(gameOver==true)ScreenGameOver();
                btn = Console.ReadKey(true); 
                if (btn.Key == ConsoleKey.D1)//нажали 1 - начинаем игру
                {
                    numCountTick = 0;
                    figure = figureNext = 55;
                    x = 5;
                    y= 0;
                    score = 0;
                    lines = 0;
                    level = 0;
                    //timerLoopGame.Change(0, tick);
                    GameLoop();
                }
                if (btn.Key == ConsoleKey.D2)
                {
                    AboutGame();
                }
            } while (btn.Key != ConsoleKey.Escape);
            Process.GetCurrentProcess().Kill(); //Environment.Exit(0);
        }
        public static void GameLoop()
        {
            GameScreen();//рисуем игровой экран
            inMenu = false;
            ConsoleKeyInfo btn;
            do
            {
                speed = tick - (level * 20);
                timerLoopGame.Interval = speed;

                btn = Console.ReadKey(true);
                if (gameOver == true)//если конец игры, то рисуем достижения игрока, ждём нажатия кнопки и выходим в меню
                {
                    ScreenGameOver();
                    switch (btn.Key)
                    {
                        case ConsoleKey.Escape:
                            //exitToMenu = true;
                            gameOver = false;
                            goto metka0;//да, да, я знаю про вред этого оператора. Но он вредит только тогда, когда им слишком часто злоупотребляют.
                        case ConsoleKey.Spacebar:
                            goto case ConsoleKey.Escape;
                    }
                }
                switch (btn.Key)
                {
                    case ConsoleKey.Escape:
                        exitToMenu = true;
                        inMenu = true;
                        break;
                    case ConsoleKey.LeftArrow:
                        ShiftLeft();
                        break;
                    case ConsoleKey.RightArrow:
                        ShiftRight();
                        break;
                    case ConsoleKey.UpArrow:
                        Rotate();
                        break;
                    case ConsoleKey.DownArrow:
                        if (СollisionСheckDown() == false)
                        {
                            DrawFigure(blank,x,y, farbe_figure);
                            y++;
                            DrawFigure(fullBlock, x, y,farbe_figure);
                        }
                        break;
                    case ConsoleKey.Spacebar://Pause
                        inMenu = !inMenu;
                        break;
                }

            } while (exitToMenu != true);
        metka0: Menu();//рисуем меню
            numCountTick = 0;//сбрасываем все переменные по умолчанию
            figure = figureNext = 55;
        }
        public static void Count(Object source, ElapsedEventArgs e)
        {
            if (inMenu == true) return;//маркер, показывающий, что сейчас нам тут делать ничего не нужно
            if (gameOver == true) return;
            switch (numCountTick)
            {
                case 0:
                    x = 5;
                    y = 0;
                    Random0_6();
                    if (GlassToTheTop() == true) //перед появлением новой фигуры на игровом поле необходимо проверить
                    {//                             не заполнено ли оно доверху?
                        ScreenGameOver();
                        gameOver = true;
                        inMenu = true;
                        return;
                    }

                    DrawFigure(fullBlock, x, y, Random9_14());
                    DrawFigureNext();
                    numCountTick++;
                    return;
                default:
                    if (СollisionСheckDown() == true)
                    {//заносим фигуру в массив field, она более не движется
                        for (int xx = 0; xx < 4; xx++)
                        {
                            for (int yy = 0; yy < 4; yy++)
                            {
                                if (tet[xx, yy] > 0)
                                {
                                    field[x + xx, y + yy] = farbe_figure;
                                }
                            }
                        }
                        //проверим игровое поле на возможность сгорания линий
                        CheckFullLines(figure);
                        numCountTick = 0;
                        break;
                    }
                    DrawFigure(blank, x, y, farbe_figure);
                    y++;
                    DrawFigure(fullBlock, x, y, farbe_figure);
                    numCountTick++;
                    break;
            }
        }
        public static void Memorize(int mem)
        {//Memorize()---вносим в массив фигуру, соответствующую figure
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tet[i,j] = 0;
                }
            }
            switch (mem)
            {
            case 0:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[2, 0] = 1;
                tet[3, 0] = 1;
                    break;
            case 1:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                    break ;
            case 2:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[1, 1] = 1;
                tet[2, 1] = 1;
                    break;
            case 3:
                tet[1, 0] = 1;
                tet[2, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                    break;
            case 4:
                tet[1, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                tet[2, 1] = 1;
                    break;
            case 5:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                tet[2, 1] = 1;
                    break;
            case 6:
                tet[2, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                tet[2, 1] = 1;
                    break;
            case 7:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[2, 0] = 1;
                tet[1, 1] = 1;
                    break;
            case 8:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[2, 0] = 1;
                tet[2, 1] = 1;
                    break;
            case 9:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[2, 0] = 1;
                tet[0, 1] = 1;
                    break;
            case 10:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[0, 2] = 1;
                tet[1, 1] = 1;
                    break;
            case 11: 
                tet[0, 1] = 1;
                tet[1, 0] = 1;
                tet[1, 1] = 1;
                tet[1, 2] = 1;
                    break;
            case 12:
                tet[0, 0] = 1;
                tet[1, 0] = 1;
                tet[1, 1] = 1;
                tet[1, 2] = 1;
                    break;
            case 13:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[0, 2] = 1;
                tet[1, 0] = 1;
                    break;
            case 14:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[0, 2] = 1;
                tet[1, 2] = 1;
                    break;
            case 15:
                tet[0, 2] = 1;
                tet[1, 0] = 1;
                tet[1, 1] = 1;
                tet[1, 2] = 1;
                    break;
            case 16:
                tet[1, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                tet[0, 2] = 1;
                    break;
            case 17:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[1, 1] = 1;
                tet[1, 2] = 1;
                    break;
            case 18:
                tet[0, 0] = 1;
                tet[0, 1] = 1;
                tet[0, 2] = 1;
                tet[0, 3] = 1;
                    break;
            }
        }
        public static void Rotate()
        {//этот кусок кода нужен для поворота фигуры и проверки возможных коллизий
            int oldFigure = figure;
            if (CollisionRotate() == true)
            {
                inMenu = false;
                return;
            }
            DrawFigure(blank, x, y, farbe_figure);
            switch (figure)
            {
                case 0:
                    figure = 18;
                    break;
                case 1:
                    return;
                case 2:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 16;
                    break;
                case 3:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 17;
                    break;
                case 4:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 10;
                    break;
                case 5:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 13;
                    break;
                case 6:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 14;
                    break;
                case 7:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 11;
                    break;
                case 8:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 15;
                    break;
                case 9:
                    //if (y > 17) break;//высота уже не позволяет повернуть фигуру
                    figure = 12;
                    break;
                case 10:
                    figure = 7;
                    if (x == 9) x = 8;
                    break;
                case 11:
                    figure = 4;
                    if (x == 9) x = 8;
                    break;
                case 12:
                    figure = 6;
                    if (x == 9) x = 8;
                    break;
                case 13:
                    figure = 8;
                    if (x == 9) x = 8;
                    break;
                case 14:
                    figure = 9;
                    if (x == 9) x = 8;
                    break;
                case 15:
                    figure = 5;
                    if (x == 9) x = 8;
                    break;
                case 16:
                    figure = 2;
                    if (x == 9) x = 8;
                    break;
                case 17:
                    figure = 3;
                    if (x == 9) x = 8;
                    break;
                case 18:
                    if (y > 16) break;//высота уже не позволяет повернуть палку
                    figure = 0;
                    if (x > 7) x =7 ;
                    break;
            }
            Memorize(figure);
            DrawFigure(fullBlock, x, y, farbe_figure);
        }
        private static bool CollisionRotate()
        {
            int fnext = 0;
            switch (figure)
            {
                case 0:
                    fnext = 18;
                    break;
                case 2:
                    fnext = 16;
                    break;
                case 3:
                    fnext = 17;
                    break;
                case 4:
                    fnext = 10;
                    break;
                case 5:
                    fnext = 13;
                    break;
                case 6:
                    fnext = 14;
                    break;
                case 7:
                    fnext = 11;
                    break;
                case 8:
                    fnext = 15;
                    break;
                case 9:
                    fnext = 12;
                    break;
                case 10:
                    fnext = 7;
                    break;
                case 11:
                    fnext = 4;
                    break;
                case 12:
                    fnext = 6;
                    break;
                case 13:
                    fnext = 8;
                    break;
                case 14:
                    fnext = 9;
                    break;
                case 15:
                    fnext = 5;
                    break;
                case 16:
                    fnext = 2;
                    break;
                case 17:
                    fnext = 3;
                    break;
                case 18:
                    fnext = 0;
                    if (x > 7) //этот блок необходим для того, чтобы можно было повернуть палку горизонтально
                    {//          если она расположена вертикально рядом с правой стенкой
                        if (field[x - 1, y] == 0 & field[x - 2, y] == 0 & field[x - 3, y] == 0)
                        {//проверка на коллизии слева
                            DrawFigure(blank, x, y, farbe_figure);
                            switch (x) 
                            {
                                case 10:
                                    x = x - 3;
                                    break;
                                case 9:
                                    x = x - 2;
                                    break;
                                case 8:
                                    x = x - 1;
                                    break;
                            }
                            return false;
                        }
                    }
                    break;
            }
            ArrayToCheck(fnext);
            for (int xx = 0; xx < 4; xx++) {
                for (int yy = 0; yy < 4; yy++){
                    try
                    {
                        if (te[xx, yy] == 1 & field[x + xx, y + yy] == 1) return true;
                    }
                    catch
                    { return true; }//вышли за пределы массива -> 100% коллизия
                }
            }
            return false;
        }
        public static void ArrayToCheck(int fn)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    te[i, j] = 0;
                }
            }
            switch (fn)
            {
                case 0:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[2, 0] = 1;
                    te[3, 0] = 1;
                    break;
                case 1:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    break;
                case 2:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[1, 1] = 1;
                    te[2, 1] = 1;
                    break;
                case 3:
                    te[1, 0] = 1;
                    te[2, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    break;
                case 4:
                    te[1, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    te[2, 1] = 1;
                    break;
                case 5:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    te[2, 1] = 1;
                    break;
                case 6:
                    te[2, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    te[2, 1] = 1;
                    break;
                case 7:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[2, 0] = 1;
                    te[1, 1] = 1;
                    break;
                case 8:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[2, 0] = 1;
                    te[2, 1] = 1;
                    break;
                case 9:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[2, 0] = 1;
                    te[0, 1] = 1;
                    break;
                case 10:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[0, 2] = 1;
                    te[1, 1] = 1;
                    break;
                case 11:
                    te[0, 1] = 1;
                    te[1, 0] = 1;
                    te[1, 1] = 1;
                    te[1, 2] = 1;
                    break;
                case 12:
                    te[0, 0] = 1;
                    te[1, 0] = 1;
                    te[1, 1] = 1;
                    te[1, 2] = 1;
                    break;
                case 13:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[0, 2] = 1;
                    te[1, 0] = 1;
                    break;
                case 14:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[0, 2] = 1;
                    te[1, 2] = 1;
                    break;
                case 15:
                    te[0, 2] = 1;
                    te[1, 0] = 1;
                    te[1, 1] = 1;
                    te[1, 2] = 1;
                    break;
                case 16:
                    te[1, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    te[0, 2] = 1;
                    break;
                case 17:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[1, 1] = 1;
                    te[1, 2] = 1;
                    break;
                case 18:
                    te[0, 0] = 1;
                    te[0, 1] = 1;
                    te[0, 2] = 1;
                    te[0, 3] = 1;
                    break;
            }
        }
        public static void ShiftRight()
        {
            if (CollisionCheckRight() == true) return;
            DrawFigure(blank, x, y, farbe_figure);
            x++;
            DrawFigure(fullBlock, x, y, farbe_figure);
        }
        public static bool CollisionCheckRight()
        {
            if (x == 10) return true;
            switch (figure)
            {
                case 0:
                    if (field[x + 4, y] >0) return true;
                    break;
                case 1:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 2, y+1] > 0) return true;
                    break;
                case 2:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 3, y + 1] > 0) return true;
                    break;
                case 3:
                    if (field[x + 3, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    break;
                case 4:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 3, y + 1] > 0) return true;
                    break;
                case 5:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x + 3, y + 1] > 0) return true;
                    break;
                case 6:
                    if (field[x + 3, y] > 0) return true;
                    if (field[x + 3, y + 1] > 0) return true;
                    break;
                case 7:
                    if (field[x + 3, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    break;
                case 8:
                    goto case 6;
                case 9:
                    if (field[x + 3, y] > 0) return true;
                    if (field[x + 1, y + 1] > 0) return true;
                    break;
                case 10:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    break;
                case 11:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    if (field[x + 2, y + 2] > 0) return true;
                    break;
                case 12:
                    goto case 11;
                case 13:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 1, y + 1] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    break;
                case 14:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x + 1, y + 1] > 0) return true;
                    if (field[x + 2, y + 2] > 0) return true;
                    break;
                case 15:
                    goto case 11;
                case 16:
                    if (field[x + 2, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    break;
                case 17:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    if (field[x + 2, y + 2] > 0) return true;
                    break;
                case 18:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x + 1, y + 1] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    if (field[x + 1, y + 3] > 0) return true;
                    break;
            }
                    //  0    1   2    3    4   5      6   7    8    9   10  11  12  13  14  15   16   17   18
                    //████  ██  ██    ██   █   █      █  ███  ███  ███  █    █  ██  ██  █    █    █   █    █
                    //      ██   ██  ██   ███  ███  ███   █     █  █    ██  ██   █  █   █    █   ██   ██   █
                    //                                                  █    █   █  █   ██  ██   █     █   █
                    //                                                                                     █
                    return false;
        }
        public static void ShiftLeft()
        {
            if (CollisionCheckLeft() == true) return;
            DrawFigure(blank, x, y, farbe_figure);
            x--;
            DrawFigure(fullBlock, x, y, farbe_figure);
        }
        public static bool CollisionCheckLeft()
        {
            //if(x==1)return true;
            switch (figure)
            {
                case 0:
                    if (field[x-1, y] > 0) return true;
                    break;
                case 1:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 2:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x , y+1] > 0) return true;
                    break;
                case 3:
                    if (field[x, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 4:
                    if (field[x, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 5:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 6:
                    if (field[x + 1, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 7:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x, y+1] > 0) return true;
                    break;
                case 8:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x + 1, y+1] > 0) return true;
                    break;
                case 9:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    break;
                case 10:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x - 1, y+1] > 0) return true;
                    if (field[x - 1, y+2] > 0) return true;
                    break;
                case 11:
                    if (field[x, y] > 0) return true;
                    if (field[x - 1, y + 1] > 0) return true;
                    if (field[x, y + 2] > 0) return true;
                    break;
                case 12:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x, y + 1] > 0) return true;
                    if (field[x, y + 2] > 0) return true;
                    break;
                case 13:
                    goto case 10;
                case 14:
                    goto case 10;
                case 15:
                    if (field[x, y] > 0) return true;
                    if (field[x, y + 1] > 0) return true;
                    if (field[x-1, y + 2] > 0) return true;
                    break;
                case 16:
                    if (field[x, y] > 0) return true;
                    if (field[x-1, y + 1] > 0) return true;
                    if (field[x-1, y + 2] > 0) return true;
                    break;
                case 17:
                    if (field[x - 1, y] > 0) return true;
                    if (field[x-1, y + 1] > 0) return true;
                    if (field[x, y + 2] > 0) return true;
                    break;
                case 18:
                    if (field[x - 1, y+3] > 0) return true;
                    goto case 10;
            }
                    return false;

        }
        public static bool СollisionСheckDown()
        {
                switch(figure)
            {   case 0:
                    for (int i = 0; i < 4; i++)
                    {
                        if (field[x + i, y + 1] > 0) return true;
                    }
                    break;
                case 1:
                    for (int i = 0; i < 2; i++)
                    {
                        if (field[x + i, y + 2] > 0) return true;
                    }
                    break;
                case 2:
                    if (field[x, y + 1] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    if (field[x + 2, y + 2] > 0) return true;
                    break;
                case 3:
                    if (field[x, y + 2] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    if (field[x + 2, y + 1] > 0) return true;
                    break;
                case 4:
                    if (field[x, y + 2] > 0) return true;
                    if (field[x + 1, y + 2] > 0) return true;
                    if (field[x + 2, y + 2] > 0) return true;
                    break;
                case 5:
                    goto case 4;
                case 6:
                    goto case 4;
                case 7:
                    if (field[x, y + 1] > 0) return true;
                    if (field[x+1, y + 2] > 0) return true;
                    if (field[x+2, y + 1] > 0) return true;
                    break;
                case 8:
                    if (field[x, y + 1] > 0) return true;
                    if (field[x+1, y + 1] > 0) return true;
                    if (field[x+2, y + 2] > 0) return true;
                    break;
                case 9:
                    if (field[x, y + 2] > 0) return true;
                    if (field[x+1, y + 1] > 0) return true;
                    if (field[x+2, y + 1] > 0) return true;
                    break;
                case 10:
                    if (field[x, y + 3] > 0) return true;
                    if (field[x+1, y + 2] > 0) return true;
                    break;
                case 11:
                    if (field[x, y + 2] > 0) return true;
                    if (field[x + 1, y + 3] >0) return true;
                    break;
                case 12:
                    if (field[x, y + 1] > 0) return true;
                    if (field[x+1, y + 3] > 0) return true;
                    break;
                case 13:
                    if (field[x, y + 3] > 0) return true;
                    if (field[x+1, y + 1] > 0) return true;
                    break;
                case 14:
                    if (field[x, y + 3] > 0) return true;
                    if (field[x+1, y + 3] > 0) return true;
                    break;
                case 15:
                    goto case 14;
                case 16:
                    if (field[x, y + 3] > 0) return true;
                    if (field[x+1, y + 2] > 0) return true;
                    break;
                case 17:
                    if (field[x, y + 2] > 0) return true;
                    if (field[x+1, y + 3] > 0) return true;
                    break;
                case 18:
                    if (field[x, y + 4] > 0) return true;
                    break;
            }
            return false;
        }
        public static bool GlassToTheTop()
        {//Проверка на коллизию
            for (int xx = 0; xx < 4; xx++)
            {
                for (int yy = 0; yy < 4; yy++)
                {
                    if (tet[xx, yy] == 1 & field[x + xx, y + yy] >0) return true;
                }
            }
            return false;
        }
        public static void Cycle(int cc)
        {//вынес повторяющийся кусок кода в подпрограмму
            for (int j = y + cc; j > 0; j--)
             {
                  for (int xx = 1; xx< 11; xx++)
                  {
                      field[xx, j] = field[xx, j - 1];
                  }
             }
        }
        public static void CheckFullLines(int fff)
        {
            int i0 = 0;//счётчик заполненных клеток в линии 
            int i1 = 0;//для фигуры высотой в 2 клетки
            int i2 = 0;//---------/-----------3 клетки
            int i3 = 0;//---------/-----------4 клетки
            int counter = 0;//количество "сгоревших" строк за раз
            Array.Clear(temp);
            switch (fff)
            {
                case 0:
                    for (int xx = 1; xx < 11; xx++)
                    {
                        if (field[xx, y] >0) i0++;//счётчик заполненных ячеек в строке
                    }
                    if (i0 == 10)//если заполнено 10 из 10, то...
                    {
                        Cycle(0);//если строка заполнена, то удаляем строку, смещая данные в массиве в цикле
                        score++;
                        lines++;
                        ShowScore();
                    }
                    break;
                case 1:
                    for (int xx = 1; xx < 11; xx++)
                    {
                        if (field[xx, y] > 0) i0++;//фигура высотой 2 клетки
                        if (field[xx, y + 1] > 0) i1++;
                    }
                    if (i0 == 10)
                    {
                        Cycle(0);
                        counter++;
                    }
                    if (i1 == 10)
                    {
                        Cycle(1);
                        counter++;
                    }
                    switch (counter)
                    {
                        case 1:
                            score++;
                            lines++;
                            ShowScore();
                            break;
                        case 2:
                            score += 3;
                            lines += 2;
                            ShowScore();
                            break;
                    }

                    break;
                case 2:
                    goto case 1;
                case 3:
                    goto case 1;
                case 4:
                    goto case 1;
                case 5:
                    goto case 1;
                case 6:
                    goto case 1;
                case 7:
                    goto case 1;
                case 8:
                    goto case 1;
                case 9:
                    goto case 1;
                case 10:
                    for (int xx = 1; xx < 11; xx++)
                    {
                        if (field[xx, y] > 0) i0++;//фигура высотой 3 клетки
                        if (field[xx, y + 1] > 0) i1++;
                        if (field[xx, y + 2] > 0) i2++;
                    }
                    if (i0 == 10)
                    {
                        Cycle(0);
                        counter++;
                    }
                    if (i1 == 10)
                    {
                        Cycle(1);
                        counter++;
                    }
                    if (i2 == 10)
                    {
                        Cycle(2);
                        counter++;
                    }
                    switch (counter)
                    {
                        case 1:
                            score++;
                            lines++;
                            ShowScore();
                            break;
                        case 2:
                            score += 3;
                            lines += 2;
                            ShowScore();
                            break;
                        case 3:
                            score += 7;
                            lines += 3;
                            ShowScore();
                            break;
                    }
                    break;
                case 11:
                    goto case 10;
                case 12:
                    goto case 10;
                case 13:
                    goto case 10;
                case 14:
                    goto case 10;
                case 15:
                    goto case 10;
                case 16:
                    goto case 10;
                case 17:
                    goto case 10;
                case 18:
                    for (int xx = 1; xx < 11; xx++)
                    {
                        if (field[xx, y] > 0) i0++;//фигура высотой 4 клетки
                        if (field[xx, y + 1] > 0) i1++;
                        if (field[xx, y + 2] > 0) i2++;
                        if (field[xx, y + 3] > 0) i3++;
                    }
                    if (i0 == 10)
                    {
                        Cycle(0); 
                        counter++; 
                    }
                    if (i1 == 10) 
                    {
                        Cycle(1);
                        counter++; 
                    }
                    if (i2 == 10)
                    { 
                        Cycle(2); 
                        counter++; 
                    }
                    if (i3 == 10) 
                    {
                        Cycle(3);
                        counter++;
                    }
                    switch (counter)// 1 линия — 1 очко, 2 линии — 3 очка, 3 линии — 7 очков
                    {               // 4 линии (то есть сделать Тетрис) — 15 очков
                        case 1:
                            score++;
                            lines++;
                            ShowScore();
                            break;
                        case 2:
                            score += 3;
                            lines += 2;
                            ShowScore();
                            break;
                        case 3:
                            score += 7;
                            lines += 3;
                            ShowScore();
                            break;
                        case 4:
                            score += 15;
                            lines += 4;
                            ShowScore();
                            break;
                    }
                    break;
            }
            //inMenu = true;
            for (int line = 19; line > 0 ; line--)
            {
                for (int xx = 1; xx < 11; xx++)
                {
                    if (field[xx, line] >0)
                    {
                        WriteChar(xx, line, '█', field[xx, line]);
                    }
                    else WriteChar(xx, line, blank,0);
                }
            }
            //inMenu = false;
        }
        public static void Random0_6()
        {
            byte[] randombyte = new Byte[1];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randombyte);
            Random rnd = new Random();
            switch (numCountTick)
            {
                case 0:
                    if (figure == 55)
                    {
                        figure = rnd.Next(0, 7);
                        figureNext = rnd.Next(0, 7);
                        //figure = figureNext = 0;//отладочная заглушка
                        Memorize(figure);
                        break;
                    }
                    else
                    {
                        figure = figureNext;
                        figureNext = rnd.Next(0, 7);
                        Memorize(figure);
                        break;
                    }
            }
        }
        public static int Random9_14()
        {
            byte[] randombyte = new Byte[1];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randombyte);
            Random rnd = new Random();
            farbe_figure = rnd.Next(9, 15);
            if (farbe_figure == 10) farbe_figure = 15;//меняем зелёный цвет на белый
            return farbe_figure;
        }
        public static void DrawFigure(char cv,int x1,int y1,int farbe)
            {
            Console.ForegroundColor= (ConsoleColor)farbe;
            for (int i = 0; i < 4; i++)
            {
                    for (int j = 0; j < 4; j++)
                    {
                        if (tet[i,j]==1)
                        {
                            SetCursorPosition(x1 + i, y1 + j);
                            WriteLine(cv);
                        }
                    }
            }
            //WriteString(0, 0, farbe_figure.ToString() + " ", farbe_figure);//отладочное: для проверки цвета
        }
        public static void DrawFigureNext()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            switch (figureNext)
            {
                case 0:
                    {
                        line0FigureNext = "████";
                        line1FigureNext = "    ";
                        break;
                    }
                case 1:
                    {
                        line0FigureNext = " ██ ";
                        line1FigureNext = " ██ ";
                        break;
                    }
                case 2:
                    {
                        line0FigureNext = "██  ";
                        line1FigureNext = " ██ ";
                        break;
                    }
                case 3:
                    {
                        line0FigureNext = " ██ ";
                        line1FigureNext = "██  ";
                        break;
                    }
                case 4:
                    {
                        line0FigureNext = " █  ";
                        line1FigureNext = "███ ";
                        break;
                    }
                case 5:
                    {
                        line0FigureNext = "█   ";
                        line1FigureNext = "███ ";
                        break;
                    }
                case 6:
                    {
                        line0FigureNext = "  █ ";
                        line1FigureNext = "███ ";
                        break;
                    }
            }
            WriteString(13, 2, line0FigureNext,10);
            WriteString(13, 3, line1FigureNext,10);
        }
        public static void AboutGame()
        {
            timerMenu.Enabled = false;
            Console.Clear();
            timerAbout.Enabled = true;
            ConsoleKeyInfo btn;
            do
            {
                btn = Console.ReadKey(true);
            } while (btn.Key != ConsoleKey.Escape);
            timerAbout.Enabled = false;
            Menu();
        }
        public static void Count_About(Object source, ElapsedEventArgs e)
        {
            Console.Clear();
            WriteString(0, 1, "Use left & right", Random9_14());
            WriteString(0, 3, "  keys to move", Random9_14());
            WriteString(0, 5, "   ↑  rotate", Random9_14());
            WriteString(0, 7, "   ↓  drop", Random9_14());
            WriteString(0, 9, " space  pause", Random9_14());
        }
        public static void Menu()
        {
            timerAbout = new System.Timers.Timer(100);
            timerAbout.Elapsed += Count_About;
            timerAbout.AutoReset = true;
            timerAbout.Enabled = false;
            timerMenu = new System.Timers.Timer(100);
            timerMenu.Elapsed += Count_Menu;
            timerMenu.AutoReset = true;
            timerMenu.Enabled = true;
            Console.Clear();
        }
        public static void Count_Menu(Object source, ElapsedEventArgs e)
        {
            Console.Clear();
            WriteString(0, 3, "  Console ASCII", Random9_14());
            WriteString(0, 5, "     Tetris", Random9_14());
            WriteString(0, 10, " 1. Start game", Random9_14());
            WriteString(0, 12, " 2. About game", Random9_14());
            WriteString(0, 14, " Esc. Exit", Random9_14());
        }
        public static void GameScreen()
        {
            timerMenu.Enabled = false;
            Console.Clear();
            Array.Clear(field);//║═╚╝
            for (int i = 0; i < 21; i++)
            {
                field[0, i] = 1;
                field[11, i] = 1;
                WriteChar(0, i, '║', 10);
                WriteChar(11, i, '║', 10);
            }
            for (int i = 0; i < 12; i++)
            {
                field[i, 20] = 1;
                WriteChar(i,20, '═', 10);
            }
            WriteChar(0, 20, '╚', 10);
            WriteChar(11, 20, '╝', 10);
            //for (int xx = 0; xx < 12; xx++)
            //{
            //    for (int yy = 0; yy < 21; yy++)
            //    {
            //        if (field[xx, yy] == 1) WriteChar(xx, yy, (char)35);
            //    }
            //}
            WriteString(12, 0, "Next", 10);
            WriteString(12, 6, "Score", 10);
            WriteString(12, 10, "Lines", 10);
            WriteString(12, 14, "Level", 10);
            ShowScore();
        }
        static void ShowScore()
        {
            WriteString(13, 8, score.ToString(),10);
            WriteString(13, 12, lines.ToString(), 10);
            decimal rr = lines / 10;
            rr = Math.Truncate(rr);//просто отбрасываем дробную часть числа без округления
            level = (int)rr;//уровень прибавляется за каждые 10 сгоревших линий
            WriteString(13, 16, level.ToString(), 10);
            //WriteString(12, 18, speed.ToString()+" ");//отладочное
        }
        public static void WriteString(int xx, int yy, string ss, int farbe)
        {
            Console.ForegroundColor = (ConsoleColor)farbe;
            SetCursorPosition(xx, yy);
            WriteLine(ss);
        }
        public static void WriteChar(int xx, int yy, char cc, int farbe)
        {
            Console.ForegroundColor = (ConsoleColor)farbe;
            SetCursorPosition(xx, yy);
            WriteLine(cc);
        }
        public static void ScreenGameOver()
        {
            Console.Clear();
            WriteString(5, 6, "Score:",10);
            WriteString(5, 8, "Lines:", 10);
            WriteString(5, 10, "Level:", 10);
            WriteString(11, 6, score.ToString(), 10);
            WriteString(11, 8, lines.ToString(), 10);
            WriteString(11, 10, level.ToString(), 10);
            WriteString(0, 21, "Esc to continue", 10);
        }
    }
}