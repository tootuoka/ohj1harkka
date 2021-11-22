using Jypeli;
using Jypeli.Assets;
using Jypeli.Widgets;
using System;
using System.Text;
using System.Collections.Generic;


/// @authors Juho En‰koski & Tomi Kankaanp‰‰
/// @version 22.11.2021
/// <summary>
/// Game in which you as the player control a car avoiding
/// obstacles and collecting fuel, repairkits and coins.
/// The game features two game modes: arcade where the
/// player attempts to reach the goal line in 3 different
/// difficulties, and endurance where the player attempts
/// to survive as long as possible while collecting points.
/// </summary>
public class autopeli : PhysicsGame
{
    // Preloaded constants    
    private readonly List<Image> OBSTACLES = new List<Image>()
    { LoadImage("obstacle_1"), LoadImage("obstacle_2"), LoadImage("obstacle_3"),
      LoadImage("obstacle_4"), LoadImage("obstacle_5") };

    // Profile and score variables
    [Save] private ScoreList hiscores = new ScoreList(9, false, 0);
    [Save] private string[] profiles = new string[5] { "---", "---", "---", "---", "---" };
    [Save] private string playerName;
    private int currentSaveFile;
    
    // Profile and game status truth values
    private bool finishlineSpawned;
    private bool gameIsOn;
    private bool gamePassed;
    [Save] private bool firstCompletion = true;
    [Save] private bool gameFullyUnlocked = false;

    // In game physics objects and variables
    private string difficulty;
    private string car;
    private PhysicsObject player;
    private List<PhysicsObject> startItems;
    private List<PhysicsObject> addedItems;
    private PhysicsObject finishline;
    private Surface[] railings;

    // In game movements
    private Vector gameSpeed;
    private Vector[] playerMovements;

    // Unique vehicle properties and difficulty multipliers
    private double[,] carDimensions;
    private double resistanceMultiplier;
    private double consumptionMultiplier;
    private double[] zoneMultipliers;

    // In game Timers
    private List<Timer> gameTimers;
    private List<Timer> zoneTimers;

    // Player UI variables
    private Label healthMeter;
    private Label fuelMeter;
    private Label pointMeter;
    private GameObject pointMultiplierDisplay;
    private DoubleMeter healthRemaining;
    private DoubleMeter fuelRemaining;
    private DoubleMeter distanceRemaining;
    private DoubleMeter pointTotal;
    private IntMeter pointMultiplier;
    private ProgressBar healthBar;
    private ProgressBar fuelBar;

    // Car selection display variables
    private List<Label> titlesOfAvailableCars;
    private List<GameObject> availableCars;
    private Label[][] propertyLabelsOfAllAvailableCars;
    private List<GameObject[][]> allStars;
    private List<int[][]> allActiveStars;

    // Menu sound effect variables
    private bool soundPlayed = false;
    private bool mouseOnButton = false;


    /// <summary>
    /// Loads possible pre-existing profiles and hiscores from appropriate files
    /// and overwrites default profiles and hiscores variables with the filedata.
    /// Finally calls for the initial opening menu to begin the game.
    /// </summary>
    public override void Begin()
    {
        if (DataStorage.Exists("profiles.xml")) profiles = DataStorage.TryLoad<string[]>(profiles, "profiles.xml");
        if (DataStorage.Exists("hiscores.xml")) hiscores = DataStorage.TryLoad<ScoreList>(hiscores, "hiscores.xml");

        for (int i = 0; i < profiles.Length; i++)
        {
            if (DataStorage.Exists($"player{i + 1}.xml") == false)
            {
                profiles[i] = "---";
                DataStorage.Save<string[]>(profiles, "profiles.xml");
            }
        }

        OpeningMenu();
    }
    

    /// <summary>
    /// Creates the opening menu with buttons for continuing last game session,
    /// creating new profile and loading an existing profile.
    /// Pressing each button calls for their respective method.
    /// </summary>
    private void OpeningMenu()
    {
        ClearAll();
        AddBackgroundMusic("OST");
        Level.Background.Image = LoadImage("IMG_opening");
        Level.Background.FitToLevel();

        int lastUsedProfileSlot = 0;

        GameObject avatarShadowLining = new GameObject(155, 155, Shape.Circle, Screen.Left + 110, Screen.Top - 95);
        avatarShadowLining.Color = new Color(255, 255, 150);
        Add(avatarShadowLining);

        GameObject avatarShadowBase = new GameObject(150, 150, Shape.Circle, Screen.Left + 110, Screen.Top - 95);
        avatarShadowBase.Color = new Color(100, 100, 100);
        Add(avatarShadowBase, 0);

        GameObject jKing = new GameObject(120, 120);
        jKing.Position = avatarShadowBase.Position + new Vector(-5, 10);
        jKing.Image = LoadImage("jKing");
        Add(jKing, 1);

        GameObject hunt4iSW = new GameObject(150, 75);
        hunt4iSW.Position = avatarShadowBase.Position + new Vector(-5, -50);
        hunt4iSW.Image = LoadImage("hunt4iSW");
        hunt4iSW.Angle = Angle.FromDegrees(10);
        Add(hunt4iSW, 2);

        GameObject shadow = new GameObject(180, 200);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, -2);

        List<Label> openingMenuButtons = new List<Label>() { CreateLabel("Continue", Color.Gray, y: 50), CreateLabel("New Profile", Color.Gray, y: 0), CreateLabel("Load Profile", Color.Gray, y: -50) };
        foreach (Label button in openingMenuButtons) Add(button, -1);

        Mouse.ListenMovement(1, OpeningMenuMovement, null, openingMenuButtons);

