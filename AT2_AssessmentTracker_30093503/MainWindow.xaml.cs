using Microsoft.Win32;
using Notification.Core;
using Notification.Wpf;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
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
        private bool showComplete = true; // boolean to show completed assessments (completion sorting effectively)
        private bool showIncomplete = true; // boolean to show incomplete assessments (completion sorting effectively)
        private int sortMethod = 0; // 0 = dueDate, 1 = Name, 2 = Unit, 3 = Type, 4+ = invalid (will automatically reset to 0)
        private INotificationManager notifier = new NotificationManager();

        /// <summary>
        /// Initializes a new instance of the MainWindow class, sets the due date to today, loads assessments from a
        /// file, and displays them.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            dueDate.SelectedDate = DateTime.Today; // set the dueDate in assessment Inputs to today's date
            MSA.IsChecked = true;
            MSD.IsChecked = true;
            if (File.Exists(textFile))
            {
                ReadFromFile();
            }
            else
            {
                WriteToFile();
            }

            DisplayAssessments(); // display assessments

            DisplayOverdue(); // display overdue assessments
        }

        private void DisplayOverdue()
        {
            foreach (string[] assessment in assessmentList)
            {
                string Title = assessment[4] == "NYM" ? "OverDue!" : assessment[4] == "NYS" ? "Check!" : "WTF";
                bool od = isOverDue(assessment[0]);
                if (Title != "Check!" && od)
                {
                    notifier.Show(NotificationBuilder
                        .Create(Title, $"{assessment[1]}: {assessment[0]}")
                        .AsWarning()
                        .NeverExpires()
                        .WithPriority(NotificationPriority.High)
                        .OnClick(() => Console.WriteLine("Clicked"))
                        .Build());
                }
                else if (od)
                {
                    notifier.Show(NotificationBuilder
                        .Create(Title, $"{assessment[1]}: {assessment[0]}")
                        .AsSuccess()
                        .NeverExpires()
                        .WithPriority(NotificationPriority.High)
                        .OnClick(() => Console.WriteLine("Clicked"))
                        .Build());
                }
            }
        }

        private bool isOverDue(string date)
        {
            if (!DateTime.TryParseExact(
                date, "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime dateOutput))
            {
                return false;
            }

            if (dateOutput < DateTime.Today)
            {
                return true;
            }
            return false;
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

            if (showIncomplete)
            {

                sortList();

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
            }
            if (showComplete)
            {
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
        }

        /// <summary>
        /// Sorts the lists based on sortMethod (int)
        /// will only sort the completed list if it requires displaying
        /// will only sort if there has been a change since last sort (later)
        /// (should automatically be easy, display should only be called if there has been a change)
        /// </summary>
        private void sortList()
        {
            // always sort assessmentList
            List<string[]> aList = assessmentList.OrderBy(arr => arr[sortMethod]).ToList();
            aList = aList.OrderByDescending(arr => arr[4]).ToList();
            assessmentList = aList;

            // only sort completed list if we are showing completed assessments
            if (showComplete)
            {
                List<string[]> cList = completedList.OrderBy(arr => arr[sortMethod]).ToList();
                completedList = cList;
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
        /// opens a dialog to select a file, then sets textFile to the selection (it will end up being a path)
        /// after selecting it will automatically load and display that file
        /// </summary>
        private void LoadFromFile()
        {
            if (File.Exists(textFile))
            {
                WriteToFile();
            } 
            else
            {
                var Result = MessageBox.Show("Warning, File Does not Exist: \nCreate new File?", "Create new?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (Result == MessageBoxResult.Yes)
                {
                    SaveFileAs();
                }
            }
                OpenFileDialog ofd = new();
            ofd.InitialDirectory = Directory.GetCurrentDirectory();
            ofd.Filter = "Text|*.txt|All|*.*";
            if (ofd.ShowDialog() == true)
            {
                assessmentList = new List<string[]>();
                completedList = new List<string[]>();
                textFile = ofd.FileName;
                ReadFromFile();
                DisplayAssessments();
                return;
            }
            MessageBox.Show("No File selected");
        }

        /// <summary>
        /// opens a dialog to select a file, sets the textFile to the selection (it will end up being a path)
        /// after selecting it will automatically save to that file.
        /// </summary>
        private void SaveFileAs()
        {
            SaveFileDialog sfd = new();
            sfd.InitialDirectory = Directory.GetCurrentDirectory();
            sfd.Filter = "Text|*.txt|All|*.*";
            if (sfd.ShowDialog() == true)
            {
                textFile = sfd.FileName;
                WriteToFile();
                return;
            }
            MessageBox.Show("No File Selected");
        }

        private void NewFileAs()
        {
            SaveFileDialog sfd = new();
            sfd.InitialDirectory = Directory.GetCurrentDirectory();
            sfd.Filter = "Text|*.txt|All|*.*";
            if (sfd.ShowDialog() == true)
            {
                textFile = sfd.FileName;
                completedList = new List<string[]>();
                assessmentList = new List<string[]>();
                WriteToFile();
                return;
            }
            MessageBox.Show("No File Selected");
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

            //WriteToFile();
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
                    //WriteToFile();
                    DisplayAssessments();
                }
                else
                {
                    completedList.Remove(selectedAssessment);
                    //WriteToFile();
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
                        //WriteToFile();
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
            //WriteToFile();
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for creating a new file from the menu.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            NewFileAs();
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for the Open File menu item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            LoadFromFile();
        }

        /// <summary>
        /// Handles the click event for the Save File menu item.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            WriteToFile();
            MessageBox.Show($"Saved assessments to \"{textFile}\"");
        }

        /// <summary>
        /// Handles the click event for the 'Save As File' menu option.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }

        /// <summary>
        /// Handles the Quit menu item click event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the routed event.</param>
        private void MenuQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the click event for displaying the 'show complete' menu, but performs no action yet.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuShowComplete_Click(object sender, RoutedEventArgs e)
        {
            showComplete = true;
            showIncomplete = false;
            MSA.IsChecked = false;
            MSI.IsChecked = false;
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by name.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data associated with the click event.</param>
        private void MenuSortName_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            if (button != null)
            {
                if (MSN.IsChecked)
                {
                    sortMethod = 1;
                    MST.IsChecked = false;
                    MSU.IsChecked = false;
                    MSD.IsChecked = false;
                }
                else
                {
                    MSN.IsChecked = true;
                }
            }
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by date.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortDate_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            if (button != null)
            {
                if (MSD.IsChecked)
                {
                    sortMethod = 0;
                    MSN.IsChecked = false;
                    MSU.IsChecked = false;
                    MST.IsChecked = false;
                }
                else
                {
                    MSD.IsChecked = true;
                }
            }
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by unit.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortUnit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            if (button != null)
            {
                if (MSU.IsChecked)
                {
                    sortMethod = 2;
                    MSN.IsChecked = false;
                    MST.IsChecked = false;
                    MSD.IsChecked = false;
                }
                else
                {
                    MSU.IsChecked = true;
                }
            }
            DisplayAssessments();
        }

        /// <summary>
        /// Handles the click event for the menu item that sorts assessments by type.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void MenuSortType_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            if (button != null)
            {
                if (MST.IsChecked)
                {
                    sortMethod = 3;
                    MSN.IsChecked = false;
                    MSU.IsChecked = false;
                    MSD.IsChecked = false;
                }
                else
                {
                    MST.IsChecked = true;
                }
            }
            DisplayAssessments();
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

        private void MenuShowAll_Click(object sender, RoutedEventArgs e)
        {
            showComplete = true;
            showIncomplete = true;
            MSC.IsChecked = false;
            MSI.IsChecked = false;
            DisplayAssessments();
        }

        private void MenuShowIncomplete_Click(object sender, RoutedEventArgs e)
        {
            showIncomplete = true;
            showComplete = false;
            MSC.IsChecked = false;
            MSA.IsChecked = false;
            DisplayAssessments();
        }
    }
}