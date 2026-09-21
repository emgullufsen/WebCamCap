using WebCamCap;
using System.Drawing;
using Xunit;
using System.Windows.Media.Imaging;

namespace WebCamCapTest
{
    //[Collection("Sequential Graphics Tests")]
    public class ImageTester
    {
        [Fact]
        public void TestConvertToBitmap()
        {
            // create dummy Bitmap from photo, to convert to BitmapImage
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string relativePath = Path.Combine(baseDir, "testdata", "testphoto.jpg");
            
            if (File.Exists(relativePath))
            { 
                using (Bitmap bitmap = new Bitmap(relativePath))
                {
                    BitmapImage? bmi = WebCamCap.MainWindow.ConvertBitMap(bitmap);
                    Assert.NotNull(bmi);
                }
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
