using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace csharp_kinect_info
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // kinect sensor handle
        KinectSensor sensor;
        // UI event binding
        private MainWindowViewModel viewModel;
        // color stream related
        private byte[] pixelData;
        // customized variables

        // program entry point
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += this.MainWindow_Loaded;
            this.viewModel = new MainWindowViewModel();
            this.DataContext = this.viewModel;
        }

        // this function has to be registered to be loaded
        protected void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // monitoring status
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            if (KinectSensor.KinectSensors.Count > 0)
            {
                // get sensor handle
                // this.sensor = KinectSensor.KinectSensors[0];
                this.sensor = KinectSensor.KinectSensors.FirstOrDefault(sensorItem => sensorItem.Status == KinectStatus.Connected);

                // start Kinect to manipulate scene
                this.StartSensor();

                // enable streams
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                

                // implement handle function for image stream
                this.sensor.ColorFrameReady += Sensor_ColorFrameReady;
                this.sensor.DepthFrameReady += Sensor_DepthFrameReady;
                // depth stream handler registration

                // UI
                // SetKinectInfo();

                // 1-time camera settings
                defaultCameraSettings();
            }
            else {
                MessageBox.Show("No device is connected with system!");
                this.Close();
            }
        }

        private void Sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthimageFrame = e.OpenDepthImageFrame())
            {
                if (depthimageFrame == null)
                {
                    return;
                }
                short[] pixelData = new short[depthimageFrame.PixelDataLength];
                int stride = depthimageFrame.Width * 2;
                depthimageFrame.CopyPixelDataTo(pixelData);
                this.DepthImageControl.Source = BitmapSource.Create(depthimageFrame.Width, depthimageFrame.Height, 96, 96, PixelFormats.Gray16, null, pixelData, stride);
            }
        }

        private void defaultCameraSettings()
        {
            this.sensor.ColorStream.CameraSettings.AutoExposure = false;
            this.sensor.ColorStream.CameraSettings.ExposureTime = 4000.0f;
            this.SaveButton.Content = "Save-" + (this.sensor.ColorStream.CameraSettings.FrameInterval = 50.0f).ToString();
            // this.sensor.ColorStream.CameraSettings.AutoWhiteBalance = false;
            // this.sensor.ColorStream.CameraSettings.Brightness = 0.9f;

            this.sensor.DepthStream.Range = DepthRange.Near;
        }
        
        private void Sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // throw new NotImplementedException();
            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if(imageFrame == null)
                {
                    // dropped frame detected
                    return;
                }
                else
                {
                    // "get" the pixel data (allocated byte array)
                    this.pixelData = new byte[imageFrame.PixelDataLength];

                    // copy the pixel data
                    imageFrame.CopyPixelDataTo(this.pixelData);

                    // calculate the stride
                    int stride = imageFrame.Width * imageFrame.BytesPerPixel;

                    // assign to image control
                    this.VideoControl.Source = BitmapSource.Create(
                        imageFrame.Width,
                        imageFrame.Height,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null,
                        pixelData,
                        stride);

                    // update UI
                    this.viewModel.FrameNumber = imageFrame.FrameNumber;
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:

                    break;
                case KinectStatus.Disconnected:

                    break;
            }
        }

        private void StartSensor()
        {
            if(this.sensor != null && !this.sensor.IsRunning)
            {
                this.sensor.Start();
                SetKinectInfo();
                defaultCameraSettings();
            }
        }
        private void StopSensor()
        {
            if (this.sensor != null && this.sensor.IsRunning)
            {
                this.sensor.Stop();
            }
        }

        private void SetKinectInfo()
        {
            if(this.sensor != null && this.sensor.IsRunning)
            {
                this.viewModel.ConnectionID = this.sensor.DeviceConnectionId;
                this.viewModel.SensorAngle = this.sensor.ElevationAngle;
                this.viewModel.SensorStatus = this.sensor.Status.ToString();
                // other property values
                // ...   
            }
            else if(!this.sensor.IsRunning)
            {
                MessageBox.Show("Kinect is shutdown and not running");
            }
            else
            {
                MessageBox.Show("Cannot set Kinect info because sensor is null");
            }
        }

        private void SaveImage(string decorates)
        {
            // color stream image
            using (System.IO.FileStream filestream = new System.IO.FileStream(
                // Guid.NewGuid().ToString() <---- generate random string
                string.Format("{0}..\\..\\..\\fi_{1}.Jpg", System.AppDomain.CurrentDomain.BaseDirectory, decorates),
                System.IO.FileMode.Create)
            )
            {
                BitmapSource imageSource = (BitmapSource)VideoControl.Source;
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(imageSource));
                jpegEncoder.Save(filestream);
                filestream.Close();
            }
            // depth stream image
            using (System.IO.FileStream filestream = new System.IO.FileStream(
                // Guid.NewGuid().ToString() <---- generate random string
                string.Format("{0}..\\..\\..\\fi_{1}_depth.Jpg", System.AppDomain.CurrentDomain.BaseDirectory, decorates),
                System.IO.FileMode.Create)
            )
            {
                BitmapSource imageSource = (BitmapSource)this.DepthImageControl.Source;
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(imageSource));
                jpegEncoder.Save(filestream);
                filestream.Close();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string buttonName = (sender as Button).Content.ToString();
            int incrementAngle = 7;

            if(buttonName == "Up")
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate ()
                        {
                            if (this.sensor.ElevationAngle + incrementAngle > this.sensor.MaxElevationAngle)
                            {
                                this.sensor.ElevationAngle = this.sensor.MaxElevationAngle;
                            }
                            else
                            {
                                this.sensor.ElevationAngle += incrementAngle;
                            }
                        }
                    )
                );
            }
            else if(buttonName == "Down")
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate ()
                        {
                            if (this.sensor.ElevationAngle - incrementAngle < this.sensor.MinElevationAngle)
                            {
                                this.sensor.ElevationAngle = this.sensor.MinElevationAngle;
                            }
                            else
                            {
                                this.sensor.ElevationAngle -= incrementAngle;
                            }
                        }
                    )
                );
            }
            else if(buttonName == "Start")
            {
                StartSensor();
            }
            else if(buttonName == "Stop")
            {
                StopSensor();
            }
            else // save
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate()
                        {
                            double fi = this.sensor.ColorStream.CameraSettings.FrameInterval;
                            this.SaveImage(fi.ToString());
                            if ((fi *= 2.0f) > 4000.0f)
                            {
                                StopSensor();
                            }
                            else
                            {
                                // update kinect frame interval
                                this.sensor.ColorStream.CameraSettings.FrameInterval = fi;
                                // update UI
                                (sender as Button).Content = "Save-" + fi.ToString();
                            }
                        }
                        ));
                
            }
            // update UI
            SetKinectInfo();
            
        }
    }
}

// setup camera angle 

// depth data representation
