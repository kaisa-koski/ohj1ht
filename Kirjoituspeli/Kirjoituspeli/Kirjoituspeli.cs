using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Kaisa Koski
/// @version 25.11.2020


public class Kirjoituspeli : Game
{
    private List<string> lauseet;
    private Label mallilause;
    private Label ohje; //TODO: Onko tarpeellinen?
    private Timer aikalaskuri;
    private InputWindow kysymysikkuna; //TODO: saisiko poistettua?
    private int toistot = 1;
    private bool voikoKirjoittaa = false; //TODO: Arvioi, voisiko tämän tehdä paremmin
    ScoreList toplista = new ScoreList(5, true, 999.999, "nimetön");



    /// <summary>
    /// Käynnistää pelin 
    /// </summary>
    public override void Begin()
    {
        Level.BackgroundColor = Color.SkyBlue;
        //Level.Background.CreateGradient(Color.SkyBlue, Color.Pink);
        LataaLauseet();
        LataaPisteet();
        MallilauseEsille();
        LuoAikaLaskuri();

        HighScoreWindow topIkkuna = new HighScoreWindow(Level.Width * 0.5, Level.Height * 0.5, "TOP5", toplista); //TODO: poista kun ei enää tarvitse
        
        Add(topIkkuna);


        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli"); //TODO: Tätä ei ehkä tarvitse, jos peliin tulee alkuvalikko myöhemmin
        Keyboard.Listen(Key.LeftShift, ButtonState.Pressed, VastaaKysymykseen, null);
    }


    /// <summary>
    /// Lataa kirjoitettavat esimerkkilauseet tekstitiedostosta ja lisää ne listaan.
    /// </summary>
    private void LataaLauseet()
    {
        try
        {
            lauseet = File.ReadAllLines("lauseet.txt").ToList();
        }
        catch (FileNotFoundException)
        {
            Label virheilmoitus = new Label("Pelin tekstitiedosto virheellinen, sulje peli");
            virheilmoitus.TextColor = Color.Red;
            virheilmoitus.Position = new Vector(0, Level.Bottom * 0.80);
            Add(virheilmoitus);
        }
    }


    /// <summary>
    /// Lataa top5-pisteet tekstitiedostosta ja lisää ne listaan.
    /// </summary>
    private void LataaPisteet()
    {
        try
        {
            toplista = DataStorage.TryLoad<ScoreList>(toplista, "pisteet.txt");

        }
        catch (FileNotFoundException)
        {
            Label virheilmoitus = new Label("HUOM! Pelin pistetiedostossa virhe, parhaiden aikojen tallennus ei onnistu");
            virheilmoitus.TextColor = Color.Red;
            virheilmoitus.Position = new Vector(0, Level.Bottom * 0.9);
            Add(virheilmoitus);
        }
    }


    /// <summary>
    /// Aliohjelma lisää kirjoitettavan mallilauseen ruudulle pelin alussa.
    /// </summary>
    private void MallilauseEsille()
    {
        mallilause = new Label(ArvoLause());
        mallilause.Position = new Vector(0, Level.Top * 0.5);
        Add(mallilause);
    }


    /// <summary>
    /// Aliohjelma luo ja lisää peliin aikalaskurin.
    /// </summary>
    private void LuoAikaLaskuri()
    {
        aikalaskuri = new Timer();

        Label aikanaytto = new Label();
        aikanaytto.TextColor = Color.White;
        aikanaytto.DecimalPlaces = 3;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        aikanaytto.Position = new Vector(Level.Right * 0.9, Level.Top * 0.9);
        Add(aikanaytto);
    }


    /// <summary>
    /// Avaa kirjoitusikkunan ja laittaa sekuntikellon päälle.
    /// </summary>
    private void VastaaKysymykseen()
    {
        if (voikoKirjoittaa)
        {
            ohje.Text = ""; //TODO: Mieti tätä vielä, pystyisikö tekemään ilman attribuuttia.                                
            LuoKysymysikkuna();
            aikalaskuri.Start();
        }
    }


