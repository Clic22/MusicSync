using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SongsPage : Page
    {
        public SongsPage()
        {
            this.InitializeComponent();
            songsManager_ = new SongsManager();
            songsManager_.updateAllSongs();
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            songsManager_.updateAllSongs();
            await displayContentDialog("All Songs Updated");
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await addNewSongContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                songsManager_.addSong(songTitle.Text, songFile.Text, songLocalPath.Text);
            }
            songTitle.Text = String.Empty;
            songFile.Text = String.Empty;
            songLocalPath.Text = String.Empty;
        }

        private async void folders_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                var files = await folderPicked.GetFilesAsync();
                foreach (var file in files)
                {
                    if (file.Name.Contains(".song"))
                    {
                        songTitle.Text = folderPicked.Name;
                        songFile.Text = file.Name;
                        songLocalPath.Text = folderPicked.Path;
                    }
                }
            }
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
            await displayContentDialog($"Song '{song.title}' Updated");
        }

        private async void uploadNewVersionClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await uploadNewSongVersionContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Song song = (sender as Button).DataContext as Song;
                songsManager_.uploadNewSongVersion(song, title.Text, description.Text);
                await displayContentDialog($"New Version of '{song.title}' Uploaded");
            }
            title.Text = String.Empty;
            description.Text = String.Empty;

        }

        private async void openSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            bool opened = songsManager_.openSong(song);
            if (!opened)
            {
                await displayContentDialog($"Song Locked");
            }
        }

        private void revertSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            songsManager_.revertSong(song);
        }

        private async Task displayContentDialog(string text)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = text;
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private SongsManager songsManager_;
    }
}
