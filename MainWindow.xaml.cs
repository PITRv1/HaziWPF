using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DogaTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string Fejlec = "Rajtszam;Nev;Versenyszam;Nem;Eletkor;OR;VR;ER";

        public ObservableCollection<Versenyzo> Versenyzok { get; } = new();
        public ObservableCollection<Versenyzo> LathatoVersenyzok { get; } = new();
        private bool feluletKesz = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            feluletKesz = true;
            FrissitSzures();
        }

        private void Eletkor_Valtozott(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (txtEletkor != null)
            {
                txtEletkor.Text = (int)e.NewValue + " év";
            }
        }

        private bool Foglalt(int rajtszam)
        {
            foreach (Versenyzo v in Versenyzok)
            {
                if (v.Rajtszam == rajtszam)
                {
                    return true;
                }
            }
            return false;
        }

        private int ElsoSzabad()
        {
            bool[] foglaltak = new bool[10000];
            foreach (Versenyzo v in Versenyzok)
            {
                foglaltak[v.Rajtszam] = true;
            }

            for (int i = 1; i <= 9999; i++)
            {
                if (!foglaltak[i])
                {
                    return i;
                }
            }
            return 0;
        }

        private void FrissitSzures()
        {
            LathatoVersenyzok.Clear();
            foreach (Versenyzo v in Versenyzok)
            {
                bool bekerul = false;
                if (cmbRekordSzuro.SelectedIndex == 0)
                {
                    bekerul = true;
                }
                else if (cmbRekordSzuro.SelectedIndex == 1)
                {
                    bekerul = v.OR;
                }
                else if (cmbRekordSzuro.SelectedIndex == 2)
                {
                    bekerul = v.VR;
                }
                else if (cmbRekordSzuro.SelectedIndex == 3)
                {
                    bekerul = v.ER;
                }

                if (bekerul)
                {
                    LathatoVersenyzok.Add(v);
                }
            }

            txtLetszam.Text = "Látható: " + LathatoVersenyzok.Count + " / Összesen: " + Versenyzok.Count;
        }

        private void Szuro_Valtozott(object sender, SelectionChangedEventArgs e)
        {
            if (!feluletKesz)
            {
                return;
            }
            FrissitSzures();
        }

        private static string Bekezdes(Versenyzo v)
        {
            string nem = v.Nem == "F" ? "férfi" : "női";
            string orSzoveg = v.OR ? "rendelkezik" : "nem rendelkezik";
            string vrSzoveg = v.VR ? "rendelkezik" : "nem rendelkezik";
            string erSzoveg = v.ER ? "rendelkezik" : "nem rendelkezik";

            return "A " + v.Eletkor + " éves " + nem + " versenyző a(z) " +
                   v.Versenyszam + " versenyszámban indul, rajtszáma " + v.Rajtszam + ". " +
                   "Olimpiai csúccsal " + orSzoveg + ", világcsúccsal " + vrSzoveg + ", " +
                   "Európa-csúccsal " + erSzoveg + ".";
        }

        private void Regisztral_Click(object sender, RoutedEventArgs e)
        {
            string nev = txtNev.Text.Trim();
            if (nev == "")
            {
                MessageBox.Show("A versenyző nevének megadása kötelező.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (nev.Contains(';') || nev.Contains('|'))
            {
                MessageBox.Show("A névben pontosvessző és függőleges vonal nem használható.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbVersenyszam.SelectedItem == null)
            {
                MessageBox.Show("Válasszon versenyszámot.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (rbFerfi.IsChecked != true && rbNo.IsChecked != true)
            {
                MessageBox.Show("Adja meg a versenyző nemét.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string rajtszamSzoveg = txtRajtszam.Text.Trim();
            int rajtszam = 0;

            if (rajtszamSzoveg != "")
            {
                if (!int.TryParse(rajtszamSzoveg, NumberStyles.None, CultureInfo.InvariantCulture, out rajtszam) || rajtszam < 1 || rajtszam > 9999)
                {
                    MessageBox.Show("A rajtszám 1 és 9999 közötti egész szám lehet.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (Foglalt(rajtszam))
                {
                    MessageBox.Show("A kért rajtszám már foglalt. Adjon meg másikat, vagy hagyja üresen a mezőt.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                rajtszam = ElsoSzabad();
                if (rajtszam == 0)
                {
                    MessageBox.Show("Nincs több szabad rajtszám.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            ComboBoxItem kijelolt = (ComboBoxItem)cmbVersenyszam.SelectedItem;
            string versenyszam = (string)kijelolt.Content;

            Versenyzo uj = new Versenyzo();
            uj.Rajtszam = rajtszam;
            uj.Nev = nev;
            uj.Versenyszam = versenyszam;
            uj.Nem = rbFerfi.IsChecked == true ? "F" : "N";
            uj.Eletkor = (int)sldEletkor.Value;
            uj.OR = chkOlimpiai.IsChecked == true;
            uj.VR = chkVB.IsChecked == true;
            uj.ER = chkEB.IsChecked == true;

            Versenyzok.Add(uj);
            FrissitSzures();

            MessageBox.Show("A regisztráció sikeres. Kiosztott rajtszám: " + rajtszam, "Információ", MessageBoxButton.OK, MessageBoxImage.Information);

            txtNev.Clear();
            txtRajtszam.Clear();
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "CSV-fájl (*.csv)|*.csv|Szövegfájl (*.txt)|*.txt";
            dialog.FilterIndex = 1;
            dialog.FileName = "atletika";
            dialog.AddExtension = false;
            dialog.OverwritePrompt = false;

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            string elvart = dialog.FilterIndex == 1 ? ".csv" : ".txt";
            string kapott = System.IO.Path.GetExtension(dialog.FileName);
            if (kapott != "" && kapott.ToLower() != elvart)
            {
                MessageBox.Show("A fájltípus és a kiterjesztés nem egyezik.", "Figyelmeztetés", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string fajl = kapott == "" ? dialog.FileName + elvart : dialog.FileName;

            if (File.Exists(fajl))
            {
                MessageBoxResult valasz = MessageBox.Show("A fájl már létezik. Felülírja?", "Megerősítés", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (valasz != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            try
            {
                if (dialog.FilterIndex == 1)
                {
                    List<string> sorok = new List<string>();
                    sorok.Add(Fejlec);

                    foreach (Versenyzo v in Versenyzok)
                    {
                        string sor = v.Rajtszam + ";" + v.Nev + ";" + v.Versenyszam + ";" + v.Nem + ";" + v.Eletkor + ";" +
                                     (v.OR ? "1" : "0") + ";" + (v.VR ? "1" : "0") + ";" + (v.ER ? "1" : "0");
                        sorok.Add(sor);
                    }

                    File.WriteAllLines(fajl, sorok, new UTF8Encoding(false));
                }
                else
                {
                    string nl = Environment.NewLine;
                    List<string> blokkok = new List<string>();

                    foreach (Versenyzo v in Versenyzok)
                    {
                        blokkok.Add(v.Nev + nl + Bekezdes(v));
                    }

                    File.WriteAllText(fajl, string.Join(nl + nl, blokkok), new UTF8Encoding(false));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a mentés közben: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Betoltes_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "CSV-fájl (*.csv)|*.csv";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string[] sorok = File.ReadAllLines(dialog.FileName, Encoding.UTF8);
                List<Versenyzo> ujLista = new List<Versenyzo>();
                HashSet<int> rajtszamok = new HashSet<int>();

                for (int i = 1; i < sorok.Length; i++)
                {
                    if (sorok[i].Trim() == "")
                    {
                        continue;
                    }

                    string[] mezok = sorok[i].Split(';');
                    for (int j = 0; j < mezok.Length; j++)
                    {
                        mezok[j] = mezok[j].Trim();
                    }

                    Versenyzo v = new Versenyzo();
                    v.Rajtszam = int.Parse(mezok[0]);
                    v.Nev = mezok[1];
                    v.Versenyszam = mezok[2];
                    v.Nem = mezok[3];
                    v.Eletkor = int.Parse(mezok[4]);
                    v.OR = mezok[5] == "1";
                    v.VR = mezok[6] == "1";
                    v.ER = mezok[7] == "1";

                    if (!rajtszamok.Add(v.Rajtszam))
                    {
                        MessageBox.Show("A fájlban ismétlődő rajtszám található: " + v.Rajtszam, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ujLista.Add(v);
                }

                Versenyzok.Clear();
                foreach (Versenyzo uj in ujLista)
                {
                    Versenyzok.Add(uj);
                }
                FrissitSzures();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltés közben: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
