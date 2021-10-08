using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class autopeli : PhysicsGame
{
    Vector moveUp;
    Vector moveDown;
    Vector moveLeft;
    Vector moveRight;
    Vector speed;

    string difficulty;
    string playerName;
    bool hardMode;
    bool finishlineSpawned;
    bool gameIsOn;
    bool gamePassed;
    bool gameFullyUnlocked = false;
    bool firstCompletion = true;

    List<bool> controlInputs;

    List<Label> mainMenuButtons;
    List<Label> difficultyMenuButtons;
    List<Label> endMenuButtons;

    List<PhysicsObject> objectGroup;

    PhysicsObject player;
    PhysicsObject finishline;
    PhysicsObject rightBorder;
    PhysicsObject leftBorder;
    PhysicsObject bottomBorder;
    PhysicsObject topBorder;

    IntMeter hullIntegrity;
    ProgressBar hullLife;

    DoubleMeter distanceRemaining;
    Label distanceMeter;
    Timer distanceHelpTimer;

    DoubleMeter fuelRemaining;
    Label fuelMeter;
    ProgressBar fuelLife;
    Timer fuelHelpTimer;

    DoubleMeter endTimer;
    Label endTimerDisplay;
    Timer endHelpTimer;
    Label endReason;

    ScoreList hiscores = new ScoreList(20, false, 0);
    HighScoreWindow hiscoresWindow;

    Timer hovertime;
    Label fuelAdded;

    Timer debrisCreator;
    Timer fuelCreator;
    Timer repairkitCreator;

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
            Label button1 = CreateButton("Arcade Mode", 60.0, mainMenuButtons);
            Label button2 = CreateButton("Endurance Mode", 20.0, mainMenuButtons);
            Label button3 = CreateButton("Hiscores", -20.0, mainMenuButtons);
            Label button4 = CreateButton("Exit", -60.0, mainMenuButtons);

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
            Label button1 = CreateButton("Arcade Mode", 20.0, mainMenuButtons);
            Label button2 = CreateButton("Exit", -20.0, mainMenuButtons);

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

        Label easy = CreateButton("Easy", 50.0, difficultyMenuButtons);
        Label medium = CreateButton("Medium", 0.0, difficultyMenuButtons);
        Label hard = CreateButton("Hard", -50.0, difficultyMenuButtons);

        foreach (Label button in difficultyMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, difficultyMenuButtons);
        Mouse.ListenOn(easy, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "easy");
        Mouse.ListenOn(medium, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "medium");
        Mouse.ListenOn(hard, MouseButton.Left, ButtonState.Pressed, CreateStage, null, "hard");
    }


    public void CreateStage(string selectedDifficulty)
    {
        ClearAll();

        difficulty = selectedDifficulty;
        bool hardMode = false;
        bool finishlineSpawned = false;
        bool gamePassed = false;

        objectGroup = new List<PhysicsObject>();

        distanceRemaining = new DoubleMeter(1.0, 0.0, 5.0);
        AddPlayer();
        AddRoad();
        SetControls();

        Level.BackgroundColor = Color.Gray;
        Camera.ZoomToLevel();

        switch (difficulty)
        {
            case "easy":
            {
                distanceRemaining.Value += 0.50;
                CreateDebris(0.1, 1.2, 10.0, 20.0);
                CreateFuel(1.5, 3.0);
                CreateRepairkit(3.0, 6.0);
                StartGame(-250.0);
                break;
            }
            case "medium":
            {
                distanceRemaining.Value += 1.50;
                CreateDebris(0.05, 0.8, 12.5, 30.0);
                CreateFuel(2.0, 4.0);
                CreateRepairkit(6.0, 8.0);
                StartGame(-300.0);
                break;
            }
            case "hard":
            {
                hardMode = true;
                distanceRemaining.Value += 3.00;
                CreateDebris(0.0, 0.4, 15.0, 40.0);
                CreateFuel(2.5, 5.0);
                CreateRepairkit(9.0, 10.0);
                StartGame(-350.0);
                break;
            }
        }
    }


    public void AddPlayer()
    {
        moveUp = new Vector(0, 200);
        moveDown = new Vector(-0, -200);
        moveLeft = new Vector(-200, 0);
        moveRight= new Vector(200, 0);
        
        fuelRemaining = new DoubleMeter(100.0, 0.0, 100.0);

        player = new PhysicsObject(40.0, 80.0);
        player.Shape = Shape.Rectangle;
        player.Image = LoadImage("carYellow3");
        player.Position = new Vector(0.0, -250.0);
        player.CanRotate = false;
        player.Restitution = 0.35;
        AddCollisionHandler(player, "debris_group", CollisionWithDebris);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel);
        AddCollisionHandler(player, "repairkit_group", CollisionWithRepairkit);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline);
        Add(player);
    }


    public void CreateBorders()
    {
        bottomBorder = AddBorder(Level.CreateBottomBorder, 0.0, false);
        topBorder = AddBorder(Level.CreateTopBorder, 0.0, false);
        leftBorder = AddBorder(Level.CreateLeftBorder, 1.0, true);
        rightBorder = AddBorder(Level.CreateRightBorder, 1.0, true);
    }

    public PhysicsObject AddBorder(Func<PhysicsObject> location, double restitution, bool isVisible)
    {
        PhysicsObject border = location();
        border.Restitution = restitution;
        border.IsVisible = isVisible;
        border.AddCollisionIgnoreGroup(1);
        Add(border);
        return border;
    }


    public void AddRoad()
    {
        PhysicsObject roadMidline = new PhysicsObject(20.0, 720.0);
        roadMidline.Position = new Vector(0.0, 720.0);
        roadMidline.Image = LoadImage("midline");
        roadMidline.CanRotate = false;
        roadMidline.IgnoresCollisionResponse = true;
        Add(roadMidline);
        roadMidline.Hit(speed * roadMidline.Mass);
    }


    public void CreateDebris(double spawnMin, double spawnMax, double sizeMin, double sizeMax)
    {
        debrisCreator = new Timer();
        debrisCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);
        debrisCreator.Timeout += delegate { AddDebris(spawnMin, spawnMax, sizeMin, sizeMax); };
        debrisCreator.Start();
    }

    private void AddDebris(double spawnMin, double spawnMax, double sizeMin, double sizeMax)
    {
        if (finishlineSpawned)
        {
            repairkitCreator.Stop();
            repairkitCreator.Reset();
        }

        debrisCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

        PhysicsObject debris = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax), RandomGen.NextDouble(sizeMin, sizeMax));
        debris.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
        debris.Angle = RandomGen.NextAngle();
        debris.Color = Color.White;
        debris.Image = LoadImage("debris");
        debris.CanRotate = false;
        debris.IgnoresCollisionResponse = true;
        debris.Tag = "debris_group";
        debris.LifetimeLeft = TimeSpan.FromSeconds(5.0);
        debris.AddCollisionIgnoreGroup(1);
        objectGroup.Add(debris);
        Add(debris);
        debris.Hit(speed * debris.Mass);
    }


    public void CreateFuel(double spawnMin, double spawnMax)
    {
        fuelCreator = new Timer();
        fuelCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);
        fuelCreator.Timeout += delegate { AddFuel(spawnMin, spawnMax); };
        fuelCreator.Start();
    }


    private void AddFuel(double spawnMin, double spawnMax)
    {
        if (finishlineSpawned)
        {
            repairkitCreator.Stop();
            repairkitCreator.Reset();
        }

        fuelCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

        PhysicsObject fuel = new PhysicsObject(20.0, 25.0);
        fuel.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
        fuel.Image = LoadImage("fuel");
        fuel.CanRotate = false;
        fuel.IgnoresCollisionResponse = true;
        fuel.Tag = "fuel_group";
        fuel.LifetimeLeft = TimeSpan.FromSeconds(5.0);
        fuel.AddCollisionIgnoreGroup(1);
        objectGroup.Add(fuel);
        Add(fuel);
        fuel.Hit(speed * fuel.Mass);
    }


    public void CreateRepairkit(double spawnMin, double spawnMax)
    {
        repairkitCreator = new Timer();
        repairkitCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);
        repairkitCreator.Timeout += delegate { AddRepairkit(spawnMin, spawnMax); };
        repairkitCreator.Start();
    }

    private void AddRepairkit(double spawnMin, double spawnMax)
    {
        if (finishlineSpawned)
        {
            repairkitCreator.Stop();
            repairkitCreator.Reset();
        }

        repairkitCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

        PhysicsObject repairkit = new PhysicsObject(20.0, 25.0);
        repairkit.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
        repairkit.Image = LoadImage("repairkit");
        repairkit.CanRotate = false;
        repairkit.IgnoresCollisionResponse = true;
        repairkit.Tag = "repairkit_group";
        repairkit.LifetimeLeft = TimeSpan.FromSeconds(5.0);
        repairkit.AddCollisionIgnoreGroup(1);
        objectGroup.Add(repairkit);
        Add(repairkit);
        repairkit.Hit(speed * repairkit.Mass);
    }


    public void StartGame(double carSpeed)
    {
        AddBackgroundMusic("default_5");

        speed = new Vector(0.0, carSpeed);

        CreateBorders();
        AddDistanceMeter();
        AddFuelMeter();
        AddHullBar();

        Camera.StayInLevel = true;
        gameIsOn = true;
    }


    public void SetControls()
    {
        Keyboard.Listen(Key.W, ButtonState.Pressed, SetPlayerMovementSpeed, "Accelerate", moveUp);
        Keyboard.Listen(Key.W, ButtonState.Released, SetPlayerMovementSpeed, null, -moveUp);
        Keyboard.Listen(Key.S, ButtonState.Pressed, SetPlayerMovementSpeed, "Decelerate", moveDown);
        Keyboard.Listen(Key.S, ButtonState.Released, SetPlayerMovementSpeed, null, -moveDown);
        Keyboard.Listen(Key.A, ButtonState.Pressed, SetPlayerMovementSpeed, "Steer left", moveLeft);
        Keyboard.Listen(Key.A, ButtonState.Released, SetPlayerMovementSpeed, null, -moveLeft);
        Keyboard.Listen(Key.D, ButtonState.Pressed, SetPlayerMovementSpeed, "Steer right", moveRight);
        Keyboard.Listen(Key.D, ButtonState.Released, SetPlayerMovementSpeed, null, -moveRight);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show controls");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "End game");

        // TODO: Tee puhelimelle ja X-Box -ohjaimelle yhteensopivat ohjaimet
        // PhoneBackButton.Listen(ConfirmExit, "End Game");
    }


    public void AddDistanceMeter()
    {
        distanceMeter = new Label();
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.TextColor = Color.White;
        distanceMeter.Color = Color.Black;
        distanceMeter.DecimalPlaces = 3;
        distanceMeter.Position = new Vector(Screen.Right - 62.5, Screen.Top - 120.0);
        Add(distanceMeter);

        GameObject road = new GameObject(25.0, 25.0);
        road.Position = new Vector(distanceMeter.X - 52.5, distanceMeter.Y - 6.0);
        road.Image = LoadImage("road");
        Add(road);

        distanceHelpTimer = new Timer();
        distanceHelpTimer.Interval = 1.0;
        distanceHelpTimer.Timeout += DistanceHelpTimer_Timeout;
        distanceHelpTimer.Start();
    }


    private void DistanceHelpTimer_Timeout()
    {
        distanceRemaining.Value -= 0.05;

        if (distanceRemaining.Value <= 0.0 && !finishlineSpawned)
        {
            finishline = new PhysicsObject(Screen.Width, 30.0);
            finishline.Y = (Screen.Top + 10.0);
            finishline.Image = LoadImage("finishline");
            finishline.CanRotate = false;
            finishline.IgnoresCollisionResponse = true;
            finishline.Tag = "finishline_group";
            finishline.AddCollisionIgnoreGroup(1);
            Add(finishline);
            finishline.Hit(speed * finishline.Mass);
            finishlineSpawned = true;
        }
    }


    public void AddFuelMeter()
    {
        fuelMeter = new Label();
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.Position = new Vector(Screen.Right - 70.0, Screen.Top - 35.0);
        fuelMeter.Color = Color.Black;
        fuelMeter.DecimalPlaces = 1;
        Add(fuelMeter);

        fuelLife = new ProgressBar(40.0, 3.0);
        fuelLife.BindTo(fuelRemaining);
        fuelLife.Position = new Vector(Screen.Right - 69, Screen.Top - 53.0);
        fuelLife.Color = Color.Black;
        fuelLife.BorderColor = Color.Black;
        Add(fuelLife);

        GameObject jerrycan = new GameObject(22.5, 27.5);
        jerrycan.Position = new Vector(Screen.Right - 116.0, Screen.Top - 47.5);
        jerrycan.Image = LoadImage("jerrycan");
        Add(jerrycan);

        fuelHelpTimer = new Timer();
        fuelHelpTimer.Interval = 0.1;
        fuelHelpTimer.Timeout += FuelHelpTimer_Timeout;
        fuelHelpTimer.Start();
    }


    private void FuelHelpTimer_Timeout()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ExitGame, "Exit Game");
        fuelRemaining.Value -= 0.5;

        switch (fuelRemaining.Value)
        {
            case double n when (n >= 50.0):
                {
                    fuelMeter.TextColor = Color.LightGreen;
                    fuelLife.BarColor = Color.LightGreen;
                    break;
                }
            case double n when (n < 50.0 && n >= 25.0):
                {
                    fuelMeter.TextColor = Color.Yellow;
                    fuelLife.BarColor = Color.Yellow;
                    break;
                }
            case double n when (n < 25.0 && n >= 10.0):
                {
                    fuelMeter.TextColor = Color.Orange;
                    fuelLife.BarColor = Color.Orange;
                    break;
                }

            case double n when (n < 10.0 && n >= 0.0):
                {
                    fuelMeter.TextColor = Color.Red;
                    fuelLife.BarColor = Color.Red;
                    break;
                }
        }

        if (fuelRemaining.Value <= 0.0)
        {
            FuelRanOut();
        }
    }


    public void AddHullBar()
    {
        hullIntegrity = new IntMeter(3, 0, 4);

        hullLife = new ProgressBar(70.0, 8.0);
        hullLife.BindTo(hullIntegrity);
        hullLife.Position = new Vector(Screen.Right - 54.0, Screen.Top - 80.0);
        hullLife.Color = Color.Black;
        hullLife.BorderColor = Color.Black;

        switch (hullIntegrity.Value)
        {
            case 4: hullLife.BarColor = Color.LightGreen; Add(hullLife); break;
            case 3: hullLife.BarColor = Color.Yellow; Add(hullLife); break;
            case 2: hullLife.BarColor = Color.Orange; Add(hullLife); break;
            case 1: hullLife.BarColor = Color.Red; Add(hullLife); break;
        }

        GameObject health = new GameObject(27.5, 27.5);
        health.Position = new Vector(hullLife.X - 61.0, hullLife.Y - 6.0);
        health.Image = LoadImage("health");
        health.Color = Color.Black;
        Add(health);
    }


    public void SetPlayerMovementSpeed(Vector direction)
    {
        player.Velocity += direction;
    }


    public void CollisionWithDebris(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect crash = LoadSoundEffect("intense_explosion");
            crash.Play();
            target.Destroy();
            hullIntegrity.Value--;

            switch (hullIntegrity.Value)
            {
                case 3:
                    hullLife.BarColor = Color.Yellow;
                    player.Image = LoadImage("carYellow3");
                    break;
                case 2:
                    hullLife.BarColor = Color.Orange;
                    player.Image = LoadImage("carYellow2");
                    break;
                case 1:
                    hullLife.BarColor = Color.Red;
                    player.Image = LoadImage("carYellow1");
                    break;
                case 0:
                    ExplodeCar();
                    break;
            }
        }
    }


    public void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect replenish = LoadSoundEffect("fuel");
            replenish.Play();
            target.Destroy();
            double add = RandomGen.NextDouble(10.0, 30.0);
            fuelRemaining.Value += add;
            string addition = add.ToString();

            fuelAdded = new Label();
            fuelAdded.Text = "+ " + addition.Substring(0, 4) + " liters";
            fuelAdded.Position = target.Position;
            fuelAdded.TextColor = Color.LightGreen;
            Add(fuelAdded);

            hovertime = new Timer();
            hovertime.Interval = 0.5;
            hovertime.Timeout += delegate
            {
                fuelAdded.Destroy();
                hovertime.Stop();
                hovertime.Reset();
            };

            hovertime.Start();
        }
    }


    public void CollisionWithRepairkit(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect improvement = LoadSoundEffect("repairkit");
            improvement.Play();
            target.Destroy();
            hullIntegrity.Value++;

            switch (hullIntegrity.Value)
            {
                case 4:
                    hullLife.BarColor = Color.LightGreen;
                    player.Image = LoadImage("carYellow4");
                    break;
                case 3:
                    hullLife.BarColor = Color.Yellow;
                    player.Image = LoadImage("carYellow3");
                    break;
                case 2:
                    hullLife.BarColor = Color.Orange;
                    player.Image = LoadImage("carYellow2");
                    break;
            }
        }
    }


    public void CollisionWithFinishline(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            GameWin("You made it!");
        }
    }


    public void ExplodeCar()
    {
        Explosion carExplosion = new Explosion(5 * player.Width);
        carExplosion.Position = player.Position;
        carExplosion.UseShockWave = false;
        carExplosion.Speed = 300.0;
        //carExplosion.Sound = LoadSoundEffect(/* TODO: Lisää oma ääniefekti.*/);
        Add(carExplosion);
        SoundEffect destruction = LoadSoundEffect("destruction");
        destruction.Play();
        player.Image = LoadImage("carYellow0");

        GameLoss("Your car broke down!");
    }


    public void FuelRanOut()
    {
        SoundEffect empty = LoadSoundEffect("fuel_out");
        empty.Play();

        GameLoss("You ran out of fuel!");
    }


    public void GameWin(string winMessage)
    {
        if (hardMode);
        {
            gameFullyUnlocked = true;
        }

        gameIsOn = false;
        gamePassed = true;
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.D);

        debrisCreator.Stop();
        debrisCreator.Reset();
        fuelCreator.Stop();
        fuelCreator.Reset();
        repairkitCreator.Stop();
        repairkitCreator.Reset();
        distanceHelpTimer.Stop();
        distanceHelpTimer.Reset();
        fuelHelpTimer.Stop();
        fuelHelpTimer.Reset();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Velocity = Vector.Zero;
        }

        finishline.Velocity = Vector.Zero;
        player.Hit(new Vector(0.0, 1000.0));
        player.LifetimeLeft = TimeSpan.FromSeconds(3.0);

        EndCountdown(winMessage);

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, EndMenu, null, "win");
    }


    public void GameLoss(string lossMessage)
    {
        gameIsOn = false;
        gamePassed = false;
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.D);

        StopGameTimers();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Velocity = Vector.Zero;
        }

        player.Velocity = Vector.Zero;

        EndCountdown(lossMessage);

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, EndMenu, null, "loss");
    }


    public void StopGameTimers()
    {
        debrisCreator.Stop();
        fuelCreator.Stop();
        repairkitCreator.Stop();
        distanceHelpTimer.Stop();
        fuelHelpTimer.Stop();
    }


    public void EndCountdown(string endMessage)
    {
        endTimer = new DoubleMeter(3);

        if (endTimer.Value == 3)
        {
            endReason = new Label();
            endReason.Text = endMessage;
            endReason.Y = -80;
            Add(endReason);

            endTimerDisplay = new Label();
            endTimerDisplay.Y = -120;
            endTimerDisplay.BindTo(endTimer);
            Add(endTimerDisplay);
        }

        endHelpTimer = new Timer();
        endHelpTimer.Interval = 1;
        endHelpTimer.Timeout += delegate
        {
            endTimer.Value -= 1;

            if (endTimer.Value == 0.0)
            {
                endHelpTimer.Stop();
                endTimerDisplay.Text = "Press Enter to Continue";
            }
        };

        endHelpTimer.Start();
    }


    public void EndMenu(string instance)
    {
        SoundEffect x = LoadSoundEffect(instance);
        x.Play();
        endReason.Destroy();
        endTimerDisplay.Destroy();
        MediaPlayer.Stop();

        endMenuButtons = new List<Label>();

        Label retry = CreateButton("Retry", 50.0, endMenuButtons);
        
        if (gamePassed == false)
        {
            Label changeDifficulty = CreateButton("Change difficulty", 0, endMenuButtons);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null, "Arcade Mode");
        }
        else if (gamePassed == true)
        {
            Label hiscores = CreateButton("Hiscores", 0, endMenuButtons);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
        }

        Label mainMenu = CreateButton("MainMenu", -50.0, endMenuButtons);

        foreach (Label button in endMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, endMenuButtons);
        Mouse.ListenOn(endMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, difficulty);
        Mouse.ListenOn(endMenuButtons[2], MouseButton.Left, ButtonState.Pressed, MainMenu, null);
    }


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


    public Label CreateButton(string buttonText, double buttonY, List<Label> buttonList)
    {
        Label button = new Label(buttonText);
        button.Y = buttonY;
        button.TextColor = Color.White;
        buttonList.Add(button);
        return button;
    }


    public void DisplayUnlockMessage()
    {
        firstCompletion = false;
        Label unlocks = new Label("You have beaten Arcade Mode on hard difficulty and unlocked Endurance Mode!");
    }
    // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");
}