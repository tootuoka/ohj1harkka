using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class autopeli : PhysicsGame
{
    Vector moveUp = new Vector(0, 400);
    Vector moveDown = new Vector(-0, -400);
    Vector moveLeft = new Vector(-400, 0);
    Vector moveRight = new Vector(400, 0);

    string playerName;

    List<Label> mainMenuButtons;
    List<Label> difficultyMenuButtons;
    List<Label> endMenuButtons;

    PhysicsObject debris;
    PhysicsObject fuel;
    PhysicsObject carepackage;
    PhysicsObject player;
    PhysicsObject rightBorder;
    PhysicsObject leftBorder;
    PhysicsObject topBorder;
    PhysicsObject bottomBorder;

    IntMeter hullIntegrity;
    IntMeter distanceRemaining;
    DoubleMeter fuelRemaining;

    public override void Begin()
    {
        SetPlayerName();
        MainMenu();
        SetControls();
        AddMeters();
        AddTimers();
        StartGame();
    }

    public void SetPlayerName()
    {
        InputWindow nameQuery = new InputWindow("Player Name: ");
        Add(nameQuery);

        playerName = nameQuery.InputBox.Text;
    }

    public void MainMenu()
    {
        /*MultiSelectWindow mainMenu = new MultiSelectWindow("Main Menu", "Play", "Hiscore", "Exit");
        mainMenu.Color = Color.Gray;
        mainMenu.AddItemHandler(0, SelectDifficulty);
        mainMenu.AddItemHandler(1, OpenHiscore);
        mainMenu.AddItemHandler(2, SelectDifficulty);
        mainMenu.DefaultCancel = 2;*/

        ClearAll();

        mainMenuButtons = new List<Label>();

        Label button1 = new Label("Play");
        button1.Y = 50.0;
        mainMenuButtons.Add(button1);

        Label button2 = new Label("Hiscore");
        button2.Y = 0;
        mainMenuButtons.Add(button2);

        Label button3 = new Label("Exit");
        button3.Y = -50.0;
        mainMenuButtons.Add(button3);

        foreach (Label button in mainMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, mainMenuButtons);
        Mouse.ListenOn(button1, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        Mouse.ListenOn(button2, MouseButton.Left, ButtonState.Pressed, Hiscores, null);
        Mouse.ListenOn(button3, MouseButton.Left, ButtonState.Pressed, ExitGame, null);
    }

    public void MenuMovement(List<Label> menuType)
    {
        foreach (Label button in menuType)
        {
            if (Mouse.IsCursorOn(button))
            {
                button.TextColor = Color.Red;
            }
            else
            {
                button.TextColor = Color.White;
            }
        }
    }

    public void DifficultySelection()
    {
        ClearAll();

        difficultyMenuButtons = new List<Label>();

        Label easy = new Label("Easy");
        easy.Y = 50.0;
        difficultyMenuButtons.Add(easy);

        Label medium = new Label("Medium");
        medium.Y = 0;
        difficultyMenuButtons.Add(medium);

        Label hard = new Label("Hard");
        hard.Y = -50.0;
        difficultyMenuButtons.Add(hard);

        foreach (Label button in difficultyMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, difficultyMenuButtons);
        Mouse.ListenOn(easy, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "easy");
        Mouse.ListenOn(medium, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "medium");
        Mouse.ListenOn(hard, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "hard");
    }

    public void CreateStage(string difficulty)
    {
        ClearAll();
        CreatePlayer();
        CreateBorders();
        CreateRoad();

        // TODO: korjaa?
        AddCollisionHandler(player, HandleCollisions);

        Level.BackgroundColor = Color.Gray;
        Camera.ZoomToLevel();

        if (difficulty == "easy")
        {
            CreateObjects(RandomGen.NextDouble(3, 6), RandomGen.NextDouble(3, 6), 50, 10, 3, -250.0);
            StartGame();
        }
        else if (difficulty == "medium")
        {
            CreateObjects(RandomGen.NextDouble(4, 8), RandomGen.NextDouble(4, 8), 75, 8, 2, -300.0);
            StartGame();
        }
        else if (difficulty == "hard")
        {
            CreateObjects(RandomGen.NextDouble(5, 10), RandomGen.NextDouble(5, 10), 100, 6, 1, -350.0);
            StartGame();
        }
        MainMenu();
    }

    public void CreatePlayer()
    {
        player = new PhysicsObject(5.0, 10.0);
        player.Shape = Shape.Rectangle;
        // TODO: player.Image = ???.
        player.Y = -150.0;
        player.Restitution = 0.35;
        Add(player);
    }

    public void CreateBorders()
    {
        // TODO: for loop?
        rightBorder = Level.CreateRightBorder();
        rightBorder.Restitution = 0.5;
        rightBorder.IsVisible = true;

        leftBorder = Level.CreateLeftBorder();
        leftBorder.Restitution = 0.5;
        leftBorder.IsVisible = true;

        topBorder = Level.CreateTopBorder();
        topBorder.Restitution = 0;
        topBorder.IsVisible = false;

        bottomBorder = Level.CreateBottomBorder();
        bottomBorder.Restitution = 0;
        bottomBorder.IsVisible = false;
    }

    public void CreateRoad()
    {
        // TODO: Lisää tien ominaisuudet (keskiviivat??).
        // TODO: Lisää maaliviiva.
    }

    public void CreateObjects(double debrisX, double debrisY, int debrisAmount, int fuelAmount, int carepackageAmount, double carSpeed)
    {
        Vector speed = new Vector(0.0, carSpeed);

        debris = new PhysicsObject(debrisX, debrisY);
        debris.Shape = Shape.Hexagon;
        // TODO: debris.Image = ???.

        fuel = new PhysicsObject(5.0, 5.0);
        fuel.Shape = Shape.Circle;
        // TODO: fuel.Image = ???.

        carepackage = new PhysicsObject(5.0, 5.0);
        carepackage.Shape = Shape.Circle;
        // TODO: carepackage.Image = ???.

        for (int i = 0; i < debrisAmount + 1; i++)
        {
            debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            debris.Hit(speed * debris.Mass);
        }

        for (int i = 0; i < fuelAmount + 1; i++)
        {
            fuel.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            fuel.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            fuel.Hit(speed * fuel.Mass);
        }

        for (int i = 0; i < carepackageAmount + 1; i++)
        { 
            carepackage.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            carepackage.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            carepackage.Hit(speed * carepackage.Mass);
        }
    }

    public void StartGame()
    {

    }

    public void SetControls()
    {
        Keyboard.Listen(Key.W, ButtonState.Down, SetSpeed, "Accelerate", moveUp);
        Keyboard.Listen(Key.W, ButtonState.Released, SetSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, SetSpeed, "Decelerate", moveDown);
        Keyboard.Listen(Key.S, ButtonState.Released, SetSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.A, ButtonState.Down, SetSpeed, "Steer left", moveLeft);
        Keyboard.Listen(Key.A, ButtonState.Released, SetSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, SetSpeed, "Steer right", moveRight);
        Keyboard.Listen(Key.D, ButtonState.Released, SetSpeed, null, Vector.Zero);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show controls");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "End game");

        // TODO: Tee puhelimelle ja X-Box -ohjaimelle yhteensopivat ohjaimet
        // PhoneBackButton.Listen(ConfirmExit, "End Game");
    }

    public void SetSpeed(Vector direction)
    {
        // TODO: Estä sivuttaisliikkeen pysähtyminen törmätessä.
        if (((direction.Y > 0) && (player.Top >= topBorder.Top)) || ((direction.Y < 0) && (player.Bottom <= bottomBorder.Bottom)))
        {
            player.Velocity = Vector.Zero;
        }
        player.Velocity = direction;
    }

    public void AddMeters()
    {
        // TODO: Määritä laskurit.
        hullIntegrity = new IntMeter(3);
        hullIntegrity.MaxValue = 4;
        hullIntegrity.MinValue = 0;

        /*
        if (hullIntegrity.Value == 4)
        {
            player.Image = ???;
        }
        else if (hullIntegrity.Value == 3)
        {
            player.Image = ???;
        }
        else if (hullIntegrity.Value == 2)
        {
            player.Image = ???;
        }
        else if (hullIntegrity.Value == 1)
        {
            player.Image = ???;
        }
        else if (hullIntegrity.Value == 0)
        {
            ExplodeCar();
        }
        MainMenu();*/

        distanceRemaining = new IntMeter(1000);
        distanceRemaining.MinValue = 0;
        Label distanceMeter = new Label();
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.X = Screen.Left + 50.0;
        distanceMeter.Y = Screen.Top - 50.0;

        fuelRemaining = new DoubleMeter(100.0);
        fuelRemaining.MaxValue = 100.0;
        fuelRemaining.MinValue = 0.0;
        Label fuelMeter = new Label();
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.X = Screen.Right - 50.0;
        fuelMeter.Y = Screen.Top - 50.0;
    }

    public void AddTimers()
    {

    }

    public void HandleCollisions(PhysicsObject player, PhysicsObject target)
    {
        // TODO: Muita vaihtoehtoja kuin debris?

        if (target == debris)
        {
            // TODO: Poista debris (räjäytä tjms.?).
            // TODO: Laske pelaajan hullIntegrityä 1:llä ja muuta auton ulkonäköä.
        }
    }

    public void ExplodeCar()
    {
        // TODO: kentän pysähtyminen.
        // TODO: räjähdys.
        // TODO: player.Image = ???.

        Task.Delay(2000);

        GameOver("Game Over: Your car broke down!");
    }

    public void GameWin()
    {
        // TODO: Määritä voitto.
    }

    public void GameOver(string loseMessage)
    {
        Label lossReason = new Label(loseMessage);
        Add(lossReason);

        Task.Delay(2000);

        // TODO: Poista lossReason näytöltä.

        /* TODO: 2-3 sekunnin alaspäin laskeva laskuri ja label.
        IntMeter lossMeter = new IntMeter(3);
        Timer lossTimer = new Timer();
        lossTimer.Interval = 1;
        lossTimer.Start();
        Label lossTimerDisplay = new Label();
        lossTimerDisplay.TextColor = Color.White;
        lossTimerDisplay.BindTo(lossMeter);
        Add(lossTimerDisplay);
        */

        Mouse.Listen(Key./*TODO: ?.*/, ButtonState.Pressed, EndMenu, null);
        Keyboard.Listen(Key./*TODO: ?.*/, ButtonState.Pressed, EndMenu, null);
    }

    public void EndMenu()
    {
        endMenuButtons = new List<Label>();

        Label retry = new Label("Retry");
        retry.Y = 50.0;
        endMenuButtons.Add(retry);

        /* TODO:
        if (??? == ???)
        {
            Label changeDifficulty = new Label("Change difficulty");
            changeDifficulty.Y = 0;
            endMenuButtons.Add(changeDifficulty);
        }
        else if (??? == ???)
        {
            Label hiscores = new Label("Hiscores");
            hiscores.Y = 0;
            endMenuButtons.Add(hiscores);
        }
        */

        Label quit = new Label("Quit");
        quit.Y = -50.0;
        endMenuButtons.Add(quit);

        foreach (Label button in endMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, endMenuButtons);
        Mouse.ListenOn(retry, MouseButton.Left, ButtonState.Pressed, /* TODO: ???.*/, null, /* TODO: ???.*/);
        Mouse.ListenOn(changeDifficulty, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        Mouse.ListenOn(quit, MouseButton.Left, ButtonState.Pressed, MainMenu, null);
    }

    public void Hiscores()
    {
        // TODO: Määritä pistelista.
    }

    public void ExitGame()
    {
        // TODO: Määritä pelistä poistuminen.
    }
}