        if (DataStorage.Exists("lastUsedProfile.xml"))
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                if (DataStorage.TryLoad<string>(playerName, "lastUsedProfile.xml") == DataStorage.TryLoad<string>(playerName, $"player{i + 1}.xml")) lastUsedProfileSlot = i;
            }

            openingMenuButtons[0].TextColor = Color.White;
            Mouse.ListenOn(openingMenuButtons[0], MouseButton.Left, ButtonState.Pressed, LoadProfile, null, lastUsedProfileSlot);
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i] == "---")
            {
                openingMenuButtons[1].TextColor = Color.White;
                Mouse.ListenOn(openingMenuButtons[1], MouseButton.Left, ButtonState.Pressed, NewProfile, null);
                break;
            }
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (DataStorage.Exists($"player{i + 1}.xml"))
            {
                openingMenuButtons[2].TextColor = Color.White;
                Mouse.ListenOn(openingMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LoadMenu, null);
                break;
            }
        }
    }


    /// <summary>
    /// Prompts the player a field to input a new profile name
    /// and saves the entered text as a new profile, entering main menu in the end.
    /// </summary>
    private void NewProfile()
    {
        CreateSound("selected");

        ClearAll();

        Level.Background.CreateGradient(Color.LightGreen, new Color(30, 30, 30));

        InputWindow nameQuery = new InputWindow("Player Name:  (max length 14)");
        nameQuery.InputBox.Font = Font.FromContent("font1.otf");
        Add(nameQuery);

        nameQuery.TextEntered += delegate { playerName = nameQuery.InputBox.Text.Replace("\n", "").Trim(); };
        nameQuery.Closed += delegate
        {
            if (1 < playerName.Trim().Length && playerName.Trim().Length < 14)
            {
                SavePlayer(playerName);
                SaveCompletion();
                SaveUnlocks();
                MainMenu(playerName);
            }
            else NewProfile();
        };
    }


    /// <summary>
    /// Creates the load menu with buttons for each 5 profileslots.
    /// Left clicking an occupied profile calls for LoadProfile method.
    /// Middle clicking on an occupied profile calls for DeleteProfile method.
    /// Right clicking returns to main menu if there are any profiles
    /// left to load and opening menu if not.
    /// </summary>
    private void LoadMenu()
    {
        ClearAll();
        FormatSounds();

        Level.Background.CreateGradient(Color.LightPink, new Color(30, 30, 30));

        Label playerIndicator = CreateLabel($"player: {playerName}", Color.Gray, Screen.Left + 80, Screen.Top - 30, 0.5);
        Add(playerIndicator, 1);

        List<Label> profileLabels = new List<Label>();

        for (int i = 0, y = 80; i < profiles.Length; i++, y -= 40)
        {
            Label profileLabel = CreateLabel($"Profile {i + 1}:  {profiles[i]}", Color.Black, scale: 0.9);
            profileLabel.Y = y;
            profileLabels.Add(profileLabel);

            if (DataStorage.Exists($"player{i + 1}.xml"))
            {
                Mouse.ListenOn(profileLabels[i], MouseButton.Left, ButtonState.Pressed, LoadProfile, null, i);
            }

            Add(profileLabels[i], 1);
        }

        Label[] specialKeys = new Label[2] { CreateLabel("Press  \"MouseRight\"  to return", Color.Black, y: -320, scale: 0.8), CreateLabel("Press  \"MouseMiddle\"  to delete", Color.Black, y: -340, scale: 0.8) };
        foreach (Label key in specialKeys) Add(key);

        Mouse.ListenMovement(1, LoadMenuMovement, null, profileLabels);
        
        if (playerName == null) Mouse.Listen(MouseButton.Right, ButtonState.Pressed, OpeningMenu, null);
        else Mouse.Listen(MouseButton.Right, ButtonState.Pressed, MainMenu, null, playerName);

        for (int i = 0; i < profileLabels.Count; i++)
        {
            if (profiles[i] != "---") Mouse.ListenOn(profileLabels[i], MouseButton.Middle, ButtonState.Pressed, DeleteMenu, null, i);
        }
    }


    /// <summary>
    /// Loads the selected profile and all filedata related to it.
    /// Finally enters main menu.
    /// </summary>
    /// <param name="profileIndex">Profile to be loaded</param>
    private void LoadProfile(int profileIndex)
    {
        currentSaveFile = profileIndex + 1;

        if (!DataStorage.Exists($"unlocks{currentSaveFile}.xml"))
        {
            gameFullyUnlocked = false;
            firstCompletion = true;
        }

        playerName = DataStorage.TryLoad<string>(playerName, $"player{currentSaveFile}.xml");
        gameFullyUnlocked = DataStorage.TryLoad<bool>(gameFullyUnlocked, $"unlocks{currentSaveFile}.xml");
        firstCompletion = DataStorage.TryLoad<bool>(firstCompletion, $"completion{currentSaveFile}.xml");
        MainMenu(playerName);
    }


    /// <summary>
    /// Prompts the player with a choice to delete the selected profile.
    /// Proceeding with the delete removes the profile and re-enters main menu.
    /// </summary>
    /// <param name="profileIndex">Profile to be deleted</param>
    private void DeleteMenu(int profileIndex)
    {
        CreateSound("selected");

        GameObject shadow = new GameObject(400, 250);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 2);

        Label confirmPrompt = CreateLabel("Do you really wish to delete this profile?", Color.Red, y: 60, scale: 0.85);
        Add(confirmPrompt, 3);

        Label[] options = new Label[2] { CreateLabel("Press  \"Y\"  to continue", Color.White, y: 0), CreateLabel("Press  \"N\"  to cancel", Color.White, y: -40) };
        foreach (Label option in options) Add(option, 3);

        Keyboard.Listen(Key.Y, ButtonState.Pressed, DeleteProfile, null, profileIndex);
        Keyboard.Listen(Key.N, ButtonState.Pressed, LoadMenu, null);
    }


    /// <summary>
    /// Deletes the profile and all files related to it.
    /// Finally re-enters load menu.
    /// </summary>
    /// <param name="profileIndex">Profile to be deleted</param>
    private void DeleteProfile(int profileIndex)
    {
        if (DataStorage.TryLoad<string>(playerName, "lastUsedProfile.xml") == DataStorage.TryLoad<string>(playerName, $"player{profileIndex + 1}.xml"))
        {
            DataStorage.Delete("lastUsedProfile.xml");
            playerName = null;
        }

        DataStorage.Delete($"player{profileIndex + 1}.xml");
        DataStorage.Delete($"completion{profileIndex + 1}.xml");
        DataStorage.Delete($"unlocks{profileIndex + 1}.xml");
        profiles[profileIndex] = "---";
        DataStorage.Save<string[]>(profiles, "profiles.xml");
        LoadMenu();
    }


    /// <summary>
    /// Creates the main menu with buttons for arcade and
    /// endurance modes, load profile, hiscores and exit game.
    /// Pressing each button calls for their respective method.
    /// Having completed the full game unlocks different options in the menu.
    /// </summary>
    /// <param name="player">Loaded profile</param>
    private void MainMenu(string player)
    {
        DataStorage.Save<string>(playerName, "lastUsedProfile.xml");
        ClearAll();
        FormatSounds();
        Level.Background.Image = LoadImage("IMG_main");
        Level.Background.ScaleToLevelByWidth();

        Label mainMenuTitle = CreateLabel("MAIN MENU", Color.White, y: 300, scale: 1.4);
        mainMenuTitle.BorderColor = Color.White;
        Add(mainMenuTitle);

        Label playerIndicator = CreateLabel($"player: {player}", Color.Gray, Screen.Left + 80, Screen.Top - 30, 0.5);
        Add(playerIndicator, 1);

        Label[] mainMenuButtons = new Label[5] { CreateLabel("Arcade Mode", Color.White, y: 70.0), CreateLabel("Endurance Mode", Color.White, y: 35.0), CreateLabel("Load Profile", Color.White, y: 0), CreateLabel("Hiscores", Color.White, y: -35.0), CreateLabel("Exit", Color.White, y: -70.0) };
        foreach (Label button in mainMenuButtons) Add(button, -1);

        Mouse.ListenMovement(1, MainMenuMovement, null, mainMenuButtons);
        Mouse.ListenOn(mainMenuButtons[0], MouseButton.Left, ButtonState.Pressed, DifficultyMenu, null);
        Mouse.ListenOn(mainMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LoadMenu, null);
        Mouse.ListenOn(mainMenuButtons[4], MouseButton.Left, ButtonState.Pressed, ConfirmExit, null);
        if (gameFullyUnlocked)
        {
            Timer unlock = new Timer(0.5);
            unlock.Timeout += delegate { if (firstCompletion) DisplayUnlockMessage(); };
            unlock.Start(1);

            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "endurance");
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, Hiscores, null);
        }
        else
        {
            mainMenuButtons[1].TextColor = Color.Gray;
            mainMenuButtons[3].TextColor = Color.Gray;
            Mouse.ListenOn(mainMenuButtons[1], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(mainMenuButtons[3], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
        }
    }


    /// <summary>
    /// Creates the difficulty menu with buttons for beginner,
    /// standard and maddness difficulties.
    /// Maddness difficulty button needs game's full completion to access.
    /// Pressing each button calls the method CarMenu with their respective parameter.
    /// </summary>
    private void DifficultyMenu()
    {
        ClearAll();
        FormatSounds();
        Level.Background.CreateGradient(Color.Orange, new Color(30, 30, 30));

        GameObject shadow = new GameObject(180, 180);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 1);

        Label difficultyMenuTitle = CreateLabel("Difficulty Selection", Color.White, y: 180, scale: 1.3);
        Add(difficultyMenuTitle, 2);

        List<Label> difficultyMenuButtons = new List<Label>()
        {
            CreateLabel("Beginner", Color.White, y: 45.0),
            CreateLabel("Standard", Color.White),
            CreateLabel("Madness", Color.Gray, y: -45)
        };
        foreach (Label button in difficultyMenuButtons) Add(button);

        Label goBack = CreateLabel("Press  \"MouseRight\"  to return", Color.Black, y: -320, scale: 0.8);
        Add(goBack);

        Mouse.ListenMovement(0.5, DifficultyMenuMovement, null, difficultyMenuButtons);
        Mouse.Listen(MouseButton.Right, ButtonState.Pressed, MainMenu, null, playerName);
        Mouse.ListenOn(difficultyMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "beginner");
        Mouse.ListenOn(difficultyMenuButtons[1], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "standard");
        if (gameFullyUnlocked)
        {
            difficultyMenuButtons[2].TextColor = Color.White;
            Mouse.ListenOn(difficultyMenuButtons[2], MouseButton.Left, ButtonState.Pressed, CarMenu, null, "madness");
        }
        else
        {
            Mouse.ListenOn(difficultyMenuButtons[2], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Label message = CreateLabel("(Complete the game on standard difficulty to unlock new content.)", Color.Black, y: -300, scale: 0.65);
            Add(message);
        }
    }


    /// <summary>
    /// Creates the car menu with images of available cars and
    /// their individual properties.
    /// Some cars need game's full completion to access.
    /// Pressing each car calls the method CreateStage with their respective parameter.
    /// </summary>
    /// <param name="selectedDifficulty">Selected game difficulty</param>
    private void CarMenu(string selectedDifficulty)
    {
        difficulty = selectedDifficulty;
        ClearAll();
        FormatSounds();
        Level.Background.CreateGradient(Color.LightGreen, new Color(30, 30, 30));

        GameObject shadow = new GameObject(305, 136, Shape.Rectangle, Screen.Left + 179, Screen.Bottom + 80);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 1);

        Label[] descriptions = new Label[6]
        {
        CreateLabel("SIZE = size (evasiveness of the vehicle).", Color.White, -365, -260, 0.6),
        CreateLabel("MOB = mobility (directional maneuvering speed).", Color.White, -345, -278, 0.6),
        CreateLabel("DUR = durability (overall constitution and healing rate).", Color.White, -328.5, -296, 0.6),
        CreateLabel("RES = damage resistance (defence against crashing).", Color.White, -331.4, -314, 0.6),
        CreateLabel("CAP = capacity (size of fuel tank & refueling rate).", Color.White, -339, -332, 0.6),
        CreateLabel("CON = consumption (conservation of fuel usage).", Color.White, -343.6, -350, 0.6)
        };
        foreach (Label description in descriptions) Add(description, 2);

        Label goBack = CreateLabel("Press  \"MouseRight\"  to return", Color.Black, y: -320, scale: 0.8);
        Add(goBack);

        AddCars();
        AddStars();

        if (difficulty != "endurance") Mouse.Listen(MouseButton.Right, ButtonState.Pressed, DifficultyMenu, null);
        else Mouse.Listen(MouseButton.Right, ButtonState.Pressed, MainMenu, null, playerName);

        Mouse.ListenMovement(1, CarMenuMovement, null);
        Mouse.ListenOn(availableCars[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Basic");
        Mouse.ListenOn(availableCars[1], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Sports");
        Mouse.ListenOn(availableCars[2], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Power");
        if (gameFullyUnlocked)
        {
            Mouse.ListenOn(availableCars[3], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Heavy");
            Mouse.ListenOn(availableCars[4], MouseButton.Left, ButtonState.Pressed, CreateStage, null, "car_Super");
        }
        else
        {
            Mouse.ListenOn(availableCars[3], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
            Mouse.ListenOn(availableCars[4], MouseButton.Left, ButtonState.Pressed, LockedContent, null);
        }
    }


    /// <summary>
    /// Resets in-game truth values and timers for a fresh game start.
    /// Adds the playable character based on player's earlier selections.
    /// Creates the skeleton of the stage around the playable character.
    /// Creates the UI for the player.
    /// Finally calls for the method StartGame.
    /// </summary>
    /// <param name="selectedCar">Selected vehicle</param>
    private void CreateStage(string selectedCar)
    {
        CreateSound("selected");
        ClearAll();
        Level.Background.CreateGradient(new Color(10, 10, 10), new Color(70, 70, 70));

        finishlineSpawned = false;
        gamePassed = false;
        gameIsOn = true;
        gameTimers = new List<Timer>();
        zoneTimers = new List<Timer>();
        addedItems = new List<PhysicsObject>();
        zoneMultipliers = new double[4] { 1, 1, 1, 1 };
        distanceRemaining = new DoubleMeter(1000, 0, 5000);

        AddPlayer(selectedCar);
        AddWalls();
        AddStartScreenItems();
        StartGame();
    }


    /// <summary>
    /// Adds the playable character based on player's selection.
    /// Creates event handlers for crashes between the playable
    /// character and other objects.
    /// </summary>
    /// <param name="selectedCar">Selected vehicle</param>
    private void AddPlayer(string selectedCar)
    {
        car = selectedCar;
        List<Image> carConditions = new List<Image>();

        player = new PhysicsObject(40, 80);
        player.Shape = Shape.Rectangle;
        player.CanRotate = false;
        player.Restitution = -1;
        player.Position = new Vector(0.0, -280.0);

        playerMovements = new Vector[4];

        switch (selectedCar)
        {
            case "car_Basic":
                player.Width = carDimensions[0, 0] / 2.25;
                player.Height = carDimensions[1, 0] / 2.25;
                player.Image = LoadImage("car1");
                playerMovements = new Vector[4] { new Vector(0, 230), new Vector(0, -230), new Vector(-230, 0), new Vector(230, 0) };
                healthRemaining = new DoubleMeter(280, 0, 280);
                resistanceMultiplier = 1.3;
                fuelRemaining = new DoubleMeter(150, 0, 150);
                consumptionMultiplier = 1.25;
                carConditions = new List<Image>() { LoadImage("car1_5"), LoadImage("car1_4"), LoadImage("car1_3"), LoadImage("car1_2"), LoadImage("car1_1"), LoadImage("car1") };
                break;
            case "car_Sports":
                player.Width = carDimensions[0, 1] / 2.5;
                player.Height = carDimensions[1, 1] / 2.5;
                player.Image = LoadImage("car2");
                playerMovements = new Vector[4] { new Vector(0, 330), new Vector(0, -330), new Vector(-330, 0), new Vector(330, 0) };
                healthRemaining = new DoubleMeter(250, 0, 250);
                resistanceMultiplier = 1.0;
                fuelRemaining = new DoubleMeter(70, 0, 70);
                consumptionMultiplier = 1.1;
                carConditions = new List<Image>() { LoadImage("car2_5"), LoadImage("car2_4"), LoadImage("car2_3"), LoadImage("car2_2"), LoadImage("car2_1"), LoadImage("car2") };
                break;
            case "car_Power":
                player.Width = carDimensions[0, 2] / 2.1;
                player.Height = carDimensions[1, 2] / 2.1;
                player.Image = LoadImage("car3");
                playerMovements = new Vector[4] { new Vector(0, 270), new Vector(0, -270), new Vector(-270, 0), new Vector(270, 0) };
                healthRemaining = new DoubleMeter(360, 0, 360);
                resistanceMultiplier = 2.5;
                fuelRemaining = new DoubleMeter(120, 0, 120);
                consumptionMultiplier = 1.8;
                carConditions = new List<Image>() { LoadImage("car3_5"), LoadImage("car3_4"), LoadImage("car3_3"), LoadImage("car3_2"), LoadImage("car3_1"), LoadImage("car3") };
                break;
            case "car_Heavy":
                player.Width = carDimensions[0, 3] / 1.9;
                player.Height = carDimensions[1, 3] / 1.9;
                player.Image = LoadImage("car4");
                playerMovements = new Vector[4] { new Vector(0, 180), new Vector(0, -180), new Vector(-180, 0), new Vector(180, 0) };
                healthRemaining = new DoubleMeter(520, 0, 520);
                resistanceMultiplier = 2.3;
                fuelRemaining = new DoubleMeter(210, 0, 210);
                consumptionMultiplier = 1.35;
                carConditions = new List<Image>() { LoadImage("car4_5"), LoadImage("car4_4"), LoadImage("car4_3"), LoadImage("car4_2"), LoadImage("car4_1"), LoadImage("car4") };
                break;
            case "car_Super":
                player.Width = carDimensions[0, 4] / 2.4;
                player.Height = carDimensions[1, 4] / 2.4;
                player.Image = LoadImage("car5");
                playerMovements = new Vector[4] { new Vector(0, 390), new Vector(0, -390), new Vector(-390, 0), new Vector(390, 0) };
                healthRemaining = new DoubleMeter(130, 0, 130);
                resistanceMultiplier = 1.5;
                fuelRemaining = new DoubleMeter(90, 0, 90);
                consumptionMultiplier = 1.4;
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
        CreatePlayerUI();
        Add(player, -2);
    }


    /// <summary>
    /// Creates the player UI, including the interface, fuel bar and meter, health bar and meter
    /// and either points and zone meter or distance meter depending on game mode.
    /// </summary>
    private void CreatePlayerUI()
    {
        GameObject shadow = new GameObject(115, 180);
        shadow.Position = new Vector(Screen.Right - 65, Screen.Bottom + 140);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 1);

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
    }


    /// <summary>
    /// Creates the UI for displaying the remaining distance
    /// to the finishline in arcade mode.
    /// </summary>
    private void AddDistanceUI()
    {
        Label distanceMeter = CreateLabel("", Color.White, Screen.Right - 85, Screen.Bottom + 80, 1.1);
        distanceMeter.BindTo(distanceRemaining);
        distanceMeter.Color = Color.Black;
        distanceMeter.BorderColor = Color.White;
        distanceMeter.DecimalPlaces = 0;
        Add(distanceMeter, 2);

        GameObject distanceUI = new GameObject(30, 30);
        distanceUI.Position = new Vector(distanceMeter.X + 50, distanceMeter.Y);
        distanceUI.Image = LoadImage("distanceUI");
        Add(distanceUI, 2);

        Timer distanceReductionTimer = new Timer(0.01);
        gameTimers.Add(distanceReductionTimer);

        distanceReductionTimer.Timeout += delegate
        {
            distanceRemaining.Value -= 0.5;

            if (distanceRemaining.Value == distanceRemaining.MinValue && !finishlineSpawned)
            {
                finishline = new PhysicsObject(Screen.Width, 50);
                finishline.Y = (Screen.Top + 200);
                finishline.Image = LoadImage("finishline");
                finishline.CanRotate = false;
                finishline.IgnoresCollisionResponse = true;
                finishline.Tag = "finishline_group";
                finishline.AddCollisionIgnoreGroup(1);
                addedItems.Add(finishline);
                Add(finishline, -3);
                finishline.Hit(gameSpeed * finishline.Mass);
                finishlineSpawned = true;
            }
        };
    }


    /// <summary>
    /// Creates the UI for displaying the remaining fuel.
    /// Reduces fuel over time.
    /// </summary>
    private void AddFuelUI()
    {
        fuelMeter = CreateLabel("", Color.White, Screen.Right - 85, Screen.Bottom + 140, 1.2);
        fuelMeter.BindTo(fuelRemaining);
        fuelMeter.Color = Color.Black;
        fuelMeter.BorderColor = new Color(0.0, 1.0, 0.0);
        fuelMeter.DecimalPlaces = 0;
        Add(fuelMeter, 2);

        fuelBar = new ProgressBar(fuelMeter.Width, 6);
        fuelBar.BindTo(fuelRemaining);
        fuelBar.Position = new Vector(fuelMeter.X, fuelMeter.Y - 20);
        fuelBar.Color = Color.Black;
        fuelBar.BorderColor = Color.Black;
        fuelBar.BarColor = new Color(0.0, 1.0, 0.0);
        Add(fuelBar, 2);

        GameObject fuelUI = new GameObject(35, 35);
        fuelUI.Position = new Vector(fuelMeter.X + 50, fuelMeter.Y - 4);
        fuelUI.Image = LoadImage("fuelUI");
        Add(fuelUI, 2);

        Timer fuelReductionTimer = new Timer(0.1);
        gameTimers.Add(fuelReductionTimer);

        fuelReductionTimer.Timeout += delegate
        {
            fuelRemaining.Value -= 0.28 * consumptionMultiplier;
            ChangeFuelCondition();
        };
    }


    /// <summary>
    /// Creates the UI for displaying the remaining health.
    /// </summary>
    private void AddHealthUI()
    {
        healthMeter = CreateLabel("", Color.White, Screen.Right - 85, Screen.Bottom + 200, 1.2);
        healthMeter.BindTo(healthRemaining);
        healthMeter.Color = Color.Black;
        healthMeter.BorderColor = new Color(0.0, 1.0, 0.0);
        healthMeter.DecimalPlaces = 0;
        Add(healthMeter, 2);

        healthBar = new ProgressBar(healthMeter.Width, 6);
        healthBar.BindTo(healthRemaining);
        healthBar.Position = new Vector(healthMeter.X, healthMeter.Y - 20);
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


    /// <summary>
    /// Creates the UI for displaying the total points earned.
    /// Increases points over time.
    /// </summary>
    private void AddPointsUI()
    {
        pointTotal = new DoubleMeter(0.0);
        pointMultiplier = new IntMeter(1, 1, 16);

        pointMeter = CreateLabel("", Color.White, Screen.Right - 85, Screen.Bottom + 80, 1.2);
        pointMeter.BindTo(pointTotal);
        pointMeter.Color = Color.Black;
        pointMeter.BorderColor = Color.Red;
        pointMeter.DecimalPlaces = 0;
        Add(pointMeter, 2);

        pointMultiplierDisplay = new GameObject(35, 35);
        pointMultiplierDisplay.Position = new Vector(pointMeter.X + 50.0, pointMeter.Y);
        pointMultiplierDisplay.Image = LoadImage("multi1");
        pointMultiplierDisplay.Color = Color.Black;
        Add(pointMultiplierDisplay, 2);

        Timer pointIncrementTimer = new Timer(0.1);
        gameTimers.Add(pointIncrementTimer);

        pointIncrementTimer.Timeout += delegate
        {
            pointTotal.Value += 0.01 * pointMultiplier.Value * zoneMultipliers[0];
        };
    }


    /// <summary>
    /// Creates event handlers for listening WASD and Esc keys.
    /// </summary>
    /// <param name="playerMovements">Direction of player's movement</param>
    private void SetControls(Vector[] playerMovements)
    {
        Keyboard.Listen(Key.Up, ButtonState.Down, SetPlayerMovementSpeed, null, playerMovements[0]);
        Keyboard.Listen(Key.Up, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[0]);
        Keyboard.Listen(Key.Down, ButtonState.Down, SetPlayerMovementSpeed, null, playerMovements[1]);
        Keyboard.Listen(Key.Down, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[1]);
        Keyboard.Listen(Key.Left, ButtonState.Down, SetPlayerMovementSpeed, null, playerMovements[2]);
        Keyboard.Listen(Key.Left, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[2]);
        Keyboard.Listen(Key.Right, ButtonState.Down, SetPlayerMovementSpeed, null, playerMovements[3]);
        Keyboard.Listen(Key.Right, ButtonState.Released, ResetPlayerMovementSpeed, null, -playerMovements[3]);

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, null);
    }


    /// <summary>
    /// Manages playable character's movement on screen,
    /// increasing it in the direction of the received parameter.
    /// Turns the character based on its velocity.
    /// </summary>
    /// <param name="direction">Change in character's movement</param>
    private void SetPlayerMovementSpeed(Vector direction)
    {
        if (direction.X < 0 && player.Velocity.X < 0) return;
        if (direction.X > 0 && player.Velocity.X > 0) return;
        if (direction.Y < 0 && player.Velocity.Y < 0) return;
        if (direction.Y > 0 && player.Velocity.Y > 0) return;

        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-8);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(8);
        else player.Angle = Angle.FromDegrees(0);
    }


    /// <summary>
    /// Manages playable character's movement on screen,
    /// decreasing it in the direction of the received parameter.
    /// Turns the character based on its velocity.
    /// </summary>
    /// <param name="direction">Change in character's movement</param>
    private void ResetPlayerMovementSpeed(Vector direction)
    {
        if (player.Velocity.X == 0 && direction.X != 0)
        {
            player.Angle = Angle.FromDegrees(0);
            return;
        }
        if (player.Velocity.Y == 0 && direction.Y != 0) return;
        player.Velocity += direction;

        if (player.Velocity.X > 0) player.Angle = Angle.FromDegrees(-8);
        else if (player.Velocity.X < 0) player.Angle = Angle.FromDegrees(8);
        else player.Angle = Angle.FromDegrees(0);
    }


    /// <summary>
    /// Adds the "grass" besids the road as well as the boundaries of the road,
    /// limiting the character's movement inside them.
    /// </summary>
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


    /// <summary>
    /// Creates the items on the screen as the game starts,
    /// including the road lines and the startline.
    /// </summary>
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
            addedItems.Add(openingMidline);
            Add(openingMidline, -3);
        }

        PhysicsObject startLine = new PhysicsObject(Screen.Width, 50);
        startLine.Y = (player.Top + 50);
        startLine.Image = LoadImage("finishline");
        startLine.CanRotate = false;
        startLine.IgnoresCollisionResponse = true;
        startLine.LifetimeLeft = TimeSpan.FromSeconds(10);
        startItems.Add(startLine);
        addedItems.Add(startLine);
        Add(startLine, -3);
    }


    /// <summary>
    /// Starts countdown after which the playable character is set in motion
    /// and the player is given control of the character.
    /// Finally item creators are called with parameters based on the selected difficulty.
    /// </summary>
    private void StartGame()
    {
        if (difficulty != "endurance") IncreaseDistance();

        AddBackgroundMusic("default_5");
        string[] statements = new string[3] { "Ready", "Set", "Go!" };
        int i = 0;

        DoubleMeter counter = new DoubleMeter(0);

        CreateTrafficLight($"lights_-1");

        Timer countdown = new Timer(0.9);
        countdown.Start(3);

        countdown.Timeout += delegate
        {
            counter.Value += countdown.Interval;
            if (countdown.Interval * 0 < counter.Value && counter.Value < countdown.Interval * 4)
            {
                CreateSound($"lights{i}");
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
                        CreateObstacle(10, 30, 0.1, 1.0);
                        CreateCollectible("fuel", "fuel_group", 2, 4);
                        CreateCollectible("repairkit", "repairkit_group", 4, 6);
                        gameSpeed = new Vector(0, -250);
                        break;
                    case "standard":
                        CreateObstacle(15, 40, 0.05, 0.7);
                        CreateCollectible("fuel", "fuel_group", 3, 6);
                        CreateCollectible("repairkit", "repairkit_group", 6, 9);
                        gameSpeed = new Vector(0, -350);
                        break;
                    case "madness":
                        CreateObstacle(25, 60, 0.0, 0.3);
                        CreateCollectible("fuel", "fuel_group", 4, 8);
                        CreateCollectible("repairkit", "repairkit_group", 8, 12);
                        gameSpeed = new Vector(0, -500);
                        break;
                    case "endurance":
                        CreateObstacle(10.0, 30.0, 0.5, 2.0);
                        CreateCollectible("fuel", "fuel_group", 3, 4);
                        CreateCollectible("repairkit", "repairkit_group", 5, 7);
                        CreateCollectible("coin", "coin_group", 9, 11);
                        gameSpeed = new Vector(0, -250);
                        break;
                }

                foreach (PhysicsObject item in startItems) item.Hit(gameSpeed * item.Mass);
            }
        };
    }


    /// <summary>
    /// Arcade mode's required travel distance is adjusted based on selected difficulty.
    /// </summary>
    private void IncreaseDistance()
    {
        if (difficulty == "beginner") return;
        else if (difficulty == "standard") distanceRemaining.Value += 2000;
        else distanceRemaining.Value += 4000;
    }


    /// <summary>
    /// Adds a game object with an image of a traffic light.
    /// </summary>
    /// <param name="imageName">Image used</param>
    private void CreateTrafficLight(string imageName)
    {
        GameObject trafficLight = new GameObject(60, 180);
        trafficLight.Image = LoadImage(imageName);
        trafficLight.Y = 180;
        trafficLight.Angle = Angle.FromDegrees(90);
        trafficLight.LifetimeLeft = TimeSpan.FromSeconds(0.9);
        Add(trafficLight);
    }


    /// <summary>
    /// Periodically creates a new road midline.
    /// </summary>
    private void CreateRoadMidline()
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
            addedItems.Add(roadMidline);
            Add(roadMidline, -3);
            roadMidline.Hit(gameSpeed * roadMidline.Mass * zoneMultipliers[3]);
        }; 
    }


    /// <summary>
    /// Periodically creates a new obstacle.
    /// </summary>
    /// <param name="sizeMin">Minimum width/height of the obstacle</param>
    /// <param name="sizeMax">Maximum width/height of the obstacle</param>
    /// <param name="spawnMin">Minimum spawn interval of the obstacle</param>
    /// <param name="spawnMax">Maximum spawn interval of the obstacle</param>
    private void CreateObstacle(double sizeMin, double sizeMax, double spawnMin, double spawnMax)
    {
        Timer obstacleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax) / zoneMultipliers[1]);
        gameTimers.Add(obstacleCreator);
        obstacleCreator.Start();

        obstacleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                obstacleCreator.Stop();
                return;
            }

            obstacleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax) / zoneMultipliers[1];

            PhysicsObject obstacle = new PhysicsObject(RandomGen.NextDouble(sizeMin, sizeMax) * zoneMultipliers[2], RandomGen.NextDouble(sizeMin, sizeMax) * zoneMultipliers[2]);
            obstacle.Position = new Vector(RandomGen.NextDouble(railings[0].Right + obstacle.Width / 2 + 15, railings[1].Left + obstacle.Width / 2 - 15), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            obstacle.Angle = RandomGen.NextAngle();
            obstacle.Image = OBSTACLES[RandomGen.NextInt(1, 5)];
            obstacle.CanRotate = false;
            obstacle.IgnoresCollisionResponse = true;
            obstacle.Tag = "obstacle_group";
            obstacle.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            addedItems.Add(obstacle);
            Add(obstacle, -2);
            obstacle.Hit(gameSpeed * obstacle.Mass * zoneMultipliers[3]);
        };
    }


    /// <summary>
    /// Periodically creates a new collectible.
    /// </summary>
    /// <param name="collectibleImage">Collectible image</param>
    /// <param name="collectibleGroup">Group to which collectible to be created belongs to</param>
    /// <param name="spawnMin">Minimum spawn interval of the obstacle</param>
    /// <param name="spawnMax">Maximum spawn interval of the obstacle</param>
    private void CreateCollectible(string collectibleImage, string collectibleGroup, double spawnMin, double spawnMax)
    {
        Timer collectibleCreator = new Timer(RandomGen.NextDouble(spawnMin, spawnMax));
        gameTimers.Add(collectibleCreator);
        collectibleCreator.Start();

        collectibleCreator.Timeout += delegate
        {
            if (finishlineSpawned || !gameIsOn)
            {
                collectibleCreator.Stop();
                return;
            }

            collectibleCreator.Interval = RandomGen.NextDouble(spawnMin, spawnMax);

            PhysicsObject collectible = new PhysicsObject(30, 30);
            collectible.Position = new Vector(RandomGen.NextDouble(railings[0].Right + collectible.Width / 2 + 15, railings[1].Left + collectible.Width / 2 - 15), RandomGen.NextDouble(Screen.Top + 10.0, Screen.Top + 40.0));
            collectible.Image = LoadImage(collectibleImage);
            collectible.CanRotate = false;
            collectible.IgnoresCollisionResponse = true;
            collectible.Tag = collectibleGroup;
            collectible.LifetimeLeft = TimeSpan.FromSeconds(5.0);
            collectible.AddCollisionIgnoreGroup(1);
            addedItems.Add(collectible);
            Add(collectible, -1);
            collectible.Hit(gameSpeed * collectible.Mass * zoneMultipliers[3]);
        };   
    }


    /// <summary>
    /// Explodes the collided target and damages the playable character.
    /// Calls the method ChangeCarCondition after damaging the character.
    /// Halves the point multiplier value in endurance mode.
    /// Displays the amount of damage dealt on screen.
    /// </summary>
    /// <param name="player">Playable character</param>
    /// <param name="target">Target of character collision</param>
    /// <param name="conditions">Current status of the character</param>
    private void CollisionWithObstacle(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        CreateSound("intense_explosion");

        Explosion obstacleExplosion = new Explosion(2.5 * target.Width);
        obstacleExplosion.Position = target.Position;
        obstacleExplosion.UseShockWave = false;
        Add(obstacleExplosion);
        target.Destroy();
        double removeHealth = RandomGen.NextInt(100, 180) / resistanceMultiplier;
        healthRemaining.Value -= removeHealth;

        CreateFlow(CreateLabel($"-{removeHealth, 2:00} Damage", Color.Red, scale: 0.8));

        ChangeCarCondition(conditions);

        if (difficulty == "endurance" && pointMultiplier.Value > 1)
        {
            pointMultiplier.Value /= 2;
            ChangePointCondition();
        }
    }


    /// <summary>
    /// Destroys the collided target and replenishes the player's fuel reserves.
    /// Displays the amount of replenished fuel on screen.
    /// </summary>
    /// <param name="player">Playable character</param>
    /// <param name="target">Target of character collision</param>
    private void CollisionWithFuel(PhysicsObject player, PhysicsObject target)
    {
        CreateSound("fuel");

        target.Destroy();
        double addFuel = RandomGen.NextInt(30, 40) * (0.5 + fuelRemaining.MaxValue / 150);
        fuelRemaining.Value += addFuel;

        CreateFlow(CreateLabel($"+{addFuel, 2:00} Fuel", new Color(0.0, 1.0, 0.0), scale: 0.8));
    }


    /// <summary>
    /// Destroys the collided target and heals the playable character.
    /// Calls the method ChangeCarCondition after healing the character.
    /// Offers extra points in endurance mode when at full health.
    /// Displays the amount of health replenished on screen.
    /// </summary>
    /// <param name="player">Playable character</param>
    /// <param name="target">Target of character collision</param>
    private void CollisionWithRepairkit(PhysicsObject player, PhysicsObject target, List<Image> conditions)
    {
        CreateSound("repairkit");

        target.Destroy();
        double addHealth = RandomGen.NextInt(50, 100) * (0.5 + healthRemaining.MaxValue / 300);
        healthRemaining.Value += addHealth;

        ChangeCarCondition(conditions);

        if (difficulty == "endurance" && healthRemaining.Value == healthRemaining.MaxValue)
        {
            pointTotal.Value += 2;
            CreateFlow(CreateLabel($"+2 Score", Color.Yellow, scale: 0.8));
        }
        else
        {
            CreateFlow(CreateLabel($"+{addHealth, 2:00} Health", Color.HotPink, scale: 0.8));
        }
    }


    /// <summary>
    /// Stops the movement of finishline and hits the player offscreen.
    /// Sets the game as passed and calls for the method GameEnd.
    /// </summary>
    /// <param name="player">Playable Character</param>
    /// <param name="target">Target of character collision</param>
    private void CollisionWithFinishline(PhysicsObject player, PhysicsObject target)
    {
        gamePassed = true;
        GameEnd("You made it!");

        finishline.Velocity = Vector.Zero;
        player.AddCollisionIgnoreGroup(1);
        player.Hit(-gameSpeed * 2 * player.Mass);
        player.LifetimeLeft = TimeSpan.FromSeconds(3.0);
    }


    /// <summary>
    /// Destroys the collided target and doubles the point multiplier value.
    /// Offers extra points when point modifier is at its max value (16).
    /// Displays the updated point multiplier value on screen.
    /// </summary>
    /// <param name="player">Playable character</param>
    /// <param name="target">Target of character collision</param>
    private void CollisionWithCoin(PhysicsObject player, PhysicsObject target)
    {
        CreateSound("5");

        target.Destroy();

        if (pointMultiplier.Value == pointMultiplier.MaxValue)
        {
            pointTotal.Value += 10;
            CreateFlow(CreateLabel($"+10 Score", Color.Yellow, scale: 0.8));
        }
        else
        {
            pointMultiplier.Value *= 2;
            CreateFlow(CreateLabel($"Score X{pointMultiplier.Value}", new Color(0.0, 0.8, 1.0), scale: 0.8));
        }

        Timer displayTimer = new Timer(0.5);
        displayTimer.Start();

        displayTimer.Timeout += delegate
        {
            displayTimer.Stop();
        };

        ChangePointCondition();
    }


    /// <summary>
    /// Updates playable character's image and health UI based on the remaining health.
    /// Calls for the method ExplodeCar when player's health reaches its minimum value.
    /// </summary>
    /// <param name="conditions">Current status of the character</param>
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


    /// <summary>
    /// Updates the fuel UI's appearance based on the remaining fuel.
    /// Calls for the method FuelRanOut when player's fuel reaches its minimum value.
    /// </summary>
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


    /// <summary>
    /// Changes the appearance of the score UI based on point multiplier value.
    /// </summary>
    private void ChangePointCondition()
    {
        pointMultiplierDisplay.Image = LoadImage($"multi{pointMultiplier.Value}");

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


    /// <summary>
    /// Sets the game as not passed.
    /// Explodes and stops the movement of the playable character.
    /// Calls for the method GameEnd.
    /// </summary>
    private void ExplodeCar()
    {
        gamePassed = false;
        Explosion carExplosion = new Explosion(4 * player.Width);
        carExplosion.Position = player.Position;
        carExplosion.UseShockWave = false;
        carExplosion.Speed = 200.0;
        carExplosion.Sound = LoadSoundEffect("1");
        Add(carExplosion);

        CreateSound("destruction");
        player.Velocity = Vector.Zero;
        GameEnd("Your car broke down!");
    }


    /// <summary>
    /// Sets the game as not passed.
    /// Stops the movement of the playable character.
    /// Calls for the method GameEnd.
    /// </summary>
    private void FuelRanOut()
    {
        gamePassed = false;
        CreateSound("fuel_out");
        player.Velocity = Vector.Zero;
        GameEnd("You ran out of fuel!");
    }


    /// <summary>
    /// Sets the game as not on.
    /// Potentially saves new progress/hiscore for the profile should the
    /// player have passed the game.
    /// Calls for methods DisableControls and StopGameTimers.
    /// Stops the movement of all items on screen.
    /// Finally calls for the method EndMenu after a three second countdown.
    /// </summary>
    /// <param name="message"></param>
    private void GameEnd(string message)
    {
        gameIsOn = false;

        if (gamePassed && difficulty == "standard") SaveUnlocks();
        if (difficulty == "endurance") SaveScore();

        if (gamePassed) CreateSound("win");
        else CreateSound("loss");

        DisableControls();
        StopGameTimers();

        foreach (PhysicsObject item in addedItems) item.LifetimeLeft = TimeSpan.FromMinutes(2);
        foreach (PhysicsObject x in addedItems) x.Velocity = Vector.Zero;

        Label endReason = CreateLabel(message, new Color(255, 255, 100), y: 20, scale: 1.3);
        Add(endReason);

        Timer endMenuDelayer = new Timer(3);
        endMenuDelayer.Start(1);

        endMenuDelayer.Timeout += delegate
        {
            endMenuDelayer.Stop();
            endReason.Destroy();
            EndMenu();
        };
    }


    /// <summary>
    /// Disables movement controls of the playable character.
    /// </summary>
    private void DisableControls()
    {
        Keyboard.Disable(Key.Up);
        Keyboard.Disable(Key.Down);
        Keyboard.Disable(Key.Left);
        Keyboard.Disable(Key.Right);
    }


    /// <summary>
    /// Stops all in-game timers.
    /// </summary>
    private void StopGameTimers()
    {
        foreach (Timer t in gameTimers) t.Stop();
        foreach (Timer t in zoneTimers) t.Stop();
    }


    /// <summary>
    /// Creates the end menu with buttons for retry, main menu
    /// and change difficulty or hiscores depending on the game mode.
    /// Left clicking on a button calls for their respective method.
    /// </summary>
    private void EndMenu()
    {
        MediaPlayer.Stop();

        GameObject shadow = new GameObject(190, 170);
        shadow.Color = new Color(0, 0, 0, 0.75);
        Add(shadow, 1);

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

        Mouse.ListenMovement(1.0, EndMenuMovement, null, endMenuButtons);
        Mouse.ListenOn(endMenuButtons[0], MouseButton.Left, ButtonState.Pressed, CreateStage, null, car);
        Mouse.ListenOn(endMenuButtons[2], MouseButton.Left, ButtonState.Pressed, MainMenu, null, playerName);
    }


    /// <summary>
    /// Saves pointTotal.Value data into an external .xml file.
    /// </summary>
    private void SaveScore()
    {
        StringBuilder newEntry = new StringBuilder($"{playerName} ({car.Replace("car_", "")} Car)");
        pointTotal.Value = Math.Round(pointTotal.Value, 1);
        hiscores.Add(newEntry.ToString(), pointTotal.Value);
        DataStorage.Save<ScoreList>(hiscores, "hiscores.xml");
    }


    /// <summary>
    /// Saves player profile data into an external .xml file.
    /// </summary>
    /// <param name="playerName">Current profile name</param>
    private void SavePlayer(string playerName)
    {
        for (int i = 0; i < profiles.Length; i++)
        {
            if (DataStorage.Exists($"player{i + 1}.xml")) continue;
            profiles[i] = playerName;
            currentSaveFile = i + 1;
            DataStorage.Save<string[]>(profiles, "profiles.xml");
            DataStorage.Save<string>(playerName, $"player{currentSaveFile}.xml");
            return;
        }
    }


    /// <summary>
    /// Sets the first completion of the game as false.
    /// Saves the profile's game completion data into an external .xml file.
    /// </summary>
    private void SaveCompletion()
    {
        if (!firstCompletion) return;
        if (gamePassed)
        {
            firstCompletion = false;
            DataStorage.Save<bool>(firstCompletion, $"completion{currentSaveFile}.xml");
        }
        else DataStorage.Save<bool>(true, $"completion{currentSaveFile}.xml");
    }


    /// <summary>
    /// Sets the full unlocking of the game as true.
    /// Saves the profile's game unlocks data into an external .xml file.
    /// </summary>
    private void SaveUnlocks()
    {
        if (gameFullyUnlocked) return;
        if (gamePassed)
        {
            gameFullyUnlocked = true;
            DataStorage.Save<bool>(gameFullyUnlocked, $"unlocks{currentSaveFile}.xml");
        }
        else DataStorage.Save<bool>(false, $"unlocks{currentSaveFile}.xml");
    }


    /// <summary>
    /// Creates a list of best scores with their respective makers
    /// and cars they were made with.
    /// Returns to main menu when the list is closed.
    /// </summary>
    private void Hiscores()
    {
        CreateSound("selected");
        ClearAll();

        Label playerIndicator = CreateLabel($"player: {playerName}", Color.Gray, Screen.Left + 80, Screen.Top - 30, 0.5);
        Add(playerIndicator, 1);

        Level.Background.Image = LoadImage("IMG_main");
        Level.Background.ScaleToLevelByWidth();

        HighScoreWindow hiscoresWindow = new HighScoreWindow(450, 500, "Top Score (Endurance Mode)", hiscores);
        hiscoresWindow.Message.Font = Font.FromContent("font1.otf");
        Add(hiscoresWindow);

        hiscoresWindow.Closed += delegate { MainMenu(playerName); };
    }


    /// <summary>
    /// Defines the variables used in creating the items of the car menu.
    /// Calls for the methods CreateCarAvatar and CreateCarName for each available car.
    /// Some cars require full game completion to gain full effects of this method.
    /// </summary>
    private void AddCars()
    {
        availableCars = new List<GameObject>();
        titlesOfAvailableCars = new List<Label>();
        string[] carPropertyAcronyms = new string[6] { "SIZE:", "MOB:", "DUR:", "RES:", "CAP:", "CON:" };
        string[] carNames = new string[5] { "Basic Car", "Sports Car", "Power Car", "Heavy Car", "Super Car" };
        carDimensions = new double[2, 5] { { 112, 101, 94, 120, 105 }, { 220, 220, 220, 220, 220 } };
        propertyLabelsOfAllAvailableCars = new Label[6][] { new Label[6], new Label[6], new Label[6], new Label[6], new Label[6], new Label[6] };

        for (int i = 0, x = -300; i < carNames.Length - 2; i++, x += 150)
        {
            CreateCarAvatar($"car{i + 1}", x, carDimensions[0, i], carDimensions[1, i]);
            CreateCarName(carNames[i], x);
        }

        if (gameFullyUnlocked)
        {
            for (int i = 3, x = 150; i < carNames.Length; i++, x += 150)
            {
                CreateCarAvatar($"car{i + 1}", x, carDimensions[0, i], carDimensions[1, i]);
                CreateCarName(carNames[i], x);
            }
        }
        else
        {
            for (int i = 3, x = 150; i < carNames.Length; i++, x += 150)
            {
                CreateCarAvatar($"car{i + 1}Locked", x, carDimensions[0, i], carDimensions[1, i]);
            }
        }

        for (int i = 0, x = -330; i < 6; i++, x += 150)
        {
            for (int j = 0, y = -85; j < 6; j++, y -= 16)
            {
                propertyLabelsOfAllAvailableCars[i][j] = CreateLabel(carPropertyAcronyms[j], Color.Black, x, y, 0.6, false);
                Add(propertyLabelsOfAllAvailableCars[i][j]);
            }
        }
    }


    /// <summary>
    /// Creates a game object with an image of a gray star in each index of the list "allStars".
    /// Changes the image of some of the game objects on list "allStars" to a bigger yellow star.
    /// </summary>
    private void AddStars()
    {
        allStars = new List<GameObject[][]>() { new GameObject[6][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                new GameObject[6][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                new GameObject[6][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                new GameObject[6][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] },
                                                new GameObject[6][] { new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5], new GameObject[5] } };

        allActiveStars = new List<int[][]>() { new int[6][] { new int[4], new int[2], new int[2], new int[2], new int[4], new int[3] },
                                               new int[6][] { new int[5], new int[4], new int[2], new int[1], new int[1], new int[4] },
                                               new int[6][] { new int[3], new int[3], new int[3], new int[5], new int[3], new int[1] },
                                               new int[6][] { new int[1], new int[2], new int[5], new int[4], new int[5], new int[3] },
                                               new int[6][] { new int[4], new int[5], new int[1], new int[2], new int[2], new int[2] } };

        for (int i = 0, x = -330; i < availableCars.Count; i++, x += 150)
        {
            for (int j = 0, y = -85; j < 6; j++, y -= 16, x -= 11 * 5)
            {
                for (int k = 0; k < allStars[i][j].Length; k++, x += 11)
                {
                    allStars[i][j][k] = new GameObject(9, 9);
                    allStars[i][j][k].Position = new Vector(x + 25, y);
                    allStars[i][j][k].Image = LoadImage("star_passive");
                    allStars[i][j][k].IsVisible = false;
                    Add(allStars[i][j][k]);
                }
            }
        }

        for (int i = 0; i < availableCars.Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < allActiveStars[i][j].Length; k++)
                {
                    allStars[i][j][k].Image = LoadImage("star_active");
                    allStars[i][j][k].Width = 12;
                    allStars[i][j][k].Height = 12;
                }
            }
        }
    }


    /// <summary>
    /// Creates a custom game object with an image of a car.
    /// Adds the game object to be created to a specific list.
    /// </summary>
    /// <param name="carImage">Imagefile name</param>
    /// <param name="x">X-coordinate of the game object</param>
    /// <param name="width">Width of the game object</param>
    /// <param name="height">height of the game object</param>
    private void CreateCarAvatar(string carImage, double x, double width, double height)
    {
        GameObject car = new GameObject(width / 1.25, height / 1.25);
        car.Position = new Vector(x, 30.0);
        car.Image = LoadImage(carImage);
        availableCars.Add(car);
        Add(car);
    }


    /// <summary>
    /// Creates a custom label with a name of a car.
    /// Adds the label to be created to a specific list.
    /// </summary>
    /// <param name="name">Name of the car</param>
    /// <param name="x">X-coordinate of the label</param>
    /// <param name="y">Y-coordinate of the label</param>
    private void CreateCarName(string name, double x, double y = 150)
    {
        Label carName = CreateLabel(name, Color.White, x, y, 0.8);
        titlesOfAvailableCars.Add(carName);
        Add(carName);
    }


    /// <summary>
    /// Turns specific game objects in the list of "allStars" visible
    /// or invisible depending on mouse coursor position.
    /// </summary>
    /// <param name="availableCars">Available car avatars</param>
    /// <param name="carIndex">Car's index number on the list</param>
    private void ActivateStars(List<GameObject> availableCars, int carIndex)
    {
        if (Mouse.IsCursorOn(availableCars[carIndex]))
        {
            foreach (GameObject[] carPropertyStars in allStars[carIndex])
            {
                foreach (GameObject star in carPropertyStars) star.IsVisible = true;
            }
        }
        else
        {
            foreach (GameObject[] carPropertyStars in allStars[carIndex])
            {
                foreach (GameObject star in carPropertyStars) star.IsVisible = false;
            }
        }
    }


    /// <summary>
    /// Defines the property multipliers of the game zones in endurance mode.
    /// Alters the zone multiplier values depending on the current zone in-game.
    /// Finally calls for the method ZonePause between each zone change.
    /// </summary>
    private void AddZones()
    {
        double pointBalancer = 2.0;
        double spawnBalancer = 2.0;
        double sizeBalancer = 1.25;
        double speedBalancer = 1.15;

        IntMeter zoneCurrent = new IntMeter(1, 1, 7);
        Label zoneMeter = CreateLabel($"Zone {zoneCurrent.Value}", Color.White, Screen.Right - 65, Screen.Bottom + 25);
        zoneMeter.Color = new Color(0, 0, 0, 0.75);
        Add(zoneMeter);

        Timer zoneTimer = new Timer(30);
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


    /// <summary>
    /// Alerts the player of the changing zone in-game.
    /// Changes the appearance of the zone UI.
    /// Momentarily stops the creation of all in-game physics items.
    /// Momentarily stops the depletion of fuel and increase of the points.
    /// </summary>
    /// <param name="pauseLength"></param>
    /// <param name="zoneMeter"></param>
    /// <param name="zoneCurrent"></param>
    /// <param name="speedBalancer"></param>
    private void ZonePause(double pauseLength, Label zoneMeter, IntMeter zoneCurrent, double speedBalancer)
    {
        CreateSound("3");

        Label zoneSwitch = CreateLabel("Zone Up!", new Color(0.0, 1.0, 0.0), scale: 1.5);

        switch (zoneCurrent.Value)
        {
            case 2:
                {
                    zoneSwitch.TextColor = new Color(0.0, 1.0, 0.0);
                    pointTotal.Value += 10;
                    CreateFlow(CreateLabel($"+10 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
            case 3:
                {
                    zoneSwitch.TextColor = Color.GreenYellow;
                    pointTotal.Value += 15;
                    CreateFlow(CreateLabel($"+15 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
            case 4:
                {
                    zoneSwitch.TextColor = Color.Yellow;
                    pointTotal.Value += 20;
                    CreateFlow(CreateLabel($"+20 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
            case 5:
                {
                    zoneSwitch.TextColor = Color.Orange;
                    pointTotal.Value += 25;
                    CreateFlow(CreateLabel($"+25 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
            case 6:
                {
                    zoneSwitch.TextColor = Color.OrangeRed;
                    pointTotal.Value += 30;
                    CreateFlow(CreateLabel($"+30 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
            case 7:
                {
                    zoneMeter.TextColor = Color.Red; zoneMeter.Text = "Zone Max"; zoneSwitch.TextColor = Color.Red; zoneSwitch.Text = "Zone Max!";
                    pointTotal.Value += 100;
                    CreateFlow(CreateLabel($"+100 Score", Color.Yellow, player.X, player.Y, 1));
                    break;
                }
        }
        Add(zoneSwitch);

        foreach (Timer t in gameTimers) t.Stop();
        foreach (PhysicsObject item in addedItems) item.Velocity = new Vector(0, item.Velocity.Y * speedBalancer);
        speedBalancer -= 0.03;

        Timer pauseTimer = new Timer(pauseLength);
        zoneTimers.Add(pauseTimer);
        pauseTimer.Start(1);

        pauseTimer.Timeout += delegate
        {
            zoneSwitch.Destroy();
            foreach (Timer t in gameTimers) t.Start();
        };
    }


    /// <summary>
    /// Creates a custom label based on the received parameters.
    /// </summary>
    /// <param name="labelText">Text of the label to be made</param>
    /// <param name="textColor">Text color of the label to be made</param>
    /// <param name="x">X-coordinate of the label to be made</param>
    /// <param name="y">Y-coordinate of the label to be made</param>
    /// <param name="scale">Text size multiplier of the label to be made</param>
    /// <param name="isVisible">Visibility of the label to be made</param>
    /// <returns>Customized label</returns>
    private Label CreateLabel(string labelText, Color textColor, double x = 0, double y = 0, double scale = 1, bool isVisible = true)
    {
        Label label = new Label(labelText);
        label.TextScale = new Vector(scale, scale);
        label.Position = new Vector(x, y);
        label.TextColor = textColor;
        label.IsVisible = isVisible;
        label.Font = Font.FromContent("font1.otf");
        return label;
    }


    /// <summary>
    /// Updates the color and size multiplier of a label.
    /// </summary>
    /// <param name="l">Label to be updated</param>
    /// <param name="updatedColor">Updated color</param>
    /// <param name="sizeMultiplier">Updated size multiplier</param>
    private void UpdateLabel(Label l, Color updatedColor, double sizeMultiplier)
    {
        l.TextColor = updatedColor;
        l.TextScale = new Vector(sizeMultiplier, sizeMultiplier);
    }


    /// <summary>
    /// Slowly lifts a label starting from the playable character's
    /// position and destroys it after a while.
    /// </summary>
    /// <param name="label">Label to be lifted</param>
    private void CreateFlow(Label label)
    {
        Add(label);

        PhysicsObject lift = new PhysicsObject(1, 1);
        lift.Position = new Vector(player.X, player.Y + 50);
        lift.IgnoresCollisionResponse = true;
        lift.Color = Color.Transparent;
        lift.LifetimeLeft = TimeSpan.FromSeconds(0.8);
        Add(lift);
        lift.Hit(new Vector(0, 100 * lift.Mass));

        Timer tracker = new Timer(0.01);
        tracker.Start();
        tracker.Timeout += delegate { label.Position = lift.Position; if (lift.IsDestroyed) { label.Destroy(); tracker.Stop(); } };
    }


    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="soundFileName">Soundfile name</param>
    private void CreateSound(string soundFileName)
    {
        SoundEffect sound = LoadSoundEffect(soundFileName);
        sound.Play();
    }


    /// <summary>
    /// Plays the selected music file on loop.
    /// </summary>
    /// <param name="track">Selected music file</param>
    private void AddBackgroundMusic(string track)
    {
        MediaPlayer.Stop();
        MediaPlayer.Play(track);
        MediaPlayer.IsRepeating = true;
    }


    /// <summary>
    /// Resets the truth values of "mouseOnButton"
    /// and "soundPlayed" back to default.
    /// </summary>
    private void FormatSounds()
    {
        CreateSound("selected");
        mouseOnButton = false;
        soundPlayed = false;
    }


    /// <summary>
    /// Displays a message to indicate that the player has completed the
    /// game on standard difficulty and unlocked the full game.
    /// </summary>
    private void DisplayUnlockMessage()
    {
        SaveCompletion();

        Label unlocks = CreateLabel("You have beaten arcade mode and unlocked new content!", new Color(0.0, 1.0, 0.0), scale: 0.9);
        unlocks.LifetimeLeft = TimeSpan.FromSeconds(5);
        Add(unlocks, 1);

        GameObject shadow = new GameObject(unlocks.Width + 40, unlocks.Height + 40);
        shadow.Position = unlocks.Position;
        shadow.Color = new Color(0, 0, 0, 0.75);
        shadow.LifetimeLeft = TimeSpan.FromSeconds(5);
        Add(shadow, 0);

        CreateSound("4");
    }


    /// <summary>
    /// Creates a shortlived label of "locked" on screen.
    /// </summary>
    private void LockedContent()
    {
        CreateSound("locked");

        Label lockedContent = CreateLabel("Locked", Color.White, Mouse.PositionOnScreen.X, Mouse.PositionOnScreen.Y, 0.8);
        Add(lockedContent, 1);

        Timer lockedHangTime = new Timer(0.3);
        lockedHangTime.Timeout += delegate { lockedContent.Destroy(); };
        lockedHangTime.Start(1);
    }


    /// <summary>
    /// Calls for the method HandleButton for a label when conditions are met.
    /// </summary>
    /// <param name="openingMenuButtons">List of the labels for which the method is potentially called</param>
    private void OpeningMenuMovement(List<Label> openingMenuButtons)
    {
        mouseOnButton = false;

        if (DataStorage.Exists("lastUsedProfile.xml"))
        {
            HandleButton(openingMenuButtons[0], Color.White, new Color(0, 255, 0), 1, 1);
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i] == "---")
            {
                HandleButton(openingMenuButtons[1], Color.White, new Color(0, 255, 0), 1, 1);
            }
        }

        for (int i = 0; i < profiles.Length; i++)
        {
            if (DataStorage.Exists($"player{i + 1}.xml"))
            {
                HandleButton(openingMenuButtons[2], Color.White, new Color(0, 255, 0), 1, 1);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    /// <summary>
    /// Calls for the method HandleButton for a label when conditions are met.
    /// </summary>
    /// <param name="profileLabels">List of the labels for which the method is potentially called</param>
    private void LoadMenuMovement(List<Label> profileLabels)
    {
        mouseOnButton = false;

        for (int i = 0; i < 5; i++)
        {
            if (DataStorage.Exists($"player{i + 1}.xml"))
            {
                HandleButton(profileLabels[i], Color.Black, new Color(0, 255, 0), 0.9, 0.9);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    /// <summary>
    /// Calls for the method HandleButton for each label in an array.
    /// Game completion affects the the label for which the method is called.
    /// </summary>
    /// <param name="mainMenuButtons">Array of the labels for which the method is potentially called</param>
    private void MainMenuMovement(Label[] mainMenuButtons)
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


    /// <summary>
    /// Calls for the method HandleButton for each label in a list.
    /// Game completion affects the the label for which the method is called.
    /// </summary>
    /// <param name="difficultyMenuButtons">List of the labels for which the method is potentially called</param>
    private void DifficultyMenuMovement(List<Label> difficultyMenuButtons)
    {
        mouseOnButton = false;

        Color[] buttonColors = new Color[3] { new Color(0, 255, 0), Color.GreenYellow, Color.Yellow };

        if (gameFullyUnlocked)
        {
            for (int i = 0; i < difficultyMenuButtons.Count; i++)
            {
                HandleButton(difficultyMenuButtons[i], Color.White, buttonColors[i]);
            }
        }
        else
        {
            for (int i = 0; i < difficultyMenuButtons.Count - 1; i++)
            {
                HandleButton(difficultyMenuButtons[i], Color.White, buttonColors[i]);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    /// <summary>
    /// Calls for the method HandleCarLabel and ActivateStars for each
    /// label in the list "availableCars", altering their appearance based
    /// on the location of the mouse cursor.
    /// </summary>
    private void CarMenuMovement()
    {
        mouseOnButton = false;

        for (int i = 0; i < titlesOfAvailableCars.Count; i++)
        {
            if (Mouse.IsCursorOn(availableCars[i]))
            {               
                foreach (Label propertyOfCar in propertyLabelsOfAllAvailableCars[i]) propertyOfCar.IsVisible = true;
                HandleCarLabel(availableCars, i);
                ActivateStars(availableCars, i);
            }
            else
            {
                foreach (Label propertyOfCar in propertyLabelsOfAllAvailableCars[i]) propertyOfCar.IsVisible = false;
                HandleCarLabel(availableCars, i);
                ActivateStars(availableCars, i);
            }
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    /// <summary>
    /// Calls for the method HandleButton for each of the label in a list.
    /// </summary>
    /// <param name="endMenuButtons">List of the labels for which the method is called</param>
    private void EndMenuMovement(Label[] endMenuButtons)
    {
        mouseOnButton = false;

        for (int i = 0; i < endMenuButtons.Length; i++)
        {
            HandleButton(endMenuButtons[i], Color.White, Color.Gold);
        }

        if (!mouseOnButton) soundPlayed = false;
    }


    /// <summary>
    /// Calls for the methods CreateSound and UpdateLabel
    /// based on the location of the mouse cursor and the truth values
    /// of the variables "mouseOnButton" and "soundPlayed".
    /// </summary>
    /// <param name="button">Label for which the methods are called for
    /// depending on the mouse cursor location related to them</param>
    /// <param name="normal">Default color of the label</param>
    /// <param name="hilight">Highlight color of the label</param>
    /// <param name="sizeNorm">Default size multiplier of the label</param>
    /// <param name="sizeHilight">Highlight size multiplier of the label</param>
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


    /// <summary>
    /// Calls for the methods CreateSound and UpdateLabel
    /// based on the location of the mouse cursor and the truth values
    /// of the variables "mouseOnButton" and "soundPlayed".
    /// </summary>
    /// <param name="availableCars">List of the avatars of the cars' for which
    /// the methods are called for depending on the location of the mouse related to them.
    /// <param name="carNum">Index number of the car on list "availableCars"</param>
    private void HandleCarLabel(List<GameObject> availableCars, int carNum)
    {
        if (Mouse.IsCursorOn(availableCars[carNum]))
        {
            mouseOnButton = true;

            if (!soundPlayed)
            {
                CreateSound("hover");
                soundPlayed = true;
            }

            UpdateLabel(titlesOfAvailableCars[carNum], Color.HotPink, 1);
            titlesOfAvailableCars[carNum].Y = 160;

            availableCars[carNum].Width = carDimensions[0, carNum] / 1.15;
            availableCars[carNum].Height = carDimensions[1, carNum] / 1.15;
        }
        else
        {
            UpdateLabel(titlesOfAvailableCars[carNum], Color.White, 0.8);
            titlesOfAvailableCars[carNum].Y = 150;

            availableCars[carNum].Width = carDimensions[0, carNum] / 1.25;
            availableCars[carNum].Height = carDimensions[1, carNum] / 1.25;
        }
    }
}