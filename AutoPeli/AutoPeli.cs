using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/*namespace AutoPeli
{
    public class CarClass : PhysicsObject
    {
        public string Name;
        public Vector[] playerMovement;
        public IntMeter healthRemaining;
        public double fuelConsumptionMultiplier;
        public DoubleMeter fuelRemaining;

        public CarClass(string sCarName, double CarMovement, int HullDurability, double Fconsumption, double FuelTankSize, double width = 40, double height = 80) : base(width, height)
        {
            this.Position = new Vector(0.0, -250.0);
            this.Restitution = -1;
            this.CanRotate = false;
            this.Shape = Shape.Rectangle;

            Name = sCarName;

            playerMovement = SetMovement(CarMovement);

            healthRemaining = new IntMeter(HullDurability, 0, HullDurability);

            fuelConsumptionMultiplier = Fconsumption;

            fuelRemaining = new DoubleMeter(FuelTankSize, 0.0, FuelTankSize);
        }

        private Vector[] SetMovement(double CarMovement)
        {

            Vector[] vMovementSpeed = new Vector[4] { new Vector(0, CarMovement), new Vector(0, -CarMovement), new Vector(-CarMovement, 0), new Vector(CarMovement, 0) };

            return vMovementSpeed;
        }
    }
}*/


public class autopeli : PhysicsGame
{
    private Vector gameSpeed;
    private Vector[] playerMovements;

    private string car;
    private string difficulty;
    private string playerName;
    private bool finishlineSpawned;
    private bool gameIsOn;
    private bool gamePassed;
    private bool gameFullyUnlocked = true;
    private bool firstCompletion = true;
    private double durabilityMultiplier;
    private double consumptionMultiplier;
    private bool descriptionExists = false;

    private Label difficultyDescription;

    private double[] zoneMultipliers;

    private List<Image> carConditions;

    private List<Timer> gameTimers;

    private GameObject pointMultiplierUI;

    private Label carInfo;
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

    private ScoreList hiscores = new ScoreList(20, false, 0);
    private HighScoreWindow hiscoresWindow;


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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        
        Level.Background.Image = LoadImage("mainmenu_bgimg");

        Label mainMenuTitle = CreateLabel("MAIN MENU", Color.White, y: 200, scale: 1.2);
        mainMenuTitle.BorderColor = Color.White;
        Add(mainMenuTitle);

        Label playerIndicator = CreateLabel($"Player: {playerName}", Color.Gray, -220, 255, 0.5);
        Add(playerIndicator, 1);

