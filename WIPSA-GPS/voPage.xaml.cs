using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; 
using System.Windows.Input; 
using System.Windows.Media; 
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Microsoft.Devices.Sensors; 
using Microsoft.Devices; 
using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Graphics; 
using Matrix = Microsoft.Xna.Framework.Matrix; 
using WIPSA_GPS_PhoneComponent;

namespace WIPSA_GPS
{
    public partial class Page2 : PhoneApplicationPage
    {
        //Direct3DBackground m_d3dBackground = null;

        Motion motion;
        PhotoCamera cam;

        List<TextBlock> textBlocks;
        List<Vector3> points;
        System.Windows.Point pointOnScreen;

        Rectangle viewport;
        Matrix projection;
        Matrix view;
        Matrix attitude; 


        public Page2()
        {
            InitializeComponent();

            // Initialize the list of TextBlock and Vector3 objects. 
            textBlocks = new List<TextBlock>();
            points = new List<Vector3>(); 
        }
 
        public void InitializeViewport() 
        { 
            // Initialize the viewport and matrixes for 3d projection. 
            viewport = new Rectangle(0, 0, (int)this.ActualWidth, (int)this.ActualHeight); 
            float aspect = viewport.Height / viewport.Width; 
            projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 1, 12); 
            view = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up); 
        } 
 
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e) 
        {
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
            {
                cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                //Event is fired when the PhotoCamera object has been initialized 
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);
                viewfinderBrush.SetSource(cam); 
            }
            else
            {
                MessageBox.Show("the camera is not supported on this device.");
                return; 

            }
            if (!Motion.IsSupported) 
            { 
                MessageBox.Show("the Motion API is not supported on this device."); 
                return; 
            } 
 
            // If the Motion object is null, initialize it and add a CurrentValueChanged 
            // event handler. 
            if (motion == null) 
            { 
                motion = new Motion(); 
                motion.TimeBetweenUpdates = TimeSpan.FromMilliseconds(20); 
                motion.CurrentValueChanged += new EventHandler<SensorReadingEventArgs<MotionReading>>(motion_CurrentValueChanged); 
            } 
 
            // Try to start the Motion API. 
            try 
            { 
                motion.Start(); 
            } 
            catch (Exception) 
            { 
                MessageBox.Show("unable to start the Motion API."); 
            } 
 
            // Hook up the event handler for when the user taps the screen. 
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseLeftButtonUp); 
 
            AddDirectionPoints(); 
 
            base.OnNavigatedTo(e); 
        }
 
         //Update UI if initialization succeeds 
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e) 
        { 
            if (e.Succeeded) 
            { 
                this.Dispatcher.BeginInvoke(delegate() 
                { 
                    MessageBox.Show("Camera initialized"); 
                }); 
 
            } 
        } 

        /*private void DrawingSurfaceBackground_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_d3dBackground == null)
            {
                m_d3dBackground = new Direct3DBackground();
            }
        }*/

        void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) 
        { 

        } 
 
 
 
        void motion_CurrentValueChanged(object sender, SensorReadingEventArgs<MotionReading> e) 
        { 
            // This event arrives on a background thread. Use BeginInvoke 
            // to call a method on the UI thread. 
            Dispatcher.BeginInvoke(() => CurrentValueChanged(e.SensorReading)); 
        } 
 
 
 
        private void CurrentValueChanged(MotionReading reading) 
        { 
            if (viewport.Width == 0) 
            { 
                InitializeViewport(); 
            } 
 
 
            // Get the RotationMatrix from the MotionReading. 
            // Rotate it 90 degrees around the X axis to put it in xna coordinate system. 
            Matrix attitude = Matrix.CreateRotationX(MathHelper.PiOver2) * reading.Attitude.RotationMatrix; 
 
            // Loop through the points in the list 
            for (int i = 0; i < points.Count; i++) 
            { 
                // Create a World matrix for the point. 
                Matrix world = Matrix.CreateWorld(points[i], new Vector3(0, 0, 1), new Vector3(0, 1, 0)); 
 
                // Use Viewport.Project to project the point from 3D space into screen coordinates. 
                Vector3 projected = Vector3.Zero; //viewport.Project(Vector3.Zero, projection, view, world * attitude); 
 
 
                if (projected.Z > 1 || projected.Z < 0) 
                { 
                    // If the point is outside of this range, it is behind the camera. 
                    // So hide the TextBlock for this point. 
                    textBlocks[i].Visibility = Visibility.Collapsed; 
                } 
                else 
                { 
                    // Otherwise, show the TextBlock 
                    textBlocks[i].Visibility = Visibility.Visible; 
 
                    // Create a TranslateTransform to position the TextBlock. 
                    // Offset by half of the TextBlock’s RenderSize to center it on the point. 
                    TranslateTransform tt = new TranslateTransform(); 
                    tt.X = projected.X - (textBlocks[i].RenderSize.Width / 2); 
                    tt.Y = projected.Y - (textBlocks[i].RenderSize.Height / 2); 
                    textBlocks[i].RenderTransform = tt; 
                } 
            } 
        }
 
 
        private void AddPoint(Vector3 point, string name) 
        { 
            // Create a new TextBlock. Set the Canvas.ZIndexProperty to make sure 
            // it appears above the camera rectangle. 
            TextBlock textblock = new TextBlock(); 
            textblock.Text = name; 
            textblock.FontSize = 124; 
            textblock.SetValue(Canvas.ZIndexProperty, 2); 
            textblock.Visibility = Visibility.Collapsed; 
 
            // Add the TextBlock to the LayoutRoot container. 
            LayoutRoot.Children.Add(textblock); 
 
            // Add the TextBlock and the point to the List collections. 
            textBlocks.Add(textblock); 
            points.Add(point); 
 
 
        } 
 
        private void AddDirectionPoints() 
        { 
            AddPoint(new Vector3(0, 0, -10), "front"); 
            AddPoint(new Vector3(0, 0, 10), "back"); 
            AddPoint(new Vector3(10, 0, 0), "right"); 
            AddPoint(new Vector3(-10, 0, 0), "left"); 
            AddPoint(new Vector3(0, 10, 0), "top"); 
            AddPoint(new Vector3(0, -10, 0), "bottom"); 
        } 
        /* 
        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e) 
        { 
            // When the TextBox loses focus. Hide the Canvas containing it. 
            TextBoxCanvas.Visibility = Visibility.Collapsed; 
        } 
         */ 
        private void NameTextBox_KeyUp(object sender, KeyEventArgs e) 
        { 
            // If the key is not the Enter key, don’t do anything. 
            if (e.Key != Key.Enter) 
            { 
                return; 
            } 
 
            // When the TextBox loses focus. Hide the Canvas containing it. 
            TextBoxCanvas.Visibility = Visibility.Collapsed; 
 
            // If any of the objects we need are not present, exit the event handler. 
            if (true)//NameTextBox.Text == "" || pointOnScreen == null || motion == null) 
            { 
                return; 
            } 
 
            // Translate the point before projecting it. 
            System.Windows.Point p = pointOnScreen; 
            p.X = LayoutRoot.RenderSize.Width - p.X; 
            p.Y = LayoutRoot.RenderSize.Height - p.Y; 
            p.X *= .5; 
            p.Y *= .5; 
 
 
            // Use the attitude Matrix saved in the OnMouseLeftButtonUp handler. 
            // Rotate it 90 degrees around the X axis to put it in xna coordinate system. 
            attitude = Matrix.CreateRotationX(MathHelper.PiOver2) * attitude; 
 
            // Use Viewport.Unproject to translate the point on the screen to 3D space. 
            Vector3 unprojected = Vector3.Zero; // viewport.Unproject(new Vector3((float)p.X, (float)p.Y, -.9f), projection, view, attitude); 
            unprojected.Normalize(); 
            unprojected *= -10; 
 
            // Call the helper method to add this point 
            //AddPoint(unprojected, NameTextBox.Text); 
 
            // Clear the TextBox 
            //NameTextBox.Text = ""; 
        }

    }
}