using App1.Adapters;
using App1.Models;
using App1.Models.Ports;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    public sealed partial class SongsPage : Page
    {
        public SongsPage()
        {
            this.InitializeComponent();
            Saver = new LocalSettingsSaver();
            VersionTool = new GitSongVersioning();
            SongsManager = new SongsManager(VersionTool, Saver);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string errorMessage =  await SongsManager.updateAllSongsAsync();
            if (errorMessage != string.Empty)
            {
                await displayContentDialog(errorMessage);
            }
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            string errorMessage = await SongsManager.updateAllSongsAsync();
            if (errorMessage != string.Empty)
            {
                await displayContentDialog(errorMessage);
            }
            else
            {
                await displayContentDialog("All Songs Updated");
            }
            
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await addNewSongContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                SongsManager.addSong(songTitle.Text, songFile.Text, songLocalPath.Text);
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

        private async void deleteSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            await SongsManager.deleteSong(song);
        }

        private async void updateSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            string errorMessage = await SongsManager.updateSongAsync(song);
            if (errorMessage != string.Empty)
            {
                await displayContentDialog(errorMessage);
            }
            else
            {
                await displayContentDialog($"Song '{song.Title}' Updated");
            }
            
        }

        private async void uploadNewVersionClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await uploadNewSongVersionContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Song song = (sender as Button).DataContext as Song;
                string errorMessage = await SongsManager.uploadNewSongVersion(song, title.Text, description.Text);
                if (errorMessage != string.Empty)
                {
                    await displayContentDialog(errorMessage);
                }
                else
                {
                    await displayContentDialog($"New Version of '{song.Title}' Uploaded");
                }

            }
            title.Text = String.Empty;
            description.Text = String.Empty;

        }

        private async void openSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            (bool, string) opened = await SongsManager.openSong(song);
            if (!opened.Item1)
            {
                await displayContentDialog(opened.Item2);
            }
        }

        private async void revertSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            string errorMessage = await SongsManager.revertSong(song);
            if (errorMessage != string.Empty)
            {
                await displayContentDialog(errorMessage);
            }
            else
            {
                await displayContentDialog($"'{song.Title}' Reverted");
            }
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

        private SongsManager SongsManager;
        private ISaver Saver;
        private IVersionTool VersionTool;
    }
}
