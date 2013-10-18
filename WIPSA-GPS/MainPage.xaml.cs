using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace WIPSA_GPS
{
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void lsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/gpsPage.xaml", UriKind.Relative));
        }

        private void voButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/voPage.xaml", UriKind.Relative));
        }
    }
}