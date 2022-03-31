using App1.Models;
using App1.Models.Ports;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Storage.Pickers;
using WinUIApp;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            Saver = new LocalSettingsSaver();
            importSettings();
        }

        public async void saveSettingsClick(object sender, RoutedEventArgs e)
        {
            User user = new User(BandName.Text, BandPassword.Password, Username.Text, BandEmail.Text);
            string musicSyncFolder = MusicSyncFolder.Text;
            Saver.saveSettings(user, musicSyncFolder);
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Settings Saved";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private void importSettings()
        {
            User user = Saver.savedUser();
            loadUserSettingsInUI(user);
            string musicSyncFolder = Saver.savedMusicSyncFolder();
            loadMusicSyncFolderSettingInUI(musicSyncFolder);
        }

        private void loadUserSettingsInUI(User user)
        {
            BandName.Text = user.BandName;
            BandPassword.Password = user.BandPassword;
            Username.Text = user.Username;
            BandEmail.Text = user.BandEmail;
        }

        private void loadMusicSyncFolderSettingInUI(string musicSyncFolder)
        {
            MusicSyncFolder.Text = musicSyncFolder;
        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (revealModeCheckBox.IsChecked == true)
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                BandPassword.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        private async void folder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.WindowHandle);
            var folderPicked = await folderPicker.PickSingleFolderAsync();
            if (folderPicked != null)
            {
                MusicSyncFolder.Text = folderPicked.Path;
            }
        }

        ISaver Saver;
    }
}
