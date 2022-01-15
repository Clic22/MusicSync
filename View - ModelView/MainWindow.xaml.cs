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
        public MainWindow(SongsManager songsManager)
        {
            this.InitializeComponent();
            songsManager_ = songsManager;
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            AddNewSongPopUp dialog = new AddNewSongPopUp(this, songsManager_);
            await dialog.ShowAsync();
        }

        private void deleteSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            songsManager_.deleteSong(song);
        }

        private async void updateSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            songsManager_.updateSong(song);
            PopUp popUp = new PopUp(this, $"Song '{song.title}' Updated");
            await popUp.ShowAsync();
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            songsManager_.updateAllSongs();
            PopUp popUp = new PopUp(this, "All Songs Updated");
            await popUp.ShowAsync();
        }

        private async void uploadNewVersionClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            UploadNewVersionPopUp dialog = new UploadNewVersionPopUp(this, songsManager_, song);
            await dialog.ShowAsync();
        }

        private SongsManager songsManager_;
    }
}
