using App1.Models;
using App1.Models.Ports;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage(ISaver NewSaver)
        {
            this.InitializeComponent();
            Saver = NewSaver;
            importSavedUser();
        }

        public async void saveSettingsClick(object sender, RoutedEventArgs e)
        {
            User user = new User(gitLabUsername.Text, gitLabPassword.Password, gitUsername.Text, gitEmail.Text);
            Saver.saveUser(user);
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Settings Saved";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private void importSavedUser()
        {
            User user = Saver.savedUser();
            loadUserSettingsInUI(user);
        }

        private void loadUserSettingsInUI(User user)
        {
            gitLabUsername.Text = user.GitLabUsername;
            gitLabPassword.Password = user.GitLabPassword;
            gitUsername.Text = user.GitUsername;
            gitEmail.Text = user.GitEmail;
        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (revealModeCheckBox.IsChecked == true)
            {
                gitLabPassword.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                gitLabPassword.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }

        ISaver Saver;
    }
}
