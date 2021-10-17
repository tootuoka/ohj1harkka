using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CarAvatar : GameObject
{
    public CarAvatar(double width, double height, int mobility = 1, int durability = 1, int consumption = 1, int capacity = 1) : base(width, height)
    {
        int Mobility = mobility;
        int Durability = durability;
        int Consumption = consumption;
        int Capacity = capacity;
    }
}

public class autopeli : PhysicsGame
{
    Vector gameSpeed;

    string car;
    string difficulty;
    string playerName;
    bool standardMode;
    bool finishlineSpawned;
    bool gameIsOn;
    bool gamePassed;
    bool gameFullyUnlocked = true;
    bool firstCompletion = true;
    double fuelConsumptionMultiplier;
    private bool descriptionExists = false;

    Label difficultyDescription;

    double[] zoneMultipliers;

    List<Image> carConditions;

    List<Timer> itemCreationTimers;

    GameObject multiplier;

    Label carInfo;
    List<GameObject> carList;
    List<Label> carNameList;

    List<Label> carMobilityList;
    List<Label> carDurabilityList;
    List<Label> carConsumptionList;
    List<Label> carCapacityList;

    List<GameObject[][]> allActiveStars;

    List <PhysicsObject> objectGroup;

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
    Timer pointHelpTimer;

    ScoreList hiscores = new ScoreList(20, false, 0);
    HighScoreWindow hiscoresWindow;


    public override void Begin()
    {
        hiscores = DataStorage.TryLoad<ScoreList>(hiscores, "hiscores.xml");
        SetPlayerName();
    }


