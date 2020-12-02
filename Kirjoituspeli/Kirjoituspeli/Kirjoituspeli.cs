using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jypeli;
using Jypeli.Widgets;

/// @author Kaisa Koski
/// @version 27.11.2020


public class Kirjoituspeli : Game
{
    private List<string> lauseet;
    private Label mallilause;
    private Label ohje;
    private Timer aikalaskuri;
    private int toistot;
    private bool voikoKirjoittaa = false;
    private ScoreList toplista;

    private const int MAX_TOISTOT = 3;
    private const double ODOTUS = 2.2;

    //TODO: Koristelut: värit, kuvat, äänet
    //TODO: Pelitilanteesta Escilla takaisin alkuvalikkoon?
    //TODO: Uusi ennätys ei tallennu, jos pääsee top-listaan, mutta sulkee pelin sulkematta ensin listaa.

    /// <summary>
    /// Käynnistää pelin alussa. 
    /// </summary>
    public override void Begin()
    {
        Aloita();
    }


    /// <summary>
    /// Aliohjelma tyhjentää pelin olioista, asettaa taustavärin, lataa pistelistan
    /// ja avaa alkuvalikon.
    /// </summary>
    private void Aloita() 
    {
        ClearAll();
        Level.Background.CreateGradient(Color.SkyBlue, Color.Pink);
        AvaaAlkuvalikko();
        LataaPisteet();
        Keyboard.Listen(Key.LeftShift, ButtonState.Pressed, VastaaKysymykseen, null);
    }


    /// <summary>
    /// Avaa alkuvalikon.
    /// </summary>
    private void AvaaAlkuvalikko()
    {
        MultiSelectWindow alkuvalikko = new MultiSelectWindow("Kirjoituspeli", "Uusi peli", "Ohjeet", "Parhaat pisteet", "Lopeta");
        alkuvalikko.AddItemHandler(0, AloitaPeli);
        alkuvalikko.AddItemHandler(1, NaytaPeliohjeet);
        alkuvalikko.AddItemHandler(2, NaytaToplista, "");
        alkuvalikko.AddItemHandler(3, Exit);
        alkuvalikko.DefaultCancel = -1;
        Add(alkuvalikko);
    }