    /// <summary>
    /// Arpoo ja palauttaa mallilauseista satunnaisen lauseen, kunnes lauseita on arvottu 
    /// toistojen verran. Kun lause on arvottu, se poistetaan listasta, joten 
    /// samaa lausetta ei tule samalla pelikerralla kahdesti. Jos listassa on
    /// vähemmän lauseita kuin toistojen määrä, palauttaa virheilmoituksen.
    /// </summary>
    /// <returns>Mallilause, pelikierroksen loppumisilmoitus tai virheilmoitus</returns>
    private string ArvoLause()
    {
        if ((lauseet == null) || (lauseet.Count < toistot)) return "Mallilausetta ei voitu ladata";
        toistot--;
        int lauseenIndeksi = RandomGen.NextInt(lauseet.Count());
        string lause = lauseet[lauseenIndeksi];
        lauseet.RemoveAt(lauseenIndeksi);
        OhjeEsille();
        return lause;
    }


    /// <summary>
    /// Laittaa painikeohjeen esille. Kun ohje on esillä, 
    /// vastaamisen pystyy aloittamaan. 
    /// </summary>
    private void OhjeEsille()
    {
        ohje = new Label("(Paina Shift aloittaaksesi kirjoittaminen)");
        ohje.TextColor = Color.White;
        Add(ohje);
        voikoKirjoittaa = true;
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
        if (vastaus.Equals(mallilause.Text))
        {
            aikalaskuri.Pause();
            voikoKirjoittaa = false;

            mallilause.Text = "Oikein!";
            if (toistot == 0)
            {
                double aika = aikalaskuri.CurrentTime;
                string aikaMj = aika.ToString("N3");
                mallilause.Text = "Valmis! " +
                                   "\n Aikasi oli " + aikaMj + " sekuntia.";
                Timer.SingleShot(2.5, delegate { PelinLopetus(aika); });

            }
            else Timer.SingleShot(2.5, delegate { mallilause.Text = ArvoLause(); });
        }
        else
        {
            LuoKysymysikkuna("Yritä uudelleen!");
            //TODO: Laittaisiko tähän kellon menemään pauselle ja näyttäisi mikä meni väärin?
        }
    }


    //TODO: Dokumentointi
    /// <summary>
    /// 
    /// </summary>
    /// <param name="aika"></param>
    private void PelinLopetus(double aika)
    {

        if (toplista.Qualifies(aika))
        {
            InputWindow kysyNimi = new InputWindow("Pääsit TOP5-listalle, anna nimesi:");
            kysyNimi.MaxCharacters = 10;
            kysyNimi.InputBox.Color = Color.White;
            kysyNimi.BorderColor = Color.Darker(Color.SkyBlue, 50);
            kysyNimi.TextEntered += LisaaNimi;
            Add(kysyNimi);
        }
        //TODO: Alkuvalikkoon palaaminen
    }

    //TODO: Dokumentointi
    /// <summary>
    /// 
    /// </summary>
    /// <param name="kysyNimi"></param>
    private void LisaaNimi(InputWindow kysyNimi)
    {
        string nimi = kysyNimi.InputBox.Text;
        string pvm = DateTime.Now.ToString("g");
        string listamerkinta = nimi + "\t\t\t\t" + pvm;
        int sijoitus = toplista.Add(listamerkinta, Math.Round(aikalaskuri.CurrentTime, 4)) - 1;
        Label ennatys = new Label("Onnittelut, pääsit sijalle " + sijoitus);
        ennatys.Position = new Vector(0, mallilause.Y * 0.8);
        Add(ennatys);
        DataStorage.Save<ScoreList>(toplista, "pisteet.txt");
        HighScoreWindow topIkkuna = new HighScoreWindow(Level.Width * 0.5, Level.Height * 0.5, "TOP5", toplista);
        topIkkuna.Position = new Vector(0, ennatys.Y * 0.8 - topIkkuna.Height / 2);
        Add(topIkkuna);
    }
}