    public void SetPlayerName()
    {
        InputWindow nameQuery = new InputWindow("Player Name: ");
        nameQuery.TextEntered += delegate { playerName = nameQuery.InputBox.Text; };
        nameQuery.Closed += delegate { MainMenu(); };
        Add(nameQuery);
    }

    
    public void MainMenu()
    {
        ClearAll();

        
        Level.Background.Image = LoadImage("mainmenu_bgimg");

        Label mainMenuTitle = CreateLabel("MAIN MENU", Color.White, y: 200, scale: 1.2);
        mainMenuTitle.BorderColor = Color.White;
        Add(mainMenuTitle);

        Label playerIndicator = CreateLabel($"Player: {playerName}", Color.Gray, -220, 255, 0.5);
        Add(playerIndicator);

        Label[] mainMenuButtons = new Label[4] { CreateLabel("Arcade Mode", Color.White, y: 60.0), CreateLabel("Endurance Mode", Color.White, y: 20.0), CreateLabel("Hiscores", Color.White, y: -20.0), CreateLabel("Exit", Color.White, y: -60.0) };
        foreach (Label button in mainMenuButtons) Add(button);

        if (gameFullyUnlocked)
        {
            if (firstCompletion) DisplayUnlockMessage();

            //AddBackgroundMusic("menu_cmpl");
            Mouse.ListenMovement(0.5, MainMenuMovement, null, mainMenuButtons);
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "endurance");
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
        else
        {
            //AddBackgroundMusic("menu_orig");
            Mouse.ListenMovement(0.5, MainMenuMovement, null, mainMenuButtons);
            mainMenuButtons[1].TextColor = Color.Gray;
            mainMenuButtons[2].TextColor = Color.Gray;
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
    }


    public void MainMenuMovement(Label[] mainMenuButtons)
    {
        if (gameFullyUnlocked)
        {
            foreach (Label button in mainMenuButtons)
            {
                if (Mouse.IsCursorOn(button))
                {
                    button.TextColor = Color.Gold;
                    button.TextScale = new Vector(1.05, 1.05);
                }
                else
                {
                    button.TextColor = Color.White;
                    button.TextScale = new Vector(1, 1);
                }
            }
        }
        else
        {
            for (int i = 0; i < 4; i += 3)
            {
                if (Mouse.IsCursorOn(mainMenuButtons[i]))
                {
                    mainMenuButtons[i].TextColor = Color.Gold;
                    mainMenuButtons[i].TextScale = new Vector(1.05, 1.05);
                }
                else
                {
                    mainMenuButtons[i].TextColor = Color.White;
                    mainMenuButtons[i].TextScale = new Vector(1, 1);
                }
            }
        }
    }


    public void DifficultyMenu()
    {
        ClearAll();

        Level.BackgroundColor = Color.Gray;

        Label difficultyMenuTitle = CreateLabel("DIFFICULTY SELECTION", Color.White, y: 200, scale: 1.2);
        Add(difficultyMenuTitle);

        List<Color> buttonColors = new List<Color>() { Color.LightGreen, Color.GreenYellow, Color.Yellow };

        List<string> descriptions = new List<string>() { "Very easy and meant only for practicing the game basics.",
                                                       "Main difficulty of the game.\nComplete this to unlock new content.",
                                                       "Challenge yourself and take on\nthe full might of the developer!"};

        List<Label> difficultyMenuButtons = new List<Label>() { CreateLabel("Beginner", Color.White, y: 50.0, scale: 1.1), CreateLabel("Standard", Color.White, scale: 1.1) };

        if (gameFullyUnlocked)
        {
            difficultyMenuButtons.Add(CreateLabel("Madness", Color.White, y: -50, scale: 1.1));
            Mouse.ListenOn(difficultyMenuButtons[2], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "Madness");
        }
        foreach (Label button in difficultyMenuButtons) Add(button);

        object[] difficultyButtonParameters = new object[2] { descriptions, difficultyDescription };

        Mouse.ListenMovement(0.5, DifficultyMenuMovement, null, difficultyMenuButtons, buttonColors, descriptions);
        Mouse.ListenOn(difficultyMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "Beginner");
        Mouse.ListenOn(difficultyMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "Standard");
    }


    public void DifficultyMenuMovement(List<Label> difficultyMenuButtons, List<Color> buttonColors, List<string> descriptions)
    {
        for (int i = 0; i < difficultyMenuButtons.Count; i++)
        {
            if (Mouse.IsCursorOn(difficultyMenuButtons[i]))
            {
                difficultyMenuButtons[i].TextColor = buttonColors[i];
                difficultyMenuButtons[i].TextScale = new Vector(1.2, 1.2);
                difficultyMenuButtons[i].BorderColor = buttonColors[i];

                if (!descriptionExists)
                {
                    difficultyDescription = CreateLabel(descriptions[i], buttonColors[i], y: -150, scale: 0.65);
                    Add(difficultyDescription);
                    descriptionExists = true;
                }

                break;
                // TODO: Add sound to menu buttons.
            }
            else
            {
                difficultyMenuButtons[i].TextColor = Color.White;
                difficultyMenuButtons[i].TextScale = new Vector(1.1, 1.1);
                difficultyMenuButtons[i].BorderColor = Color.Transparent;

                if (descriptionExists)
                {
                    difficultyDescription.Destroy();
                    descriptionExists = false;
                }
            }
        }
    }


    public void CarMenu(string selectedDifficulty)
    {
        ClearAll();

        difficulty = selectedDifficulty;
        Level.BackgroundColor = Color.Gray;

        carInfo = CreateLabel("CAR-INFO", Color.White, -300, 300, 0.55);
        carInfo.BorderColor = Color.White;
        carInfo.Color = Color.Black;
        Add(carInfo);

        Label[] descriptions = new Label[4] { CreateLabel("MOB stands for mobility and defines how easily the car maneuvers around the stage", Color.Red, 0, Screen.Top - 50, 0.7, false),
                                              CreateLabel("DUR stands for durability and defines how resistant the car is against crash-inflicted damage", Color.Yellow, 0, Screen.Top - 70, 0.7, false),
                                              CreateLabel("CON stands for consumption and defines how conservative the car is in its fuel usage", Color.JungleGreen, 0, Screen.Top - 90, 0.7, false),
                                              CreateLabel("CAP stands for capacity and defines the car's fuel tank size", Color.SkyBlue, 0, Screen.Top - 110, 0.7, false) };
        foreach (Label description in descriptions) Add(description);

        CreateCarSelectionItems();
        AddPassiveStars();
        AddActiveStars();

        Mouse.ListenMovement(0.5, CarMenuMovement, null, descriptions);

        Mouse.ListenOn(carList[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Basic");
        Mouse.ListenOn(carList[1], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Sports");
        Mouse.ListenOn(carList[2], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Power");
        Mouse.ListenOn(carList[3], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Heavy");
        Mouse.ListenOn(carList[4], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Super");
    }


    public void CreateStage(string selectedCar)
    {
        ClearAll();

        AddPlayer(selectedCar);

        standardMode = false;
        finishlineSpawned = false;
        gamePassed = false;
        gameIsOn = true;


        objectGroup = new List<PhysicsObject>();

        distanceRemaining = new DoubleMeter(1.0, 0.0, 5.0);        

        Level.BackgroundColor = Color.Gray;

        PhysicsObject[] collectibles = new PhysicsObject[3];

        switch (difficulty)
        {
            case "beginner":
                AddDistanceMeter();
                distanceRemaining.Value += 0.50;
                CreateDebris(12.5, 30.0, 0.1, 1.2);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 1.5, 3.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 3.0, 6.0);
                StartGame(-250.0);
                break;
            case "standard":
                standardMode = true;
                AddDistanceMeter();
                distanceRemaining.Value += 1.50;
                CreateDebris(12.5, 30.0, 0.05, 0.8);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 2.0, 4.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 6.0, 8.0);
                StartGame(-300.0);
                break;
            case "madness":
                AddDistanceMeter();
                distanceRemaining.Value += 3.00;
                CreateDebris(12.5, 30.0, 0.0, 0.4);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 2.5, 5.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 9.0, 10.0);
                StartGame(-350.0);
                break;
            case "endurance":
                itemCreationTimers = new List<Timer>();
                AddPointMeter();
                AddZones();
                CreateDebris(10.0, 30.0, 0.5, 2.0);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 2.0, 6.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 5.0, 10.0);
                CreateCollectibles(collectibles[2], "coin", "coin_group", 3.0, 15.0);
                StartGame(-200.0);
                break;
        }
    }


    public void AddPlayer(string selectedCar)
    {
        car = selectedCar;

        player = new PhysicsObject(40, 80);
        player.Shape = Shape.Rectangle;
        player.CanRotate = false;
        player.Restitution = -1;
        player.Position = new Vector(0.0, -250.0);

        Vector[] playerMovements = new Vector[4];

        switch (selectedCar)
        {
            case "car_Basic":
                player.Image = LoadImage("car1");
                playerMovements = new Vector[4] { new Vector(0, 250), new Vector(0, -250), new Vector(-250, 0), new Vector(250, 0) };
                hullIntegrity = new IntMeter(3, 0, 3);
                fuelConsumptionMultiplier = 1.3;
                fuelRemaining = new DoubleMeter(110.0, 0.0, 100.0);
                carConditions = new List<Image>() { LoadImage("car1_3"), LoadImage("car1_2"), LoadImage("car1_1"), LoadImage("car1") };
                break;
            case "car_Sports":
                player.Image = LoadImage("car2");
                playerMovements = new Vector[4] { new Vector(0, 300), new Vector(0, -300), new Vector(-300, 0), new Vector(300, 0) };
                hullIntegrity = new IntMeter(2, 0, 2);
                fuelConsumptionMultiplier = 1.0;
                fuelRemaining = new DoubleMeter(70.0, 0.0, 70.0);
                carConditions = new List<Image>() { LoadImage("car2_2"), LoadImage("car2_1"), LoadImage("car2") };
                break;
            case "car_Power":
                player.Image = LoadImage("car3");
                playerMovements = new Vector[4] { new Vector(0, 200), new Vector(0, -200), new Vector(-200, 0), new Vector(200, 0) };
                hullIntegrity = new IntMeter(4, 0, 4);
                fuelConsumptionMultiplier = 2.1;
                fuelRemaining = new DoubleMeter(130.0, 0.0, 130.0);
                carConditions = new List<Image>() { LoadImage("car3_4"), LoadImage("car3_3"), LoadImage("car3_2"), LoadImage("car3_1"), LoadImage("car3") };
                break;
            case "car_Heavy":
                player.Image = LoadImage("car4");
                playerMovements = new Vector[4] { new Vector(0, 150), new Vector(0, -150), new Vector(-150, 0), new Vector(150, 0) };
                hullIntegrity = new IntMeter(5, 0, 5);
                fuelConsumptionMultiplier = 1.9;
                fuelRemaining = new DoubleMeter(150.0, 0.0, 150.0);
                carConditions = new List<Image>() { LoadImage("car4_5"), LoadImage("car4_4"), LoadImage("car4_3"), LoadImage("car4_2"), LoadImage("car4_1"), LoadImage("car4") };
                break;
            case "car_Super":
                player.Image = LoadImage("car5");
                playerMovements = new Vector[4] { new Vector(0, 350), new Vector(0, -350), new Vector(-350, 0), new Vector(350, 0) };
                hullIntegrity = new IntMeter(1, 0, 1);
                fuelConsumptionMultiplier = 1.6;
                fuelRemaining = new DoubleMeter(90.0, 0.0, 90.0);
                carConditions = new List<Image>() { LoadImage("car5_1"), LoadImage("car5") };
                break;

                // TODO: CreateCar():lla switchin autojen luonti?
        }

        SetControls(playerMovements);

        CollisionHandler<PhysicsObject, PhysicsObject> DebrisHandler = (player, target) => CollisionWithDebris(player, target, carConditions);
        CollisionHandler<PhysicsObject, PhysicsObject> RepairkitHandler = (player, target) => CollisionWithRepairkit(player, target, carConditions);

        AddCollisionHandler(player, "debris_group", DebrisHandler);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel);
        AddCollisionHandler(player, "repairkit_group", RepairkitHandler);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline);
        AddCollisionHandler(player, "coin_group", CollisionWithCoin);
        Add(player);
    }


    public void CreateBorders()
    {
        PhysicsObject[] borders = new PhysicsObject[4] { Level.CreateTopBorder(-1, false), Level.CreateBottomBorder(-1, false), Level.CreateLeftBorder(-1, false), Level.CreateRightBorder(-1, false) };
        
        foreach (PhysicsObject border in borders)
        {
            border.Tag = "border_group";
            border.AddCollisionIgnoreGroup(1);
            Add(border);
        }
    }


    public void AddRoadMidline()
    {
        Timer roadMidlineCreator = new Timer(0.8);

        roadMidlineCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                roadMidlineCreator.Stop();
                return;
            }

            PhysicsObject roadMidline = new PhysicsObject(8.0, 50.0);
            roadMidline.Position = new Vector(0.0, 360.0 + roadMidline.Height);
            roadMidline.CanRotate = false;
            roadMidline.IgnoresCollisionResponse = true;
            roadMidline.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            objectGroup.Add(roadMidline);
            Add(roadMidline);
            roadMidline.Hit(gameSpeed * roadMidline.Mass);
        };
        roadMidlineCreator.Start();
    }


    public void CreateDebris(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer debrisCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        if (difficulty == "endurance") itemCreationTimers.Add(debrisCreator);

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

        // TODO: Lisää zone multiplierit enduranceen.
    }


    public void CreateCollectibles(PhysicsObject collectible, string collectibleImage, string collectibleGroup, double spawnMin, double spawnMax)
    {
        Timer collectibleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        if (difficulty == "endurance") itemCreationTimers.Add(collectibleCreator);

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

        // TODO: Lisää enduranceen zone multiplierit.
    }


    public void StartGame(double speed)
    {
        AddBackgroundMusic("default_5");

        gameSpeed = new Vector(0.0, speed);

        Camera.StayInLevel = true;

        CreateBorders();
        AddRoadMidline();
        AddFuelMeter();
        AddHullBar();
    }


    public void SetControls(Vector[] playerMovements)
    {
        Keyboard.Listen(Key.W, ButtonState.Pressed, SetPlayerMovementSpeed, "Accelerate", playerMovements[0]);
        Keyboard.Listen(Key.W, ButtonState.Released, SetPlayerMovementSpeed, null, -playerMovements[0]);
        Keyboard.Listen(Key.S, ButtonState.Pressed, SetPlayerMovementSpeed, "Decelerate", playerMovements[1]);
        Keyboard.Listen(Key.S, ButtonState.Released, SetPlayerMovementSpeed, null, -playerMovements[1]);
        Keyboard.Listen(Key.A, ButtonState.Pressed, SetPlayerMovementSpeed, "Steer left", playerMovements[2]);
        Keyboard.Listen(Key.A, ButtonState.Released, SetPlayerMovementSpeed, null, -playerMovements[2]);
        Keyboard.Listen(Key.D, ButtonState.Pressed, SetPlayerMovementSpeed, "Steer right", playerMovements[3]);
        Keyboard.Listen(Key.D, ButtonState.Released, SetPlayerMovementSpeed, null, -playerMovements[3]);

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
        fuelMeter.Position = new Vector(Screen.Right - 70.0, Screen.Bottom + 35.0);
        fuelMeter.Color = Color.Black;
        fuelMeter.DecimalPlaces = 1;
        Add(fuelMeter);

        fuelLife = new ProgressBar(40.0, 3.0);
        fuelLife.BindTo(fuelRemaining);
        fuelLife.Position = new Vector(Screen.Right - 69, Screen.Bottom + 53.0);
        fuelLife.Color = Color.Black;
        fuelLife.BorderColor = Color.Black;
        Add(fuelLife);

        GameObject jerrycan = new GameObject(22.5, 27.5);
        jerrycan.Position = new Vector(Screen.Right - 116.0, Screen.Bottom + 47.5);
        jerrycan.Image = LoadImage("jerrycan");
        Add(jerrycan);

        fuelHelpTimer = new Timer(0.1);
        if (difficulty == "endurance") itemCreationTimers.Add(fuelHelpTimer);
        fuelHelpTimer.Timeout += delegate
        {
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


    public void AddHullBar()
    {
        hullLife = new ProgressBar(70.0, 8.0);
        hullLife.BindTo(hullIntegrity);
        hullLife.Position = new Vector(Screen.Right - 54.0, Screen.Bottom + 80.0);
        hullLife.Color = Color.Black;
        hullLife.BorderColor = Color.Black;

        switch (hullIntegrity.Value)
        {
            case 5: hullLife.BarColor = Color.Aqua; Add(hullLife); break;
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


    public void AddPointMeter()
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

        pointHelpTimer = new Timer(0.1);
        itemCreationTimers.Add(pointHelpTimer);
        pointHelpTimer.Timeout += delegate
        {
            pointTotal.Value += 0.01 * pointMultiplier.Value * zoneMultipliers[0];
        };
        pointHelpTimer.Start();
    }


    public void SetPlayerMovementSpeed(Vector direction)
    {
        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-10);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(10);
        else player.Angle = Angle.FromDegrees(0);
    }


    public void CollisionWithDebris(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        if (gameIsOn)
        {
            SoundEffect crash = LoadSoundEffect("intense_explosion");
            crash.Play();
            target.Destroy();
            hullIntegrity.Value--;

            if (difficulty == "endurance" && pointMultiplier.Value > 1)
            {
                pointMultiplier.Value /= 2;
                //multiplier.Image = LoadImage("multiplied1");
            }

            ChangeCarCondition(conditions);
        }

        // TODO: enduranceen zone multiplierit.
    }


    private void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect replenish = LoadSoundEffect("fuel");
            replenish.Play();
            target.Destroy();
            double add = RandomGen.NextDouble(10.0, 30.0);
            fuelRemaining.Value += add;
            string addition = add.ToString();

            Label fuelAdded = CreateLabel("+ " + addition.Substring(0, 4) + " liters", Color.LightGreen, target.X, target.Y);
            Add(fuelAdded);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                fuelAdded.Destroy();
                displayTimer.Stop();
            };
            displayTimer.Start();

            // TODO: enduranceen zone multiplierit.
        }
    }


    private void CollisionWithRepairkit(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        if (gameIsOn)
        {
            SoundEffect improvement = LoadSoundEffect("repairkit");
            improvement.Play();
            target.Destroy();
            hullIntegrity.Value++;

            ChangeCarCondition(conditions);
        }

        // TODO: enduranceen zone multiplierit.
    }


    private void CollisionWithFinishline(PhysicsObject player, PhysicsObject target)
    {
        gamePassed = true;
        if (gameIsOn) GameEnd("You made it!");

        finishline.Velocity = Vector.Zero;
        player.AddCollisionIgnoreGroup(1);
        player.Hit(new Vector(0.0, 400.0) * player.Mass);
        player.LifetimeLeft = TimeSpan.FromSeconds(3.0);
    }


    private void CollisionWithCoin(PhysicsObject player, PhysicsObject target)
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
                case 2: multiplier.Image = LoadImage("multiplied2"); break;
                case 4: multiplier.Image = LoadImage("multiplied4"); break;
                case 8: multiplier.Image = LoadImage("multiplied8"); break;
            }

            Label pointsMultiplied = CreateLabel("Points X2", Color.RosePink, target.X, target.Y);
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


    private void ChangeCarCondition(List<Image> conditions)
    {
        switch (hullIntegrity.Value)
        {
            case 5:
                hullLife.BarColor = Color.Aqua;
                player.Image = conditions[5];
                break;
            case 4:
                hullLife.BarColor = Color.LightGreen;
                player.Image = conditions[4];
                break;
            case 3:
                hullLife.BarColor = Color.Yellow;
                player.Image = conditions[3];
                break;
            case 2:
                hullLife.BarColor = Color.Orange;
                player.Image = conditions[2];
                break;
            case 1:
                hullLife.BarColor = Color.Red;
                player.Image = conditions[1];
                break;
            case 0:
                hullLife.BarColor = Color.Black;
                player.Image = conditions[0];
                ExplodeCar();
                break;
        }
    }


    public void ExplodeCar()
    {
        gamePassed = false;
        Explosion carExplosion = new Explosion(5 * player.Width);
        carExplosion.Position = player.Position;
        carExplosion.UseShockWave = false;
        carExplosion.Speed = 300.0;
        carExplosion.Sound = LoadSoundEffect("1");
        Add(carExplosion);
        SoundEffect destruction = LoadSoundEffect("destruction");
        destruction.Play();

        player.Velocity = Vector.Zero;

        GameEnd("Your car broke down!");
    }


    public void FuelRanOut()
    {
        gamePassed = false;
        SoundEffect empty = LoadSoundEffect("fuel_out");
        empty.Play();

        player.Velocity = Vector.Zero;

        GameEnd("You ran out of fuel!");
    }


    public void GameEnd(string message)
    {
        gameIsOn = false;
        if (gamePassed && standardMode && firstCompletion) gameFullyUnlocked = true;

        DisableControls();
        StopGameTimers();
        EndCountdown(message);

        foreach (PhysicsObject item in objectGroup) item.LifetimeLeft = TimeSpan.FromMinutes(2);
        foreach (PhysicsObject x in objectGroup) x.Velocity = Vector.Zero;

        if (gamePassed) Keyboard.Listen(Key.Enter, ButtonState.Pressed, EndMenu, null, "win");
        else Keyboard.Listen(Key.Enter, ButtonState.Pressed, EndMenu, null, "loss");
    }

    public void DisableControls()
    {
        Keyboard.Disable(Key.W);
        Keyboard.Disable(Key.S);
        Keyboard.Disable(Key.A);
        Keyboard.Disable(Key.D);

        // TODO: Muut näppäimet.
    }


    public void StopGameTimers()
    {
        if (difficulty != "endurance") distanceHelpTimer.Stop();
        else pointHelpTimer.Stop();

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

        Label[] endMenuButtons = new Label[3] { CreateLabel("Retry", Color.Black, y: 50), new Label(), CreateLabel("MainMenu", Color.Black, y: -50) };
        
        if (difficulty != "endurance")
        {
            endMenuButtons[1] = CreateLabel("Change Difficulty", Color.Black, y: 0);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
        }
        else
        {
            endMenuButtons[1] = CreateLabel("Hiscores", Color.Black, y: 0);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
        }

        foreach (Label button in endMenuButtons) Add(button);

        Mouse.ListenMovement(1.0, EndMenuMovement, null, endMenuButtons);
        Mouse.ListenOn(endMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, car);
        Mouse.ListenOn(endMenuButtons[2], MouseButton.Left, ButtonState.Pressed, MainMenu, null);
    }


    private void EndMenuMovement(Label[] endMenuButtons)
    {
        for (int i = 0; i < endMenuButtons.Length; i++)
        {
            if (Mouse.IsCursorOn(endMenuButtons[i]))
            {
                endMenuButtons[i].TextColor = Color.White;
                endMenuButtons[i].TextScale = new Vector(1.1, 1.1);
            }
            else
            {
                endMenuButtons[i].TextColor = Color.Black;
                endMenuButtons[i].TextScale = new Vector(1, 1);
            }
        }
    }


    public Label CreateLabel(string labelText, Color textColor, double x = 0, double y = 0, double scale = 1, bool isVisible = true)
    {
        Label label = new Label(labelText);
        label.TextScale = new Vector(scale, scale);
        label.Position = new Vector(x, y);
        label.TextColor = textColor;
        label.IsVisible = isVisible;
        return label;
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


    public void AddBackgroundMusic(string track)
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(track);
        MediaPlayer.IsRepeating = true;
    }


    public void DisplayUnlockMessage()
    {
        firstCompletion = false;
        Label unlocks = CreateLabel("You have beaten arcade mode and unlocked new content!", Color.LightGreen, scale: 0.6);
        Add(unlocks);
        SoundEffect popUp = LoadSoundEffect("4");
        popUp.Play();
    }
    // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");



    //---------------------------------------------------------
    //---------------------------------------------------------
    //---------------------------------------------------------


    private void CreateCarSelectionItems()
    {
        carList = new List<GameObject>();
        carNameList = new List<Label>();

        carMobilityList = new List<Label>();
        carDurabilityList = new List<Label>();
        carConsumptionList = new List<Label>();
        carCapacityList = new List<Label>();

        CreateCarAvatar(-300, "car1");
        CreateCarName(-300, 190, "Basic Car");

        CreateCarAvatar(-150, "car2");
        CreateCarName(-150, 190, "Sports Car");

        CreateCarAvatar(0, "car3");
        CreateCarName(0, 190, "Power Car");

        if (gameFullyUnlocked)
        {
            CreateCarAvatar(150, "car4");
            CreateCarName(150, 190, "Heavy Car");

            CreateCarAvatar(300, "car5");
            CreateCarName(300, 190, "Super Car");
        }
        else
        {
            CreateCarAvatar(150, "car4Locked");
            CreateCarAvatar(300, "car5Locked");
        }

        for (int i = 1, j = -330, h = -50; i < 6; i++)
        {
            CreateCarProperty(j, h, "MOB:", carMobilityList);
            j += 150;
        }

        for (int i = 1, j = -330, h = -70; i < 6; i++)
        {
            CreateCarProperty(j, h, "DUR:", carDurabilityList);
            j += 150;
        }

        for (int i = 1, j = -330, h = -90; i < 6; i++)
        {
            CreateCarProperty(j, h, "CON:", carConsumptionList);
            j += 150;
        }

        for (int i = 1, j = -330, h = -110; i < 6; i++)
        {
            CreateCarProperty(j, h, "CAP:", carCapacityList);
            j += 150;
        }
    }


    private void AddPassiveStars()
    {
        List<GameObject[][]> allPassiveStars= new List<GameObject[][]> { new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                                         new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                                         new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                                         new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                                         new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] } };

        for (int i = 0, x = -330; i < carList.Count; i++, x += 150)
        {
            for (int j = 0, y = -50; j < 4; j++, y -= 20, x -= 12 * 5)
            {
                for (int k = 0; k < allPassiveStars[i][j].Length; k++, x += 12)
                {
                    allPassiveStars[i][j][k] = CreateStar("star_passive", x, y, 9);
                }
            }
        }
    }


    private void AddActiveStars()
    {
        allActiveStars = new List<GameObject[][]>() { new GameObject[4][] { new GameObject[3], new GameObject[3], new GameObject[4], new GameObject[3] },
                                                      new GameObject[4][] { new GameObject[4], new GameObject[2], new GameObject[5], new GameObject[1] },
                                                      new GameObject[4][] { new GameObject[2], new GameObject[4], new GameObject[1], new GameObject[4] },
                                                      new GameObject[4][] { new GameObject[1], new GameObject[5], new GameObject[2], new GameObject[5] },
                                                      new GameObject[4][] { new GameObject[5], new GameObject[1], new GameObject[3], new GameObject[2] } };

        for (int i = 0, x = -330; i < carList.Count; i++, x += 150)
        {
            for (int j = 0, y = -50; j < 4; j++, y -= 20, x -= 12 * allActiveStars[i][j - 1].Length)
            {
                for (int k = 0; k < allActiveStars[i][j].Length; k++, x += 12)
                {
                    allActiveStars[i][j][k] = CreateStar("star_active", x, y, 9);
                }
            }
        }
    }


    private void CreateCarAvatar(double x, string carImage)
    {
        GameObject car = new GameObject(75.0, 150.0);
        car.Position = new Vector(x, 70.0);
        car.Image = LoadImage(carImage);
        carList.Add(car);
        Add(car);
    }


    private void CreateCarName(double x, double y, string name)
    {
        Label carName = CreateLabel(name, Color.White, x, y, 0.8);
        carNameList.Add(carName);
        Add(carName);
    }


    private void CreateCarProperty(double x, double y, string property, List<Label> propertyList)
    {
        Label carProperty = CreateLabel(property, Color.Black, x, y, 0.65, false);
        propertyList.Add(carProperty);
        Add(carProperty);
    }


    private GameObject CreateStar(string image, double x, double y, double size)
    {
        GameObject star = new GameObject(size, size);
        star.Position = new Vector(x + 30.0, y);
        star.Image = LoadImage(image);
        star.IsVisible = false;
        Add(star);
        return star;
    }


    private void CarMenuMovement(Label[] descriptions)
    {
        for (int i = 0; i < 3; i++)
        {
            if (Mouse.IsCursorOn(carList[i]))
            {
                carList[i].Width = 85.0;
                carList[i].Height = 170.0;

                carNameList[i].TextScale = new Vector(1.0, 1.0);
                carNameList[i].Y = 200;
                carNameList[i].TextColor = Color.Gold;

                carMobilityList[i].IsVisible = true;
                carDurabilityList[i].IsVisible = true;
                carConsumptionList[i].IsVisible = true;
                carCapacityList[i].IsVisible = true;

                ActivateStars(carList, i);
            }
            else
            {
                carList[i].Width = 75.0;
                carList[i].Height = 150.0;

                carNameList[i].TextScale = new Vector(0.8, 0.8);
                carNameList[i].Y = 190;
                carNameList[i].TextColor = Color.White;

                carMobilityList[i].IsVisible = false;
                carDurabilityList[i].IsVisible = false;
                carConsumptionList[i].IsVisible = false;
                carCapacityList[i].IsVisible = false;

                ActivateStars(carList, i);
            }
        }

        if (gameFullyUnlocked)
        {
            for (int i = 3; i < 5; i++)
            {
                if (Mouse.IsCursorOn(carList[i]))
                {
                    carList[i].Width = 85.0;
                    carList[i].Height = 170.0;

                    carNameList[i].TextScale = new Vector(1.0, 1.0);
                    carNameList[i].Y = 200;
                    carNameList[i].TextColor = Color.Gold;

                    carMobilityList[i].IsVisible = true;
                    carDurabilityList[i].IsVisible = true;
                    carConsumptionList[i].IsVisible = true;
                    carCapacityList[i].IsVisible = true;

                    ActivateStars(carList, i);
}
                else
                {
                    carList[i].Width = 75.0;
                    carList[i].Height = 150.0;

                    carNameList[i].TextScale = new Vector(0.8, 0.8);
                    carNameList[i].Y = 190;
                    carNameList[i].TextColor = Color.White;

                    carMobilityList[i].IsVisible = false;
                    carDurabilityList[i].IsVisible = false;
                    carConsumptionList[i].IsVisible = false;
                    carCapacityList[i].IsVisible = false;

                    ActivateStars(carList, i);
                }
            }

            if (Mouse.IsCursorOn(carInfo))
            {
                foreach (Label description in descriptions) description.IsVisible = true;
                carInfo.IsVisible = false;
            }
            else
            {
                foreach (Label description in descriptions) description.IsVisible = false;
                carInfo.IsVisible = true;
            }
        }
    }


    private void ActivateStars(List<GameObject> carList, int i)
    {
        if (Mouse.IsCursorOn(carList[i]))
        {
            switch (i)
            {
                case 0:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[0])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = true;
                    }
                    break;
                case 1:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[1])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = true;
                    }
                    break;
                case 2:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[2])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = true;
                    }
                    break;
            }
        }
        else
        {
            switch (i)
            {
                case 0:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[0])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = false;
                    }
                    break;
                case 1:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[1])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = false;
                    }
                    break;
                case 2:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[2])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = false;
                    }
                    break;
            }
        }

        if (gameFullyUnlocked)
        {
            switch (i)
            {
                case 3:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[3])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = true;
                    }
                    break;
                case 4:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[4])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = true;
                    }
                    break;
            }

        }
        else
        {
            switch (i)
            {
                case 3:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[3])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = false;
                    }
                    break;
                case 4:
                    foreach (GameObject[] carPropertyActiveStars in allActiveStars[4])
                    {
                        foreach (GameObject activeStar in carPropertyActiveStars) activeStar.IsVisible = false;
                    }
                    break;
            }
        }
    }




    public void AddZones()
    {
        zoneMultipliers = new double[4] { 1, 1, 1, 1 };
        double pointBalancer = 2.0;
        double spawnBalancer = 2.0;
        double sizeBalancer = 1.25;
        double speedBalancer = 1.15;

        IntMeter zoneCurrent = new IntMeter(1, 1, 7);
        Label zoneMeter = CreateLabel($"Zone {zoneCurrent.Value}", Color.White, Screen.Left + 70.0, Screen.Bottom + 50.0);
        zoneMeter.Color = Color.Black;
        Add(zoneMeter);

        Timer zoneTimer = new Timer(30.0);
        
        if (!gameIsOn)
        {
            zoneTimer.Stop();
            return;
        }

        zoneTimer.Timeout += delegate
        {
            if (zoneCurrent < 7)
            {
                zoneCurrent.Value++;
                zoneMeter.Text = ($"Zone {zoneCurrent.Value}");

                zoneMultipliers[0] *= pointBalancer;
                pointBalancer -= 0.2;
                zoneMultipliers[1] *= spawnBalancer;
                spawnBalancer -= 0.2;
                zoneMultipliers[2] *= sizeBalancer;
                sizeBalancer -= 0.05;
                zoneMultipliers[3] *= speedBalancer;
                speedBalancer -= 0.03;
            }
            else
            {
                zoneMultipliers[0] = 10.0;
                zoneMultipliers[1] = 10.0;
                zoneMultipliers[2] = 2.05;
                zoneMultipliers[3] = 1.55;
                zoneMeter.Text = "Zone Max";
                zoneTimer.Stop();
            }

            ZonePause(5.00, zoneMeter, zoneCurrent);
        };
        zoneTimer.Start();
    }


    private void ZonePause(double pauseLength, Label zoneMeter, IntMeter zoneCurrent)
    {
        SoundEffect zone = LoadSoundEffect("3");
        zone.Play();

        Label zoneSwitch = CreateLabel("Zone Up!", Color.LightGreen, scale: 1.5);

        switch (zoneCurrent.Value)
        {
            case 2: zoneMeter.TextColor = Color.LightGreen; zoneSwitch.TextColor = Color.LightGreen; break;
            case 3: zoneMeter.TextColor = Color.GreenYellow; zoneSwitch.TextColor = Color.GreenYellow; break;
            case 4: zoneMeter.TextColor = Color.Yellow; zoneSwitch.TextColor = Color.Yellow; break;
            case 5: zoneMeter.TextColor = Color.Orange; zoneSwitch.TextColor = Color.Orange; break;
            case 6: zoneMeter.TextColor = Color.OrangeRed; zoneSwitch.TextColor = Color.OrangeRed; break;
            case 7: zoneMeter.TextColor = Color.Red; zoneSwitch.TextColor = Color.Red; zoneSwitch.Text = "Zone Max!"; break;
        }
        Add(zoneSwitch);

        foreach (Timer t in itemCreationTimers) t.Stop();

        Timer pauseTimer = new Timer(pauseLength);
        pauseTimer.Timeout += delegate
        {
            zoneSwitch.Destroy();
            foreach (Timer t in itemCreationTimers) t.Start();
        };

        pauseTimer.Start(1);
    }
}