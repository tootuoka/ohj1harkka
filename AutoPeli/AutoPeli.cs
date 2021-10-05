using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class autopeli : PhysicsGame
{
    Vector moveUp = new Vector(0, 200);
    Vector moveDown = new Vector(-0, -200);
    Vector moveLeft = new Vector(-200, 0);
    Vector moveRight = new Vector(200, 0);
    Vector speed;

    string playerName;
    int arcadeDifficulty = 0;
    bool finishlineSpawned = false;
    bool gameIsOn = false;
    bool gamePassed = false;
    bool gameFullyUnlocked = true;
    bool firstCompletion = true;

    List<Label> mainMenuButtons;
    List<Label> difficultyMenuButtons;
    List<Label> endMenuButtons;

    List<PhysicsObject> objectGroup = new List<PhysicsObject>();

    PhysicsObject player;
    PhysicsObject finishline;
    PhysicsObject rightBorder;
    PhysicsObject leftBorder;
    PhysicsObject bottomBorder;

    IntMeter hullIntegrity = new IntMeter(3, 0, 4);
    ProgressBar hullLife;

    DoubleMeter distanceRemaining = new DoubleMeter(1.0, 0.0, 5.0);
    Label distanceMeter = new Label();
    Timer distanceHelpTimer = new Timer();

    DoubleMeter fuelRemaining = new DoubleMeter(100.0, 0.0, 100.0);
    Label fuelMeter = new Label();
    Timer fuelHelpTimer = new Timer();

    DoubleMeter endTimer = new DoubleMeter(3);
    Label endTimerDisplay = new Label();
    Timer endHelpTimer = new Timer();
    Label endReason = new Label();

    ScoreList hiscores = new ScoreList(20, false, 0);
    HighScoreWindow hiscoresWindow;

    Timer hovertime = new Timer();
    Label fuelAdded = new Label();

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
        AddRoad();
        SetControls();

        Level.BackgroundColor = Color.Gray;
        Camera.ZoomToLevel();

        if (difficulty == "easy")
        {
            arcadeDifficulty = 1;
            distanceRemaining.Value += 0.50;
            AddDebris(RandomGen.NextDouble(15, 30), RandomGen.NextDouble(15, 30), 50);
            AddFuel(10);
            AddRepairkit(3);
            StartGame(-250.0);
            return;
        }
        else if (difficulty == "medium")
        {
            arcadeDifficulty = 2;
            distanceRemaining.Value += 1.50;
            AddDebris(RandomGen.NextDouble(20, 40), RandomGen.NextDouble(20, 40), 75);
            AddFuel(8);
            AddRepairkit(2);
            StartGame(-300.0);
            return;
        }
        else if (difficulty == "hard")
        {
            arcadeDifficulty = 3;
            distanceRemaining.Value += 3.00;
            AddDebris(RandomGen.NextDouble(25, 50), RandomGen.NextDouble(25, 50), 100);
            AddFuel(6);
            AddRepairkit(1);
            StartGame(-350.0);
            return;
        }
        MainMenu();
    }


    public void AddPlayer()
    {
        player = new PhysicsObject(40.0, 80.0);
        player.Shape = Shape.Rectangle;
        player.Image = LoadImage("carYellow3");
        player.Y = -150.0;
        player.CanRotate = false;
        player.Restitution = 0.35;
        AddCollisionHandler(player, "debris_group", CollisionWithDebris);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel);
        AddCollisionHandler(player, "repairkit_group", CollisionWithRepairkit);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline);
        Add(player);
    }


    public void AddBorders()
    {
        PhysicsObject bottomBorder = Level.CreateBottomBorder();
        bottomBorder.Restitution = 0.5;
        bottomBorder.IsVisible = false;

        AddCollisionHandler(bottomBorder, "debris_group", DebrisAvoided);
    }


    public void AddRoad()
    {
        // TODO: Lisää tien ominaisuudet (keskiviivat??).
    }


    public void AddDebris(double debrisX, double debrisY, int debrisAmount)
    {
        /*Timer debrisCreator = new Timer();
        debrisCreator.Interval = RandomGen.NextDouble(0.0, 1.0);
        debrisCreator.Timeout += CreateDebris;
        debrisCreator.Start();*/

        for (int i = 0; i < debrisAmount + 1; i++)
        {
            PhysicsObject debris = new PhysicsObject(debrisX, debrisY);
            debris.X = RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0);
            debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 10000.0);
            debris.Angle = RandomGen.NextAngle();
            debris.Color = Color.White;
            debris.Image = LoadImage("debris");
            debris.CanRotate = false;
            debris.IgnoresCollisionResponse = true;
            debris.Tag = "debris_group";
            objectGroup.Add(debris);
            Add(debris);
        }
    }

    /*private void CreateDebris(double debrisX, double debrisY, int debrisAmount)
    {
        PhysicsObject debris = new PhysicsObject(debrisX, debrisY);
        debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
        debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
        debris.Shape = RandomGen.NextShape();
        debris.Angle = RandomGen.NextAngle();
        debris.Color = Color.White;
        // TODO: debris.Image = ???.
        debris.CanRotate = false;
        debris.IgnoresCollisionResponse = true;
        debris.Tag = "debris_group";
        objectGroup.Add(debris);
        Add(debris);
    }*/

    public void AddFuel(int fuelAmount)
    {
        for (int i = 0; i < fuelAmount + 1; i++)
        {
            PhysicsObject fuel = new PhysicsObject(20.0, 25.0);
            fuel.X = RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0);
            fuel.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 10000.0);
            fuel.Image = LoadImage("fuel");
            fuel.CanRotate = false;
            fuel.IgnoresCollisionResponse = true;
            fuel.Tag = "fuel_group";
            objectGroup.Add(fuel);
            Add(fuel);
        }
    }


    public void AddRepairkit(int repairkitAmount)
    {
        for (int i = 0; i < repairkitAmount + 1; i++)
        {
            PhysicsObject repairkit= new PhysicsObject(25.0, 25.0);
            repairkit.X = RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0);
            repairkit.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 10000.0);
            repairkit.Image = LoadImage("repairkit");
            repairkit.CanRotate = false;
            repairkit.IgnoresCollisionResponse = true;
            repairkit.Tag = "repairkit_group";
            objectGroup.Add(repairkit);
            Add(repairkit);
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

        AddBorders();
        AddDistanceMeter();
        AddFuelMeter();
        AddHullBar();

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
        distanceMeter.TextColor = Color.White;
        distanceMeter.Color = Color.Black;
        distanceMeter.DecimalPlaces = 3;
        distanceMeter.Position = new Vector(Screen.Right - 62.5, Screen.Top - 120.0);
        Add(distanceMeter);

        PhysicsObject road = new PhysicsObject(25.0, 25.0);
        road.Position = new Vector(distanceMeter.X - 52.5, distanceMeter.Y - 6.0);
        road.CanRotate = false;
        road.IgnoresCollisionResponse = true;
        road.Image = LoadImage("road");
        Add(road);

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
            Add(finishline);
            finishline.Hit(speed * finishline.Mass);
            finishlineSpawned = true;
        }
    }


    public void AddFuelMeter()
    {
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.Position = new Vector(Screen.Right - 70.0, Screen.Top - 40.0);
        fuelMeter.Color = Color.Black;
        fuelMeter.DecimalPlaces = 1;
        Add(fuelMeter);

        PhysicsObject jerrycan = new PhysicsObject(22.5, 27.5);
        jerrycan.Position = new Vector(fuelMeter.X - 46.0, fuelMeter.Y - 7.5);
        jerrycan.CanRotate = false;
        jerrycan.IgnoresCollisionResponse = true;
        jerrycan.Image = LoadImage("jerrycan");
        Add(jerrycan);

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
            case double n when (n >= 50.0): fuelMeter.TextColor = Color.LightGreen; break;
            case double n when (n < 50.0 && n >= 25.0): fuelMeter.TextColor = Color.Yellow; break;
            case double n when (n < 25.0 && n >= 10.0): fuelMeter.TextColor = Color.Orange; break;
            case double n when (n < 10.0 && n >= 0.0): fuelMeter.TextColor = Color.Red; break;
        }

        if (fuelRemaining.Value <= 0.0)
        {
            FuelRanOut();
        }
    }


    public void AddHullBar()
    {
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

        PhysicsObject health = new PhysicsObject(27.5, 27.5);
        health.Position = new Vector(hullLife.X - 61.0, hullLife.Y - 6.0);
        health.CanRotate = false;
        health.IgnoresCollisionResponse = true;
        health.Image = LoadImage("health");
        health.Color = Color.Black;
        Add(health);
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

            fuelAdded.Text = "+ " + addition.Substring(0, 4) + " liters";
            fuelAdded.Position = target.Position;
            fuelAdded.TextColor = Color.LightGreen;
            Add(fuelAdded);

            hovertime.Interval = 0.5;
            hovertime.Timeout += RemoveHover;
            hovertime.Start();
        }
    }

    public void RemoveHover()
    {
        fuelAdded.Destroy();
        hovertime.Stop();
        hovertime.Reset();
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


    public void DebrisAvoided(PhysicsObject bottom, PhysicsObject target)
    {
        target.Destroy();
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
        if (arcadeDifficulty >= 3)
        {
            gameFullyUnlocked = true; 
        }

        gameIsOn = false;
        gamePassed = true;
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.D);

        distanceHelpTimer.Stop();
        fuelHelpTimer.Stop();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Velocity = Vector.Zero;
        }

        finishline.Velocity = Vector.Zero;
        player.Hit(new Vector(0.0, 1000.0));

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

        distanceHelpTimer.Stop();
        fuelHelpTimer.Stop();

        foreach (PhysicsObject x in objectGroup)
        {
            x.Velocity = Vector.Zero;
        }

        player.Velocity = Vector.Zero;

        EndCountdown(lossMessage);

        Keyboard.Listen(Key.Enter, ButtonState.Pressed, EndMenu, null, "loss");
    }


    public void EndCountdown(string endMessage)
    {
        if (endTimer.Value == 3)
        {
            endReason.Text = endMessage;
            endReason.Y = -80;
            Add(endReason);

            endTimerDisplay.Y = -120;
            endTimerDisplay.BindTo(endTimer);
            Add(endTimerDisplay);
        }

        endHelpTimer.Interval = 1;
        endHelpTimer.Timeout += EndHelpTimer_Timeout;
        endHelpTimer.Start();
    }


    void EndHelpTimer_Timeout()
    {
        endTimer.Value -= 1;

        if (endTimer.Value == 0.0 || endTimerDisplay.Text == "Press Enter to Continue")
        {
            endHelpTimer.Stop();
            endTimerDisplay.Text = "Press Enter to Continue";
        }
    }


    public void EndMenu(string instance)
    {
        SoundEffect x = LoadSoundEffect(instance);
        x.Play();

        endReason.Destroy();
        endTimerDisplay.Destroy();

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
        //Mouse.ListenOn(retry, MouseButton.Left, ButtonState.Pressed,  Retry, null, /* TODO: ???.*//*);
        //Mouse.ListenOn(changeDifficulty, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        //Mouse.ListenOn(hiscores, MouseButton.Left, ButtonState.Pressed, DifficultySelection, null);
        //Mouse.ListenOn(quit, MouseButton.Left, ButtonState.Pressed, MainMenu, null);
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


    public Label CreateButton(string buttonText, double buttonY)
    {
        Label button = new Label(buttonText);
        button.Y = buttonY;
        mainMenuButtons.Add(button);
        return button;
    }


    public void DisplayUnlockMessage()
    {
        firstCompletion = false;
        Label unlocks = new Label("You have beaten Arcade Mode on hard difficulty and unlocked Endurance Mode!");
    }
   // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");
}
