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

namespace AT2_AssessmentTracker_30093503
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string textFile = "default.txt";
        private List<string[]> assessmentList = new List<string[]>();
        public MainWindow()
        {
            InitializeComponent();
            dueDate.SelectedDate = DateTime.Today;
            ReadFromFile();
            DisplayAssessments();
        }

        private void DisplayAssessments()
        {
            lvwAssessments.Items.Clear();

            foreach (var assessment in assessmentList)
            {
                var displayItem = new
                {
                    Data = assessment,
                    Date = assessment[0],
                    Name = assessment[1],
                    Unit = assessment[2],
                    UnitType = assessment[3]
                };
                lvwAssessments.Items.Add(displayItem);
            }
        }

        private void WriteToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(textFile))
                {
                    foreach (var assessment in assessmentList)
                    {
                        writer.WriteLine($"{assessment[0]}|{assessment[1]}|{assessment[2]}|{assessment[3]}");
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ReadFromFile()
        {
            try
            {
                using (StreamReader reader = new StreamReader(textFile))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length == 4)
                        {
                            assessmentList.Add(new string[]
                            {
                                parts[0],
                                parts[1],
                                parts[2],
                                parts[3]
                            });
                        }
                        
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void BtnAddAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (dueDate.SelectedDate == null
                || string.IsNullOrWhiteSpace(assName.Text)
                || string.IsNullOrWhiteSpace(unitName.Text)
                || string.IsNullOrWhiteSpace(assType.Text))
            {
                MessageBox.Show("Please fill in Date, Name, Unit, and Type", "Input Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string[] row = new string[]
            {
                dueDate.SelectedDate.Value.ToString("dd-MM-yyyy"),
                assName.Text.Trim(),
                unitName.Text.Trim(),
                assType.Text.Trim()
            };

            assessmentList.Add(row);

            WriteToFile();
            DisplayAssessments();

            dueDate.SelectedDate = DateTime.Today;
            assName.Clear();
            unitName.Clear();
            assType.Clear();
            dueDate.Focus();

        }

        private void BtnDeleteAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string[] selectedAssessment)
            {
                assessmentList.Remove(selectedAssessment);
                WriteToFile();
                DisplayAssessments();
            }
        }

        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuShowComplete_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSortName_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSortDate_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSortUnit_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void MenuSortType_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void BtnMarkAssessment_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}