﻿using WinUIApp;
using GitVersionTool;
using App1.Models;
using App1.Models.Ports;
using App1.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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
            ISaver saver = new LocalSettingsSaver();
            IFileManager fileManager = new FileManager();
            IVersionTool versionTool = new GitSongVersioning(saver.savedMusicSyncFolder(), saver, fileManager);
            ISongsManager songsManager = new SongsManager(versionTool, saver, fileManager);
            SongsPageViewModel = new SongsPageViewModel(songsManager);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                await SongsPageViewModel.refreshSongsVersionedAsync();
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void updateAllSongsClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await SongsPageViewModel.updateAllSongsAsync();
                await displayContentDialog("All Songs Updated");
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void addSongClick(object sender, RoutedEventArgs e)
        {
            try
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
                        await SongsPageViewModel.addSharedSongAsync(songSharedTitle.Text, sharedLink.Text, songSharedLocalPath.Text);
                    }
                    else
                    {
                        throw new InvalidOperationException("Please provide all needed informations.");
                    }
                }
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
            finally
            {
                songTitle.Text = string.Empty;
                songFile.Text = string.Empty;
                songLocalPath.Text = string.Empty;
                sharedLink.Text = string.Empty;
                songSharedTitle.Text = string.Empty;
                songSharedLocalPath.Text = string.Empty;
            }
            
        }

        private async void songFolder_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void folder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderPicker = new FolderPicker();
                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
                var folderPicked = await folderPicker.PickSingleFolderAsync();
                if (folderPicked != null)
                {
                    songSharedLocalPath.Text = folderPicked.Path;
                }
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void deleteSongClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SongVersioned song = (sender as Button).DataContext as SongVersioned;
                await SongsPageViewModel.deleteSongAsync(song);
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void shareSongClick(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void updateSongClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SongVersioned song = (sender as Button).DataContext as SongVersioned;
                await SongsPageViewModel.updateSongAsync(song);
                await displayContentDialog($"Song '{song.Title}' Updated");
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void uploadNewVersionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialogResult result = await uploadNewSongVersionContentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    SongVersioned song = (sender as Button).DataContext as SongVersioned;
                    await SongsPageViewModel.uploadNewSongVersionAsync(song, title.Text, description.Text, compo, mix, mastering);
                    await displayContentDialog($"New Version of '{song.Title}' Uploaded");
                }
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
            finally
            {
                title.Text = String.Empty;
                description.Text = String.Empty;
                Compo.IsChecked = false;
                Mix.IsChecked = false;
                Mastering.IsChecked = false;
            }
        }

        private async void openSongClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SongVersioned song = (sender as Button).DataContext as SongVersioned;
                await SongsPageViewModel.openSongAsync(song);
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
            }
        }

        private async void revertSongClick(object sender, RoutedEventArgs e)
        {
            try
            {
                SongVersioned song = (sender as Button).DataContext as SongVersioned;
                await SongsPageViewModel.revertSongAsync(song);
                await displayContentDialog($"'{song.Title}' Reverted");
            }
            catch (Exception ex)
            {
                await displayErrorDialog(ex.Message);
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
