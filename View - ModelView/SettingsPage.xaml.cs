using App1;
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
        public SettingsPage()
        {
            this.InitializeComponent();
            importSavedUser();
        }

        public async void saveSettingsClick(object sender, RoutedEventArgs e)
        {
            Saver saver = new Saver();
            User user = new User(gitLabUsername.Text, gitLabPassword.Password, gitUsername.Text, gitEmail.Text);
            saver.saveUser(user);
            ContentDialog dialog = new ContentDialog();
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "Settings Saved";
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            await dialog.ShowAsync();
        }

        private void importSavedUser()
        {
            Saver saver = new Saver();
            User user = saver.savedUser();
            loadUserSettingsInUI(user);
        }

        private void loadUserSettingsInUI(User user)
        {
            gitLabUsername.Text = user.gitLabUsername;
            gitLabPassword.Password = user.gitLabPassword;
            gitUsername.Text = user.gitUsername;
            gitEmail.Text = user.gitEmail;
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
    }
}
