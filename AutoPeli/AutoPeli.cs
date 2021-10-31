using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


public class autopeli : PhysicsGame
{
    private Vector gameSpeed;
    private Vector[] playerMovements;

    private string car;
    private string difficulty;
    [Save] private string[] profiles = new string[5] { "*Empty*", "*Empty*", "*Empty*", "*Empty*", "*Empty*" };
    [Save] private string playerName;
    private double durabilityMultiplier;
    private double consumptionMultiplier;
    private bool finishlineSpawned;
    private bool gameIsOn;
    private bool gamePassed;
    [Save] private bool gameFullyUnlocked = true;
    [Save] private bool firstCompletion = true;

    private readonly int saveSlots = 5;

    private double[] zoneMultipliers;

    private List<Image> carConditions;

    private List<Timer> gameTimers;
    private List<Timer> zoneTimers;

    private GameObject pointMultiplierUI;

    private List<GameObject> carList;
    private List<Label> carNameList;

    private Label[][] propertiesOfAllCars;

    private List<GameObject[][]> allStars;
    private List<int[][]> allActiveStars;

    private List <PhysicsObject> objectGroup;
    private List<PhysicsObject> startItems;

    private PhysicsObject player;
    private PhysicsObject finishline;

    private DoubleMeter healthRemaining;
    private Label healthMeter;
    private ProgressBar healthBar;

    private DoubleMeter distanceRemaining;
    private Label distanceMeter;
    private Timer distanceHelpTimer;

    private DoubleMeter fuelRemaining;
    private Label fuelMeter;
    private ProgressBar fuelBar;
    private Timer fuelHelpTimer;

    private IntMeter pointMultiplier;

    private DoubleMeter pointTotal;
    private Label pointMeter;
    private Timer pointHelpTimer;

    private Surface[] railings;

    [Save] private ScoreList hiscores = new ScoreList(15, false, 0);
    private bool soundPlayed = false;
    private bool mouseOnButton = false;


    public override void Begin()
    {
        if (DataStorage.Exists("profiles.xml")) profiles = DataStorage.TryLoad<string[]>(profiles, "profiles.xml");
        if (DataStorage.Exists("hiscores.xml")) hiscores = DataStorage.TryLoad<ScoreList>(hiscores, "hiscores.xml");

        for (int i = 0; i < profiles.Length; i++)
        {
            if (DataStorage.Exists($"player{i}.xml") == false)
            {
                profiles[i] = "*Empty*";
                DataStorage.Save<string[]>(profiles, "profiles.xml");
            }
        }

        Level.BackgroundColor = Color.Gray;
        OpeningMenu();
    }
    

    private void OpeningMenu()
    {
        List<Label> openingMenuButtons = new List<Label>() { CreateLabel("Continue", Color.Gray, y: 40), CreateLabel("New Profile", Color.White, y: 0), CreateLabel("Load Profile", Color.White, y: -40) };
        foreach (Label button in openingMenuButtons) Add(button, -1);

        if (DataStorage.Exists("lastUsedProfile.xml"))
        {
            openingMenuButtons[0].TextColor = Color.White;
            Mouse.ListenOn(openingMenuButtons[0], MouseButton.Left, ButtonState.Pressed, MainMenu, null, DataStorage.TryLoad<string>(playerName, "lastUsedProfile.xml"));
        }

        Mouse.ListenMovement(1, OpeningMenuMovement, null, openingMenuButtons);
        Mouse.ListenOn(openingMenuButtons[1], MouseButton.Left, ButtonState.Pressed, NewProfile, null);
        Mouse.ListenOn(openingMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LoadMenu, null);
    }


    public void NewProfile()
    {
        ClearAll();

        InputWindow nameQuery = new InputWindow("Player Name: ");
        nameQuery.TextEntered += delegate { playerName = nameQuery.InputBox.Text; };
        nameQuery.Closed += delegate { if (playerName.Trim().Length > 1) { SavePlayer(playerName); MainMenu(playerName); } else NewProfile(); };
        Add(nameQuery);
    }


    private void LoadMenu()
    {
        ClearAll();

        List<Label> profileLabels = new List<Label>();

        for (int i = 0, y = 80; i < profiles.Length; i++, y -= 40)
        {
            Label profileLabel = CreateLabel($"Profile {i + 1}:  {profiles[i]}", Color.Black, scale: 0.9);
            profileLabel.Y = y;
            profileLabels.Add(profileLabel);

            if (DataStorage.Exists($"player{i}.xml"))
            {
                Mouse.ListenOn(profileLabels[i], MouseButton.Left, ButtonState.Pressed, LoadProfile, null, i);
            }

            Add(profileLabels[i]);
        }

        Mouse.ListenMovement(1, LoadMenuMovement, null, profileLabels);
        Mouse.ListenOn(Key.Delete, ButtonState.Pressed, DeleteProfile, null);
    }


    private void DeleteProfile()
    {
        // TODO: sex
    }


