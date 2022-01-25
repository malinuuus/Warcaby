using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WarcabyTest
{
    public partial class Form1 : Form
    {
        public static Button[,] przyciski;
        public static int[,] ustawieniePionkow;

        public static PrzesuwanyPionek przesuwanyPionek;

        public static List<Pozycja> pozycjePodswietlanychPol = new List<Pozycja>();
        public static List<Pozycja> pozycjePionkowZmuszonychDoZbicia = new List<Pozycja>();

        public static bool kolejBialych;

        Image obrazekCzarnegoPionka, obrazekBialegoPionka, obrazekCzarnejDamki, obrazekBialejDamki;

        public static StylKolorow aktualnyStyl;
        public static int aktualnyStylIndex;

        // 2 - czarny damka, 1 - czarny, 0 - puste, -1 - biały, -2 - biały damka

        public struct PrzesuwanyPionek
        {
            public int indexI;
            public int indexJ;
            public int znak;
        }

        public struct Pozycja
        {
            public int indexI;
            public int indexJ;

            public Pozycja(int i, int j)
            {
                indexI = i;
                indexJ = j;
            }
        }

        public struct StylKolorow
        {
            public Color kolorPrzycisku1;
            public Color kolorPrzycisku2;
            public Color kolorTla;
            public Color kolorPodswietlenia;

            public StylKolorow(Color przycisk1, Color przycisk2, Color tlo, Color podstwietlenie)
            {
                kolorPrzycisku1 = przycisk1;
                kolorPrzycisku2 = przycisk2;
                kolorTla = tlo;
                kolorPodswietlenia = podstwietlenie;
            }
        }

        public Form1()
        {
            InitializeComponent();

            ustawieniePionkow = new int[8, 8];

            przyciski = new Button[8, 8]
            {
                {button1, button2, button3, button4, button5, button6, button7, button8},
                {button9, button10, button11, button12, button13, button14, button15, button16},
                {button17, button18, button19, button20, button21, button22, button23, button24},
                {button25, button26, button27, button28, button29, button30, button31, button32},
                {button33, button34, button35, button36, button37, button38, button39, button40},
                {button41, button42, button43, button44, button45, button46, button47, button48},
                {button49, button50, button51, button52, button53, button54, button55, button56},
                {button57, button58, button59, button60, button61, button62, button63, button64},
            };

            string[] sciezki = { "pionekCzarny.png", "pionekBialy.png", "pionekCzarnyDamka.png", "pionekBialyDamka.png" };
            if (SprawdzPlikiZdjec(sciezki))
            {
                obrazekCzarnegoPionka = new Bitmap(sciezki[0]);
                obrazekBialegoPionka = new Bitmap(sciezki[1]);
                obrazekCzarnejDamki = new Bitmap(sciezki[2]);
                obrazekBialejDamki = new Bitmap(sciezki[3]);
            }
            else
            {
                MessageBox.Show("Nie znaleziono plików z obrazkami pionków lub mają błędne nazwy!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Shown += new EventHandler(Form1_CloseOnStart);
            }

            aktualnyStylIndex = 0;
            ZmienStyl();

            labelWygrana.Visible = false;
            przesuwanyPionek.znak = -1;
            kolejBialych = true;
            ZablokujPola();
        }

        private void Form1_CloseOnStart(object sender, EventArgs e)
        {
            Close();
        }

        public bool SprawdzPlikiZdjec(string[] sciezki)
        {
            foreach (string sciezka in sciezki)
            {
                if (!File.Exists(sciezka))
                {
                    return false;
                }
            }

            return true;
        }

        public void Ruch(int indexI, int indexJ)
        {
            // zapisanie pozycji i znaku kliknięto pionka
            if (ustawieniePionkow[indexI, indexJ] != 0)
            {
                przesuwanyPionek.indexI = indexI;
                przesuwanyPionek.indexJ = indexJ;
                przesuwanyPionek.znak = ustawieniePionkow[indexI, indexJ];

                ResetujPodswietleniePol();
            }

            PodstwietleniePolDoZbicia(indexI, indexJ);

            if (pozycjePionkowZmuszonychDoZbicia.Count > 0 && pozycjePodswietlanychPol.Count == 0)
            {
                WyswietlKomunikat("Musisz zbić jeśli jest taka możliwość!");
            }

            // sprawdzenie, czy kliknięto w pole na które można przesunąć pionka

            bool czyZnaleziono = false;

            foreach (Pozycja pozycja in pozycjePodswietlanychPol)
            {
                // przesunięcie pionka
                if (pozycja.indexI == indexI && pozycja.indexJ == indexJ)
                {
                    ustawieniePionkow[przesuwanyPionek.indexI, przesuwanyPionek.indexJ] = 0;
                    przyciski[przesuwanyPionek.indexI, przesuwanyPionek.indexJ].BackgroundImage = null;

                    // biały pionek zmienia się w damkę
                    if (indexI == 0 && przesuwanyPionek.znak == -1)
                    {
                        ustawieniePionkow[indexI, indexJ] = -2;
                        przyciski[indexI, indexJ].BackgroundImage = obrazekBialejDamki;
                    }
                    // czarny pionek zmienia się w damkę
                    else if (indexI == 7 && przesuwanyPionek.znak == 1)
                    {
                        ustawieniePionkow[indexI, indexJ] = 2;
                        przyciski[indexI, indexJ].BackgroundImage = obrazekCzarnejDamki;
                    }
                    else
                    {
                        ustawieniePionkow[indexI, indexJ] = przesuwanyPionek.znak;

                        switch (przesuwanyPionek.znak)
                        {
                            case -2:
                                przyciski[indexI, indexJ].BackgroundImage = obrazekBialejDamki;
                                break;

                            case -1:
                                przyciski[indexI, indexJ].BackgroundImage = obrazekBialegoPionka;
                                break;

                            case 1:
                                przyciski[indexI, indexJ].BackgroundImage = obrazekCzarnegoPionka;
                                break;

                            case 2:
                                przyciski[indexI, indexJ].BackgroundImage = obrazekCzarnejDamki;
                                break;
                        }
                    }

                    ZbiciePionka(indexI, indexJ); // zbicie pionka
                    ResetujPodswietleniePol(); // wyłączenie podświetlania pól

                    labelCzyjaKolej.Text = przesuwanyPionek.znak > 0 ? "BIAŁE PIONKI" : "CZARNE PIONKI";

                    // sprawdzenie czy, przeciwnik może (musi) zbić
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (ustawieniePionkow[i, j] * przesuwanyPionek.znak < 0)
                            {
                                // jeżeli czarny kończy ruch
                                if (przesuwanyPionek.znak < 0)
                                {
                                    BicieDoTylu(i, j);

                                    // jeżeli czarna damka może bić do przodu
                                    if (ustawieniePionkow[i, j] == 2)
                                        BicieDoPrzodu(i, j);
                                }

                                // jeżeli biały kończy ruch
                                if (przesuwanyPionek.znak > 0)
                                {
                                    BicieDoPrzodu(i, j);

                                    // jeżeli biała damka może bić do tyłu
                                    if (ustawieniePionkow[i, j] == -2)
                                        BicieDoTylu(i, j);
                                }
                            }
                        }
                    }

                    kolejBialych = !kolejBialych;
                    przesuwanyPionek.znak *= -1;
                    czyZnaleziono = true;
                    break;
                }
            }

            ZablokujPola();

            // podświetlnie dostępnych pól do przesunięcia się bez bicia

            if (!czyZnaleziono && ustawieniePionkow[indexI, indexJ] != 0 && pozycjePionkowZmuszonychDoZbicia.Count == 0)
            {
                // jeżeli czarny lub damka
                if (przesuwanyPionek.znak == 1 || Math.Abs(przesuwanyPionek.znak) == 2)
                {
                    // dolna granica
                    if (indexI <= 6)
                    {
                        // lewa granica
                        if (indexJ >= 1)
                        {
                            if (ustawieniePionkow[indexI + 1, indexJ - 1] == 0)
                                PodswietleniePola(indexI + 1, indexJ - 1);
                        }

                        // prawa granica
                        if (indexJ <= 6)
                        {
                            if (ustawieniePionkow[indexI + 1, indexJ + 1] == 0)
                                PodswietleniePola(indexI + 1, indexJ + 1);
                        }
                    }
                }

                // jeżeli biały lub damka
                if (przesuwanyPionek.znak == -1 || Math.Abs(przesuwanyPionek.znak) == 2)
                {
                    // górna granica
                    if (indexI >= 1)
                    {
                        // lewa granica
                        if (indexJ >= 1)
                        {
                            if (ustawieniePionkow[indexI - 1, indexJ - 1] == 0)
                                PodswietleniePola(indexI - 1, indexJ - 1);
                        }

                        // prawa granica
                        if (indexJ <= 6)
                        {
                            if (ustawieniePionkow[indexI - 1, indexJ + 1] == 0)
                                PodswietleniePola(indexI - 1, indexJ + 1);
                        }
                    }
                }
            }
        }

        public void ZablokujPola()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (kolejBialych)
                    {
                        if (ustawieniePionkow[i, j] > 0)
                            przyciski[i, j].Enabled = false;

                        else
                            przyciski[i, j].Enabled = true;
                    }
                    else
                    {
                        if (ustawieniePionkow[i, j] < 0)
                            przyciski[i, j].Enabled = false;

                        else
                            przyciski[i, j].Enabled = true;
                    }
                }
            }
        }

        public void ResetujPodswietleniePol()
        {
            foreach (Pozycja pozycja in pozycjePodswietlanychPol)
            {
                int i = pozycja.indexI;
                int j = pozycja.indexJ;

                if ((i + j) % 2 != 0)
                {
                    przyciski[i, j].BackColor = aktualnyStyl.kolorPrzycisku1;
                }
                else
                {
                    przyciski[i, j].BackColor = aktualnyStyl.kolorPrzycisku2;
                }
            }

            pozycjePodswietlanychPol.Clear();
        }

        public void PodswietleniePola(int indexI, int indexJ)
        {
            przyciski[indexI, indexJ].BackColor = aktualnyStyl.kolorPodswietlenia;
            pozycjePodswietlanychPol.Add(new Pozycja(indexI, indexJ));
        }

        public void PodstwietleniePolDoZbicia(int indexI, int indexJ)
        {
            if (pozycjePionkowZmuszonychDoZbicia.Count > 0)
            {
                if (ustawieniePionkow[indexI, indexJ] != 0)
                {
                    // jeżeli czarny lub damka
                    if (przesuwanyPionek.znak == 1 || Math.Abs(przesuwanyPionek.znak) == 2)
                    {
                        // zbijanie - dolna granica
                        if (indexI <= 5)
                        {
                            // zbijanie - lewa granica
                            if (indexJ >= 2)
                            {
                                // czy inny znak
                                if ((ustawieniePionkow[indexI + 1, indexJ - 1] * ustawieniePionkow[indexI, indexJ] < 0) && ustawieniePionkow[indexI + 2, indexJ - 2] == 0)
                                    PodswietleniePola(indexI + 2, indexJ - 2);
                            }

                            // zbijanie - prawa granica
                            if (indexJ <= 5)
                            {
                                // czy inny znak
                                if ((ustawieniePionkow[indexI + 1, indexJ + 1] * ustawieniePionkow[indexI, indexJ] < 0) && ustawieniePionkow[indexI + 2, indexJ + 2] == 0)
                                    PodswietleniePola(indexI + 2, indexJ + 2);
                            }
                        }
                    }

                    // jeżeli biały lub damka
                    if (przesuwanyPionek.znak == -1 || Math.Abs(przesuwanyPionek.znak) == 2)
                    {
                        // zbijanie - górna granica
                        if (indexI >= 2)
                        {
                            // zbijanie - lewa granica
                            if (indexJ >= 2)
                            {
                                // czy inny znak
                                if ((ustawieniePionkow[indexI - 1, indexJ - 1] * ustawieniePionkow[indexI, indexJ] < 0) && ustawieniePionkow[indexI - 2, indexJ - 2] == 0)
                                    PodswietleniePola(indexI - 2, indexJ - 2);
                            }

                            // zbijanie - prawa granica
                            if (indexJ <= 5)
                            {
                                // czy inny znak
                                if ((ustawieniePionkow[indexI - 1, indexJ + 1] * ustawieniePionkow[indexI, indexJ] < 0) && ustawieniePionkow[indexI - 2, indexJ + 2] == 0)
                                    PodswietleniePola(indexI - 2, indexJ + 2);
                            }
                        }
                    }
                }
            }
        }

        public void BicieDoPrzodu(int i, int j)
        {
            // zbijanie - górna granica
            if (i >= 2)
            {
                // zbijanie - lewa granica
                if (j >= 2)
                {
                    // czy inny znak
                    if ((ustawieniePionkow[i - 1, j - 1] * ustawieniePionkow[i, j] < 0) && ustawieniePionkow[i - 2, j - 2] == 0)
                    {
                        pozycjePionkowZmuszonychDoZbicia.Add(new Pozycja(i, j));
                    }
                }

                // zbijanie - prawa granica
                if (j <= 5)
                {
                    // czy inny znak
                    if ((ustawieniePionkow[i - 1, j + 1] * ustawieniePionkow[i, j] < 0) && ustawieniePionkow[i - 2, j + 2] == 0)
                    {
                        pozycjePionkowZmuszonychDoZbicia.Add(new Pozycja(i, j));
                    }
                }
            }
        }

        public void BicieDoTylu(int i, int j)
        {
            // zbijanie - dolna granica
            if (i <= 5)
            {
                // zbijanie - lewa granica
                if (j >= 2)
                {
                    // czy inny znak
                    if ((ustawieniePionkow[i + 1, j - 1] * ustawieniePionkow[i, j] < 0) && ustawieniePionkow[i + 2, j - 2] == 0)
                        pozycjePionkowZmuszonychDoZbicia.Add(new Pozycja(i, j));
                }

                // zbijanie - prawa granica
                if (j <= 5)
                {
                    // czy inny znak
                    if ((ustawieniePionkow[i + 1, j + 1] * ustawieniePionkow[i, j] < 0) && ustawieniePionkow[i + 2, j + 2] == 0)
                        pozycjePionkowZmuszonychDoZbicia.Add(new Pozycja(i, j));
                }
            }
        }

        public void WieleRuchow()
        {
            WyswietlKomunikat("Musisz zrobić jeszcze jeden ruch!");
            kolejBialych = !kolejBialych;
            przesuwanyPionek.znak *= -1;
        }

        public void ZbiciePionka(int aktualnyI, int aktualnyJ)
        {
            // warunek zbicia - skok o 2 pola na ukos
            if (Math.Abs(przesuwanyPionek.indexI - aktualnyI) == 2 && Math.Abs(przesuwanyPionek.indexJ - aktualnyJ) == 2)
            {
                pozycjePionkowZmuszonychDoZbicia.Clear();

                // pozycja zbitego pionka
                int i = (przesuwanyPionek.indexI + aktualnyI) / 2;
                int j = (przesuwanyPionek.indexJ + aktualnyJ) / 2;

                ustawieniePionkow[i, j] = 0;
                przyciski[i, j].BackgroundImage = null;

                // jeśli pionek czarny zbija pionka białego
                if (przesuwanyPionek.znak > 0)
                {
                    int czarnePkt = int.Parse(labelCzarnePkt.Text);
                    czarnePkt++;

                    if (czarnePkt == 12)
                    {
                        KoniecGry("czarne");
                    }

                    labelCzarnePkt.Text = Convert.ToString(czarnePkt);

                    BicieDoTylu(aktualnyI, aktualnyJ);
                    if (przesuwanyPionek.znak == 2)
                    {
                        BicieDoPrzodu(aktualnyI, aktualnyJ);
                    }

                    // jeżeli można bić wiele razy z rzędu
                    if (pozycjePionkowZmuszonychDoZbicia.Count > 0)
                    {
                        WieleRuchow();
                        return;
                    }
                }

                // jeśli pionek biały zbija pionka czarnego
                if (przesuwanyPionek.znak < 0)
                {
                    int bialePkt = int.Parse(labelBialePkt.Text);
                    bialePkt++;

                    if (bialePkt == 12)
                    {
                        KoniecGry("białe");
                    }

                    labelBialePkt.Text = Convert.ToString(bialePkt);

                    BicieDoPrzodu(aktualnyI, aktualnyJ);
                    if (przesuwanyPionek.znak == -2)
                    {
                        BicieDoTylu(aktualnyI, aktualnyJ);
                    }

                    // jeżeli można bić wiele razy z rzędu
                    if (pozycjePionkowZmuszonychDoZbicia.Count > 0)
                    {
                        WieleRuchow();
                        return;
                    }
                }
            }
        }

        public async void WyswietlKomunikat(string komunikat)
        {
            labelUwaga.Text = komunikat;
            await Task.Delay(5000);
            labelUwaga.Text = "";
        }

        public void KoniecGry(string wygrany)
        {
            labelWygrana.Text = wygrany == "białe" ? "WYGRAŁY BIAŁE PIONKI" : "WYGRAŁY CZARNE PIONKI";
            labelWygrana.Visible = true;

            buttonZapisz.Enabled = false;

            string nazwaPliku = "ukladPlanszy.txt";
            if (File.Exists(nazwaPliku))
                File.Delete(nazwaPliku);
        }

        public void ZmienStyl()
        {
            StylKolorow[] style =
            {
                new StylKolorow(Color.LightGray, Color.White, SystemColors.Control, Color.IndianRed),
                new StylKolorow(Color.FromArgb(181, 124, 98), Color.FromArgb(214, 201, 129), Color.FromArgb(230, 201, 135), Color.Red)
            };

            if (aktualnyStylIndex >= style.Length)
                aktualnyStylIndex = 0;

            aktualnyStyl = style[aktualnyStylIndex];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 != 0)
                    {
                        przyciski[i, j].BackColor = aktualnyStyl.kolorPrzycisku1;
                    }
                    else
                    {
                        przyciski[i, j].BackColor = aktualnyStyl.kolorPrzycisku2;
                    }
                }
            }

            this.BackColor = style[aktualnyStylIndex].kolorTla;

            buttonStyl.Text = $"Zmień styl {aktualnyStylIndex + 1}/{style.Length}";
            aktualnyStylIndex++;
        }

        private void buttonStyl_Click(object sender, EventArgs e)
        {
            ZmienStyl();
        }

        private void buttonNowaGra_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((i + j) % 2 != 0)
                    {
                        przyciski[i, j].BackgroundImageLayout = ImageLayout.Stretch;

                        if (i <= 2)
                        {
                            ustawieniePionkow[i, j] = 1;
                            przyciski[i, j].BackgroundImage = obrazekCzarnegoPionka;
                        }
                        else if (i >= 5)
                        {
                            ustawieniePionkow[i, j] = -1;
                            przyciski[i, j].BackgroundImage = obrazekBialegoPionka;
                        }
                        else
                        {
                            ustawieniePionkow[i, j] = 0;
                            przyciski[i, j].BackgroundImage = null;
                        }
                    }
                }
            }

            kolejBialych = true;
            labelBialePkt.Text = "0";
            labelCzarnePkt.Text = "0";
            labelCzyjaKolej.Text = "BIAŁE PIONKI";
            ZablokujPola();

            buttonNowaGra.Enabled = false;
            buttonWczytaj.Enabled = false;
            buttonZapisz.Enabled = true;
        }

        private void buttonZapisz_Click(object sender, EventArgs e)
        {
            StreamWriter zapisDanych = new StreamWriter("ukladPlanszy.txt");

            // zapis czyja kolej i punkty
            zapisDanych.WriteLine("[Ustawienie pionkow]");
            zapisDanych.WriteLine(kolejBialych);
            zapisDanych.WriteLine(int.Parse(labelBialePkt.Text));
            zapisDanych.WriteLine(int.Parse(labelCzarnePkt.Text));

            // zapis planszy
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    zapisDanych.Write(ustawieniePionkow[i, j] + " ");
                }
                zapisDanych.WriteLine();
            }

            // zapisz pozycji pionków zmuszonych do bicia oraz ich ilości
            zapisDanych.WriteLine(pozycjePionkowZmuszonychDoZbicia.Count);

            foreach (Pozycja pozycja in pozycjePionkowZmuszonychDoZbicia)
            {
                zapisDanych.WriteLine(pozycja.indexI + " " + pozycja.indexJ);
            }

            zapisDanych.Close();
        }

        private void buttonWczytaj_Click(object sender, EventArgs e)
        {
            string nazwaPliku = "ukladPlanszy.txt";

            if (File.Exists(nazwaPliku))
            {
                StreamReader odczytDanych = new StreamReader(nazwaPliku);

                if (odczytDanych.ReadLine() == "[Ustawienie pionkow]")
                {
                    // odczyt czyja kolej i punktów
                    kolejBialych = bool.Parse(odczytDanych.ReadLine());
                    labelCzyjaKolej.Text = kolejBialych ? "BIAŁE PIONKI" : "CZARNE PIONKI";
                    labelBialePkt.Text = odczytDanych.ReadLine();
                    labelCzarnePkt.Text = odczytDanych.ReadLine();

                    // odczyt planszy
                    for (int i = 0; i < 8; i++)
                    {
                        int j = 0;
                        string linia = odczytDanych.ReadLine();

                        foreach (var liczba in linia.Trim().Split(' '))
                        {
                            int znakPionka = int.Parse(liczba);
                            ustawieniePionkow[i, j] = znakPionka;
                            przyciski[i, j].BackgroundImageLayout = ImageLayout.Stretch;

                            switch (znakPionka)
                            {
                                case -2:
                                    przyciski[i, j].BackgroundImage = obrazekBialejDamki;
                                    break;

                                case -1:
                                    przyciski[i, j].BackgroundImage = obrazekBialegoPionka;
                                    break;

                                case 1:
                                    przyciski[i, j].BackgroundImage = obrazekCzarnegoPionka;
                                    break;

                                case 2:
                                    przyciski[i, j].BackgroundImage = obrazekCzarnejDamki;
                                    break;
                            }

                            j++;
                        }
                    }

                    // odczyt pozycji pionków zmuszonych do bicia oraz ich liczby
                    int iloscPozycji = int.Parse(odczytDanych.ReadLine());
                    for (int i = 0; i < iloscPozycji; i++)
                    {
                        var linia = odczytDanych.ReadLine().Trim().Split(' ');
                        int indexI = int.Parse(linia[0]);
                        int indexJ = int.Parse(linia[1]);

                        pozycjePionkowZmuszonychDoZbicia.Add(new Pozycja(indexI, indexJ));
                    }

                    ZablokujPola();
                    buttonWczytaj.Enabled = false;
                    buttonZapisz.Enabled = true;
                }
                else
                    WyswietlKomunikat("Brak zapisanej gry lub błędna nazwa pliku!");

                odczytDanych.Close();
            }
            else
                WyswietlKomunikat("Brak zapisanej gry lub błędna nazwa pliku!");
        }

        // ----- 1. rząd -----

        private void button2_Click(object sender, EventArgs e)
        {
            Ruch(0, 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Ruch(0, 3);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Ruch(0, 5);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Ruch(0, 7);
        }

        // ----- 2. rząd -----

        private void button9_Click(object sender, EventArgs e)
        {
            Ruch(1, 0);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Ruch(1, 2);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Ruch(1, 4);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Ruch(1, 6);
        }

        // ----- 3. rząd -----

        private void button18_Click(object sender, EventArgs e)
        {
            Ruch(2, 1);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Ruch(2, 3);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            Ruch(2, 5);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            Ruch(2, 7);
        }

        // ----- 4. rząd -----

        private void button25_Click(object sender, EventArgs e)
        {
            Ruch(3, 0);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            Ruch(3, 2);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            Ruch(3, 4);
        }

        private void button31_Click(object sender, EventArgs e)
        {
            Ruch(3, 6);
        }

        // ----- 5. rząd -----

        private void button34_Click(object sender, EventArgs e)
        {
            Ruch(4, 1);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            Ruch(4, 3);
        }

        private void button38_Click(object sender, EventArgs e)
        {
            Ruch(4, 5);
        }

        private void button40_Click(object sender, EventArgs e)
        {
            Ruch(4, 7);
        }

        // ----- 6. rząd -----

        private void button41_Click(object sender, EventArgs e)
        {
            Ruch(5, 0);
        }

        private void button43_Click(object sender, EventArgs e)
        {
            Ruch(5, 2);
        }

        private void button45_Click(object sender, EventArgs e)
        {
            Ruch(5, 4);
        }

        private void button47_Click(object sender, EventArgs e)
        {
            Ruch(5, 6);
        }

        // ----- 7. rząd -----

        private void button50_Click(object sender, EventArgs e)
        {
            Ruch(6, 1);
        }

        private void button52_Click(object sender, EventArgs e)
        {
            Ruch(6, 3);
        }

        private void button54_Click(object sender, EventArgs e)
        {
            Ruch(6, 5);
        }

        private void button56_Click(object sender, EventArgs e)
        {
            Ruch(6, 7);
        }

        // ----- 8. rząd -----

        private void button57_Click(object sender, EventArgs e)
        {
            Ruch(7, 0);
        }

        private void button59_Click(object sender, EventArgs e)
        {
            Ruch(7, 2);
        }

        private void button61_Click(object sender, EventArgs e)
        {
            Ruch(7, 4);
        }

        private void button63_Click(object sender, EventArgs e)
        {
            Ruch(7, 6);
        }
    }
}
