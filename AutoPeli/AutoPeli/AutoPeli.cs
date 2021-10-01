using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class autopeli : PhysicsGame
{
    Vector moveUp = new Vector(0, 400);
    Vector moveDown = new Vector(-0, -400);
    Vector moveLeft = new Vector(-400, 0);
    Vector moveRight = new Vector(400, 0);

    PhysicsObject debris;
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
        OpenMainMenu();
        ChooseDifficulty();
        CreateStage();
        SetControls();
        AddMeters();
        AddTimers();
        StartGame();
    }

    public void OpenMainMenu()
    {
        // TODO: Määritä päävalikko.
    }

    public void CreateStage()
    {
        player = new PhysicsObject(25.0, 25.0);
        player.Shape = Shape.Rectangle;
        player.Y = -150.0;
        player.Restitution = 0.35;
        Add(player);

        // TODO: korjaa?
        AddCollisionHandler(player, HandleCollisions);

        // TODO: for loop
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

        Level.BackgroundColor = Color.Black;
        Camera.ZoomToLevel();
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
        if (hullIntegrity == 4)
        {
            player.Image = ???;
        }
        if (hullIntegrity == 3)
        {
            player.Image = ???;
        }
        else if (hullIntegrity == 2)
        {
            player.Image = ???;
        }
        else if (hullIntegrity == 1)
        {
            player.Image = ???;
        }
        else
        {
            // TODO: Räjäytä auto.
            GameOver();
        }*/

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
        //TODO: Määritä ajastimet.
    }

    public void HandleCollisions(PhysicsObject player, PhysicsObject target)
    {
        // TODO: Määritä törmäykset.
    }

    public void ChooseDifficulty()
    {
        // TODO: Määritä roju.
        if (??? == ???)
        {
            /*CreateDebris();
            StartGame();*/

            debris = new PhysicsObject(5.0, 5.0);
            Vector lvl1 = new Vector(0.0, -250.0);

            for (int i = 0; i < 50; i++)
            {
                debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
                debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
                debris.Hit(lvl1 * debris.Mass);
            }
        }
        else if (??? == ???)
        {
            /*CreateDebris();
            StartGame();*/

            debris = new PhysicsObject(7.5, 7.5);
            Vector lvl2 = new Vector(0.0, -300.0);

            for (int i = 0; i < 75; i++)
            {
                debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
                debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
                debris.Hit(lvl2 * debris.Mass);
            }
        }
        else
        {
            /*CreateDebris();
            StartGame();*/

            debris = new PhysicsObject(10.0, 10.0);
            Vector lvl3 = new Vector(0.0, -350.0);

            for (int i = 0; i < 100; i++)
            {
                debris.X = RandomGen.NextDouble(Screen.Left + 5.0, Screen.Right - 5.0);
                debris.Y = RandomGen.NextDouble(Screen.Top + 20.0, Screen.Top + 980.0);
                debris.Hit(lvl3 * debris.Mass);
            }
        }
    }

    public void CreateDebris()
    {
        // TODO: Siirrä roippeen luonti tänne.
    }

    public void StartGame()
    {
        // TODO: Siirrä roippeen liikkeellelaitto tänne.
    }

    public void EndGame()
    {
        // TODO: Määritä voitto.
    }

    public void GameOver()
    {
        // TODO: Määritä häviö.
    }
}