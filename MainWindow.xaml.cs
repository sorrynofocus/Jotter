using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jotter
{
    public partial class MainWindow : Window
    {
        public class Note
        {
            public string? Title { get; set; }
            public string? Text { get; set; }
        }

        public ObservableCollection<Note> Notes { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Notes = new ObservableCollection<Note>();
            DataContext = this;
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            Notes.Add(new Note { Title = "New Note", Text = "Enter your text here." });
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null)
                Notes.Remove((Note)MyNotesListView.SelectedItem);
        }

        private void DeleteNoteFromContextMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MyNotesListView.SelectedItem != null)
                Notes.Remove((Note)MyNotesListView.SelectedItem);
        }

        private void Note_DoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext is Note note)
                MessageBox.Show($"Double-clicked on: {note.Title}\n\nText: {note.Text}");
        }

        private void OpenNote_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for opening a note
        }

        private void ExitApp_Click(object sender, RoutedEventArgs e)
        {
            // Implement logic for saving settings/notes
            Environment.Exit(0);
        }

        //Coee behind for dragging window since windowstyle = none
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }



    }
}
