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
using Microsoft.Xna.Framework.Media; 
using Matrix = Microsoft.Xna.Framework.Matrix; 
using WIPSA_GPS_PhoneComponent;
using System.IO;

// Directives 
using Microsoft.Devices;
using System.Windows.Media.Imaging;
using System.Threading; 


namespace WIPSA_GPS
{
    public partial class Page2 : PhoneApplicationPage
    {
        //Direct3DBackground m_d3dBackground = null;
        private int savedCounter = 0;
        //MediaLibrary library = new MediaLibrary();
        Motion motion;
        PhotoCamera cam;

        List<TextBlock> textBlocks;
        List<Vector3> points;
        System.Windows.Point pointOnScreen;

        Rectangle viewport;
        Matrix projection;
        Matrix view;
        Matrix attitude;

        private WriteableBitmap wb;
        private Thread ARGBFramesThread;
        private bool pumpARGBFrames; 

        private static ManualResetEvent pauseFramesEvent = new ManualResetEvent(true); 




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

                // from http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh202956(v=vs.105).aspx#BKMK_CreatingTheCameraUI
                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);
                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

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

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
            }
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        // Cannot capture an image until the previous capture has completed.
                        txtDebug.Text = ex.Message;
                    });
                }
            }
        }

        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
        }

    // Informs when full resolution photo has been taken, 
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            string fileName = savedCounter + ".jpg";

            try
            {   // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Captured image available, saving photo.";
                });

                // Save photo to the media library camera roll.
                //library.SavePictureToCameraRoll(fileName, e.ImageStream);

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Photo has been dispatched";

                });

                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);


                // below uses a memorystream to 
                MemoryStream ms = new MemoryStream();
                byte[] buffer = new byte[e.ImageStream.Length];
                int read;
                while ((read = e.ImageStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                byte[] byteArray = ms.ToArray();

                process(byteArray);

                /*
                // Save photo as JPEG to the local folder.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to the local folder. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }
                */
                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    txtDebug.Text = "Photo has been \"saved\" to the local folder.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }


        void process(byte[] input)
        {
            // do processing here
            return;
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


        // ARGB frame pump 
        void PumpARGBFrames()
        {
            // Create capture buffer. 
            int[] ARGBPx = new int[(int)cam.PreviewResolution.Width * (int)cam.PreviewResolution.Height];

            try
            {
                PhotoCamera phCam = (PhotoCamera)cam;
                int width = (int)cam.PreviewResolution.Width;
                int height = (int)cam.PreviewResolution.Height;

                while (pumpARGBFrames)
                {
                    pauseFramesEvent.WaitOne();
                    
                    // Copies the current viewfinder frame into a buffer for further manipulation. 
                    phCam.GetPreviewBufferArgb32(ARGBPx);

                    // Conversion to grayscale. 
                    int p1x = 40; int p1y = 20; int p2x = 120; int p2y = 90; int size = 10;

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            shader(ARGBPx, i, j, p1x, p1y, p2x, p2y, size, width, height);
                        }
                    }
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            shader(ARGBPx, i, j, p1x, p1y, p2x, p2y, size, width, height);
                        }
                    }
                    pauseFramesEvent.Reset();
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        // Copy to WriteableBitmap. 
                        ARGBPx.CopyTo(wb.Pixels, 0);
                        wb.Invalidate();

                        pauseFramesEvent.Set();
                    });
                }

            }
            catch (Exception e)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Display error message. 
                    txtDebug.Text = e.Message;
                });
            }
        }

        internal int ColorToGray(int color)
        {
            int gray = 0;

            int a = color >> 24;
            int r = (color & 0x00ff0000) >> 16;
            int g = (color & 0x0000ff00) >> 8;
            int b = (color & 0x000000ff);

            if ((r == g) && (g == b))
            {
                gray = color;
            }
            else
            {
                // Calculate for the illumination. 
                // I =(int)(0.109375*R + 0.59375*G + 0.296875*B + 0.5) 
                int i = (7 * r + 38 * g + 19 * b + 32) >> 6;

                gray = ((a & 0xFF) << 24) | ((i & 0xFF) << 16) | ((i & 0xFF) << 8) | (i & 0xFF);
            }
            return gray;
        }

        internal void shader(int[] img, int x, int y, int p1x, int p1y, int p2x, int p2y, int size, int resWidth, int resHeight)
        {
            const int yellow = -256; //0xffffff00;

            int d1 = (x - p1x) * (x - p1x) + (y - p1y) * (y - p1y);
            int d2 = (x - p2x) * (x - p2x) + (y - p2y) * (y - p2y);

            if (size * size > d1 || size * size > d2) // if it's within the circles of the endpts
            {
                img[y * resWidth + x] = yellow;
                return;
            }


            else
            {
                if (p1x > p2x) // force p1 to left side of p2
                {
                    int tempx = p2x; int tempy = p2y;
                    p2x = p1x; p2y = p1y;
                    p1x = tempx; p1y = tempy;
                }

                if (x > p1x && x < p2x) // if x is between
                {
                    float dx = (float)(x - p1x) / (float)(p2x - p1x);
                    int ly = (int)(dx * (p2y - p1y)) + p1y;
                    if (abs(ly - y) < size)
                    {
                        img[y * resWidth + x] = yellow;
                        return;
                    }
                }
            }
            return;
        }

        int abs(int a)
        {
            if (a < 0)
                return -a;
            return a;
        }

        private void GrayOn_Clicked(object sender, RoutedEventArgs e) 
        { 
            MainImage.Visibility = Visibility.Visible;
            MainImage.RenderTransform = new RotateTransform() { Angle = 0, CenterX = 0.5, CenterY = 0.5 };
            pumpARGBFrames = true; 
            ARGBFramesThread = new System.Threading.Thread(PumpARGBFrames); 
 
            wb = new WriteableBitmap((int)cam.PreviewResolution.Width, (int)cam.PreviewResolution.Height);
            this.MainImage.Source = wb; 
 
            // Start pump. 
            ARGBFramesThread.Start(); 
            this.Dispatcher.BeginInvoke(delegate() 
            { 
                txtDebug.Text = "ARGB to Grayscale"; 
            }); 
        } 
 
        // Stop ARGB to grayscale pump. 
        private void GrayOff_Clicked(object sender, RoutedEventArgs e) 
        { 
            MainImage.Visibility = Visibility.Collapsed; 
            pumpARGBFrames = false; 
 
            this.Dispatcher.BeginInvoke(delegate() 
            { 
                txtDebug.Text = ""; 
            }); 
        } 
    }


}