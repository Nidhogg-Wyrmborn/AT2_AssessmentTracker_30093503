using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AT2_AssessmentTracker_30093503
{
    /// <summary>
    /// Interaction logic for editDialog.xaml
    /// </summary>
    partial class editDialog : Window
    {
        public editDialog()
        {
            InitializeComponent();
            score = "NYM";
        }

        private string score;

        public DateTime? dDate
        {
            get { return dueDate.SelectedDate; }
            set { dueDate.SelectedDate = value; }
        }

        public string aName
        {
            get { return assName.Text; }
            set { assName.Text = value; }
        }

        public string uName
        {
            get { return unitName.Text; }
            set { unitName.Text = value; }
        }

        public string aType
        {
            get { return assType.Text; }
            set { assType.Text = value; }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        public void SetScore(int i)
        {
            this.score = i == 0 ? "NYM" : i == 1 ? "NYS" : "S";
            ScoreText.Text = this.score;
        }

        public string GetScore()
        {
            return this.score;
        }

        private void SetScoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as FrameworkElement;
            if (button != null)
            {
                button.ContextMenu.IsOpen = true;
            }
        }

        private void ChangeScore(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            if (button != null)
            {
                if (button.Tag != null)
                {
                    this.score = button.Tag.ToString();
                    ScoreText.Text = this.score;
                }
            }
        }
    }
}
