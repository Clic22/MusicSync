
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using App1.ViewModels;
using Windows.Storage.Pickers;
using App1.Models.Ports;
using App1.Models;
using App1.Adapters;
using Microsoft.UI;
using Windows.ApplicationModel.DataTransfer;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    public sealed partial class SongsPage : Page
    {
        public SongsPage()
        {
            this.InitializeComponent();
            ISaver saver = new LocalSettingsSaver();
            IVersionTool versionTool = new GitSongVersioning();
            IFileManager fileManager = new FileManager();
            ISongsManager songsManager = new SongsManager(versionTool, saver, fileManager);
            SongsPageViewModel = new SongsPageViewModel(songsManager);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string errorMessage =  await SongsPageViewModel.updateAllSongsAsync();
            if (errorMessage != string.Empty)
            {
                await displayErrorDialog(errorMessage);
            }
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            string errorMessage = await SongsPageViewModel.updateAllSongsAsync();
            if (errorMessage != string.Empty)
            {
                await displayErrorDialog(errorMessage);
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
                if (songTitle.Text != string.Empty && songFile.Text != string.Empty && songLocalPath.Text != string.Empty)
                {
                    SongsPageViewModel.addLocalSong(songTitle.Text, songFile.Text, songLocalPath.Text);
                }
                else if (songSharedTitle.Text != string.Empty && sharedLink.Text != string.Empty && songSharedLocalPath.Text != string.Empty)
                {
                    string errorMessage = await SongsPageViewModel.addSharedSongAsync(songSharedTitle.Text, sharedLink.Text, songSharedLocalPath.Text);
                    if (errorMessage != string.Empty)
                    {
                        await displayErrorDialog(errorMessage);
                    }
                }
                else
                {
                    string errorMessage = "Please provide all needed informations.";
                    await displayErrorDialog(errorMessage);
                }
            }
            songTitle.Text = string.Empty;
            songFile.Text = string.Empty;
            songLocalPath.Text = string.Empty;
            sharedLink.Text = string.Empty;
            songSharedTitle.Text = string.Empty;
            songSharedLocalPath.Text = string.Empty;
        }

        private async void songFolder_Click(object sender, RoutedEventArgs e)
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

        private async void folder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                songSharedLocalPath.Text = folderPicked.Path;
            }
        }

        private async void deleteSongClick(object sender, RoutedEventArgs e)
        {
            SongVersioned song = (sender as Button).DataContext as SongVersioned;
            await SongsPageViewModel.deleteSongAsync(song);
        }

        private async void shareSongClick(object sender, RoutedEventArgs e)
        {
            SongVersioned song = (sender as Button).DataContext as SongVersioned;
            string shareLink = await SongsPageViewModel.shareSongAsync(song);
            ContentDialogResult result = await displayShareLinkDialog(shareLink);
            if (result == ContentDialogResult.Primary)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetText(shareLink);
                Clipboard.SetContent(dataPackage);
            }
        }

        private async void updateSongClick(object sender, RoutedEventArgs e)
        {
            SongVersioned song = (sender as Button).DataContext as SongVersioned;
            string errorMessage = await SongsPageViewModel.updateSongAsync(song);
            if (errorMessage != string.Empty)
            {
                await displayErrorDialog(errorMessage);
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
                SongVersioned song = (sender as Button).DataContext as SongVersioned;
                string errorMessage = await SongsPageViewModel.uploadNewSongVersionAsync(song, title.Text, description.Text, compo, mix, mastering);
                if (errorMessage != string.Empty)
                {
                    await displayErrorDialog(errorMessage);
                }
                else
                {
                    await displayContentDialog($"New Version of '{song.Title}' Uploaded");
                }

            }
            title.Text = String.Empty;
            description.Text = String.Empty;
            Compo.IsChecked = false;
            Mix.IsChecked = false;
            Mastering.IsChecked = false;
        }

        private async void openSongClick(object sender, RoutedEventArgs e)
        {
            SongVersioned song = (sender as Button).DataContext as SongVersioned;
            (bool, string) opened = await SongsPageViewModel.openSongAsync(song);
            if (!opened.Item1)
            {
                await displayContentDialog(opened.Item2);
            }
        }

        private async void revertSongClick(object sender, RoutedEventArgs e)
        {
            SongVersioned song = (sender as Button).DataContext as SongVersioned;
            string errorMessage = await SongsPageViewModel.revertSongAsync(song);
            if (errorMessage != string.Empty)
            {
                await displayErrorDialog(errorMessage);
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

        private async Task<ContentDialogResult> displayShareLinkDialog(string text)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Share Link";
            dialog.Content = text;
            dialog.PrimaryButtonText = "Copy";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Primary;
            return await dialog.ShowAsync();
        }

        private async Task displayErrorDialog(string text)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Error";
            dialog.Content = text;
            dialog.CloseButtonText = "Close";
            var bst = new Style(typeof(Button));
            bst.Setters.Add(new Setter(Button.BackgroundProperty, Colors.DarkRed));
            bst.Setters.Add(new Setter(Button.ForegroundProperty, Colors.White));
            bst.Setters.Add(new Setter(Button.CornerRadiusProperty, "4"));
            dialog.CloseButtonStyle = bst;
            await dialog.ShowAsync();
        }

        private async Task<string> findSongFile(string songLocalPath)
        {
            var folder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(songLocalPath);
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                if (file.Name.Contains(".song"))
                {
                    return file.Name;
                }
            }
            return string.Empty;
        }

        private void Compo_Checked(object sender, RoutedEventArgs e)
        {
            compo = true;
            enableUploadButton();
        }

        private void Compo_Unchecked(object sender, RoutedEventArgs e)
        {
            compo = false;
            enableUploadButton();
        }

        private void Mix_Checked(object sender, RoutedEventArgs e)
        {
            mix = true;
            enableUploadButton();
        }

        private void Mix_Unchecked(object sender, RoutedEventArgs e)
        {
            mix = false;
            enableUploadButton();
        }

        private void Mastering_Checked(object sender, RoutedEventArgs e)
        {
            mastering = true;
            enableUploadButton();
        }

        private void Mastering_Unchecked(object sender, RoutedEventArgs e)
        {
            mastering = false;
            enableUploadButton();
        }

        private void title_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableUploadButton();
        }

        private void enableUploadButton()
        {
            uploadNewSongVersionContentDialog.IsPrimaryButtonEnabled = false;
            if ((compo || mix || mastering) && !string.IsNullOrEmpty(title.Text))
                uploadNewSongVersionContentDialog.IsPrimaryButtonEnabled = true;
        }

        private SongsPageViewModel SongsPageViewModel;
        private bool compo;
        private bool mix;
        private bool mastering;
    }
}
