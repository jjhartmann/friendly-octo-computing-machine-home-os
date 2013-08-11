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
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace HomeOS.Hub.Apps.VolumeWp7
{
    public partial class MainPage : PhoneApplicationPage
    {
        private SimplexContractClient client;
        
        /// <summary>
        /// Basic configuration, it can be changed in settins window
        /// </summary>
        private string urlBasic = "http://192.168.0.2:51430/Hawdziejuk/Volume";
        private float minValue;
        private float maxValue;
        private float actualValue;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            GlobalState.SetConfSetting("url", this.urlBasic);
            client = this.InitClient();

            //finds min, max & actucal volume to set
            this.client.GetMinVolumeAsync();
            this.client.GetMaxVolumeAsync();
            this.client.GetActualVolumeAsync();
            this.RefreshUIAboutVolume();
        }

        void client_GetActualVolumeCompleted(object sender, GetActualVolumeCompletedEventArgs e)
        {
            this.actualValue = e.Result;
            this.RefreshUIAboutVolume();
        }

        void client_GetMaxVolumeCompleted(object sender, GetMaxVolumeCompletedEventArgs e)
        {
            this.maxValue = e.Result;
            RefreshUIAboutVolume();
        }

        private SimplexContractClient InitClient()
        {
            if (GlobalState.ContainsConfSetting("url"))
            {
                Binding binding = new BasicHttpBinding();
                ((BasicHttpBinding)binding).MaxReceivedMessageSize = 2147483647;

                SimplexContractClient simplexClient = new SimplexContractClient(binding,
                    new EndpointAddress((String)GlobalState.GetConfSetting("url")));

                simplexClient.GetMaxVolumeCompleted += this.client_GetMaxVolumeCompleted;
                simplexClient.GetMinVolumeCompleted += this.client_GetMinVolumeCompleted;
                simplexClient.GetActualVolumeCompleted += this.client_GetActualVolumeCompleted;
                return simplexClient;
            }
            else
            {
                GlobalState.AddConsoleMessage("URL is not set");
            }

            return null;
        }

        #region Info about volume

        private void RefreshUIAboutVolume()
        {
            float actual = this.actualValue - this.minValue;
            float maxim = this.maxValue - this.minValue;
            int procent = (int)(100 * actual / maxim);

            this.PageTitle.Text = procent + "%";
        }

        void client_GetMinVolumeCompleted(object sender, GetMinVolumeCompletedEventArgs e)
        {
            this.minValue = e.Result;
            RefreshUIAboutVolume();
        }

        #endregion

        #region Changing Volume

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            client.UpdateVolumeAsync(1);
            client.GetActualVolumeAsync();
        }

        private void substractButton_Click(object sender, RoutedEventArgs e)
        {
            client.UpdateVolumeAsync(-1);
            client.GetActualVolumeAsync();
        }

        private void zeroButton_Click(object sender, RoutedEventArgs e)
        {
            client.UpdateVolumeAsync(0);
            client.GetActualVolumeAsync();
        }

        #endregion

        #region Settings

        /// <summary>
        /// Raises after clicking settings button, shows page with setting lan addrress of the service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settings_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        #endregion
    }
}