    /// <summary>
    /// Lataa ohjeet tekstitiedostosta ja asettaa ne näytille.
    /// </summary>
    private void NaytaPeliohjeet()
    {
        List<string> peliohje = new List<string>(LataaTiedosto("ohjeet.txt"));
        Label ohjeteksti = new Label("");
        StringBuilder sb = new StringBuilder(peliohje[0]);
        for (int i = 1; i < peliohje.Count; i++)
        {
            sb.Append("\n" + peliohje[i]);
        }
        ohjeteksti.Text = sb.ToString();
        ohjeteksti.TextColor = Color.MediumPurple;
        //ohjeteksti.BorderColor = Color.Black;
        Font omaFontti = new Font(30);
        // omaFontti.StrokeAmount = 1;
        ohjeteksti.Font = omaFontti;
        Add(ohjeteksti);
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, delegate { Remove(ohjeteksti); AvaaAlkuvalikko(); }, null);
    }


    /// <summary>
    /// Aloittaa varsinaisen pelikerran, lataa lauseet tiedostosta,
    /// laittaa ensimmäisen mallilauseen esille ja luo aikalaskurin.
    /// </summary>
    private void AloitaPeli()
    {
        lauseet = LataaTiedosto("lauseet.txt");
        toistot = MAX_TOISTOT;
        mallilause = new Label(ArvoLause());
        mallilause.Position = new Vector(0, Level.Top * 0.5);
        Add(mallilause);
        LuoAikaLaskuri();
    }


    /// <summary>
    /// Lisää parametrina annetun tekstitiedoston rivit merkkijonolistaan. 
    /// </summary>
    /// <param name="tiedosto">Tekstitiedosto</param>
    /// <returns>String-lista tiedoston riveistä</returns>
    private List<string> LataaTiedosto(string tiedosto)
    {
        List<string> lista = new List<string>();
        try
        {
            lista = File.ReadAllLines(tiedosto).ToList();
        }
        catch (FileNotFoundException)
        {
            Virheilmoitus("Tekstitiedosto puuttuu");
        }
        return lista;
    }


    /// <summary>
    /// Lataa top5-pisteet tekstitiedostosta ja lisää ne listaan.
    /// </summary>
    private void LataaPisteet()
    {
        toplista = new ScoreList(5, true, 999.99, "-");
        try
        {
            toplista = DataStorage.TryLoad<ScoreList>(toplista, "pisteet.xml");
        }
        catch (FileNotFoundException)
        {
            Virheilmoitus("Puuttuva pistetiedosto");
        }
        catch (System.Xml.XmlException)
        {
            Virheilmoitus("Virheellinen pistetiedosto");
        }
    }


    /// <summary>
    /// Lisää peliin aikalaskurin.
    /// </summary>
    private void LuoAikaLaskuri()
    {
        aikalaskuri = new Timer();
        Label aikanaytto = new Label();
        aikanaytto.TextColor = Color.White;
        aikanaytto.DecimalPlaces = 2;
        aikanaytto.BindTo(aikalaskuri.SecondCounter);
        aikanaytto.Position = new Vector(Level.Right * 0.9, Level.Top * 0.9);
        Add(aikanaytto);
    }


    /// <summary>
    /// Avaa kirjoitusikkunan ja laittaa sekuntikellon päälle,
    /// jos vastaamisen aloittaminen on mahdollista.
    /// </summary>
    private void VastaaKysymykseen()
    {
        if (voikoKirjoittaa)
        {
            ohje.Text = "";
            LuoKysymysikkuna();
            aikalaskuri.Start();
        }
    }


    /// <summary>
    /// Arpoo ja palauttaa listasta satunnaisen lauseen, jos toistojen 
    /// määrä on yli 0. Kun lause on arvottu, se poistetaan listasta. 
    /// Jos listassa on vähemmän lauseita kuin toistojen määrä, 
    /// tekee virheilmoituksen ja palauttaa tyhjän merkkijonon.
    /// </summary>
    /// <returns>Satunnainen alkio string-listasta tai tyhjä merkkijono</returns>
    private string ArvoLause()
    {
        if (toistot <= 0)
        {
            Virheilmoitus("Virhe toistojen määrässä");
            return "";
        }
        if ((lauseet == null) || (lauseet.Count < toistot))
        {
            Virheilmoitus("Lausetta ei voitu ladata");
            return "";
        }
        toistot--;
        int lauseenIndeksi = RandomGen.NextInt(lauseet.Count());
        string lause = lauseet[lauseenIndeksi];
        lauseet.RemoveAt(lauseenIndeksi);
        OhjeEsille();
        return lause;
    }


    /// <summary>
    /// Tyhjentää peliruudulta kaikki oliot ja lisää virheestä
    /// ilmoittavan ilmoituksen ruudulle.
    /// </summary>
    /// <param name="virhe">Merkkijono, joka tarkentaa mistä virheestä on kyse</param>
    private void Virheilmoitus(string virhe)
    {
        ClearAll();
        Level.Background.CreateGradient(Color.SkyBlue, Color.Pink);
        Label[] v = new Label[3];
        string[] t = new string[] { "VIRHE", virhe, "Paina Enter-painiketta sulkeaksesi pelin" }; //Ilmoitukset eri labeleilla, jotta asettelu on kauniimpi.
        double y = Level.Top * 0.3;
        for (int i = 0; i < v.Length; i++)
        {
            v[i] = new Label(t[i]);
            v[i].TextColor = Color.Red;
            v[i].Position = new Vector(0, y);
            Add(v[i]);
            y -= v[i].Height * 1.5;
        }
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, Exit, null);
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
    /// <param name="teksti">Teksti, joka tulee kirjoitusikkunan kirjoituskenttään, oletuksena tyhjä</param>
    private void LuoKysymysikkuna( string viesti = "", string teksti="")
    {
        InputWindow kysymysikkuna = new InputWindow(viesti);
        kysymysikkuna.InputBox.Text = teksti;
        kysymysikkuna.InputBox.Cursor.Width = 2.5;
        kysymysikkuna.InputBox.Cursor.Color = Color.Pink;
        kysymysikkuna.TextEntered += ProcessInput;
        kysymysikkuna.InputBox.Color = Color.White;
        kysymysikkuna.BorderColor = Color.Darker(Color.SkyBlue, 50);
        Add(kysymysikkuna);
    }


    /// <summary>
    /// Aliohjelmassa verrataan käyttäjän teksti-ikkunaan 
    /// kirjoittamaa tekstiä mallilauseeseen. Jos vastaus on oikein,
    /// arpoon uuden mallilauseen (jos 0 < toistot) tai siirtyy ajan 
    /// vertailuun (toistot=0). Jos vastaus on väärin, avaa uuden
    /// vastausikkunan.
    /// </summary>
    /// <param name="ikkuna">Teksti-ikkuna, johon käyttäjä kirjoittaa vastauksensa</param>
    private void ProcessInput(InputWindow ikkuna)
    {
        string vastaus = ikkuna.InputBox.Text.Trim('\r', '\n');
        if (vastaus.Equals(mallilause.Text))
        {
            aikalaskuri.Pause();
            voikoKirjoittaa = false;
            if (toistot == 0)
            {
                mallilause.Text = "Oikein! \n Valmis!";
                double aika = aikalaskuri.CurrentTime;
                Timer.SingleShot(ODOTUS, delegate { mallilause.Text = ""; Ajantarkastus(aika); });
            }
            else
            {
                mallilause.Text = "Oikein!";
                Timer.SingleShot(ODOTUS, delegate { mallilause.Text = ArvoLause(); });
            }
        }
        else
        {
            LuoKysymysikkuna("Yritä uudelleen!", vastaus);
        }
    }


    /// <summary>
    /// Tarkastaa, pääseekö parametrilla annetulla arvolla
    /// parhaiden tulosten joukkoon.
    /// </summary>
    /// <param name="aika">Uusi arvo, jonka kelpaavuutta top-listalle tarkastellaan</param>
    private void Ajantarkastus(double aika)
    {
        if (toplista.Qualifies(aika))
        {
            InputWindow kysyNimi = new InputWindow("Pääsit TOP5-listalle, anna nimesi:");
            kysyNimi.MaxCharacters = 6;
            kysyNimi.InputBox.Color = Color.White;
            kysyNimi.BorderColor = Color.Darker(Color.SkyBlue, 50);
            kysyNimi.TextEntered += delegate { LisaaNimi(kysyNimi); };
            Add(kysyNimi);
        }
        else
        {
            string eiEnnatys = "Tällä kertaa ei tullut uutta ennätystä. \n Aikasi oli " + aika.ToString("N2");
            NaytaToplista(eiEnnatys);
        }
    }



    /// <summary>
    /// Lisää käyttäjän antaman nimen sekä päivämäärän ja ajan 
    /// tuloslistalle. Sitten näyttää tuloslistan.
    /// </summary>
    /// <param name="ikkuna">Kysymysikkuna, johon käyttäjä on kirjoittanut nimensä</param>
    private void LisaaNimi(InputWindow ikkuna)
    {
        string nimi = ikkuna.InputBox.Text.Trim('\r', '\n');
        string pvm = DateTime.Now.ToString("g");
        string listamerkinta = nimi.PadRight(20) + pvm;
        int sijoitus = toplista.Add(listamerkinta, Math.Round(aikalaskuri.CurrentTime, 4)) - 1;
        string ennatys = "Onnittelut, pääsit sijalle " + sijoitus;
        NaytaToplista(ennatys);
    }


    /// <summary>
    /// Näyttää tuloslistan ja sen yllä parametrina annetun tekstin
    /// (oletuksena tyhjä). Kun ikkuna suljetaan, tallentaa toplistan tiedostoon,
    /// poistaa kaikki pelin oliot japalaa pelin aloitukseen.
    /// </summary>
    /// <param name="teksti">Teksti joka näytetään tuloslistan yläpuolella</param>
    private void NaytaToplista(string teksti = "")
    {
        Label label = new Label(teksti);
        label.Position = new Vector(0, Level.Top * 0.4);
        Add(label);
        HighScoreWindow topIkkuna = new HighScoreWindow(Level.Width * 0.5, Level.Height * 0.5, "TOP5", toplista);
        topIkkuna.Position = new Vector(0, Level.Top * -0.2);
        Add(topIkkuna);
        topIkkuna.Closed += delegate
        {
            DataStorage.Save<ScoreList>(toplista, "pisteet.xml");
            Aloita();
        };
    }
}

