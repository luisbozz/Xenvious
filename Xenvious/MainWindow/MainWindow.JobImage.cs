using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Xenvious.Logging;
using static Xenvious.GTA;
using static Xenvious.GTA.Offsets.Editor;

namespace Xenvious
{
    // Part of MainWindow: Reading and replacing the job image.
    public partial class MainWindow
    {
        // The job image is a JPEG inside the creator's photo buffer (verified
        // live on Enhanced, same chain on Legacy):
        //   [base + img_ptr + 0x18]        -> buffer
        //   buffer + OFFSET_image - 4      -> JPEG length (int)
        //   buffer + OFFSET_image          -> JPEG, FF D8 ... FF D9
        // The length is exact, so one read fetches the whole image (~50 KB).
        const int MaxJobImageSize = 0x80000;
        bool reading_image = false;
        byte[] lastJobImage;   // raw bytes of the image shown, as the game holds them

        System.Windows.Controls.Image JobImageControl()
        {
            return (System.Windows.Controls.Image)MainImageContainer.Template.FindName("JobImage", MainImageContainer);
        }

        /// <summary>Absolute address of the JPEG, 0 when the pointer is not available.</summary>
        long JobImageAddress()
        {
            long rva = GTA.getIMGPointer().ToInt64();
            if (rva == 0)
                return 0;
            long buffer = m.memory(m.memory(rva).GetAddress() + 0x18).Get<long>();
            return buffer == 0 ? 0 : buffer + GTA.Offsets.Editor.Image.img;
        }

        /// <summary>The job image as stored in the game, or null when there is no valid JPEG.</summary>
        byte[] ReadJobImage()
        {
            long addy = JobImageAddress();
            if (addy == 0)
                return null;
            // Heap addresses lie below the module base, so they go in as hex
            // strings: memory(long) would take them for an RVA.
            int size = m.memory((addy - 4).ToString("X")).Get<int>();
            if (size < 4 || size > MaxJobImageSize)
                return null;
            byte[] data = m.memory(addy.ToString("X")).GetBytes(size);
            bool jpeg = data[0] == 0xFF && data[1] == 0xD8 && data[size - 2] == 0xFF && data[size - 1] == 0xD9;
            return jpeg ? data : null;
        }

        private static BitmapImage byteArrayToImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public byte[] ImageSourceToBytes(BitmapEncoder encoder, ImageSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;

            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }

            return bytes;
        }
        private void TimerGetJobImage_Tick(object sender, EventArgs e)
        {
            if (!m.IsProcOpen || reading_image)
                return;
            // The job has a photo when bit 0 of the flags field is set (saved as "photo").
            if (!Functions.Read.checkbinary(1, GTA.Offsets.Editor.photo) && !IsInCreator())
                return;

            reading_image = true;
            try
            {
                byte[] data = ReadJobImage();
                // Decode only when the game holds a different image than the one shown.
                if (data == null || (lastJobImage != null && data.SequenceEqual(lastJobImage)))
                    return;
                lastJobImage = data;
                JobImageControl().Source = byteArrayToImage(data);
            }
            catch (Exception ex)
            {
                Log.Debug($"job image: {ex.Message}", source: "image");
            }
            finally
            {
                reading_image = false;
            }
        }

