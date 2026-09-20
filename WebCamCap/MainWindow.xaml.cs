using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging; // ImageFormat
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebCamCap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoDevice;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // look for video input devices on this machine
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count > 0) {
                // use first device found in list
                // must supply "moniker" string of first device to constructor
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                // set event handler for NewFrame event
                videoDevice.NewFrame += new NewFrameEventHandler(VideoDevice_NewFrame);
                videoDevice.Start();
            }

        }
        private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // display the frame in the "Image" window
            // must convert the frame first
            // thanks to this StackOverflow answer:
            // https://stackoverflow.com/questions/2006055/implementing-a-webcam-on-a-wpf-app-using-aforge-net
            try
            {
                using (Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Save the frame bitmap to a memory stream
                        bitmap.Save(ms, ImageFormat.Bmp);
                        ms.Position = 0;

                        BitmapImage bmi = new BitmapImage();
                        bmi.BeginInit();
                        bmi.StreamSource = ms;
                        bmi.CacheOption = BitmapCacheOption.OnLoad;
                        bmi.EndInit();
                        bmi.Freeze();

                        // Update the WPF Image control on the main UI thread
                        Dispatcher.Invoke(() =>
                        {
                            imageFrame.Source = bmi;
                        });
                    }
                }
            }
            catch (Exception ex) { 
            
            }

        }
    }
}