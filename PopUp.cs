using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace App1
{
    internal class PopUp : ContentDialog
    {
        public PopUp(Window window, string Title)
        {
            this.Title = Title;
            this.CloseButtonText = "Close";
            this.DefaultButton = ContentDialogButton.Close;
            this.XamlRoot = window.Content.XamlRoot;
        }
    }
}