    private void LoadProfile(int profileSlot)
    {
        switch (profileSlot)
        {
            case 0:
                playerName = DataStorage.TryLoad<string>(playerName, $"player{0}.xml");
                gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{0}.xml");
                firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{0}.xml");
                break;
            case 1:
                playerName = DataStorage.TryLoad<string>(playerName, $"player{1}.xml");
                gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{1}.xml");
                firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{1}.xml");
                break;
            case 2:
                playerName = DataStorage.TryLoad<string>(playerName, $"player{2}.xml");
                gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{2}.xml");
                firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{2}.xml");
                break;
            case 3:
                playerName = DataStorage.TryLoad<string>(playerName, $"player{3}.xml");
                gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{3}.xml");
                firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{3}.xml");
                break;
            case 4:
                playerName = DataStorage.TryLoad<string>(playerName, $"player{4}.xml");
                gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{4}.xml");
                firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{4}.xml");
                break;
        }

        MainMenu(playerName);
    }

    
    public void MainMenu(string player)
    {
        DataStorage.Save<string>(playerName, "lastUsedProfile.xml");

        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        Level.Background.Image = LoadImage("mainmenu_bgimg");

        Label mainMenuTitle = CreateLabel("MAIN MENU", Color.White, y: 200, scale: 1.2);
        mainMenuTitle.BorderColor = Color.White;
        Add(mainMenuTitle);

        Label playerIndicator = CreateLabel($"Player: {player}", Color.Gray, -220, 255, 0.5);
        Add(playerIndicator, 1);

        Label[] mainMenuButtons = new Label[5] { CreateLabel("Arcade Mode", Color.White, y: 70.0), CreateLabel("Endurance Mode", Color.White, y: 35.0), CreateLabel("Load Profile", Color.White, y: 0), CreateLabel("Hiscores", Color.White, y: -35.0), CreateLabel("Exit", Color.White, y: -70.0) };
        foreach (Label button in mainMenuButtons) Add(button, -1);

        if (gameFullyUnlocked)
        {
            Timer unlock = new Timer(0.5);
            unlock.Timeout += delegate { if (firstCompletion) DisplayUnlockMessage(); };
            unlock.Start(1);

            //AddBackgroundMusic("menu_cmpl");
            Mouse.ListenMovement(1, MainMenuMovement, null, mainMenuButtons);
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "endurance");
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LoadMenu, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
            Mouse.ListenOn(mainMenuButtons[4], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
        else
        {
            //AddBackgroundMusic("menu_orig");
            Mouse.ListenMovement(1, MainMenuMovement, null, mainMenuButtons);
            mainMenuButtons[1].TextColor = Color.Gray;
            mainMenuButtons[3].TextColor = Color.Gray;
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LoadMenu, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[4], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
    }


    public void DifficultyMenu()
    {
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        Label difficultyMenuTitle = CreateLabel("DIFFICULTY SELECTION", Color.White, y: 120, scale: 1.2);
        Add(difficultyMenuTitle);

        if (!gameFullyUnlocked)
        {
            Label message = CreateLabel("(Complete the game on standard difficulty to unlock new content.)", Color.GreenYellow, y: -300, scale: 0.65);
            Add(message);
        }

        List<Label> difficultyMenuButtons = new List<Label>()
        {
            CreateLabel("Beginner", Color.White, y: 50.0, scale: 1.1),
            CreateLabel("Standard", Color.White, scale: 1.1)
        };

        if (gameFullyUnlocked)
        {
            difficultyMenuButtons.Add(CreateLabel("Madness", Color.White, y: -50, scale: 1.1));
            Mouse.ListenOn(difficultyMenuButtons[2], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "madness");
        }
        foreach (Label button in difficultyMenuButtons) Add(button);

        Mouse.ListenMovement(0.5, DifficultyMenuMovement, null, difficultyMenuButtons);
        Mouse.ListenOn(difficultyMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "beginner");
        Mouse.ListenOn(difficultyMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "standard");
    }


    public void CarMenu(string selectedDifficulty)
    {
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        Label[] descriptions = new Label[4]
        {
        CreateLabel("MOB = mobility (directional maneuvering speed).", Color.Black, -333.6, -305, 0.6),
        CreateLabel("DUR = durability (resistance against crashing damage).", Color.Black, -315, -320, 0.6),
        CreateLabel("CON = consumption (conservation of fuel usage).", Color.Black, -331.4, -335, 0.6),
        CreateLabel("CAP = capacity (size of fuel tank & refueling rate).", Color.Black, -331.5, -350, 0.6)
        };

        foreach (Label description in descriptions) Add(description);

        difficulty = selectedDifficulty;

        AddCarList();
        AddStars();

        Mouse.ListenMovement(1, CarMenuMovement, null);

        Mouse.ListenOn(carList[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Basic");
        Mouse.ListenOn(carList[1], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Sports");
        Mouse.ListenOn(carList[2], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Power");
        if (!gameFullyUnlocked)
        {
            Mouse.ListenOn(carList[3], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(carList[4], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
        }
        else
        {
            Mouse.ListenOn(carList[3], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Heavy");
            Mouse.ListenOn(carList[4], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Super");
        }
    }


    public void CreateStage(string selectedCar)
    {
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        AddPlayer(selectedCar);

        finishlineSpawned = false;
        gamePassed = false;
        gameIsOn = true;

        gameTimers = new List<Timer>();
        zoneTimers = new List<Timer>();
        objectGroup = new List<PhysicsObject>();

        zoneMultipliers = new double[4] { 1, 1, 1, 1 };

        distanceRemaining = new DoubleMeter(1000, 0, 5000);        

        StartGame();
    }

    public void StartGame()
    {
        AddWalls();
        AddStartScreenItems();
        AddPlayerUI();
        if (difficulty != "endurance") IncreaseDistance();

        AddBackgroundMusic("default_5");
        string[] statements = new string[3] { "Ready", "Set", "Go!" };
        int i = 0;
        PhysicsObject[] collectibles = new PhysicsObject[3];

        DoubleMeter counter = new DoubleMeter(0);

        CreateTrafficLight($"lights_-1");

        Timer countdown = new Timer(1);
        countdown.Timeout += delegate
        {
            counter.Value++;
            if (counter.Value < countdown.Interval * 4 && counter.Value > countdown.Interval * 0)
            {
                SoundEffect trafficLightTick = LoadSoundEffect($"lights{i}");
                trafficLightTick.Play();
                CreateTrafficLight($"lights_{i++}");
            }
            if (counter.Value == countdown.Interval * 3)
            {
                CreateRoadMidline();
                SetControls(playerMovements);

                foreach (Timer t in gameTimers) t.Start(); 
                if (difficulty == "endurance") zoneTimers[0].Start(6);

                switch (difficulty)
                {
                    case "beginner":
                        CreateObstacle(12.5, 30.0, 0.1, 1.2);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 1.5, 3.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 3.0, 6.0);
                        gameSpeed = new Vector(0, -250);
                        break;
                    case "standard":
                        CreateObstacle(12.5, 30.0, 0.05, 0.8);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 2.0, 4.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 6.0, 8.0);
                        gameSpeed = new Vector(0, -300);
                        break;
                    case "madness":
                        CreateObstacle(12.5, 30.0, 0.0, 0.4);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 2.5, 5.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 9.0, 10.0);
                        gameSpeed = new Vector(0, -350);
                        break;
                    case "endurance":
                        CreateObstacle(10.0, 30.0, 0.5, 2.0);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 2.0, 6.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 5.0, 10.0);
                        CreateCollectible(collectibles[2], "coin", "coin_group", 3.0, 15.0);
                        gameSpeed = new Vector(0, -200);
                        break;
                }

                foreach (PhysicsObject item in startItems) item.Hit(gameSpeed * item.Mass);
            }
        };
        countdown.Start(3);
    }


    private void IncreaseDistance()
    {
        if (difficulty == "beginner") distanceRemaining.Value += 500;
        else if (difficulty == "standard") distanceRemaining.Value += 1500;
        else distanceRemaining.Value += 3000;
    }


    public void CreateTrafficLight(string imageName)
    {
        GameObject trafficLight = new GameObject(90, 270);
        trafficLight.Image = LoadImage(imageName);
        trafficLight.Y = 120;
        trafficLight.Angle = Angle.FromDegrees(90);
        trafficLight.LifetimeLeft = TimeSpan.FromSeconds(1);
        Add(trafficLight);
    }


    public void AddPlayer(string selectedCar)
    {
        car = selectedCar;

        player = new PhysicsObject(40, 80);
        player.Shape = Shape.Rectangle;
        player.CanRotate = false;
        player.Restitution = -1;
        player.Position = new Vector(0.0, -280.0);

        playerMovements = new Vector[4];

        // TODO: Fiksaa liikkuminen alussa

        switch (selectedCar)
        {
            case "car_Basic":
                player.Width = 45;
                player.Height = 90;
                player.Image = LoadImage("car1");
                playerMovements = new Vector[4] { new Vector(0, 250), new Vector(0, -250), new Vector(-250, 0), new Vector(250, 0) };
                healthRemaining = new DoubleMeter(300, 0, 300);
                durabilityMultiplier = 1.2;
                consumptionMultiplier = 1.3;
                fuelRemaining = new DoubleMeter(110, 0, 110);
                carConditions = new List<Image>() { LoadImage("car1_5"), LoadImage("car1_4"), LoadImage("car1_3"), LoadImage("car1_2"), LoadImage("car1_1"), LoadImage("car1") };
                break;
            case "car_Sports":
                player.Width = 40;
                player.Height = 80;
                player.Image = LoadImage("car2");
                playerMovements = new Vector[4] { new Vector(0, 300), new Vector(0, -300), new Vector(-300, 0), new Vector(300, 0) };
                healthRemaining = new DoubleMeter(220, 0, 220);
                durabilityMultiplier = 1.0;
                consumptionMultiplier = 1.0;
                fuelRemaining = new DoubleMeter(70, 0, 70);
                carConditions = new List<Image>() { LoadImage("car2_5"), LoadImage("car2_4"), LoadImage("car2_3"), LoadImage("car2_2"), LoadImage("car2_1"), LoadImage("car2") };
                break;
            case "car_Power":
                player.Width = 48;
                player.Height = 96;
                player.Image = LoadImage("car3");
                playerMovements = new Vector[4] { new Vector(0, 200), new Vector(0, -200), new Vector(-200, 0), new Vector(200, 0) };
                healthRemaining = new DoubleMeter(410, 0, 410);
                durabilityMultiplier = 1.7;
                consumptionMultiplier = 2.1;
                fuelRemaining = new DoubleMeter(130, 0, 130);
                carConditions = new List<Image>() { LoadImage("car3_5"), LoadImage("car3_4"), LoadImage("car3_3"), LoadImage("car3_2"), LoadImage("car3_1"), LoadImage("car3") };
                break;
            case "car_Heavy":
                player.Width = 52;
                player.Height = 98;
                player.Image = LoadImage("car4");
                playerMovements = new Vector[4] { new Vector(0, 150), new Vector(0, -150), new Vector(-150, 0), new Vector(150, 0) };
                healthRemaining = new DoubleMeter(580, 0, 580);
                durabilityMultiplier = 1.4;
                consumptionMultiplier = 1.9;
                fuelRemaining = new DoubleMeter(150, 0, 150);
                carConditions = new List<Image>() { LoadImage("car4_5"), LoadImage("car4_4"), LoadImage("car4_3"), LoadImage("car4_2"), LoadImage("car4_1"), LoadImage("car4") };
                break;
            case "car_Super":
                player.Width = 42;
                player.Height = 84;
                player.Image = LoadImage("car5");
                playerMovements = new Vector[4] { new Vector(0, 350), new Vector(0, -350), new Vector(-350, 0), new Vector(350, 0) };
                healthRemaining = new DoubleMeter(130, 0, 130);
                durabilityMultiplier = 1.1;
                consumptionMultiplier = 1.6;
                fuelRemaining = new DoubleMeter(90, 0, 90);
                carConditions = new List<Image>() { LoadImage("car5_5"), LoadImage("car5_4"), LoadImage("car5_3"), LoadImage("car5_2"), LoadImage("car5_1"), LoadImage("car5") };
                break;
        }

        CollisionHandler<PhysicsObject, PhysicsObject> ObstacleHandler = (player, target) => CollisionWithObstacle(player, target, carConditions);
        CollisionHandler<PhysicsObject, PhysicsObject> RepairkitHandler = (player, target) => CollisionWithRepairkit(player, target, carConditions);

        AddCollisionHandler(player, "obstacle_group", ObstacleHandler);
        AddCollisionHandler(player, "fuel_group", CollisionWithFuel);
        AddCollisionHandler(player, "repairkit_group", RepairkitHandler);
        AddCollisionHandler(player, "finishline_group", CollisionWithFinishline);
        AddCollisionHandler(player, "coin_group", CollisionWithCoin);
        Add(player, -2);
    }


    public void AddPlayerUI()
    {
        AddFuelUI();
        AddHealthUI();

        if (difficulty == "endurance")
        {
            AddPointsUI();
            AddZones();
        }
        else
        {
            AddDistanceUI();
            Label filler = CreateLabel(difficulty.Substring(0, 1).ToUpper() + difficulty.Substring(1), Color.White, Screen.Right - 65, Screen.Bottom + 25);
            filler.Color = new Color(0, 0, 0, 0.7);
            Add(filler);
        }

        GameObject shadow = new GameObject(115, 180);
        shadow.Position = new Vector(Screen.Right - 65, Screen.Bottom + 140);
        shadow.Color = new Color(0, 0, 0, 0.6);
        Add(shadow, 1);
    }


    private void AddWalls()
    {
        Surface[] walls = new Surface[] { new Surface(130, Screen.Height), new Surface(130, Screen.Height) };
        walls[0].X = Screen.Left + walls[0].Width / 2;
        walls[1].X = Screen.Right - walls[1].Width / 2;

        foreach (Surface wall in walls)
        {
            wall.Color = Color.Green;
            wall.IgnoresCollisionResponse = true;
            Add(wall);
        }

        railings = new Surface[] { new Surface(10, Screen.Height), new Surface(10, Screen.Height) };
        railings[0].Left = walls[0].Right;
        railings[1].Right = walls[1].Left;

        foreach (Surface railing in railings)
        {
            railing.Color = Color.White;
            Add(railing);
        }

        PhysicsObject[] borders = new PhysicsObject[] { Level.CreateTopBorder(0, false), Level.CreateBottomBorder(0, false) };
        
        foreach (PhysicsObject border in borders)
        {
            border.AddCollisionIgnoreGroup(1);
            Add(border);
        }
    }


    private void AddStartScreenItems()
    {
        startItems = new List<PhysicsObject>();

        for (int i = 0; i < 6; i += 2)
        {
            PhysicsObject openingMidline = new PhysicsObject(8, 50);
            openingMidline.Position = new Vector(0.0, 380 - (100 * i));
            openingMidline.CanRotate = false;
            openingMidline.IgnoresCollisionResponse = true;
            openingMidline.LifetimeLeft = TimeSpan.FromSeconds(10);
            startItems.Add(openingMidline);
            Add(openingMidline, -3);
        }

        PhysicsObject startLine = new PhysicsObject(Screen.Width, 50);
        startLine.Y = (player.Top + 50);
        startLine.Image = LoadImage("finishline");
        startLine.CanRotate = false;
        startLine.IgnoresCollisionResponse = true;
        startLine.LifetimeLeft = TimeSpan.FromSeconds(10);
        startItems.Add(startLine);
        Add(startLine, -3);
    }


    public void CreateRoadMidline()
    {
        Timer roadMidlineCreator = new Timer(0.8);
        zoneTimers.Add(roadMidlineCreator);
        roadMidlineCreator.Start();

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
            Add(roadMidline, -3);
            roadMidline.Hit(gameSpeed * roadMidline.Mass * zoneMultipliers[3]);
        }; 
    }


    public void CreateObstacle(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer obstacleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax) / zoneMultipliers[1]);
        gameTimers.Add(obstacleCreator);

        obstacleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                obstacleCreator.Stop();
                return;
            }

            obstacleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax) / zoneMultipliers[1];

            PhysicsObject obstacle = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax) * zoneMultipliers[2], RandomGen.NextDouble(sizeMin, sizeMax) * zoneMultipliers[2]);
            obstacle.Position = new Vector(RandomGen.NextDouble(railings[0].Right + obstacle.Width / 2 + 10, railings[1].Left + obstacle.Width / 2 - 10), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            obstacle.Angle = RandomGen.NextAngle();
            obstacle.Image = LoadImage("obstacle");
            obstacle.CanRotate = false;
            obstacle.IgnoresCollisionResponse = true;
            obstacle.Tag = "obstacle_group";
            obstacle.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            objectGroup.Add(obstacle);
            Add(obstacle, -2);
            obstacle.Hit(gameSpeed * obstacle.Mass * zoneMultipliers[3]);
        };
        obstacleCreator.Start();
    }


    public void CreateCollectible(PhysicsObject collectible, string collectibleImage, string collectibleGroup, double spawnMin, double spawnMax)
    {
        Timer collectibleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        gameTimers.Add(collectibleCreator);

        collectibleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                collectibleCreator.Stop();
                return;
            }

            collectibleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            collectible = new PhysicsObject(25.0, 25.0);
            collectible.Position = new Vector(RandomGen.NextDouble(railings[0].Right + collectible.Width / 2 + 10, railings[1].Left + collectible.Width / 2 - 10), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            collectible.Image = LoadImage(collectibleImage);
            collectible.CanRotate = false;
            collectible.IgnoresCollisionResponse = true;
            collectible.Tag = collectibleGroup;
            collectible.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            collectible.AddCollisionIgnoreGroup(1);
            objectGroup.Add(collectible);
            Add(collectible, -1);
            collectible.Hit(gameSpeed * collectible.Mass * zoneMultipliers[3]);
        };

        collectibleCreator.Start();
    }


    public void SetControls(Vector[] playerMovements)
    {
        Keyboard.Listen(Key.W, ButtonState.Down, SetPlayerMovementSpeed, "Accelerate", playerMovements[0]);
        Keyboard.Listen(Key.W, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[0]);
        Keyboard.Listen(Key.S, ButtonState.Down, SetPlayerMovementSpeed, "Decelerate", playerMovements[1]);
        Keyboard.Listen(Key.S, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[1]);
        Keyboard.Listen(Key.A, ButtonState.Down, SetPlayerMovementSpeed, "Steer left", playerMovements[2]);
        Keyboard.Listen(Key.A, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[2]);
        Keyboard.Listen(Key.D, ButtonState.Down, SetPlayerMovementSpeed, "Steer right", playerMovements[3]);
        Keyboard.Listen(Key.D, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[3]);

        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Show controls");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "End game");

        // TODO: Tee puhelimelle ja X-Box -ohjaimelle yhteensopivat ohjaimet
        // PhoneBackButton.Listen(ConfirmExit, "End Game");
    }


    public void SetPlayerMovementSpeed(Vector direction)
    {
        if (direction.X < 0 && player.Velocity.X < 0) return;
        if (direction.X > 0 && player.Velocity.X > 0) return;
        if (direction.Y < 0 && player.Velocity.Y < 0) return;
        if (direction.Y > 0 && player.Velocity.Y > 0) return;

        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-10);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(10);
        else player.Angle = Angle.FromDegrees(0);
    }


    public void ResetPlayerMovementSpeed(Vector direction)
    {
        if (player.Velocity.X == 0 && direction.X != 0)
        {
            player.Angle = Angle.FromDegrees(0);
            return;
        }

        if (player.Velocity.Y == 0 && direction.Y != 0) return;

        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-10);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(10);
        else player.Angle = Angle.FromDegrees(0);
    }


    public void AddDistanceUI()
    {
        distanceMeter = new Label();
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.Position = new Vector(Screen.Right - 85, Screen.Bottom + 80);
        distanceMeter.TextScale = new Vector(1, 1);
        distanceMeter.Color = Color.Black;
        distanceMeter.TextColor = Color.White;
        distanceMeter.BorderColor = Color.White;
        distanceMeter.DecimalPlaces = 0;
        Add(distanceMeter, 2);

        GameObject distanceUI = new GameObject(30, 30);
        distanceUI.Position = new Vector(distanceMeter.X + 50, distanceMeter.Y);
        distanceUI.Image = LoadImage("distanceUI");
        Add(distanceUI, 2);

        distanceHelpTimer = new Timer(0.01);
        gameTimers.Add(distanceHelpTimer);

        distanceHelpTimer.Timeout += delegate
        {
            distanceRemaining.Value -= 0.5;

            if (distanceRemaining.Value == distanceRemaining.MinValue && !finishlineSpawned)
            {
                finishline = new PhysicsObject(Screen.Width, 50.0);
                finishline.Y = (Screen.Top + 10.0);
                finishline.Image = LoadImage("finishline");
                finishline.CanRotate = false;
                finishline.IgnoresCollisionResponse = true;
                finishline.Tag = "finishline_group";
                finishline.AddCollisionIgnoreGroup(1);
                objectGroup.Add(finishline);
                Add(finishline, -3);
                finishline.Hit(gameSpeed * finishline.Mass);
                finishlineSpawned = true;
            }
        };
    }

    
    public void AddFuelUI()
    {
        fuelMeter = new Label();
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.Position = new Vector(Screen.Right - 85, Screen.Bottom + 140);
        fuelMeter.TextScale = new Vector(1.2, 1.2);
        fuelMeter.Color = Color.Black;
        fuelMeter.TextColor = Color.White;
        fuelMeter.BorderColor = new Color(0.0, 1.0, 0.0);
        fuelMeter.DecimalPlaces = 0;
        Add(fuelMeter, 2);

        fuelBar = new ProgressBar(fuelMeter.Width, 6);
        fuelBar.BindTo(fuelRemaining);
        fuelBar.Position = new Vector(fuelMeter.X, fuelMeter.Y -20);
        fuelBar.Color = Color.Black;
        fuelBar.BorderColor = Color.Black;
        fuelBar.BarColor = new Color(0.0, 1.0, 0.0);
        Add(fuelBar, 2);

        GameObject fuelUI = new GameObject(31, 35);
        fuelUI.Position = new Vector(fuelMeter.X + 50, fuelMeter.Y - 4);
        fuelUI.Image = LoadImage("fuelUI");
        Add(fuelUI, 2);

        fuelHelpTimer = new Timer(0.1);
        gameTimers.Add(fuelHelpTimer);

        fuelHelpTimer.Timeout += delegate
        {
            fuelRemaining.Value -= 0.25 * consumptionMultiplier;
            ChangeFuelCondition();
        };
    }


    public void AddHealthUI()
    {
        healthMeter = new Label();
        healthMeter.BindTo(healthRemaining);
        healthMeter.Position = new Vector(Screen.Right - 85, Screen.Bottom + 200);
        healthMeter.TextScale = new Vector(1.2, 1.2);
        healthMeter.Color = Color.Black;
        healthMeter.TextColor = Color.White;
        healthMeter.BorderColor = new Color(0.0, 1.0, 0.0);
        healthMeter.DecimalPlaces = 0;
        Add(healthMeter, 2);

        healthBar = new ProgressBar(healthMeter.Width, 6);
        healthBar.BindTo(healthRemaining);
        healthBar.Position = new Vector(healthMeter.X, healthMeter.Y -20);
        healthBar.Color = Color.Black;
        healthBar.BorderColor = Color.Black;
        healthBar.BarColor = new Color(0.0, 1.0, 0.0);
        Add(healthBar, 2);

        GameObject healthUI = new GameObject(35, 35);
        healthUI.Position = new Vector(healthBar.X + 50, healthMeter.Y - 4);
        healthUI.Image = LoadImage("healthUI");
        healthUI.Color = Color.Black;
        Add(healthUI, 2);
    }


    public void AddPointsUI()
    {
        pointTotal = new DoubleMeter(0.0);
        pointMultiplier = new IntMeter(1, 1, 16);

        pointMeter = new Label();
        pointMeter.BindTo(pointTotal);
        pointMeter.Position = new Vector(Screen.Right - 85, Screen.Bottom + 80);
        pointMeter.TextScale = new Vector(1.2, 1.2);
        pointMeter.Color = Color.Black;
        pointMeter.TextColor = Color.White;
        pointMeter.BorderColor = Color.Red;
        pointMeter.DecimalPlaces = 0;
        Add(pointMeter, 2);

        pointMultiplierUI = new GameObject(35, 35);
        pointMultiplierUI.Position = new Vector(pointMeter.X + 50.0, pointMeter.Y);
        pointMultiplierUI.Image = LoadImage("multi1");
        pointMultiplierUI.Color = Color.Black;
        Add(pointMultiplierUI, 2);

        pointHelpTimer = new Timer(0.1);
        gameTimers.Add(pointHelpTimer);

        pointHelpTimer.Timeout += delegate
        {
            pointTotal.Value += 0.01 * pointMultiplier.Value * zoneMultipliers[0];
        };
    }


    public void CollisionWithObstacle(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        SoundEffect crash = LoadSoundEffect("intense_explosion");
        crash.Play();

        Explosion obstacleExplosion = new Explosion(2.5 * target.Width);
        obstacleExplosion.Position = target.Position;
        obstacleExplosion.UseShockWave = false;
        Add(obstacleExplosion);
        target.Destroy();
        double removeHealth = RandomGen.NextInt(100, 180) / durabilityMultiplier;
        healthRemaining.Value -= removeHealth;

        Label collisionConsequence = CreateLabel($"-{removeHealth, 2:00} Damage", Color.Red, target.X, target.Y + 10 + 8, 0.8);
        Add(collisionConsequence);

        Timer displayTimer = new Timer(0.8);
        displayTimer.Timeout += delegate
        {
            collisionConsequence.Destroy();
            displayTimer.Stop();
        };
        displayTimer.Start();

        ChangeCarCondition(conditions);

        if (difficulty == "endurance" && pointMultiplier.Value > 1)
        {
            pointMultiplier.Value /= 2;
            ChangePointCondition();
        }
    }


    private void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        SoundEffect replenish = LoadSoundEffect("fuel");
        replenish.Play();
        target.Destroy();
        double addFuel = RandomGen.NextInt(25, 35) * (0.5 + fuelRemaining.MaxValue / 200);
        fuelRemaining.Value += addFuel;

        Label collisionConsequence = CreateLabel($"+{addFuel, 2:00} Fuel", new Color(0.0, 1.0, 0.0), target.X, target.Y + 10, 0.8);
        Add(collisionConsequence);

        Timer displayTimer = new Timer(0.5);
        displayTimer.Timeout += delegate
        {
            collisionConsequence.Destroy();
            displayTimer.Stop();
        };
        displayTimer.Start();
    }


    private void CollisionWithRepairkit(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        SoundEffect improvement = LoadSoundEffect("repairkit");
        improvement.Play();
        target.Destroy();
        int addHealth = RandomGen.NextInt(40, 80);
        healthRemaining.Value += addHealth;

        ChangeCarCondition(conditions);

        Label collisionConsequence = new Label();
        if (difficulty == "endurance" && healthRemaining.Value == healthRemaining.MaxValue)
        {
            pointTotal.Value++;
            collisionConsequence = CreateLabel($"+1 Score", Color.Yellow, target.X, target.Y + 10, 0.8);
        }
        else
        {
            collisionConsequence = CreateLabel($"+{addHealth} Health", Color.HotPink, target.X, target.Y + 10, 0.8);
        }
        Add(collisionConsequence);

        Timer displayTimer = new Timer(0.5);
        displayTimer.Timeout += delegate
        {
            collisionConsequence.Destroy();
            displayTimer.Stop();
        };
        displayTimer.Start();
    }


    private void CollisionWithFinishline(PhysicsObject player, PhysicsObject target)
    {
        gamePassed = true;
        GameEnd("You made it!");

        finishline.Velocity = Vector.Zero;
        player.AddCollisionIgnoreGroup(1);
        player.Hit(new Vector(0.0, 400.0) * player.Mass);
        player.LifetimeLeft = TimeSpan.FromSeconds(3.0);
    }


    private void CollisionWithCoin(PhysicsObject player, PhysicsObject target)
    {
        SoundEffect money = LoadSoundEffect("5");
        money.Play();
        target.Destroy();

        Label collisionConsequence;
        if (pointMultiplier.Value == pointMultiplier.MaxValue)
        {
            pointTotal.Value += 2;
            collisionConsequence = CreateLabel($"+2 Score", Color.Yellow, target.X, target.Y + 10, 0.8);
        }
        else
        {
            pointMultiplier.Value *= 2;
            collisionConsequence = CreateLabel($"Score X{pointMultiplier.Value}", new Color(0.0, 0.8, 1.0), target.X, target.Y + 10, 0.8);
        }

        Add(collisionConsequence);

        Timer displayTimer = new Timer(0.5);
        displayTimer.Timeout += delegate
        {
            collisionConsequence.Destroy();
            displayTimer.Stop();
        };
        displayTimer.Start();

        ChangePointCondition();
    }


    private void ChangeCarCondition(List<Image> conditions)
    {
        switch (healthRemaining.Value)
        {
            case double n when (n >= healthRemaining.MaxValue * 0.8):
                healthMeter.BorderColor = new Color(0.0, 1.0, 0.0);
                healthBar.BarColor = new Color(0.0, 1.0, 0.0);
                player.Image = conditions[5];
                break;
            case double n when (n < healthRemaining.MaxValue * 0.8 && n >= healthRemaining.MaxValue * 0.6):
                healthMeter.BorderColor = Color.GreenYellow;
                healthBar.BarColor = Color.GreenYellow;
                player.Image = conditions[4];
                break;
            case double n when (n < healthRemaining.MaxValue * 0.6 && n >= healthRemaining.MaxValue * 0.4):
                healthMeter.BorderColor = Color.Yellow;
                healthBar.BarColor = Color.Yellow;
                player.Image = conditions[3];
                break;
            case double n when (n < healthRemaining.MaxValue * 0.4 && n >= healthRemaining.MaxValue * 0.2):
                healthMeter.BorderColor = Color.Orange;
                healthBar.BarColor = Color.Orange;
                player.Image = conditions[2];
                break;
            case double n when (n < healthRemaining.MaxValue * 0.2 && n > healthRemaining.MinValue):
                healthMeter.BorderColor = Color.Red;
                healthBar.BarColor = Color.Red;
                player.Image = conditions[1];
                break;
            case double n when (n == healthRemaining.MinValue):
                healthMeter.BorderColor = Color.Red;
                healthBar.BarColor = Color.Red;
                player.Image = conditions[1];
                ExplodeCar();
                break;
        }
    }

    private void ChangeFuelCondition()
    {
        switch (fuelRemaining.Value)
        {
            case double n when (n >= fuelRemaining.MaxValue * 0.8):
                fuelMeter.BorderColor = new Color(0.0, 1.0, 0.0);
                fuelBar.BarColor = new Color(0.0, 1.0, 0.0);
                break;
            case double n when (n < fuelRemaining.MaxValue * 0.8 && n >= fuelRemaining.MaxValue * 0.6):
                fuelMeter.BorderColor = Color.GreenYellow;
                fuelBar.BarColor = Color.GreenYellow;
                break;
            case double n when (n < fuelRemaining.MaxValue * 0.6 && n >= fuelRemaining.MaxValue * 0.4):
                fuelMeter.BorderColor = Color.Yellow;
                fuelBar.BarColor = Color.Yellow;
                break;
            case double n when (n < fuelRemaining.MaxValue * 0.4 && n >= fuelRemaining.MaxValue * 0.2):
                fuelMeter.BorderColor = Color.Orange;
                fuelBar.BarColor = Color.Orange;
                break;
            case double n when (n < fuelRemaining.MaxValue * 0.2 && n > fuelRemaining.MinValue):
                fuelMeter.BorderColor = Color.Red;
                fuelBar.BarColor = Color.Red;
                break;
            case double n when (n == fuelRemaining.MinValue):
                FuelRanOut();
                break;
        }
    }


    private void ChangePointCondition()
    {
        pointMultiplierUI.Image = LoadImage($"multi{pointMultiplier.Value}");

        switch (pointMultiplier.Value)
        {
            case 1:
                pointMeter.BorderColor = Color.Red;
                break;
            case 2:
                pointMeter.BorderColor = Color.Orange;
                break;
            case 4:
                pointMeter.BorderColor = Color.Yellow;
                break;
            case 8:
                pointMeter.BorderColor = Color.GreenYellow;
                break;
            case 16:
                pointMeter.BorderColor = new Color(0.0, 1.0, 0.0);
                break;
        }
    }


    public void ExplodeCar()
    {
        gamePassed = false;
        Explosion carExplosion = new Explosion(4 * player.Width);
        carExplosion.Position = player.Position;
        carExplosion.UseShockWave = false;
        carExplosion.Speed = 200.0;
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
        if (gamePassed && difficulty == "standard" && firstCompletion)
        {
            gameFullyUnlocked = true;
            SaveUnlocks();
        }
        if (difficulty == "endurance")
        {
            hiscores.Add(playerName, pointTotal.Value);
            DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");
        }

        DisableControls();
        StopGameTimers();

        Label endReason = CreateLabel(message, Color.Black, y: 20);
        Add(endReason);

        DoubleMeter countdown = new DoubleMeter(3);
        Label endTimerDisplay = CreateLabel("", Color.Black, y: -20);
        endTimerDisplay.BindTo(countdown);
        endTimerDisplay.DecimalPlaces = 0;
        endTimerDisplay.TextScale = new Vector(1.5, 1.5);
        Add(endTimerDisplay);

        Timer endHelpTimer = new Timer(1);
        endHelpTimer.Timeout += delegate
        {
            countdown.Value -= 1;

            if (countdown.Value == 0)
            {
                endHelpTimer.Stop();
                if (gamePassed) EndMenu("win");
                else EndMenu("loss");
                endReason.Destroy();
                endTimerDisplay.Destroy();
            }
        };
        endHelpTimer.Start(3);

        foreach (PhysicsObject item in objectGroup) item.LifetimeLeft = TimeSpan.FromMinutes(2);
        foreach (PhysicsObject x in objectGroup) x.Velocity = Vector.Zero;
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
        foreach (Timer t in gameTimers) t.Stop();
        foreach (Timer t in zoneTimers) t.Stop();
    }


    public void EndMenu(string instance)
    {
        SoundEffect x = LoadSoundEffect(instance);
        x.Play();

        MediaPlayer.Stop();

        Label[] endMenuButtons = new Label[] { CreateLabel("Retry", Color.White, y: 50), new Label(), CreateLabel("MainMenu", Color.White, y: -50) };
        
        if (difficulty != "endurance")
        {
            endMenuButtons[1] = CreateLabel("Change Difficulty", Color.White, y: 0);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
        }
        else
        {
            endMenuButtons[1] = CreateLabel("Hiscores", Color.White, y: 0);
            Mouse.ListenOn(endMenuButtons[1], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
        }

        foreach (Label button in endMenuButtons) Add(button, 2);

        GameObject shadow = new GameObject(190, 170);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 1);

        Mouse.ListenMovement(1.0, EndMenuMovement, null, endMenuButtons);
        Mouse.ListenOn(endMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, car);
        Mouse.ListenOn(endMenuButtons[2], MouseButton.Left, ButtonState.Pressed, MainMenu, null, playerName);
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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        HighScoreWindow hiscoresWindow = new HighScoreWindow("Top Score", hiscores);
        hiscoresWindow.Closed += HiscoresWindow_Closed;
        Add(hiscoresWindow);
    }


    private void HiscoresWindow_Closed(Window sender)
    {
        MainMenu(playerName);
    }


    public void AddBackgroundMusic(string track)
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(track);
        MediaPlayer.IsRepeating = true;
    }


    public void DisplayUnlockMessage()
    {
        SaveCompletion();
        firstCompletion = false;

        Label unlocks = CreateLabel("You have beaten arcade mode and unlocked new content!", new Color(0.0, 1.0, 0.0), scale: 0.7);
        unlocks.LifetimeLeft = TimeSpan.FromSeconds(5);
        Add(unlocks, 1);

        GameObject blackBox = new GameObject(unlocks.Width + 40, unlocks.Height + 40);
        blackBox.Position = unlocks.Position;
        blackBox.Color = new Color(0, 0, 0, 0.75);
        blackBox.LifetimeLeft = TimeSpan.FromSeconds(5);
        Add(blackBox, 0);

        SoundEffect popUp = LoadSoundEffect("4");
        popUp.Play();
    }


    public void LockedContent()
    {
        SoundEffect accessDenied = LoadSoundEffect("locked");
        accessDenied.Play();

        Label lockedContent = CreateLabel("Locked", Color.White, Mouse.PositionOnScreen.X, Mouse.PositionOnScreen.Y, 0.8);
        Add(lockedContent, 1);

        Timer lockedHangTime = new Timer(0.3);
        lockedHangTime.Timeout += delegate { lockedContent.Destroy(); };
        lockedHangTime.Start(1);
    }


    //---------------------------------------------------------
    //---------------------------------------------------------
    //---------------------------------------------------------


    private void AddCarList()
    {
        carList = new List<GameObject>();
        carNameList = new List<Label>();
        string[] propertyAbbreviations = new string[4] { "MOB:", "DUR:", "CON:", "CAP:" };
        propertiesOfAllCars = new Label[5][] { new Label[4], new Label[4], new Label[4], new Label[4], new Label[4] };

        CreateCarAvatar(-300, "car1");
        CreateCarName(-300, 150, "Basic Car");

        CreateCarAvatar(-150, "car2");
        CreateCarName(-150, 150, "Sports Car");

        CreateCarAvatar(0, "car3");
        CreateCarName(0, 150, "Power Car");

        if (gameFullyUnlocked)
        {
            CreateCarAvatar(150, "car4");
            CreateCarName(150, 150, "Heavy Car");

            CreateCarAvatar(300, "car5");
            CreateCarName(300, 150, "Super Car");
        }
        else
        {
            CreateCarAvatar(150, "car4Locked");
            CreateCarAvatar(300, "car5Locked");
        }

        for (int i = 0, x = -330; i < 5; i++, x += 150)
        {
            for (int j = 0, y = -90; j < 4; j++, y -= 20)
            {
                propertiesOfAllCars[i][j] = CreateCarProperty(x, y, propertyAbbreviations[j], propertiesOfAllCars[i]);
            }
        }
    }


    private void AddStars()
    {
        allStars= new List<GameObject[][]> { new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                             new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                             new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                             new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                             new GameObject[4][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] } };

        allActiveStars = new List<int[][]>() { new int[4][] { new int[3], new int[3], new int[4], new int[3] },
                                               new int[4][] { new int[4], new int[2], new int[5], new int[1] },
                                               new int[4][] { new int[2], new int[4], new int[1], new int[4] },
                                               new int[4][] { new int[1], new int[5], new int[2], new int[5] },
                                               new int[4][] { new int[5], new int[1], new int[3], new int[2] } };

        for (int i = 0, x = -330; i < carList.Count; i++, x += 150)
        {
            for (int j = 0, y = -90; j < 4; j++, y -= 20, x -= 12 * 5)
            {
                for (int k = 0; k < allStars[i][j].Length; k++, x += 12)
                {
                    allStars[i][j][k] = CreateStar("star_passive", x, y, 9);
                }
            }
        }
    }


    private void CreateCarAvatar(double x, string carImage)
    {
        GameObject car = new GameObject(75.0, 150.0);
        car.Position = new Vector(x, 30.0);
        car.Image = LoadImage(carImage);
        carList.Add(car);
        Add(car);
    }


    private void CreateCarName(double x, double y, string name)
    {
        Label carName = CreateLabel(name, Color.Black, x, y, 0.8);
        carNameList.Add(carName);
        Add(carName);
    }


    private Label CreateCarProperty(double x, double y, string property, Label[] propertyList)
    {
        Label carProperty = CreateLabel(property, Color.Black, x, y, 0.65, false);
        Add(carProperty);
        return carProperty;
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

    private void ActivateStars(List<GameObject> carList, int i)
    {
        if (Mouse.IsCursorOn(carList[i]))
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < allActiveStars[i][j].Length; k++)
                {
                    allStars[i][j][k].Image = LoadImage("star_active");
                    allStars[i][j][k].Width = 12;
                    allStars[i][j][k].Height = 12;
                }
            }

            foreach (GameObject[] carPropertyStars in allStars[i])
            {
                foreach (GameObject star in carPropertyStars) star.IsVisible = true;
            }
        }
        else
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < allActiveStars[i][j].Length; k++)
                {
                    allStars[i][j][k].Image = LoadImage("star_passive");
                    allStars[i][j][k].Width = 9;
                    allStars[i][j][k].Height = 9;
                }
            }

            foreach (GameObject[] carPropertyStars in allStars[i])
            {
                foreach (GameObject star in carPropertyStars) star.IsVisible = false;
            }
        }
    }


    public void AddZones()
    {
        double pointBalancer = 2.0;
        double spawnBalancer = 2.0;
        double sizeBalancer = 1.25;
        double speedBalancer = 1.15;

        IntMeter zoneCurrent = new IntMeter(1, 1, 7);
        Label zoneMeter = CreateLabel($"Zone {zoneCurrent.Value}", Color.White, Screen.Right - 65, Screen.Bottom + 25);
        zoneMeter.Color = new Color(0, 0, 0, 0.7);
        Add(zoneMeter);

        Timer zoneTimer = new Timer(40);
        zoneTimers.Add(zoneTimer);

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
                zoneTimer.Stop();
            }

            ZonePause(5.00, zoneMeter, zoneCurrent, speedBalancer);
        };
    }


    private void ZonePause(double pauseLength, Label zoneMeter, IntMeter zoneCurrent, double speedBalancer)
    {
        SoundEffect zone = LoadSoundEffect("3");
        zone.Play();

        Label zoneSwitch = CreateLabel("Zone Up!", new Color(0.0, 1.0, 0.0), scale: 1.5);

        switch (zoneCurrent.Value)
        {
            case 2: zoneSwitch.TextColor = new Color(0.0, 1.0, 0.0); break;
            case 3: zoneSwitch.TextColor = Color.GreenYellow; break;
            case 4: zoneSwitch.TextColor = Color.Yellow; break;
            case 5: zoneSwitch.TextColor = Color.Orange; break;
            case 6: zoneSwitch.TextColor = Color.OrangeRed; break;
            case 7: zoneMeter.TextColor = Color.Red; zoneMeter.Text = "Zone Max"; zoneSwitch.TextColor = Color.Red; zoneSwitch.Text = "Zone Max!"; break;
        }
        Add(zoneSwitch);

        foreach (Timer t in gameTimers) t.Stop();
        foreach (PhysicsObject item in objectGroup) item.Velocity = new Vector(0, item.Velocity.Y * speedBalancer);
        speedBalancer -= 0.03;

        Timer pauseTimer = new Timer(pauseLength);
        zoneTimers.Add(pauseTimer);

        pauseTimer.Timeout += delegate
        {
            zoneSwitch.Destroy();
            foreach (Timer t in gameTimers) t.Start();
        };

        pauseTimer.Start(1);
    }


    private void SavePlayer(string playerName)
    {
        for (int i = 0; i < saveSlots; i++)
        {
            if (DataStorage.Exists($"player{i}.xml")) continue;
            DataStorage.Save<string>(playerName, $"player{i}.xml");
            profiles[i] = playerName;
            DataStorage.Save<string[]>(profiles, "profiles.xml");
            return;
        }
    }


    private void SaveCompletion()
    {
        if (!firstCompletion) return;

        for (int i = 0; i < saveSlots; i++)
        {
            if (DataStorage.Exists($"completion{i}")) continue;
            DataStorage.Save<bool>(firstCompletion, $"completion{i}.xml");
            return;
        }
    }


    private void SaveUnlocks()
    {
        if (gameFullyUnlocked) return;

        for (int i = 0; i < saveSlots; i++)
        {
            if (DataStorage.Exists($"unlocks{i}")) continue;
            DataStorage.Save<bool>(gameFullyUnlocked, $"unlocks{i}.xml");
            return;
        }
    }








    private void OpeningMenuMovement(List<Label> openingMenuButtons)
    {
        mouseOnButton = false;

        foreach (Label button in openingMenuButtons)
        {
            HandleButton(button, Color.White, Color.Gold);
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    private void LoadMenuMovement(List<Label> profileLabels)
    {
        mouseOnButton = false;

        for (int i = 0; i < 5; i++)
        {
            if (DataStorage.Exists($"player{i}.xml"))
            {
                HandleButton(profileLabels[i], Color.Black, new Color(0, 255, 0), 0.9, 0.9);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    public void MainMenuMovement(Label[] mainMenuButtons)
    {
        mouseOnButton = false;

        if (gameFullyUnlocked)
        {
            foreach (Label button in mainMenuButtons)
            {
                HandleButton(button, Color.White, Color.Gold);
            }
        }
        else
        {
            for (int i = 0; i < mainMenuButtons.Length; i += 2)
            {
                HandleButton(mainMenuButtons[i], Color.White, Color.Gold);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    public void DifficultyMenuMovement(List<Label> difficultyMenuButtons)
    {
        mouseOnButton = false;

        Color[] buttonColors = new Color[3] { new Color(0.0, 1.0, 0.0), Color.Yellow, Color.Red };

        for (int i = 0; i < difficultyMenuButtons.Count; i++)
        {
            HandleButton(difficultyMenuButtons[i], Color.White, buttonColors[i], 1.1, 1.2);
        }

        if (!mouseOnButton) soundPlayed = false;
    }

        


    private void CarMenuMovement()
    {
        mouseOnButton = false;

        for (int i = 0; i < carNameList.Count; i++)
        {
            if (Mouse.IsCursorOn(carList[i]))
            {               
                HandleCarLabel(carList, i);
                foreach (Label propertyOfCar in propertiesOfAllCars[i]) propertyOfCar.IsVisible = true;
                ActivateStars(carList, i);
            }
            else
            {
                foreach (Label propertyOfCar in propertiesOfAllCars[i]) propertyOfCar.IsVisible = false;
                HandleCarLabel(carList, i);
                ActivateStars(carList, i);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    private void EndMenuMovement(Label[] endMenuButtons)
    {
        mouseOnButton = false;

        for (int i = 0; i < endMenuButtons.Length; i++)
        {
            HandleButton(endMenuButtons[i], Color.White, Color.Gold);
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    private void HandleButton(Label button, Color normal, Color hilight, double sizeNorm = 1, double sizeHilight = 1.05)
    {
        if (Mouse.IsCursorOn(button))
        {
            mouseOnButton = true;

            if (!soundPlayed)
            {
                CreateSound("hover");
                soundPlayed = true;
            }

            UpdateLabel(button, hilight, sizeHilight);
        }
        else
        {
            UpdateLabel(button, normal, sizeNorm);
        }
    }


    private void HandleCarLabel(List<GameObject> carList, int i)
    {
        if (Mouse.IsCursorOn(carList[i]))
        {
            mouseOnButton = true;

            if (!soundPlayed)
            {
                CreateSound("hover");
                soundPlayed = true;
            }

            UpdateLabel(carNameList[i], Color.Gold, 1);
            carNameList[i].Y = 160;

            carList[i].Width = 85.0;
            carList[i].Height = 170.0;
        }
        else
        {
            UpdateLabel(carNameList[i], Color.Black, 0.8);
            carNameList[i].Y = 150;

            carList[i].Width = 75.0;
            carList[i].Height = 150.0;            
        }
    }


    public void UpdateLabel(Label l, Color updatedColor, double sizeMultiplier)
    {
        l.TextColor = updatedColor;
        l.TextScale = new Vector(sizeMultiplier, sizeMultiplier);
    }


    public void CreateSound(string soundFileName)
    {
        SoundEffect sound = LoadSoundEffect(soundFileName);
        sound.Play();
    }
}