        Label[] mainMenuButtons = new Label[4] { CreateLabel("Arcade Mode", Color.White, y: 60.0), CreateLabel("Endurance Mode", Color.White, y: 20.0), CreateLabel("Hiscores", Color.White, y: -20.0), CreateLabel("Exit", Color.White, y: -60.0) };
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
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
        else
        {
            //AddBackgroundMusic("menu_orig");
            Mouse.ListenMovement(1, MainMenuMovement, null, mainMenuButtons);
            mainMenuButtons[1].TextColor = Color.Gray;
            mainMenuButtons[2].TextColor = Color.Gray;
            Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        }
    }


    public void MainMenuMovement(Label[] mainMenuButtons)
    {
        Color colorChocen = Color.Gold;

        if (gameFullyUnlocked)
        {
            foreach (Label button in mainMenuButtons)
            {
                if (Mouse.IsCursorOn(button) && button.TextColor != Color.Gold)
                {
                    SoundEffect hover = LoadSoundEffect("hover");
                    hover.Play();

                    button.TextColor = colorChocen;
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
                if (Mouse.IsCursorOn(mainMenuButtons[i]) && mainMenuButtons[i].TextColor != Color.Gold)
                {
                    SoundEffect hover = LoadSoundEffect("hover");
                    hover.Play();

                    mainMenuButtons[i].TextColor = colorChocen;
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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        Level.BackgroundColor = Color.Gray;

        Label difficultyMenuTitle = CreateLabel("DIFFICULTY SELECTION", Color.White, y: 200, scale: 1.2);
        Add(difficultyMenuTitle);

        List<Color> buttonColors = new List<Color>() { new Color(0.0, 1.0, 0.0), Color.GreenYellow, Color.Yellow };

        List<string> descriptions = new List<string>() { "Very easy and meant only for practicing the game basics.",
                                                       "Main difficulty of the game.\nComplete this to unlock new content.",
                                                       "Challenge yourself and take on\nthe full might of the developer!"};

        List<Label> difficultyMenuButtons = new List<Label>() { CreateLabel("Beginner", Color.White, y: 50.0, scale: 1.1), CreateLabel("Standard", Color.White, scale: 1.1) };

        if (gameFullyUnlocked)
        {
            difficultyMenuButtons.Add(CreateLabel("Madness", Color.White, y: -50, scale: 1.1));
            Mouse.ListenOn(difficultyMenuButtons[2], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "madness");
        }
        foreach (Label button in difficultyMenuButtons) Add(button);

        object[] difficultyButtonParameters = new object[2] { descriptions, difficultyDescription };

        Mouse.ListenMovement(0.5, DifficultyMenuMovement, null, difficultyMenuButtons, buttonColors, descriptions);
        Mouse.ListenOn(difficultyMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "beginner");
        Mouse.ListenOn(difficultyMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "standard");
    }


    public void DifficultyMenuMovement(List<Label> difficultyMenuButtons, List<Color> buttonColors, List<string> descriptions)
    {
        for (int i = 0; i < difficultyMenuButtons.Count; i++)
        {
            if (Mouse.IsCursorOn(difficultyMenuButtons[i]))
            {
                SoundEffect hover = LoadSoundEffect("hover");
                hover.Play();

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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

        ClearAll();

        difficulty = selectedDifficulty;
        Level.BackgroundColor = Color.Gray;

        carInfo = CreateLabel("Info", Color.LightYellow, -300, -240, 0.55);
        carInfo.BorderColor = Color.Black;
        carInfo.Color = Color.Black;
        Add(carInfo);

        Label[] descriptions = new Label[4] { CreateLabel("MOB stands for mobility and defines how easily the car maneuvers around the stage.", Color.Black, 0, -210, 0.6, false),
                                              CreateLabel("DUR stands for durability and defines how resistant the car is against crash-inflicted damage.", Color.Black, 0, -230, 0.6, false),
                                              CreateLabel("CON stands for consumption and defines how conservative the car is in its fuel usage.", Color.Black, 0, -250, 0.6, false),
                                              CreateLabel("CAP stands for capacity and defines the car's fuel tank size", Color.Black, 0, -270, 0.6, false) };
        foreach (Label description in descriptions) Add(description);

        AddCarList();
        AddStars();

        Mouse.ListenMovement(1, CarMenuMovement, null, descriptions);

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
        objectGroup = new List<PhysicsObject>();
        zoneMultipliers = new double[4] { 1, 1, 1, 1 };

        distanceRemaining = new DoubleMeter(1000, 0, 5000);        

        Level.BackgroundColor = Color.Gray;

        StartGame();
    }

    public void StartGame()
    {
        AddBorders();
        AddStartScreenItems();
        AddPlayerUI();

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

                switch (difficulty)
                {
                    case "beginner":
                        distanceRemaining.Value += 500;
                        CreateObstacle(12.5, 30.0, 0.1, 1.2);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 1.5, 3.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 3.0, 6.0);
                        gameSpeed = new Vector(0, -250);
                        break;
                    case "standard":
                        distanceRemaining.Value += 1500;
                        CreateObstacle(12.5, 30.0, 0.05, 0.8);
                        CreateCollectible(collectibles[0], "fuel", "fuel_group", 2.0, 4.0);
                        CreateCollectible(collectibles[1], "repairkit", "repairkit_group", 6.0, 8.0);
                        gameSpeed = new Vector(0, -300);
                        break;
                    case "madness":
                        distanceRemaining.Value += 3000;
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

        // TODO: Fiksaa liikkuminen alussa ja viivan liikkeellelähtö

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

        AddCollisionHandler(player, "verticalwalls", CollisionWithVWalls);
        AddCollisionHandler(player, "horizontalwalls", CollisionWithHWalls);
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
        else AddDistanceUI();

        GameObject shadow = new GameObject(120, 200);
        shadow.Position = new Vector(Screen.Right - 60, Screen.Bottom + 140);
        shadow.Color = new Color(0, 0, 0, 0.5);
        Add(shadow, 1);
    }


    public void AddBorders()
    {
        PhysicsObject[] borders = new PhysicsObject[4] { Level.CreateTopBorder(-1, false), Level.CreateBottomBorder(-1, false), Level.CreateLeftBorder(-1, false), Level.CreateRightBorder(-1, false) };
        
        foreach (PhysicsObject border in borders)
        {
            border.Tag = "border_group";
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
        gameTimers.Add(roadMidlineCreator);

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
        roadMidlineCreator.Start();
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
            obstacle.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
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
            collectible.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
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


    public void AddDistanceUI()
    {
        distanceMeter = new Label();
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 80);
        distanceMeter.TextScale = new Vector(1, 1);
        distanceMeter.Color = Color.Black;
        distanceMeter.TextColor = Color.White;
        distanceMeter.BorderColor = Color.White;
        distanceMeter.DecimalPlaces = 0;
        Add(distanceMeter, 2);

        GameObject distanceUI = new GameObject(34, 34);
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
        fuelMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 140);
        fuelMeter.TextScale = new Vector(1.25, 1.25);
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

        GameObject fuelUI = new GameObject(35, 38);
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
        healthMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 200);
        healthMeter.TextScale = new Vector(1.25, 1.25);
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

        GameObject healthUI = new GameObject(38, 38);
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
        pointMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 80);
        pointMeter.TextScale = new Vector(1.25, 1.25);
        pointMeter.Color = Color.Black;
        pointMeter.TextColor = Color.White;
        pointMeter.BorderColor = Color.Red;
        pointMeter.DecimalPlaces = 0;
        Add(pointMeter, 2);

        pointMultiplierUI = new GameObject(40, 40);
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


    public void SetPlayerMovementSpeed(Vector direction)
    {
        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-10);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(10);
        else player.Angle = Angle.FromDegrees(0);
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


    private void CollisionWithVWalls(PhysicsObject player, PhysicsObject target)
    {
        player.Velocity = new Vector(0, player.Velocity.Y);
    }

    private void CollisionWithHWalls(PhysicsObject player, PhysicsObject target)
    {
        player.Velocity = new Vector(player.Velocity.X, 0);
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
        if (gamePassed && difficulty == "standard" && firstCompletion) gameFullyUnlocked = true;

        DisableControls();
        StopGameTimers();

        Label endReason = CreateLabel(message, Color.Black, y: 20);
        Add(endReason);

        DoubleMeter countdown = new DoubleMeter(3);
        Label endTimerDisplay = CreateLabel("", Color.Black, y: -20);
        endTimerDisplay.BindTo(countdown);
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
    }


    public void EndMenu(string instance)
    {
        SoundEffect x = LoadSoundEffect(instance);
        x.Play();

        MediaPlayer.Stop();

        Label[] endMenuButtons = new Label[] { CreateLabel("Retry", Color.Black, y: 50), new Label(), CreateLabel("MainMenu", Color.Black, y: -50) };
        
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
                SoundEffect hover = LoadSoundEffect("hover");
                hover.Play();

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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

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
    // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");


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


    private void CarMenuMovement(Label[] descriptions)
    {
        for (int i = 0; i < carNameList.Count; i++)
        {
            if (Mouse.IsCursorOn(carList[i]))
            {
                SoundEffect hover = LoadSoundEffect("hover");
                hover.Play();

                CarLabelChange(carList, i);
                foreach (Label propertyOfCar in propertiesOfAllCars[i]) propertyOfCar.IsVisible = true;
                ActivateStars(carList, i);
            }
            else
            {
                foreach (Label propertyOfCar in propertiesOfAllCars[i]) propertyOfCar.IsVisible = false;
                CarLabelChange(carList, i);
                ActivateStars(carList, i);
            }
        }

        if (Mouse.IsCursorOn(carInfo))
        {
            foreach (Label description in descriptions) description.IsVisible = true;
            carInfo.TextColor = Color.Gold;
            carInfo.Color = Color.Transparent;
            carInfo.BorderColor = Color.Transparent;
            carInfo.TextScale = new Vector(0.85, 0.85);
        }
        else
        {
            foreach (Label description in descriptions) description.IsVisible = false;
            carInfo.TextColor = Color.LightYellow;
            carInfo.Color = Color.Black;
            carInfo.BorderColor = Color.Black;
            carInfo.TextScale = new Vector(0.55, 0.55);
        }
    }


    private void CarLabelChange(List<GameObject> carList, int i)
    {
        //true settings
        if (Mouse.IsCursorOn(carList[i]))
        {
            carList[i].Width = 85.0;
            carList[i].Height = 170.0;

            carNameList[i].TextScale = new Vector(1.0, 1.0);
            carNameList[i].Y = 160;
            carNameList[i].TextColor = Color.Gold;
        }
        else
        {
            //false settings
            carList[i].Width = 75.0;
            carList[i].Height = 150.0;

            carNameList[i].TextScale = new Vector(0.8, 0.8);
            carNameList[i].Y = 150;
            carNameList[i].TextColor = Color.Black;
        }
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
        Label zoneMeter = CreateLabel($"Zone {zoneCurrent.Value}", Color.White, Screen.Left + 70.0, Screen.Bottom + 50.0);
        zoneMeter.Color = Color.Black;
        Add(zoneMeter);

        Timer zoneTimer = new Timer(40);
        gameTimers.Add(zoneTimer);

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
    }


    private void ZonePause(double pauseLength, Label zoneMeter, IntMeter zoneCurrent)
    {
        SoundEffect zone = LoadSoundEffect("3");
        zone.Play();

        Label zoneSwitch = CreateLabel("Zone Up!", new Color(0.0, 1.0, 0.0), scale: 1.5);

        switch (zoneCurrent.Value)
        {
            case 2: zoneMeter.TextColor = new Color(0.0, 1.0, 0.0); zoneSwitch.TextColor = new Color(0.0, 1.0, 0.0); break;
            case 3: zoneMeter.TextColor = Color.GreenYellow; zoneSwitch.TextColor = Color.GreenYellow; break;
            case 4: zoneMeter.TextColor = Color.Yellow; zoneSwitch.TextColor = Color.Yellow; break;
            case 5: zoneMeter.TextColor = Color.Orange; zoneSwitch.TextColor = Color.Orange; break;
            case 6: zoneMeter.TextColor = Color.OrangeRed; zoneSwitch.TextColor = Color.OrangeRed; break;
            case 7: zoneMeter.TextColor = Color.Red; zoneSwitch.TextColor = Color.Red; zoneSwitch.Text = "Zone Max!"; break;
        }
        Add(zoneSwitch);

        foreach (Timer t in gameTimers) t.Stop();

        Timer pauseTimer = new Timer(pauseLength);
        gameTimers.Add(pauseTimer);

        pauseTimer.Timeout += delegate
        {
            zoneSwitch.Destroy();
            foreach (Timer t in gameTimers) t.Start();
        };

        pauseTimer.Start(1);
    }
}