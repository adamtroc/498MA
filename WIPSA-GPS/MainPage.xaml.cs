//#define STADIUM_XML "C:\Users\Michael\Desktop\stadium_catalog.xml"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using WIPSA_GPS.Resources;
using System.Device.Location;
using Windows.Devices.Geolocation;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel;





namespace WIPSA_GPS
{
    
    public partial class MainPage : PhoneApplicationPage
    {
        /*These are global so we can
         * access them within other functions
         * */
        Popup my_popup_cs = new Popup();
        public struct stadium_info
        {
            public string name;
            public string school;
            public string URL;

            public void set(string input_name, string input_school, string input_URL)
            {
                name = input_name;
                school = input_school;
                URL = input_URL;
            }
        }
        stadium_info stadium = new stadium_info(); //create struct to pass in
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
            
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
           
            UpdateMap();
        }

        /*Aysnchronous because it may take time for phone to get location,
         * thus we need to let other processes run.*/
        private async void UpdateMap()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;   //can be modified to get a more precise reading

            Geoposition position =
                await geolocator.GetGeopositionAsync( //keyword:await
                TimeSpan.FromMinutes(1),
                TimeSpan.FromSeconds(30));

            /*gpsCoorCenter will hold our latitude
             * and longitude as doubles*/
            var gpsCoorCenter =                   
                new GeoCoordinate(
                    position.Coordinate.Latitude,
                    position.Coordinate.Longitude);

            /*in emulator, these get Microsoft coordinates*/
            double latitude = position.Coordinate.Latitude;
            double longitude = position.Coordinate.Longitude;
            double tolerance = .2;
            //double latitude = 40.0; //close to champaign
            //double longitude = -88.2;

            WIPSA_MAP.SetView(gpsCoorCenter, 17);
            
            /*If we are within the tolerance of a stadium, let the user know 
             * with a pop-up window*/
            if(find_stadium(latitude, longitude, tolerance, ref stadium) == 1)
            {

                    display_cspopup();
            }
           

         }

            

       

        /*Function determines if there is a stadium within reach,
         as well as parsing the XML file*/
        public static int find_stadium(double lat, double longitude, double tol, ref stadium_info stadium)
        {
            double test_result = 0;
            XDocument doc = XDocument.Load(@"Assets/stadium_catalog.xml"); 
            foreach (XElement el in doc.Root.Elements())
            {
                test_result = (Math.Pow(double.Parse(el.Element("LATITUDE").Value) - lat, 2) + Math.Pow(double.Parse(el.Element("LONGITUDE").Value) - longitude, 2));
                if ((Math.Pow(double.Parse(el.Element("LATITUDE").Value) - lat, 2) + Math.Pow(double.Parse(el.Element("LONGITUDE").Value) - longitude, 2)) <= Math.Pow(tol, 2))
                {
                    // stadium found within range
                    stadium.set(el.Element("NAME").Value, el.Element("SCHOOL").Value, el.Element("URL").Value);
                    return 1;
                }
            }
            
            // no stadium found within range
            return 0;
        }

        /*Function to display the message to link to the Team's Website*/
        public void display_cspopup()
        {
            
            Border border = new Border();                                                     // to create green color border
            border.BorderBrush = new SolidColorBrush(Colors.Green);
            border.BorderThickness = new Thickness(2);
            border.Margin = new Thickness(10, 10, 10, 10);

            StackPanel skt_pnl_outter = new StackPanel();                             // stack panel 
            skt_pnl_outter.Background = new SolidColorBrush(Colors.LightGray);
            skt_pnl_outter.Orientation = System.Windows.Controls.Orientation.Vertical;

            Image img_disclaimer = new Image();                                       // Image
            img_disclaimer.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            img_disclaimer.Stretch = Stretch.Fill;
            img_disclaimer.Margin = new Thickness(0, 15, 0, 5);
            Uri uriR = new Uri("Images/disclaimer.png", UriKind.Relative);
            BitmapImage imgSourceR = new BitmapImage(uriR);
            img_disclaimer.Source = imgSourceR;

            TextBlock txt_blk1 = new TextBlock();                                         // Textblock 1
            txt_blk1.Text = "Stadium Found";
            txt_blk1.TextAlignment = TextAlignment.Center;
            txt_blk1.FontSize = 40;
            txt_blk1.Margin = new Thickness(10, 0, 10, 0);
            txt_blk1.Foreground = new SolidColorBrush(Colors.White);

            TextBlock txt_blk2 = new TextBlock();                                      // Textblock 2
            txt_blk2.Text = "Would you like to continue to team's website?";
            txt_blk2.TextAlignment = TextAlignment.Center;
            txt_blk2.FontSize = 21;
            txt_blk2.Margin = new Thickness(10, 0, 10, 0);
            txt_blk2.Foreground = new SolidColorBrush(Colors.White);


            //Adding control to stack panel
            skt_pnl_outter.Children.Add(img_disclaimer);
            skt_pnl_outter.Children.Add(txt_blk1);
            skt_pnl_outter.Children.Add(txt_blk2);

            StackPanel skt_pnl_inner = new StackPanel();
            skt_pnl_inner.Orientation = System.Windows.Controls.Orientation.Horizontal;

            Button btn_continue = new Button();                                         // Button continue
            btn_continue.Content = "continue";
            btn_continue.Width = 215;
            btn_continue.Click += new RoutedEventHandler(btn_continue_Click); 

            Button btn_cancel = new Button();                                           // Button cancel                                     
            btn_cancel.Content = "cancel";
            btn_cancel.Width = 215;
            btn_cancel.Click += new RoutedEventHandler(btn_cancel_Click);

            skt_pnl_inner.Children.Add(btn_continue);
            skt_pnl_inner.Children.Add(btn_cancel);


            skt_pnl_outter.Children.Add(skt_pnl_inner);

            // Adding stackpanel  to border
            border.Child = skt_pnl_outter;

            // Adding border to pop-up
            my_popup_cs.Child = border;

            my_popup_cs.VerticalOffset = 400;
            my_popup_cs.HorizontalOffset = 10;

            my_popup_cs.IsOpen = true;
        }

        /*Handles button clicks*/
        private void btn_continue_Click(object sender, RoutedEventArgs e)
        {
            
            if (my_popup_cs.IsOpen)
            {
                my_popup_cs.IsOpen = false;

                /*Link to the stadium's website, upon click of
                 * continue
                 * */
                WebBrowserTask webBrowserTask = new WebBrowserTask();

                webBrowserTask.Uri = new Uri(stadium.URL, UriKind.Absolute); //sends to website in stadium.URL

                webBrowserTask.Show();
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            /*Need to add an action for when Cancel is pressed*/
            if (my_popup_cs.IsOpen)
            {
                my_popup_cs.IsOpen = false;
            }
        }


    }
}