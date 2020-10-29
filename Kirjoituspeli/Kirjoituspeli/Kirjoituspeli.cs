using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Kirjoituspeli : Game
{
    private List<string> lauseet;   //TODO: Ohjaaja: Onko jotain sääntöä milloin attribuutteja pitää/kannattaa käyttää?
    private Label mallilause;
    private Timer aikalaskuri;
    private InputWindow kysymysikkuna;
    private int toistot = 3;
    

    /// <summary>
    /// Aliohjelma käynnistää pelin 
    /// </summary>
    public override void Begin()
    {
        //Camera.ZoomToLevel(); //Ei taida tässä vaikuttaa mitenkään?
        Level.BackgroundColor = Color.SkyBlue;
        LataaLauseet();
        MallilauseEsille();
        LuoAikaLaskuri();

        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli"); //TODO: Tätä ei ehkä tarvitse, jos peliin tulee alkuvalikko myöhemmin
        Keyboard.Listen(Key.LeftShift, ButtonState.Pressed, VastaaKysymykseen, null);
    }

    /// <summary>
    /// Avaa kirjoitusikkunan ja laittaa sekuntikellon päälle.
    /// </summary>
    private void VastaaKysymykseen()
    {
        if (!(toistot < 0))             //TODO: Ohjaaja: Olisiko tähän parempaa vaihtoehtoa, ettei shift enää lopussa toteuttaisi tätä aliohjelmaa?
        {
            LuoKysymysikkuna();
            aikalaskuri.Start();
        }
    }


    /// <summary>
    /// Aliohjelma lisää kirjoitettavan mallilauseen ruudulle.
    /// </summary>
    private void MallilauseEsille()
    {
        mallilause = new Label(ArvoLause());
        mallilause.Position = new Vector(0, Level.Top * 0.5);
        Label shiftOhje = new Label("(Paina Shift aloittaaksesi kirjoittaminen)");
        shiftOhje.TextColor = Color.White;
        Add(mallilause);
        Add(shiftOhje);
    }


    /// <summary>
    /// Arpoo mallilauseista satunnaisen lauseen, kunnes lauseita on arvottu attribuuttine 
    /// määritellyn toistojen verran. Kun lause on arvottu, se poistetaan listasta, joten 
    /// samaa lausetta ei tule samalla pelikerralla kahdesti.
    /// </summary>
    /// <returns>Mallilause tai ilmoitus "Valmis!"</returns>
    private string ArvoLause()
    {
        toistot--;
        if (toistot < 0)
        {
            return "Valmis!";
            //TODO: Tässä voisi tehdä jotain sille, ettei aikalaskuri ja kysymysikkuna mene enää lopussa päälle.
            //TODO: Ajan vertailu top5 listan kanssa 
        }
        int lauseenIndeksi = RandomGen.NextInt(lauseet.Count());
        string lause = lauseet[lauseenIndeksi];
        lauseet.RemoveAt(lauseenIndeksi);
        return lause;
    }


    /// <summary>
    /// Lataa kirjoitettavat esimerkkilauseet tekstitiedostosta ja lisää ne listaan.
    /// </summary>
    private void LataaLauseet()
    {
        lauseet = File.ReadAllLines("lauseet.txt").ToList();
    }


    /// <summary>
    /// Aliohjelma luo ja lisää peliin kysymysikkunan, johon käyttäjä 
    /// kirjoittaa vastauksensa.
    /// </summary>
    /// <param name="viesti">Teksti, joka tulee kirjoitusikkunan yläosaan, oletuksena tyhjä</param>
    private void LuoKysymysikkuna(string viesti = "")
    {
        kysymysikkuna = new InputWindow(viesti);
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
    private void ProcessInput(InputWindow ikkuna)
    {
        string vastaus = ikkuna.InputBox.Text;
        if (vastaus == mallilause.Text)
        {
            aikalaskuri.Pause();
            mallilause.Text = "Oikein!";
            Timer.SingleShot(3.0,
            delegate { mallilause.Text = ArvoLause(); }
        );
        }
        else
        {
            LuoKysymysikkuna("Yritä uudelleen!"); //TODO: Laittaisiko tähän kellon menemään pauselle ja näyttäisi mikä meni väärin?
        }
    }


    /// <summary>
    /// Aliohjelma luo ja lisää peliin aikalaskurin.
    /// </summary>
    private void LuoAikaLaskuri()
    {
        aikalaskuri = new Timer();

        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.White;
        aikaNaytto.DecimalPlaces = 2;
        aikaNaytto.BindTo(aikalaskuri.SecondCounter);
        aikaNaytto.Position = new Vector(Level.Right * 0.9, Level.Top * 0.9); //TODO: Onko parempaa tapaa näiden paikkojen asettamiseen?
        Add(aikaNaytto);
    }
}

