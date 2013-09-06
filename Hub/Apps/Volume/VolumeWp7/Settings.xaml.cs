//Kamil.Hawdziejuk@uj.edu.pl
//02.01.2013

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace HomeOS.Hub.Apps.VolumeWp7
{
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            this.InitializeComponent();
            this.urlText.Text = GlobalState.GetConfSetting("url").ToString();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            GlobalState.SetConfSetting("url", this.urlText.Text);
            this.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}