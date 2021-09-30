using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using System;
using System.Collections.Generic;

public class autopeli : PhysicsGame
{
    public override void Begin()
    {
        LuoKenttä();
        AsetaOhjaimet();
        AloitaPeli();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    public static void LuoKenttä()
    {
        // TODO: Määritä kentän asetukset.
    }

    public static void AsetaOhjaimet()
    {
        // TODO: Määritä ohjaimet.
    }

    public static void AloitaPeli()
    {
        // TODO: Määritä pelin aloitus.
    }
}