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
    Vector gameSpeed;

    string car;
    string difficulty;
    string playerName;
    bool standardMode;
    bool finishlineSpawned;
    bool gameIsOn;
    bool gamePassed;
    bool gameFullyUnlocked = false;
    bool firstCompletion = true;
    double durabilityMultiplier;
    double consumptionMultiplier;
    private bool descriptionExists = false;

    Label difficultyDescription;

    double[] zoneMultipliers;

    List<Image> carConditions;

    List<Timer> itemCreationTimers;

    GameObject pointMultiplierUI;

    Label carInfo;
    List<GameObject> carList;
    List<Label> carNameList;

    Label[][] propertiesOfAllCars;

    List<GameObject[][]> allStars;
    List<int[][]> allActiveStars;

    List <PhysicsObject> objectGroup;

    PhysicsObject player;
    PhysicsObject finishline;

    IntMeter healthRemaining;
    Label healthMeter;
    ProgressBar healthBar;

    DoubleMeter distanceRemaining;
    Label distanceMeter;
    Timer distanceHelpTimer;

    IntMeter fuelRemaining;
    Label fuelMeter;
    ProgressBar fuelBar;
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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

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
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
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
                    SoundEffect hover = LoadSoundEffect("hover");
                    hover.Play();

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
                    SoundEffect hover = LoadSoundEffect("hover");
                    hover.Play();

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
        SoundEffect buttonClicked = LoadSoundEffect("selected");
        buttonClicked.Play();

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
                CreateObstacle(12.5, 30.0, 0.1, 1.2);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 1.5, 3.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 3.0, 6.0);
                StartGame(-250.0);
                break;
            case "standard":
                standardMode = true;
                AddDistanceMeter();
                distanceRemaining.Value += 1.50;
                CreateObstacle(12.5, 30.0, 0.05, 0.8);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 2.0, 4.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 6.0, 8.0);
                StartGame(-300.0);
                break;
            case "madness":
                AddDistanceMeter();
                distanceRemaining.Value += 3.00;
                CreateObstacle(12.5, 30.0, 0.0, 0.4);
                CreateCollectibles(collectibles[0], "fuel", "fuel_group", 2.5, 5.0);
                CreateCollectibles(collectibles[1], "repairkit", "repairkit_group", 9.0, 10.0);
                StartGame(-350.0);
                break;
            case "endurance":
                itemCreationTimers = new List<Timer>();
                AddPointMeter();
                AddZones();
                CreateObstacle(10.0, 30.0, 0.5, 2.0);
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
                healthRemaining = new IntMeter(300, 0, 300);
                durabilityMultiplier = 1.2;
                consumptionMultiplier = 1.3;
                fuelRemaining = new IntMeter(110, 0, 110);
                carConditions = new List<Image>() { LoadImage("car1_5"), LoadImage("car1_4"), LoadImage("car1_3"), LoadImage("car1_2"), LoadImage("car1_1"), LoadImage("car1") };
                break;
            case "car_Sports":
                player.Image = LoadImage("car2");
                playerMovements = new Vector[4] { new Vector(0, 300), new Vector(0, -300), new Vector(-300, 0), new Vector(300, 0) };
                healthRemaining = new IntMeter(220, 0, 220);
                durabilityMultiplier = 1.0;
                consumptionMultiplier = 1.0;
                fuelRemaining = new IntMeter(70, 0, 70);
                carConditions = new List<Image>() { LoadImage("car2_5"), LoadImage("car2_4"), LoadImage("car2_3"), LoadImage("car2_2"), LoadImage("car2_1"), LoadImage("car2") };
                break;
            case "car_Power":
                player.Image = LoadImage("car3");
                playerMovements = new Vector[4] { new Vector(0, 200), new Vector(0, -200), new Vector(-200, 0), new Vector(200, 0) };
                healthRemaining = new IntMeter(410, 0, 410);
                durabilityMultiplier = 1.7;
                consumptionMultiplier = 2.1;
                fuelRemaining = new IntMeter(130, 0, 130);
                carConditions = new List<Image>() { LoadImage("car3_5"), LoadImage("car3_4"), LoadImage("car3_3"), LoadImage("car3_2"), LoadImage("car3_1"), LoadImage("car3") };
                break;
            case "car_Heavy":
                player.Image = LoadImage("car4");
                playerMovements = new Vector[4] { new Vector(0, 150), new Vector(0, -150), new Vector(-150, 0), new Vector(150, 0) };
                healthRemaining = new IntMeter(580, 0, 580);
                durabilityMultiplier = 1.4;
                consumptionMultiplier = 1.9;
                fuelRemaining = new IntMeter(150, 0, 150);
                carConditions = new List<Image>() { LoadImage("car4_5"), LoadImage("car4_4"), LoadImage("car4_3"), LoadImage("car4_2"), LoadImage("car4_1"), LoadImage("car4") };
                break;
            case "car_Super":
                player.Image = LoadImage("car5");
                playerMovements = new Vector[4] { new Vector(0, 350), new Vector(0, -350), new Vector(-350, 0), new Vector(350, 0) };
                healthRemaining = new IntMeter(130, 0, 130);
                durabilityMultiplier = 1.1;
                consumptionMultiplier = 1.6;
                fuelRemaining = new IntMeter(90, 0, 90);
                carConditions = new List<Image>() { LoadImage("car5_5"), LoadImage("car5_4"), LoadImage("car5_3"), LoadImage("car5_2"), LoadImage("car5_1"), LoadImage("car5") };
                break;

                // TODO: CreateCar():lla switchin autojen luonti?
        }

        SetControls(playerMovements);

        CollisionHandler<PhysicsObject, PhysicsObject> ObstacleHandler = (player, target) => CollisionWithObstacle(player, target, carConditions);
        CollisionHandler<PhysicsObject, PhysicsObject> RepairkitHandler = (player, target) => CollisionWithRepairkit(player, target, carConditions);

        AddCollisionHandler(player, "obstacle_group", ObstacleHandler);
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


    private void AddOpeningMidline()
    {
        for (int i = 0; i < 8; i += 2)
        {
            PhysicsObject roadMidline = new PhysicsObject(8.0, 50.0);
            roadMidline.Position = new Vector(0.0, 400 - (90 * i));
            roadMidline.CanRotate = false;
            roadMidline.IgnoresCollisionResponse = true;
            roadMidline.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            objectGroup.Add(roadMidline);
            Add(roadMidline);
            roadMidline.Hit(gameSpeed * roadMidline.Mass);
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


    public void CreateObstacle(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer obstacleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        if (difficulty == "endurance") itemCreationTimers.Add(obstacleCreator);

        obstacleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                obstacleCreator.Stop();
                return;
            }

            obstacleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            PhysicsObject obstacle = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax), RandomGen.NextDouble(sizeMin, sizeMax));
            obstacle.Position = new Vector(RandomGen.NextDouble(Screen.Left + 10.0, Screen.Right - 10.0), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            obstacle.Angle = RandomGen.NextAngle();
            obstacle.Image = LoadImage("obstacle");
            obstacle.CanRotate = false;
            obstacle.IgnoresCollisionResponse = true;
            obstacle.Tag = "obstacle_group";
            obstacle.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            obstacle.AddCollisionIgnoreGroup(1);
            objectGroup.Add(obstacle);
            Add(obstacle);
            obstacle.Hit(gameSpeed * obstacle.Mass);
        };
        obstacleCreator.Start();

        // TODO: Lisää zone pointMultiplierUIit enduranceen.
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

        // TODO: Lisää enduranceen zone pointMultiplierUIit.
    }


    public void StartGame(double speed)
    {
        AddBackgroundMusic("default_5");

        gameSpeed = new Vector(0.0, speed);

        CreateBorders();
        AddOpeningMidline();
        AddRoadMidline();
        AddFuelMeter();
        AddHealthMeter();
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
        distanceMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 100);
        Add(distanceMeter);

        GameObject distanceUI = new GameObject(30.0, 30.0);
        distanceUI.Position = new Vector(distanceMeter.X - 50, distanceMeter.Y);
        distanceUI.Image = LoadImage("road");
        Add(distanceUI);

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
        fuelMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 130);
        fuelMeter.Color = Color.Black;
        Add(fuelMeter);

        fuelBar = new ProgressBar(40.0, 3.0);
        fuelBar.BindTo(fuelRemaining);
        fuelBar.Position = new Vector(Screen.Right - 80, Screen.Bottom + 120);
        fuelBar.Color = Color.Black;
        fuelBar.BorderColor = Color.Black;
        Add(fuelBar);

        GameObject fuelUI = new GameObject(27.5, 30.0);
        fuelUI.Position = new Vector(fuelMeter.X - 50.0, Screen.Bottom + 47.5);
        fuelUI.Image = LoadImage("jerrycan");
        Add(fuelUI);

        fuelHelpTimer = new Timer(0.2);
        if (difficulty == "endurance") itemCreationTimers.Add(fuelHelpTimer);
        fuelHelpTimer.Timeout += delegate
        {
            fuelRemaining.Value -= 1;

            switch (fuelRemaining.Value)
            {
                case int n when (n >= fuelRemaining.MaxValue * 0.8):
                    fuelMeter.TextColor = Color.LightGreen;
                    fuelMeter.BorderColor = Color.LightGreen;
                    fuelBar.BarColor = Color.LightGreen;
                    break;
                case int n when (n < fuelRemaining.MaxValue * 0.8 && n >= fuelRemaining.MaxValue * 0.6):
                    fuelMeter.TextColor = Color.GreenYellow;
                    fuelMeter.BorderColor = Color.GreenYellow;
                    fuelBar.BarColor = Color.GreenYellow;
                    break;
                case int n when (n < fuelRemaining.MaxValue * 0.6 && n >= fuelRemaining.MaxValue * 0.4):
                    fuelMeter.TextColor = Color.Yellow;
                    fuelMeter.BorderColor = Color.Yellow;
                    fuelBar.BarColor = Color.Yellow;
                    break;
                case int n when (n < fuelRemaining.MaxValue * 0.4 && n >= fuelRemaining.MaxValue * 0.2):
                    fuelMeter.TextColor = Color.Orange;
                    fuelMeter.BorderColor = Color.Orange;
                    fuelBar.BarColor = Color.Orange;
                    break;
                case int n when (n < fuelRemaining.MaxValue * 0.2 && n > fuelRemaining.MinValue):
                    fuelMeter.TextColor = Color.Red;
                    fuelMeter.BorderColor = Color.Red;
                    fuelBar.BarColor = Color.Red;
                    break;
                case int n when (n == fuelRemaining.MinValue):
                    FuelRanOut();
                    break;
            }
        };
        fuelHelpTimer.Start();
    }


    public void AddHealthMeter()
    {
        healthMeter = new Label();
        healthMeter.BindTo(healthRemaining);
        healthMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 160);
        healthMeter.Color = Color.Black;
        healthMeter.TextColor = Color.LightGreen;
        healthMeter.BorderColor = Color.LightGreen;
        Add(healthMeter);

        healthBar = new ProgressBar(70.0, 8.0);
        healthBar.BindTo(healthRemaining);
        healthBar.Position = new Vector(Screen.Right - 80.0, Screen.Bottom + 150.0);
        healthBar.Color = Color.Black;
        healthBar.BorderColor = Color.Black;
        healthBar.BarColor = new Color(0.4, 1.0, 0.7, 1.0);
        Add(healthBar);

        GameObject healthUI = new GameObject(30, 30);
        healthUI.Position = new Vector(healthBar.X - 50.0, healthBar.Y);
        healthUI.Image = LoadImage("health");
        healthUI.Color = Color.Black;
        Add(healthUI);
    }


    public void AddPointMeter()
    {
        pointTotal = new DoubleMeter(0.0);
        pointMultiplier = new IntMeter(1, 1, 8);

        pointMeter = new Label();
        pointMeter.BindTo(pointTotal);
        pointMeter.DecimalPlaces = 1;
        pointMeter.TextColor = Color.White;
        pointMeter.Color = Color.Black;
        pointMeter.Position = new Vector(Screen.Right - 80, Screen.Bottom + 100);
        Add(pointMeter);

        pointMultiplierUI = new GameObject(30, 30);
        pointMultiplierUI.Position = new Vector(pointMeter.X - 50.0, pointMeter.Y);
        pointMultiplierUI.Image = LoadImage("multi1");
        pointMultiplierUI.Color = Color.Black;
        Add(pointMultiplierUI);

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


    public void CollisionWithObstacle(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        if (gameIsOn)
        {
            SoundEffect crash = LoadSoundEffect("intense_explosion");
            crash.Play();
            target.Destroy();
            int removeHealth = RandomGen.NextInt(100, 180);
            healthRemaining.Value -= removeHealth;

            Label collisionConsequence = CreateLabel($"-{removeHealth} Damage", Color.Red, target.X, target.Y, 0.75);
            Add(collisionConsequence);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                collisionConsequence.Destroy();
                displayTimer.Stop();
            };
            displayTimer.Start();

            if (difficulty == "endurance" && pointMultiplier.Value > 1)
            {
                pointMultiplier.Value /= 2;
                pointMultiplierUI.Image = LoadImage($"multi{pointMultiplier.Value}");
            }

            ChangeCarCondition(conditions);
        }

        // TODO: enduranceen zone pointMultiplierUIit.
    }


    private void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        if (gameIsOn)
        {
            SoundEffect replenish = LoadSoundEffect("fuel");
            replenish.Play();
            target.Destroy();
            int addFuel = RandomGen.NextInt(25, 35);
            fuelRemaining.Value += addFuel;

            Label collisionConsequence = CreateLabel($"+{addFuel} Fuel", Color.LightGreen, target.X, target.Y, 0.75);
            Add(collisionConsequence);

            Timer displayTimer = new Timer(0.5);
            displayTimer.Timeout += delegate
            {
                collisionConsequence.Destroy();
                displayTimer.Stop();
            };
            displayTimer.Start();

            // TODO: enduranceen zone pointMultiplierUIit.
        }
    }


    private void CollisionWithRepairkit(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        if (gameIsOn)
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
                collisionConsequence = CreateLabel($"+1 Score", Color.Yellow, target.X, target.Y, 0.75);
            }
            else
            {
                collisionConsequence = CreateLabel($"+{addHealth} Health", Color.HotPink, target.X, target.Y, 0.75);
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

        // TODO: enduranceen zone pointMultiplierUIit.
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

            Label collisionConsequence;
            if (pointMultiplier.Value == pointMultiplier.MaxValue)
            {
                pointTotal.Value += 2;
                collisionConsequence = CreateLabel($"+2 Score", Color.Yellow, target.X, target.Y, 0.75);
            }
            else
            {
                pointMultiplier.Value *= 2;
                pointMultiplierUI.Image = LoadImage($"multi{pointMultiplier.Value}");
                collisionConsequence = CreateLabel($"Score X{pointMultiplier.Value}", Color.LightBlue, target.X, target.Y, 0.75);
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
    }


    private void ChangeCarCondition(List<Image> conditions)
    {
        switch (healthRemaining.Value)
        {
            case int n when (n >= healthRemaining.MaxValue * 0.8):
                healthMeter.TextColor = Color.LightGreen;
                healthMeter.BorderColor = Color.LightGreen;
                healthBar.BarColor = Color.LightGreen;
                player.Image = conditions[5];
                break;
            case int n when (n < healthRemaining.MaxValue * 0.8 && n >= healthRemaining.MaxValue * 0.6):
                healthMeter.TextColor = Color.GreenYellow;
                healthMeter.BorderColor = Color.GreenYellow;
                healthBar.BarColor = Color.GreenYellow;
                player.Image = conditions[4];
                break;
            case int n when (n < healthRemaining.MaxValue * 0.6 && n >= healthRemaining.MaxValue * 0.4):
                healthMeter.TextColor = Color.Yellow;
                healthMeter.BorderColor = Color.Yellow;
                healthBar.BarColor = Color.Yellow;
                player.Image = conditions[3];
                break;
            case int n when (n < healthRemaining.MaxValue * 0.4 && n >= healthRemaining.MaxValue * 0.2):
                healthMeter.TextColor = Color.Orange;
                healthMeter.BorderColor = Color.Orange;
                healthBar.BarColor = Color.Orange;
                player.Image = conditions[2];
                break;
            case int n when (n < healthRemaining.MaxValue * 0.2 && n > healthRemaining.MinValue):
                healthMeter.TextColor = Color.Red;
                healthMeter.BorderColor = Color.Red;
                healthBar.BarColor = Color.Red;
                player.Image = conditions[1];
                break;
            case int n when (n == healthRemaining.MinValue):
                healthMeter.TextColor = Color.Red;
                healthMeter.BorderColor = Color.Red;
                healthBar.BarColor = Color.Red;
                player.Image = conditions[1];
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
            endReason = CreateLabel(endMessage, Color.Black, y: 20);
            Add(endReason);

            endTimerDisplay = CreateLabel("", Color.Black, y: -20);
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
        endHelpTimer.Start(3);
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
        Label unlocks = CreateLabel("You have beaten arcade mode and unlocked new content!", Color.LightGreen, scale: 0.6);
        Add(unlocks);
        SoundEffect popUp = LoadSoundEffect("4");
        popUp.Play();
    }
    // DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");


    public void LockedContent()
    {
        SoundEffect accessDenied = LoadSoundEffect("locked");
        accessDenied.Play();

        Label lockedContent = CreateLabel("Locked", Color.White, Mouse.PositionOnScreen.X, Mouse.PositionOnScreen.Y, 0.8);
        Add(lockedContent);

        Timer lockedHangTime = new Timer(0.3);
        lockedHangTime.Timeout += delegate { lockedContent.Destroy(); };
        lockedHangTime.Start(1);

        // TODO: Error sound to locked message.
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