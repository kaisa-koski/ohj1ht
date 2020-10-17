using System;
using System.Collections.Generic;
using System.IO;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Kirjoituspeli : Game //TODO: Pitäisi löytyä taulukko ja silmukka. Taulukko on jo mallilauseiden varastoinnissa. Silmukka varmaankin parhaiden aikojen vertailussa?
{
    string[] lauseet;   //TODO: Voidaanko attribuutit välttää ja onko se tavoiteltavaa?
    Label mallilause;

    //TODO: Täydennä dokumentointi
    /// <summary>
    /// Aliohjelma käynnistää pelin 
    /// </summary>
    public override void Begin()
    {
        Camera.ZoomToLevel();
        Level.BackgroundColor = Color.SkyBlue;
        LataaLauseet();
        MallilauseEsille();
        LuoKysymysikkuna();

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Aliohjelma arpoo mallilausetaulukosta satunnaisen 
    /// mallilauseen ja lisää sen ruudulle.
    /// </summary>
    private void MallilauseEsille()
    {
        mallilause = new Label(lauseet[RandomGen.NextInt(lauseet.Length)]);
        mallilause.Position = new Vector(0, Level.Top * 0.5);
        Add(mallilause);
    }


    /// <summary>
    /// Lataa kirjoitettavat esimerkkilauseet tekstitiedostosta.
    /// </summary>
    private void LataaLauseet()
    {
        lauseet = File.ReadAllLines("lauseet.txt");
    }


    /// <summary>
    /// Aliohjelma luo ja lisää peliin kysymysikkunan, johon käyttäjä 
    /// kirjoittaa vastauksensa.
    /// </summary>
    /// <param name="viesti">Teksti, joka tulee kirjoitusikkunan yläpuolelle</param>
    private void LuoKysymysikkuna(string viesti = "Kirjoita tähän")
    {
        InputWindow kysymysikkuna = new InputWindow(viesti);
        kysymysikkuna.TextEntered += ProcessInput;
        kysymysikkuna.InputBox.Color = Color.White;
        kysymysikkuna.BorderColor = Color.Darker(Color.SkyBlue, 50);
        Add(kysymysikkuna);
    }


    /// <summary>
    /// Aliohjelmassa verrataan käyttäjän teksti-ikkunaan 
    /// kirjoittamaa tekstiä mallilauseeseen.
    /// </summary>
    /// <param name="ikkuna">Teksti-ikkuna, johon käyttäjä kirjoittaa vastauksensa</param>
    void ProcessInput(InputWindow ikkuna)
    {
        string vastaus = ikkuna.InputBox.Text;
        if (vastaus != mallilause.Text)  MessageDisplay.Add("Oikein!"); //TODO: Pysähtyykö kello vasta silloin kun lause menee oikein?
        else LuoKysymysikkuna("Yritä uudelleen!");
    }


}

