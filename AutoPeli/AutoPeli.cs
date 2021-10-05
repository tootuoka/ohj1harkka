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
    Vector speed;

    string playerName;
    int arcadeDifficulty = 0;
    bool gameIsOn = false;
    bool gamePassed = false;
    bool gameFullyUnlocked = false;
    bool firstCompletion = true;

    List<Label> mainMenuButtons;
    List<Label> difficultyMenuButtons;
    List<Label> endMenuButtons;

    List<PhysicsObject> objectGroup = new List<PhysicsObject>();

    PhysicsObject player;
    PhysicsObject rightBorder;
    PhysicsObject leftBorder;
    PhysicsObject topBorder;
    PhysicsObject bottomBorder;

    IntMeter hullIntegrity = new IntMeter(3, 0, 4);

    DoubleMeter distanceRemaining = new DoubleMeter(1000.0, 0.0, 1100.0);
    Label distanceMeter= new Label();
    Timer distanceHelpTimer = new Timer();

    DoubleMeter fuelRemaining = new DoubleMeter(100.0, 0.0, 100.0);
    Label fuelMeter= new Label();
    Timer fuelHelpTimer= new Timer();

    DoubleMeter endTimer = new DoubleMeter(3);
    Label endTimerDisplay = new Label();
    Timer endHelpTimer = new Timer();
    Label endReason = new Label();

    ScoreList hiscores = new ScoreList(20, false, 0);
    HighScoreWindow hiscoresWindow;


    public override void Begin()
    {
        hiscores = DataStorage.TryLoad<ScoreList>(hiscores, "hiscores.xml");
        SetPlayerName();
        Keyboard.Listen(Key.Enter, ButtonState.Down, MainMenu, null);
    }


    public void SetPlayerName()
    {
        InputWindow nameQuery = new InputWindow("Player Name: ");
        Add(nameQuery);
        playerName = nameQuery.InputBox.Text;

        Label label = new Label("Press Enter");
        label.Position = new Vector(140.0, -100.0);
        Add(label);
    }


    public void MainMenu()
    {
        ClearAll();

        AddBackgroundMusic("menu_orig");
        Level.Background.Image = LoadImage("mainmenu_bgimg");

        mainMenuButtons = new List<Label>();

        if (gameFullyUnlocked)
        {
            if (firstCompletion) DisplayUnlockMessage();

            AddBackgroundMusic("menu_cmpl");
            Label button1 = CreateButton("Arcade Mode", 60.0);
            Label button2 = CreateButton("Endurance Mode", 20.0);
            Label button3 = CreateButton("Hiscores", -20.0); ;
            Label button4 = CreateButton("Exit", -60.0);

            foreach (Label button in mainMenuButtons)
            {
                Add(button);
            }

            Mouse.ListenMovement(0.5, MenuMovement, null, mainMenuButtons);
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null, "Arcade Mode");
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null, "Endurance Mode");
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, ExitGame, null);
        }
        else
        {
            Label button1 = CreateButton("Arcade Mode", 20.0);
            Label button2 = CreateButton("Exit", -20.0);

            foreach (Label button in mainMenuButtons)
            {
                Add(button);
            }

            Mouse.ListenMovement(0.5, MenuMovement, null, mainMenuButtons);
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null, "Arcade Mode");
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, ExitGame, null);
        }
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


    public void DifficultySelection(string mode)
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

        AddPlayer();
        AddBorders();
        AddRoad();
        SetControls();

        Level.BackgroundColor = Color.Gray;
        Camera.ZoomToLevel();

        if (difficulty == "easy")
        {
            arcadeDifficulty = 1;
            AddDebris(RandomGen.NextDouble(15, 30), RandomGen.NextDouble(15, 30), 50);
            AddFuel(10);
            AddCarepackage(3);
            StartGame(-250.0);
            return;
        }
        else if (difficulty == "medium")
        {
            arcadeDifficulty = 2;
            AddDebris(RandomGen.NextDouble(20, 40), RandomGen.NextDouble(20, 40), 75);
            AddFuel(8);
            AddCarepackage(2);
            StartGame(-300.0);
            return;
        }
        else if (difficulty == "hard")
        {
            arcadeDifficulty = 3;
            AddDebris(RandomGen.NextDouble(25, 50), RandomGen.NextDouble(25, 50), 100);
            AddFuel(6);
            AddCarepackage(1);
            StartGame(-350.0);
            return;
        }
        MainMenu();
    }


    public void AddPlayer()
    {
        player = new PhysicsObject(20.0, 40.0);
        player.Shape = Shape.Rectangle;
        player.Image = LoadImage("carYellow3");
        player.Y = -150.0;
        player.CanRotate = false;
        player.Restitution = 0.35;
        AddCollisionHandler(player, "debris_group", CollisionWithDebris);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel);
        AddCollisionHandler(player, "carepackage_group", CollisionWithCarepackage);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline);
        Add(player);
    }


    public void AddBorders()
    {
        Surfaces rightBorder = Level.CreateVerticalBorders(0.5, false);
        Surfaces leftBorder = Level.CreateVerticalBorders(0.5, false);
        Surfaces topBorder = Level.CreateHorizontalBorders(0, false);
        Surfaces bottomBorder = Level.CreateHorizontalBorders(0, false);
    }


    public void AddRoad()
    {
        // TODO: Lisää tien ominaisuudet (keskiviivat??).
        PhysicsObject finishline = new PhysicsObject(Screen.Width, 20.0);
        finishline.Y = (Screen.Top + 1000.0);
        finishline.Image = LoadImage("finishline");
        finishline.CanRotate = false;
        finishline.IgnoresCollisionResponse = true;
        finishline.Tag = "finishline_group";
        objectGroup.Add(finishline);
        Add(finishline);
    }


    public void AddDebris(double debrisX, double debrisY, int debrisAmount)
    {
        for (int i = 0; i < debrisAmount + 1; i++)
        {
            PhysicsObject debris = new PhysicsObject(debrisX, debrisY);
            debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            debris.Shape = RandomGen.NextShape();
            debris.Angle = RandomGen.NextAngle();
            debris.Color = Color.White;
            // TODO: debris.Image = ???.
            debris.CanRotate = false;
            debris.Tag = "debris_group";
            objectGroup.Add(debris);
            Add(debris);
        }
    }


    public void AddFuel(int fuelAmount)
    {
        for (int i = 0; i < fuelAmount + 1; i++)
        {
            PhysicsObject fuel = new PhysicsObject(25.0, 25.0);
            fuel.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            fuel.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            fuel.Shape = Shape.Circle;
            fuel.Image = LoadImage("fuel");
            fuel.CanRotate = false;
            fuel.IgnoresCollisionResponse = true;
            fuel.Tag = "fuel_group";
            objectGroup.Add(fuel);
            Add(fuel);
        }
    }


    public void AddCarepackage(int carepackageAmount)
    {
        for (int i = 0; i < carepackageAmount + 1; i++)
        {
            PhysicsObject carepackage = new PhysicsObject(25.0, 25.0);
            carepackage.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
            carepackage.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
            carepackage.Shape = Shape.Circle;
            carepackage.Image = LoadImage("carepackage");
            carepackage.CanRotate = false;
            carepackage.IgnoresCollisionResponse = true;
            carepackage.Tag = "carepackage_group";
            objectGroup.Add(carepackage);
            Add(carepackage);
        }
    }


    public void StartGame(double carSpeed)
    {
        AddBackgroundMusic("default_5");

        speed = new Vector(0.0, carSpeed);

        foreach (PhysicsObject x in objectGroup)
        {
            x.Hit(speed * x.Mass);
        }

        AddFuelMeter();
        AddDistanceMeter();

        Camera.Follow(player);
        //Camera.FollowOffset = new Vector(Screen.Width / 2.5 - Screen.Bottom, 0.0);
        //Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        gameIsOn = true;
    }


    public void SetControls()
    {
        Keyboard.Listen(Key.W, ButtonState.Down, SetPlayerMovementSpeed, "Accelerate", moveUp);
        Keyboard.Listen(Key.W, ButtonState.Released, SetPlayerMovementSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, SetPlayerMovementSpeed, "Decelerate", moveDown);
        Keyboard.Listen(Key.S, ButtonState.Released, SetPlayerMovementSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.A, ButtonState.Down, SetPlayerMovementSpeed, "Steer left", moveLeft);
        Keyboard.Listen(Key.A, ButtonState.Released, SetPlayerMovementSpeed, null, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, SetPlayerMovementSpeed, "Steer right", moveRight);
        Keyboard.Listen(Key.D, ButtonState.Released, SetPlayerMovementSpeed, null, Vector.Zero);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show controls");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "End game");

        // TODO: Tee puhelimelle ja X-Box -ohjaimelle yhteensopivat ohjaimet
        // PhoneBackButton.Listen(ConfirmExit, "End Game");
    }

    public void AddDistanceMeter()
    {
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.X = Screen.Left + 50.0;
        distanceMeter.Y = Screen.Top - 50.0;

        distanceHelpTimer.Interval = 0.1;
        distanceHelpTimer.Timeout += DistanceHelpTimer_Timeout;
        distanceHelpTimer.Start();
    }

    private void DistanceHelpTimer_Timeout()
    {
        distanceRemaining.Value -= 0.1;

        if (distanceRemaining.Value == 0.0)
        {
            distanceRemaining.Stop();
        }
    }

    public void AddFuelMeter()
    {
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.X = Screen.Right - 50.0;
        fuelMeter.Y = Screen.Top - 50.0;

        fuelHelpTimer.Interval = 0.1;
        fuelHelpTimer.Timeout += FuelHelpTimer_Timeout;
        fuelHelpTimer.Start();
    }

    private void FuelHelpTimer_Timeout()
    {
        fuelRemaining.Value -= 0.1;

        if (fuelRemaining.Value == 0.0)
        {
            //FuelRanOut();
        }
    }


    public void SetPlayerMovementSpeed(Vector direction)
    {
        if (direction.Y > 0 && player.Top > Screen.Bottom || direction.Y < 0 && player.Bottom < Screen.Top)
        {
            player.Velocity = new Vector(direction.X, 0.0);
        }
        player.Velocity = direction;
    }


    public void CollisionWithDebris(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn == false) return;
        
        SoundEffect crash = LoadSoundEffect("intense_explosion");
        crash.Play();
        target.Destroy();
        hullIntegrity.Value--;

        switch (hullIntegrity.Value)
        {
            case 3:
                player.Image = LoadImage("carYellow3");
                break;
            case 2:
                player.Image = LoadImage("carYellow2");
                break;
            case 1:
                player.Image = LoadImage("carYellow1");
                break;
            case 0:
                //ExplodeCar();
                break;
        }
    }

    public void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn == false) return;

        SoundEffect replenish = LoadSoundEffect("fuel");
        replenish.Play();
        target.Destroy();
        fuelRemaining.Value += RandomGen.NextDouble(10.0, 30.0);
    }


    public void CollisionWithCarepackage(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn == false) return;

        SoundEffect improvement = LoadSoundEffect("carepackage");
        improvement.Play();
        target.Destroy();
        hullIntegrity.Value++;

        switch (hullIntegrity.Value)
        {
            case 4:
                player.Image = LoadImage("carYellow4");
                break;
            case 3:
                player.Image = LoadImage("carYellow3");
                break;
            case 2:
                player.Image = LoadImage("carYellow2");
                break;
        }
    }


    public void CollisionWithFinishline(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn == false) return;

        //GameWin("You made it!");
    }


    /*public void ExplodeCar()
    {
        Keyboard.DisableAll();
        gameIsOn = false;
        StopAll();
        Explosion carExplosion = new Explosion(player.Width);
        carExplosion.Position = player.Position;
        Add(carExplosion);
        SoundEffect destruction = LoadSoundEffect("destruction");
        destruction.Play();
        player.Image = LoadImage("carYellow0");

        Task.Delay(2000);
        GameOver("Your car broke down!");
    }


    public void FuelRanOut()
    {
        Keyboard.DisableAll();
        gameIsOn = false;
        StopAll();

        Task.Delay(2000);
        GameOver("You ran out of fuel!");
    }


    public void GameWin(string winMessage)
    {
        if (arcadeDifficulty >= 3)
        {
            gameFullyUnlocked = true; 
        }

        gamePassed = true;
        Keyboard.DisableAll();

        distanceHelpTimer.Stop();
        fuelHelpTimer.Stop();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Stop();
        }

        player.Hit(new Vector(0.0, 1000.0));

        Task.Delay(2000);

        Add(endReason);

        Task.Delay(3000);

        EndCountdown(winMessage);
        endReason.Destroy();

        Keyboard.EnableAll();

        Mouse.Listen(Key./*TODO: ?.*//*, ButtonState.Pressed, EndMenu, null);
        Keyboard.Listen(Key./*TODO: ?.*//*, ButtonState.Pressed, EndMenu, null);
    }


    public void GameOver(string lossMessage)
    {
        gamePassed = false;

        Add(endReason);

        Task.Delay(3000);

        EndCountdown(lossMessage);
        endReason.Destroy();

        Keyboard.EnableAll();

        Mouse.Listen(Key./*TODO: ?.*//*, ButtonState.Pressed, EndMenu, null);
        Keyboard.Listen(Key./*TODO: ?.*//*, ButtonState.Pressed, EndMenu, null);
    }


    public void EndCountdown(string endMessage)
    {
        endTimerDisplay.TextColor = Color.White;
        endTimerDisplay.BindTo(endTimer);
        Add(endTimerDisplay);

        endHelpTimer.Interval = 1;
        endHelpTimer.Timeout += EndHelpTimer_Timeout;
        endHelpTimer.Start();
    }


    void EndHelpTimer_Timeout()
    {
        endTimer.Value -= 1.0;

        if (endTimer.Value == 0)
        {
            endHelpTimer.Stop();
            endTimerDisplay.Destroy();
        }
    }


    public void EndMenu()
    {
        MediaPlayer.Stop();

        endMenuButtons = new List<Label>();

        Label retry = new Label("Retry");
        retry.Y = 50.0;
        endMenuButtons.Add(retry);

        if (gamePassed == false)
        {
            Label changeDifficulty = new Label("Change difficulty");
            changeDifficulty.Y = 0;
            endMenuButtons.Add(changeDifficulty);


        }
        else if (gamePassed == true)
        {
            Label hiscores = new Label("Hiscores");
            hiscores.Y = 0;
            endMenuButtons.Add(hiscores);
        }

        Label quit = new Label("Quit");
        quit.Y = -50.0;
        endMenuButtons.Add(quit);

        foreach (Label button in endMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, endMenuButtons);
        Mouse.ListenOn(retry, MouseButton.Left, ButtonState.Pressed, /* TODO: ???.*//*, null, /* TODO: ???.*//*);
        Mouse.ListenOn(changeDifficulty, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        Mouse.ListenOn(hiscores, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        Mouse.ListenOn(quit, MouseButton.Left, ButtonState.Pressed, MainMenu, null);
    }

    */
    public void Hiscores()
    {
        ClearAll();

        hiscoresWindow = new HighScoreWindow("Top Score", hiscores);
        hiscoresWindow.Closed += HiscoresWindow_Closed;
        Add(hiscoresWindow);
    }

    private void HiscoresWindow_Closed(Window sender)
    {
        MainMenu();
    }


    public void ExitGame()
    {
        ConfirmExit();
    }
    


    public void AddBackgroundMusic(string track)
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(track);
        MediaPlayer.IsRepeating = true;
    }



    public Label CreateButton(string buttonText, double buttonY)
    {
        Label button = new Label(buttonText);
        button.Y = buttonY;
        mainMenuButtons.Add(button);
        return button;
    }



    public void DisplayUnlockMessage()
    {
        Keyboard.DisableAll();

        Label unlocks = new Label("You have beaten Arcade Mode on hard difficulty and unlocked Endurance Mode!");
        Task.Delay(3000);
        unlocks.Destroy();
        firstCompletion = false;

        Keyboard.EnableAll();
    }
   // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");
}
