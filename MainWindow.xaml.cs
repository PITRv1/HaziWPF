using Microsoft.Win32;
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
        private const string Fejlec = "Rajtszam;Nev;Versenyszam;Nem;Eletkor;OlimpiaiCsucs;VBCsucs;EBCsucs";

        public MainWindow()
        {
            InitializeComponent();
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
            foreach (object elem in lstVersenyzok.Items)
            {
                string[] mezok = ((string)elem).Split('|');
                if (int.Parse(mezok[0].Trim()) == rajtszam)
                {
                    return true;
                }

            }
            return false;
        }

        private int ElsoSzabad()
        {

            bool[] foglaltak = new bool[10000];
            foreach (object elem in lstVersenyzok.Items)
            {
                string[] mezok = ((string)elem).Split('|');
                foglaltak[int.Parse(mezok[0].Trim())] = true;
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

        private void FrissitLetszam()
        {
        
            txtLetszam.Text = "Regisztrált versenyzők: " + lstVersenyzok.Items.Count;
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
            string nem = rbFerfi.IsChecked == true ? "F" : "N";
            int eletkor = (int)sldEletkor.Value;
            int olimpiai = chkOlimpiai.IsChecked == true ? 1 : 0;
            int vb = chkVB.IsChecked == true ? 1 : 0;
            int eb = chkEB.IsChecked == true ? 1 : 0;

            string sor = rajtszam + " | " + nev + " | " + versenyszam + " | " + nem + " | " + eletkor + " | " + olimpiai + " | " + vb + " | " + eb;
            lstVersenyzok.Items.Add(sor);
            FrissitLetszam();

            MessageBox.Show("A regisztráció sikeres. Kiosztott rajtszám: " + rajtszam, "Információ", MessageBoxButton.OK, MessageBoxImage.Information);

            txtNev.Clear();
            txtRajtszam.Clear();
        }

        private void Mentes_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = "atletika.csv";
            dialog.Filter = "CSV-fájl (*.csv)|*.csv";

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                List<string> sorok = new List<string>();
                sorok.Add(Fejlec);

                foreach (object elem in lstVersenyzok.Items)
                {
                    string[] mezok = ((string)elem).Split('|');
                    for (int i = 0; i < mezok.Length; i++)
                    {
                        mezok[i] = mezok[i].Trim();
                    }
                    sorok.Add(string.Join(";", mezok));
                }

                File.WriteAllLines(dialog.FileName, sorok, new UTF8Encoding(false));
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
                List<string> ujLista = new List<string>();
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

                    int rajtszam = int.Parse(mezok[0]);
                    if (!rajtszamok.Add(rajtszam))
                    {
                        MessageBox.Show("A fájlban ismétlődő rajtszám található: " + rajtszam, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    ujLista.Add(string.Join(" | ", mezok));
                }

                lstVersenyzok.Items.Clear();
                foreach (string sor in ujLista)
                {
                    lstVersenyzok.Items.Add(sor);
                }
                FrissitLetszam();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba a betöltés közben: " + ex.Message, "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
