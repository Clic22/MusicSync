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
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await addNewSongContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                songsManager_.addSong(songTitle.Text, songlocalPath.Text);
            }
            else
            {
                // User pressed Cancel, ESC, or the back arrow.
                // Do nothing
            }
            songTitle.Text = String.Empty;  
            songlocalPath.Text = String.Empty;
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
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = $"Song '{song.title}' Updated";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            songsManager_.updateAllSongs();
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "All Songs Updated";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private async void uploadNewVersionClick(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await uploadNewSongVersionContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Song song = (sender as Button).DataContext as Song;
                songsManager_.uploadNewSongVersion(song, title.Text, description.Text);
                ContentDialog dialog = new ContentDialog();
                dialog.XamlRoot = this.XamlRoot;
                dialog.Title = $"New Version of '{song.title}' Uploaded";
                dialog.CloseButtonText = "Close";
                dialog.DefaultButton = ContentDialogButton.Close;
                await dialog.ShowAsync();
            }
            else
            {

            }
            title.Text = String.Empty;
            description.Text = String.Empty;      

        }

        private async void folders_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                songTitle.Text = folderPicked.Name;
                songlocalPath.Text = folderPicked.Path;
            }
        }

        private void openSongClick(object sender, RoutedEventArgs e)
        {
            Song song = (sender as Button).DataContext as Song;
            songsManager_.openSong(song);
        }

        private SongsManager songsManager_;


    }
}
