using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace App1
{
    internal class VisualElements
    {
        public VisualElements(Window window)
        {
            m_window = window;
        }

        public async void displayPopUp(string Title)
        {
            ContentDialog popUp = createPopUp(Title);
            await popUp.ShowAsync();
        }

        private ContentDialog createPopUp(string Title)
        {
            ContentDialog popUp = new ContentDialog();
            popUp.Title = Title;
            popUp.CloseButtonText = "Close";
            popUp.DefaultButton = ContentDialogButton.Close;
            popUp.XamlRoot = m_window.Content.XamlRoot;
            return popUp;
        }

        private Window m_window;
    }
}
