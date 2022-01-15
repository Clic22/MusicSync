using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            songsList_ = new SongsStorage();
            versioning_ = new SongVersioning(songsList_);
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            AddNewSong dialog = new AddNewSong(this, songsList_);
            await dialog.ShowAsync();
        }

        private async void deleteSongClick(object sender, RoutedEventArgs e)
        {
            songsList_.deleteSong(songSelected_);
        }

        public void SongsListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            songSelected_ = (sender as ListBox).SelectedItem as Song;
        }

        private async void updateLocalSongClick(object sender, RoutedEventArgs e)
        {
            versioning_.updateLocalSong(songSelected_);
            PopUp popUp = new PopUp(this, $"Song '{songSelected_.title}' Updated");
            await popUp.ShowAsync();
        }

        private async void updateLocalSongsClick(object sender, RoutedEventArgs e)
        {
            versioning_.updateLocalSongs();
            PopUp popUp = new PopUp(this, "All Songs Updated");
            await popUp.ShowAsync();
        }

        private async void pushNewSongVersionClick(object sender, RoutedEventArgs e)
        {
            versioning_.pushNewSongVersion(songSelected_, commitMessageTitle.Text, commitMessageDescription.Text);
            clearTitleAndDescritpion();
            PopUp popUp = new PopUp(this, $"New Version for '{songSelected_.title}' Pushed");
            await popUp.ShowAsync();
        }

        private void clearTitleAndDescritpion()
        {
            commitMessageTitle.Text = string.Empty;
            commitMessageDescription.Text = string.Empty;
        }

        private SongVersioning versioning_;
        public SongsStorage songsList_ { get; set; }
        private Song songSelected_;
    }
}