        public static byte[] ImgToByteArray(System.Drawing.Image img)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                img.Save(mStream, img.RawFormat);
                return mStream.ToArray();
            }
        }

        // Change Image: a dialog that takes an image from the clipboard (Ctrl+V) or a file,
        // shows it and writes it into the creator only once confirmed.
        private async void BtnJobImage_Click(object sender, RoutedEventArgs e)
        {
            if (!m.IsProcOpen || !IsInCreator())
                return;

            BitmapSource picked = null;
            void Show(BitmapSource image)
            {
                if (image == null)
                    return;
                picked = image;
                DialogImage.Source = image;
                DialogImageHint.Visibility = Visibility.Collapsed;
                DialogConfirm.IsEnabled = true;
            }

            var closed = ChooseAsync(TranslateOr("jobimg_title", "Change job image"),
                TranslateOr("jobimg_text", "Paste an image with Ctrl+V or choose a file."),
                TranslateOr("jobimg_use", "Use image"), TranslateOr("jobimg_file", "Choose from file"),
                TranslateOr("dialog_cancel", "Cancel"));
            DialogImage.Source = null;
            DialogImageHint.Visibility = Visibility.Visible;
            DialogImageBox.Visibility = Visibility.Visible;
            DialogConfirm.IsEnabled = false;
            _dialogPaste = () => Show(ClipboardImage());
            _dialogAltAction = () => Show(FileImage());

            if (await closed != DialogChoice.Confirm || picked == null)
                return;
            WriteJobImage(picked);
        }

        private static BitmapSource ClipboardImage()
        {
            try
            {
                // Browsers and most tools also put a PNG on the clipboard; it keeps the colours
                // that the plain bitmap format sometimes loses.
                if (Clipboard.GetData("PNG") is MemoryStream png)
                    return LoadImage(png.ToArray());
                if (Clipboard.ContainsFileDropList())
                {
                    foreach (string file in Clipboard.GetFileDropList())
                        return LoadImage(File.ReadAllBytes(file));
                }
                if (Clipboard.ContainsImage())
                    return Clipboard.GetImage();
            }
            catch (Exception ex)
            {
                Log.Debug("job image: clipboard: " + ex.Message, source: "image");
            }
            return null;
        }

        private static BitmapSource FileImage()
        {
            var ofd = new OpenFileDialog
            {
                Title = "Select any Image file",
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.gif) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.gif",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Multiselect = false
            };
            if (ofd.ShowDialog() != true)
                return null;
            try
            {
                return LoadImage(File.ReadAllBytes(ofd.FileName));
            }
            catch (Exception ex)
            {
                Log.Debug("job image: file: " + ex.Message, source: "image");
                return null;
            }
        }

        private static BitmapSource LoadImage(byte[] data)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(data);
            image.EndInit();
            image.Freeze();
            return image;
        }

        private void WriteJobImage(BitmapSource image)
        {
            long addy = JobImageAddress();
            if (addy == 0)
                return;

            // Never larger than the photo the game holds: the buffer behind it has a fixed size.
            var current = lastJobImage != null ? byteArrayToImage(lastJobImage) : null;
            if (current != null && (image.PixelWidth > current.PixelWidth || image.PixelHeight > current.PixelHeight))
            {
                double scale = Math.Min((double)current.PixelWidth / image.PixelWidth, (double)current.PixelHeight / image.PixelHeight);
                image = new TransformedBitmap(image, new ScaleTransform(scale, scale));
            }
            // The JPEG encoder takes no alpha channel.
            if (image.Format != PixelFormats.Bgr24)
                image = new FormatConvertedBitmap(image, PixelFormats.Bgr24, null, 0);

            byte[] data = ImageSourceToBytes(new JpegBitmapEncoder { QualityLevel = 90 }, image);
            if (data == null || data.Length > MaxJobImageSize)
            {
                displayScreenMessage("the image is too big");
                return;
            }

            m.memory((addy - 0x4).ToString("X")).SetInt(data.Length);
            m.memory(addy.ToString("X")).SetBytes(data);
            lastJobImage = data;
            JobImageControl().Source = byteArrayToImage(data);
        }

        [DebuggerHidden]
#pragma warning disable CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        private async Task<bool> RemoteFileExists(string url)
#pragma warning restore CS1998 // Bei der asynchronen Methode fehlen "await"-Operatoren. Die Methode wird synchron ausgeführt.
        {
            try
            {
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                response.Close();
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }
        }

        private bool IsValidImageUrl(string url)
        {
            // Überprüfen ob der Link eine gültige URL ist
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return false;
            }

            // Überprüfen ob die URL erreichbar ist und ein Bild zurückgibt
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    return response.ContentType.ToLower().StartsWith("image/");
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
