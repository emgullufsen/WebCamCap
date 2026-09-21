using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics.Metrics;
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
        // using AForge's Grayscale filter with:
        // Red Weight:   0.2125
        // Green Weight: 0.7154
        // Blue Weight:  0.0721
        // thanks to this StackOverflow Answer:
        // https://stackoverflow.com/questions/20481848/what-are-the-parameters-for-aforges-grayscale-filter
        // these are the weights used in the GNU Image Manipulation Program (GIMP) - good open source software!
        private Grayscale grayscaleFilter = new Grayscale(0.2126, 0.7152, 0.0722);
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0) {
                    MessageBox.Show("No WebCam Found...");
                }
            }
            catch (Exception ex) {
                MessageBox.Show("An error occurred while searching webcam devices");
            }
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        { 
            

        }
        public Bitmap CreateHistogram(Bitmap bm)
        {
            // apply filter to Bitmap object
            using (UnmanagedImage grayUnmanaged = grayscaleFilter.Apply(UnmanagedImage.FromManagedImage(bm)))
            using (Bitmap grayFrame = grayUnmanaged.ToManagedImage())
            {
                // create stats object and generate histogram using AForge
                ImageStatistics stats = new ImageStatistics(grayUnmanaged);
                // basically this is an array of integers
                // buckets for how many pixels are in each shade (8 bits = 256 shades)
                Histogram histogram = stats.Gray;
                Bitmap bmp = new Bitmap(512, 100);
                int[] values = histogram.Values;
                int max = histogram.Max;

                // avoid division by zero
                if (max == 0) return bmp;

                // get a System.Drawing.Graphics object for drawing histogram
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // draw black background
                    g.Clear(System.Drawing.Color.FromArgb(30, 30, 30));
                    using (SolidBrush brush = new SolidBrush(System.Drawing.Color.DarkGray))
                    {
                        // loop over the 256 shades of gray, plot number of pixels in that shade (normalized)
                        for (int i = 0; i < 256; i++)
                        {
                            // Normalize the height to 100 pixels max
                            int pctHeight = (int)(((double)values[i] / max) * 100);
                            // Drawing is top->down, left->
                            // so y = 100 is bottom of frame
                            // so y1 is 100 - normalized height
                            // we are making our histogram bars of width 2
                            int xCoord = i * 2;
                            int yCoord = 100 - pctHeight;
                            int barWidth = 2;
                            int barHeight = pctHeight;
                            // draw bar 
                            g.FillRectangle(brush, xCoord, yCoord, barWidth, barHeight);
                        }
                    }
                }
                return bmp;
            }
            
            
        }
        public static BitmapImage? ConvertBitMap(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // save the frame bitmap to a memory stream
                    bitmap.Save(ms, ImageFormat.Bmp);
                    ms.Position = 0;

                    BitmapImage bmi = new BitmapImage();
                    bmi.BeginInit();
                    bmi.StreamSource = ms;
                    bmi.CacheOption = BitmapCacheOption.OnLoad;
                    bmi.EndInit();
                    bmi.Freeze();
                    return bmi;
                }    
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error converting BitMap to BitMapImage...");
                return null;
            }
        }

        private void VideoDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // display the frame in the "ImageFrame" window
            // also create and display histogram of grayscale values in "HistogramFrame" window
            // must convert the frame first
            // thanks to this StackOverflow answer:
            // https://stackoverflow.com/questions/2006055/implementing-a-webcam-on-a-wpf-app-using-aforge-net
            try
            {
                Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
                Bitmap bitmapGS = CreateHistogram(bitmap);
                BitmapImage? bmi = ConvertBitMap(bitmap);
                BitmapImage? bmiGS = ConvertBitMap(bitmapGS);
                // Update the WPF Image control to display webcam image
                // Update the WPF Histogram control to display histogram
                Dispatcher.Invoke(() =>
                {
                    ImageFrame.Source = bmi;
                    HistogramFrame.Source = bmiGS;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error grabbing frame from WebCam...");
            }
            

        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (videoDevices != null && videoDevices.Count > 0)
            {
                // use first device found in list
                // must supply "moniker" string of first device to constructor
                videoDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
                // set event handler for NewFrame event
                videoDevice.NewFrame += new NewFrameEventHandler(VideoDevice_NewFrame);
                videoDevice.Start();

                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("No WebCam available to start.");
            }
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void StopCamera()
        {
            if (videoDevice != null && videoDevice.IsRunning)
            {
                videoDevice.SignalToStop();
                videoDevice.NewFrame -= VideoDevice_NewFrame;
                videoDevice = null;
            }

            Dispatcher.Invoke(() =>
            {
                ImageFrame.Source = null;
                HistogramFrame.Source = null;
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
            });
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }
    }
}