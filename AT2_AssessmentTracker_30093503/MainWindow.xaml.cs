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

namespace AT2_AssessmentTracker_30093503
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        private string textFile = "default.txt";
        private List<string[]> assessmentList = new List<string[]>(); // list of In progress assessments
        private List<string[]> completedList = new List<string[]>(); // list of completed assessments

        /// <summary>
        /// Initializes a new instance of the MainWindow class, sets the due date to today, loads assessments from a
        /// file, and displays them.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            dueDate.SelectedDate = DateTime.Today; // set the dueDate in assessment Inputs to today's date
            ReadFromFile(); // read assessments from default file (this will change to open_file dialog later)
            DisplayAssessments(); // display assessments
        }

        /// <summary>
        /// Opens a dialog to edit the details of an assessment and returns the updated assessment data.
        /// </summary>
        /// <param name="selectedAssessment">An array containing the current assessment details to be edited.</param>
        /// <returns>An array with the revised assessment details if editing is successful; otherwise, null.</returns>
        private string[]? EditAssessments(string[] selectedAssessment)
        {
            var dialog = new editDialog();
            string dDate = selectedAssessment[0];
            if (!DateTime.TryParseExact(
                dDate, "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dateOutput))
            {
                return null;
            }
            dialog.dDate = dateOutput;
            dialog.aName = selectedAssessment[1];
            dialog.uName = selectedAssessment[2];
            dialog.aType = selectedAssessment[3];
            dialog.SetScore(selectedAssessment[4] == "NYM" ? 0 : selectedAssessment[4] == "NYS" ? 1 : 2);
            if (dialog.ShowDialog() == true)
            {
                string[] revisedAssessment = new string[]
                {
                    dialog.dDate.Value.ToString("dd-MM-yyyy"),
                    dialog.aName,
                    dialog.uName,
                    dialog.aType,
                    dialog.GetScore()
                };
                return revisedAssessment;
            }
            return null;
        }

        /// <summary>
        /// Populates the assessment list view with items from the assessment list.
        /// </summary>
        private void DisplayAssessments()
        {
            lvwAssessments.Items.Clear();

            foreach (var assessment in assessmentList)
            {
                var displayItem = new
                {
                    DataDelete = assessment,
                    DataEdit = assessment,
                    Date = assessment[0],
                    Name = assessment[1],
                    Unit = assessment[2],
                    UnitType = assessment[3],
                    Score = assessment[4]
                };
                lvwAssessments.Items.Add(displayItem);
            }

            foreach (var assessment in completedList)
            {
                var displayItem = new
                {
                    DataDelete = assessment,
                    DataEdit = assessment,
                    Date = assessment[0],
                    Name = assessment[1],
                    Unit = assessment[2],
                    UnitType = assessment[3],
                    Score = assessment[4]
                };
                lvwAssessments.Items.Add(displayItem);
            }
        }

        /// <summary>
        /// Writes the contents of the assessment list to a text file, formatting each assessment as a pipe-separated
        /// line.
        /// </summary>
        private void WriteToFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(textFile))
                {
                    foreach (var assessment in assessmentList)
                    {
                        writer.WriteLine($"{assessment[0]}|{assessment[1]}|{assessment[2]}|{assessment[3]}|{assessment[4]}");
                    }

                    foreach (var assessment in completedList)
                    {
                        writer.WriteLine($"{assessment[0]}|{assessment[1]}|{assessment[2]}|{assessment[3]}|{assessment[4]}");
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Reads data from the specified text file, parses each line into five parts, and adds them to the assessment
        /// list.
        /// </summary>
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
                        if (parts.Length == 5)
                        {
                            if (parts[4] != "S")
                            {
                                assessmentList.Add(new string[]
                                {
                                    parts[0],
                                    parts[1],
                                    parts[2],
                                    parts[3],
                                    parts[4]
                                });
                            }
                            else
                            {
                                completedList.Add(new string[]
                                {
                                    parts[0],
                                    parts[1],
                                    parts[2],
                                    parts[3],
                                    parts[4]
                                });
                            }
                        }
                        
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Handles the Add Assessment button click event by validating input fields, adding a new assessment to the
        /// list, updating the display, saving to file, and resetting input fields.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
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
                assType.Text.Trim(),
                "NYM"
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

        /// <summary>
        /// Handles the click event to delete the selected assessment from the list, update the data file, and refresh
        /// the displayed assessments.
        /// </summary>
        /// <param name="sender">The button that triggered the event, expected to have the selected assessment in its Tag property.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void BtnDeleteAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string[] selectedAssessment)
            {
                if (selectedAssessment[4] != "S")
                {
                    assessmentList.Remove(selectedAssessment);
                    WriteToFile();
                    DisplayAssessments();
                }
                else
                {
                    completedList.Remove(selectedAssessment);
                    WriteToFile();
                    DisplayAssessments();
                }
            }
        }

        /// <summary>
        /// Handles the Edit Assessment button click event by allowing the user to modify a selected assessment and
        /// updating the assessment list accordingly.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a Button with a Tag containing the selected assessment.</param>
        /// <param name="e">Event data associated with the button click.</param>
        private void BtnEditAssessment_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string[] selectedAssessment)
            {
                string[]? revisedAssessment = EditAssessments(selectedAssessment);
                if (revisedAssessment != null) 
                {
                    if (revisedAssessment == selectedAssessment)
                    {
                        WriteToFile();
                        DisplayAssessments();
                        return;
                    }
                    // MessageBox.Show("Test");
                    if (revisedAssessment[4] == "S")
                    {
                        if (selectedAssessment[4] != "S")
                        {
                            completedList.Add(revisedAssessment);
                            assessmentList.Remove(selectedAssessment);
                        }
                        else
                        {
                            int i = completedList.IndexOf(selectedAssessment);
                            completedList[i] = revisedAssessment;
                        }
                    }
                    else
                    {
                        if (selectedAssessment[4] == "S")
                        {
                            assessmentList.Add(revisedAssessment);
                            completedList.Remove(selectedAssessment);
                        }
                        else
                        {
                            int i = assessmentList.IndexOf(selectedAssessment);
                            assessmentList[i] = revisedAssessment;
                        }
                    }
                }
            }
            WriteToFile();
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for creating a new file from the menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the Open File menu item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the Save File menu item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the 'Save As File' menu option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the Quit menu item click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for displaying the 'show complete' menu, but performs no action yet.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuShowComplete_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuSortName_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by date.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortDate_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by unit.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortUnit_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by type.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortType_Click(object sender, RoutedEventArgs e)
        {
            return;
        }

        /// <summary>
        /// Handles the window closing event by writing data to a file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Provides data for the cancelable event.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WriteToFile(); // until save and load functionality (to custom files) is implemented, just this will suffice
        }
    }
}