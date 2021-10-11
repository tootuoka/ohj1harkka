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
    Vector gameSpeed;

    string difficulty;
    string gameMode;
    string playerName;
    bool standardMode;
    bool finishlineSpawned;
    bool gameIsOn;
    bool gamePassed;
    bool gameFullyUnlocked = true;
    bool firstCompletion = true;

    double[] enduranceMultipliers;

    GameObject multiplier;

    List<Label> mainMenuButtons;
    List<Label> difficultyMenuButtons;
    List<Label> endMenuButtons;

    List<PhysicsObject> objectGroup;

    PhysicsObject player;
    PhysicsObject finishline;

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

    IntMeter pointMultiplier;

    DoubleMeter pointTotal;
    Label pointMeter;
    Label pointMultiplierMeter;
    Timer pointHelpTimer;

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
            Label button1 = CreateButton("Arcade Mode", 60.0, mainMenuButtons);
            Label button2 = CreateButton("Endurance Mode", 20.0, mainMenuButtons);
            Label button3 = CreateButton("Hiscores", -20.0, mainMenuButtons);
            Label button4 = CreateButton("Exit", -60.0, mainMenuButtons);

            foreach (Label button in mainMenuButtons)
            {
                Add(button);
            }

            Mouse.ListenMovement(0.5, MenuMovement, null, mainMenuButtons);
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CreateStage_Endurance, null);
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
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
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


    public void DifficultySelection()
    {
        ClearAll();

        difficultyMenuButtons = new List<Label>();

        Label beginner = CreateButton("Beginner", 50.0, difficultyMenuButtons);
        Label standard = CreateButton("Standard", 0.0, difficultyMenuButtons);
        Label madness = CreateButton("Madness", -50.0, difficultyMenuButtons);

        foreach (Label button in difficultyMenuButtons)
        {
            Add(button);
        }

        Mouse.ListenMovement(1.0, MenuMovement, null, difficultyMenuButtons);
        Mouse.ListenOn(beginner, MouseButton.Left, ButtonState.Pressed, CreateStage_Arcade, null, "beginner");
        Mouse.ListenOn(standard, MouseButton.Left, ButtonState.Pressed, CreateStage_Arcade, null, "standard");
        Mouse.ListenOn(madness, MouseButton.Left, ButtonState.Pressed, CreateStage_Arcade, null, "madness");
    }


    public void CreateStage_Arcade(string selectedDifficulty)
    {
        ClearAll();

        gameMode = "arcade";
        difficulty = selectedDifficulty;
        standardMode = false;
        finishlineSpawned = false;
        gamePassed = false;

        objectGroup = new List<PhysicsObject>();

        distanceRemaining = new DoubleMeter(1.0, 0.0, 5.0);

        CreateBorders();
        AddPlayer_Arcade();
        SetControls();
        AddRoad();
        AddFuelMeter();
        AddHullBar();
        AddDistanceMeter_Arcade ();

        Level.BackgroundColor = Color.Gray;

        PhysicsObject fuel = new PhysicsObject(0.0, 0.0);
        PhysicsObject repairkit = new PhysicsObject(0.0, 0.0);

        switch (difficulty)
        {
            case "beginner":
                distanceRemaining.Value += 0.50;
                CreateDebris_Arcade(12.5, 30.0, 0.1, 1.2);
                CreateCollectibles_Arcade(fuel, "fuel", "fuel_group", 1.5, 3.0);
                CreateCollectibles_Arcade(repairkit, "repairkit", "repairkit_group", 3.0, 6.0);
                StartGame_Arcade(-250.0);
                break;
            case "standard":
                standardMode = true;
                distanceRemaining.Value += 1.50;
                CreateDebris_Arcade(12.5, 30.0, 0.05, 0.8);
                CreateCollectibles_Arcade(fuel, "fuel", "fuel_group", 2.0, 4.0);
                CreateCollectibles_Arcade(repairkit, "repairkit", "repairkit_group", 6.0, 8.0);
                StartGame_Arcade(-300.0);
                break;
            case "madness":
                distanceRemaining.Value += 3.00;
                CreateDebris_Arcade(12.5, 30.0, 0.0, 0.4);
                CreateCollectibles_Arcade(fuel, "fuel", "fuel_group", 2.5, 5.0);
                CreateCollectibles_Arcade(repairkit, "repairkit", "repairkit_group", 9.0, 10.0);
                StartGame_Arcade(-350.0);
                break;
        }
    }


    public void AddPlayer_Arcade()
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
        player.Restitution = 0;
        AddCollisionHandler(player, "debris_group", CollisionWithDebris_Arcade);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel_Arcade);
        AddCollisionHandler(player, "repairkit_group", CollisionWithRepairkit_Arcade);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline_Arcade);
        Add(player);
    }


    public void CreateBorders()
    {
        PhysicsObject bottomBorder = AddBorder(Level.CreateBottomBorder, 0.0, false);
        PhysicsObject topBorder = AddBorder(Level.CreateTopBorder, 0.0, false);
        PhysicsObject leftBorder = AddBorder(Level.CreateLeftBorder, 0.0, false);
        PhysicsObject rightBorder = AddBorder(Level.CreateRightBorder, 0.0, false);
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
        roadMidline.AddCollisionIgnoreGroup(1);
        Add(roadMidline);
        roadMidline.Hit(gameSpeed * roadMidline.Mass);
    }


    public void CreateDebris_Arcade(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer debrisCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        debrisCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                debrisCreator.Stop();
                return;
            }

            debrisCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            PhysicsObject debris = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax), RandomGen.NextDouble(sizeMin, sizeMax));
            debris.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            debris.Angle = RandomGen.NextAngle();
            debris.Image = LoadImage("debris");
            debris.CanRotate = false;
            debris.IgnoresCollisionResponse = true;
            debris.Tag = "debris_group";
            debris.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            debris.AddCollisionIgnoreGroup(1);
            objectGroup.Add(debris);
            Add(debris);
            debris.Hit(gameSpeed * debris.Mass);
        };
        debrisCreator.Start();
    }


    public void CreateCollectibles_Arcade(PhysicsObject collectible, string collectibleImage, string collectibleGroup, double spawnMin, double spawnMax)
    {
        Timer collectibleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));

        collectibleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                collectibleCreator.Stop();
                return;
            }

            collectibleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            collectible = new PhysicsObject(25.0, 25.0);
            collectible.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            collectible.Image = LoadImage(collectibleImage);
            collectible.CanRotate = false;
            collectible.IgnoresCollisionResponse = true;
            collectible.Tag = collectibleGroup;
            collectible.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            collectible.AddCollisionIgnoreGroup(1);
            objectGroup.Add(collectible);
            Add(collectible);
            collectible.Hit(gameSpeed * collectible.Mass);
        };

        collectibleCreator.Start();
    }


    public void StartGame_Arcade(double speed)
    {
        AddBackgroundMusic("default_5");

        gameSpeed = new Vector(0.0, speed);

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


    public void AddDistanceMeter_Arcade()
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
        distanceHelpTimer.Interval = 0.1;
        distanceHelpTimer.Timeout += delegate
        {
            distanceRemaining.Value -= 0.005;

            if (distanceRemaining.Value == 0.0 && !finishlineSpawned)
            {
                finishline = new PhysicsObject(Screen.Width, 30.0);
                finishline.Y = (Screen.Top + 10.0);
                finishline.Image = LoadImage("finishline");
                finishline.CanRotate = false;
                finishline.IgnoresCollisionResponse = true;
                finishline.Tag = "finishline_group";
                finishline.AddCollisionIgnoreGroup(1);
                objectGroup.Add(finishline);
                Add(finishline);
                finishline.Hit(gameSpeed * finishline.Mass);
                finishlineSpawned = true;
            }
        };

        distanceHelpTimer.Start();
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
                    fuelMeter.TextColor = Color.LightGreen;
                    fuelLife.BarColor = Color.LightGreen;
                    break;
            case double n when (n < 50.0 && n >= 25.0):
                    fuelMeter.TextColor = Color.Yellow;
                    fuelLife.BarColor = Color.Yellow;
                    break;
            case double n when (n < 25.0 && n >= 10.0):
                    fuelMeter.TextColor = Color.Orange;
                    fuelLife.BarColor = Color.Orange;
                    break;
            case double n when (n < 10.0 && n >= 0.0):
                    fuelMeter.TextColor = Color.Red;
                    fuelLife.BarColor = Color.Red;
                    break;
        }

        if (fuelRemaining.Value == 0.0) FuelRanOut();
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


    public void CollisionWithDebris_Arcade(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect crash = LoadSoundEffect("intense_explosion");
            crash.Play();
            target.Destroy();
            hullIntegrity.Value--;

            if (gameMode == "endurance")
            {
                pointMultiplier.Value = 1;
                multiplier.Image = LoadImage("multiplied1");
            }

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


    private void CollisionWithFuel_Arcade(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect replenish = LoadSoundEffect("fuel");
            replenish.Play();
            target.Destroy();
            double add = RandomGen.NextDouble(10.0, 30.0);
            fuelRemaining.Value += add;
            string addition = add.ToString();

            Label fuelAdded = new Label();
            fuelAdded.Text = "+ " + addition.Substring(0, 4) + " liters";
            fuelAdded.Position = target.Position;
            fuelAdded.TextColor = Color.LightGreen;
            Add(fuelAdded);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                fuelAdded.Destroy();
                displayTimer.Stop();
            };

            displayTimer.Start();
        }
    }


    private void CollisionWithRepairkit_Arcade(PhysicsObject player, PhysicsObject target)
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


    private void CollisionWithFinishline_Arcade(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn) GameWin("You made it!");
    }


    public void ExplodeCar()
    {
        Explosion carExplosion = new Explosion(5 * player.Width);
        carExplosion.Position = player.Position;
        carExplosion.UseShockWave = false;
        carExplosion.Speed = 300.0;
        carExplosion.Sound = LoadSoundEffect("1");
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
        if (standardMode) gameFullyUnlocked = true;

        gameIsOn = false;
        gamePassed = true;
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.D);

        StopGameTimers();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Velocity = Vector.Zero;
        }

        finishline.Velocity = Vector.Zero;
        player.AddCollisionIgnoreGroup(1);
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
        if (gameMode != "endurance") distanceHelpTimer.Stop();
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

        endHelpTimer = new Timer(1.0);
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
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
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
        Mouse.ListenOn(endMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CreateStage_Arcade, null, difficulty);
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
        Label unlocks = new Label("You have beaten Arcade Mode and unlocked new content! madness difficulty Endurance Mode!");
        Add(unlocks);
        SoundEffect popUp = LoadSoundEffect("4");
        popUp.Play();
    }
    // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");



    //---------------------------------------------------------
    //---------------------------------------------------------
    //---------------------------------------------------------



    public void CreateStage_Endurance()
    {
        ClearAll();

        gameMode = "endurance";
        finishlineSpawned = false;

        objectGroup = new List<PhysicsObject>();

        CreateBorders();
        AddPlayer_Endurance();
        SetControls();
        AddRoad();
        AddFuelMeter_Endurance();
        AddHullBar_Endurance();
        AddPointMeter_Endurance();
        AddZones_Endurance();

        Level.BackgroundColor = Color.Gray;

        PhysicsObject fuel = new PhysicsObject(0.0, 0.0);
        PhysicsObject repairkit = new PhysicsObject(0.0, 0.0);
        PhysicsObject coin = new PhysicsObject(0.0, 0.0);

        CreateDebris_Endurance(10.0, 30.0, 0.5, 2.0);
        CreateCollectibles_Endurance(fuel, "fuel", "fuel_group", 2.0, 6.0);
        CreateCollectibles_Endurance(repairkit, "repairkit", "repairkit_group", 5.0, 10.0);
        CreateCollectibles_Endurance(coin, "coin", "coin_group", 3.0, 15.0);
        StartGame_Endurance(-200.0);
    }


    public void AddPlayer_Endurance()
    {
        moveUp = new Vector(0, 200);
        moveDown = new Vector(-0, -200);
        moveLeft = new Vector(-200, 0);
        moveRight = new Vector(200, 0);

        fuelRemaining = new DoubleMeter(100.0, 0.0, 100.0);

        player = new PhysicsObject(40.0, 80.0);
        player.Shape = Shape.Rectangle;
        player.Image = LoadImage("carYellow3");
        player.Position = new Vector(0.0, -250.0);
        player.CanRotate = false;
        player.Restitution = 0;
        AddCollisionHandler(player, "debris_group", CollisionWithDebris_Endurance);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel_Endurance);
        AddCollisionHandler(player, "repairkit_group", CollisionWithRepairkit_Endurance);
        AddCollisionHandler(player, "coin_group", CollisionWithCoin_Endurance);
        Add(player);
    }


    public void CreateDebris_Endurance(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer debrisCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax) / enduranceMultipliers[1]);
        debrisCreator.Timeout += delegate
        {
            debrisCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax) / enduranceMultipliers[1];

            PhysicsObject debris = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax) * enduranceMultipliers[2], RandomGen.NextDouble(sizeMin, sizeMax) * enduranceMultipliers[2]);
            debris.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            debris.Angle = RandomGen.NextAngle();
            debris.Image = LoadImage("debris");
            debris.CanRotate = false;
            debris.IgnoresCollisionResponse = true;
            debris.Tag = "debris_group";
            debris.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            debris.AddCollisionIgnoreGroup(1);
            objectGroup.Add(debris);
            Add(debris);
            debris.Hit(gameSpeed * debris.Mass * enduranceMultipliers[3]);
        };
        debrisCreator.Start();
    }


    public void CreateCollectibles_Endurance(PhysicsObject collectible, string collectibleImage, string collectibleGroup, double spawnMin, double spawnMax)
    {
        Timer collectibleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));

        collectibleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                collectibleCreator.Stop();
                return;
            }

            collectibleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            collectible = new PhysicsObject(25.0, 25.0);
            collectible.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            collectible.Image = LoadImage(collectibleImage);
            collectible.CanRotate = false;
            collectible.IgnoresCollisionResponse = true;
            collectible.Tag = collectibleGroup;
            collectible.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            collectible.AddCollisionIgnoreGroup(1);
            objectGroup.Add(collectible);
            Add(collectible);
            collectible.Hit(gameSpeed * collectible.Mass * enduranceMultipliers[3]);
        };

        collectibleCreator.Start();
    }


    public void StartGame_Endurance(double speed)
    {
        AddBackgroundMusic("default_5");

        gameSpeed = new Vector(0.0, speed);

        Camera.StayInLevel = true;
        gameIsOn = true;
    }


    public void AddFuelMeter_Endurance()
    {
        fuelMeter = new Label();
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.Position = new Vector(Screen.Left + 70.0, Screen.Bottom + 140.0);
        fuelMeter.Color = Color.Black;
        fuelMeter.DecimalPlaces = 1;
        Add(fuelMeter);

        fuelLife = new ProgressBar(40.0, 3.0);
        fuelLife.BindTo(fuelRemaining);
        fuelLife.Position = new Vector(fuelMeter.X, fuelMeter.Y);
        fuelLife.Color = Color.Black;
        fuelLife.BorderColor = Color.Black;
        Add(fuelLife);

        GameObject jerrycan = new GameObject(22.5, 27.5);
        jerrycan.Position = new Vector(fuelMeter.X + 50.0, fuelMeter.Y + 6.0);
        jerrycan.Image = LoadImage("jerrycan");
        Add(jerrycan);

        fuelHelpTimer = new Timer();
        fuelHelpTimer.Interval = 0.1;
        fuelHelpTimer.Timeout += delegate
        {
            Keyboard.Listen(Key.Escape, ButtonState.Pressed, ExitGame, "Exit Game");
            fuelRemaining.Value -= 0.5;

            switch (fuelRemaining.Value)
            {
                case double n when (n >= 50.0):
                    fuelMeter.TextColor = Color.LightGreen;
                    fuelLife.BarColor = Color.LightGreen;
                    break;
                case double n when (n < 50.0 && n >= 25.0):
                    fuelMeter.TextColor = Color.Yellow;
                    fuelLife.BarColor = Color.Yellow;
                    break;
                case double n when (n < 25.0 && n >= 10.0):
                    fuelMeter.TextColor = Color.Orange;
                    fuelLife.BarColor = Color.Orange;
                    break;
                case double n when (n < 10.0 && n >= 0.0):
                    fuelMeter.TextColor = Color.Red;
                    fuelLife.BarColor = Color.Red;
                    break;
            }

            if (fuelRemaining.Value == 0.0) FuelRanOut();
        };
        fuelHelpTimer.Start();
    }


    public void AddHullBar_Endurance()
    {
        hullIntegrity = new IntMeter(3, 0, 4);

        hullLife = new ProgressBar(70.0, 8.0);
        hullLife.BindTo(hullIntegrity);
        hullLife.Position = new Vector(Screen.Left + 70.0, Screen.Bottom + 110.0);
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
        health.Position = new Vector(hullLife.X + 50.0, hullLife.Y + 6.0);
        health.Image = LoadImage("health");
        health.Color = Color.Black;
        Add(health);
    }


    public void AddPointMeter_Endurance()
    {
        pointTotal = new DoubleMeter(0.0);
        pointMultiplier = new IntMeter(1, 1, 8);

        pointMeter = new Label();
        pointMeter.BindTo(pointTotal);
        pointMeter.DecimalPlaces = 2;
        pointMeter.TextColor = Color.White;
        pointMeter.Color = Color.Black;
        pointMeter.Position = new Vector(Screen.Left + 70.0, Screen.Bottom + 80.0);
        Add(pointMeter);

        multiplier = new GameObject(27.5, 27.5);
        multiplier.Position = new Vector(pointMeter.X + 50.0, pointMeter.Y + 6.0);
        multiplier.Image = LoadImage("multiplied1");
        multiplier.Color = Color.Black;
        Add(multiplier);

        Timer pointHelpTimer = new Timer(0.1);
        pointHelpTimer.Timeout += delegate
        {
            pointTotal.Value += 0.01 * pointMultiplier.Value * enduranceMultipliers[0];
        };
        pointHelpTimer.Start();
    }


    public void AddZones_Endurance()
    {
        enduranceMultipliers = new double[4] { 1.0, 1.0, 1.0, 1.0 };
        double zoneMultiplier = 1.0;
        double pointBalancer = 2.0;
        double spawnBalancer = 2.0;
        double sizeBalancer = 1.25;
        double speedBalancer = 1.15;

        IntMeter zoneCurrent = new IntMeter(1, 1, 7);
        Label zoneMeter = new Label($"Zone {zoneCurrent.Value}");
        zoneMeter.Position = new Vector(Screen.Left + 70.0, Screen.Bottom + 50.0);
        zoneMeter.Color = Color.Black;
        zoneMeter.TextColor = Color.Orange;
        Add(zoneMeter);

        Timer zoneTimer = new Timer(45.0);
        zoneTimer.Timeout += delegate
        {
            if (zoneCurrent < 7)
            {
                zoneCurrent.Value++;
                zoneMeter.Text = ($"Zone {zoneCurrent.Value}");

                enduranceMultipliers[0] = zoneMultiplier * pointBalancer;
                pointBalancer -= 0.2;
                enduranceMultipliers[1] = zoneMultiplier * spawnBalancer;
                spawnBalancer -= 0.2;
                enduranceMultipliers[2] = zoneMultiplier * sizeBalancer;
                sizeBalancer -= 0.05;
                enduranceMultipliers[3] = zoneMultiplier * speedBalancer;
                speedBalancer -= 0.03;

                SoundEffect zone = LoadSoundEffect("3");
                zone.Play();

                Label zoneUp = new Label("Zone Up!");
                zoneUp.TextColor = Color.Orange;
                Add(zoneUp);

                Timer zoneSwitch = new Timer(5.0);
                zoneSwitch.Timeout += delegate
                {
                    zoneUp.Destroy();
                };
                zoneSwitch.Start(1);
            }
            else
            {
                enduranceMultipliers[0] = 10.0;
                enduranceMultipliers[1] = 10.0;
                enduranceMultipliers[2] = 2.05;
                enduranceMultipliers[3] = 1.55;
                zoneMeter.Text = "Max";
                zoneTimer.Stop();

                Label zoneUp = new Label("Zone Up!");
                zoneUp.TextColor = Color.Orange;
                Add(zoneUp);

                Timer zoneSwitch = new Timer(5.0);
                zoneSwitch.Timeout += delegate
                {
                    zoneUp.Destroy();
                };
                zoneSwitch.Start(1);
            }
        };
        zoneTimer.Start();
    }


    public void CollisionWithDebris_Endurance(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect crash = LoadSoundEffect("intense_explosion");
            crash.Play();
            target.Destroy();
            hullIntegrity.Value--;

            if (gameMode == "endurance")
            {
                pointMultiplier.Value = 1;
                multiplier.Image = LoadImage("multiplied1");
            }

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


    private void CollisionWithFuel_Endurance(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect replenish = LoadSoundEffect("fuel");
            replenish.Play();
            target.Destroy();
            double add = RandomGen.NextDouble(10.0, 30.0);
            fuelRemaining.Value += add;
            string addition = add.ToString();

            Label fuelAdded = new Label();
            fuelAdded.Text = "+ " + addition.Substring(0, 4) + " liters";
            fuelAdded.Position = target.Position;
            fuelAdded.TextColor = Color.LightGreen;
            Add(fuelAdded);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                fuelAdded.Destroy();
                displayTimer.Stop();
            };

            displayTimer.Start();
        }
    }


    private void CollisionWithRepairkit_Endurance(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect improvement = LoadSoundEffect("repairkit");
            improvement.Play();
            target.Destroy();
            hullIntegrity.Value++;

            if (hullIntegrity.Value == hullIntegrity.MaxValue) pointTotal.Value += 5.0;

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


    private void CollisionWithCoin_Endurance(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect money = LoadSoundEffect("5");
            money.Play();
            target.Destroy();

            if (pointMultiplier.Value == 8) pointTotal.Value += 10.0;
            pointMultiplier.Value = pointMultiplier.Value * 2;

            switch (pointMultiplier.Value)
            {
                case 2:
                    multiplier.Image = LoadImage("multiplied2");
                    break;
                case 4:
                    multiplier.Image = LoadImage("multiplied4");
                    break;
                case 8:
                    multiplier.Image = LoadImage("multiplied8");
                    break;
            }

            Label pointsMultiplied = new Label();
            pointsMultiplied.Text = "Points X2";
            pointsMultiplied.Position = target.Position;
            pointsMultiplied.TextColor = Color.RosePink;
            Add(pointsMultiplied);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                pointsMultiplied.Destroy();
                displayTimer.Stop();
            };

            displayTimer.Start();
        }
    }

}