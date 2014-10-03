using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Budgeteer;
using Windows.Storage.Pickers;
using Windows.Media.Capture;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.ApplicationModel.Activation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace Budgeteer_WP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, Windows.Phone.Common.IFileOpenPickerContinuable
    {
        public static MainPage Current{get; private set;}
        MediaCapture captureManager;
        public bool IsCameraPreviewing { get; private set; }
        public bool IsCameraRecording { get; private set; }
        public bool IsCameraInitialized {get; private set;}

        IRandomAccessStream currentImageStream;
        public MainPage()
        {
            MainPage.Current = this;
            this.InitializeComponent();
            
            this.NavigationCacheMode = NavigationCacheMode.Required;
            IsCameraInitialized = false;

            Ocr.ReceivedOcrData += Ocr_ReceivedOcrData;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            await StartCamera();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (IsCameraInitialized) await StopCamera(captureManager);
            base.OnNavigatedFrom(e);
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (IsCameraInitialized) await StopCamera(captureManager);
            e.Cancel = true;
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Opens a file picker in which the user can choose an image to use for ocr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseImageButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.ContinuationData.Add(ContinuationAction.ContinuationAction.ToString(), ContinuationAction.ScanImageFromLibrary.ToString());

            picker.PickSingleFileAndContinue();
        }

        /// <summary>
        /// Is run after FileOpenPicker returns after the user has picked an image
        /// </summary>
        /// <param name="file"></param>
        private async void ContinueImageChosen(StorageFile file)
        {
            //Deactivate the camera
            if (IsCameraInitialized) await StopCamera(captureManager);

            //Display the image
            await ShowPreview(await file.OpenAsync(FileAccessMode.Read));
        }


        /// <summary>
        /// Starts the camera
        /// </summary>
        /// <returns></returns>
        private async Task StartCamera()
        {
            if (!IsCameraInitialized)
            {
                //Choose rear camera
                var cameras = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
                MediaCaptureInitializationSettings settings;
                if (cameras.Count == 1)
                {
                    settings = new MediaCaptureInitializationSettings() { VideoDeviceId = cameras[0].Id };
                }
                else
                {
                    settings = new MediaCaptureInitializationSettings() { VideoDeviceId = cameras[1].Id };//front: 0, back: 1
                }

                //Start camera capturing instance
                captureManager = new MediaCapture();
                await captureManager.InitializeAsync(settings);
                IsCameraInitialized = true;

                //Start preview
                ScanPreviewImage.Source = captureManager;
                ScanPreviewImage.Stretch = Stretch.UniformToFill;
                await captureManager.StartPreviewAsync();
                IsCameraPreviewing = true;

                //Set rotation
                captureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
                captureManager.SetRecordRotation(VideoRotation.Clockwise90Degrees);

                //Enable UI camera button
                CameraButton.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Shuts down the camera sensor and disposes of associated resources. This MUST be run before the app exits, or the camera will be unusable until the next system restart
        /// </summary>
        /// <param name="capture">The MediaCapture instance currently used</param>
        /// <returns></returns>
        public async Task StopCamera(MediaCapture capture)
        {
            //TODO: Call this method every time the app loses focus or is terminated
            if (capture != null)
            {
                if (IsCameraRecording)
                {
                    await capture.StopRecordAsync();
                    IsCameraRecording = false;
                }
                if (IsCameraPreviewing)
                {
                    await capture.StopPreviewAsync();
                    ScanPreviewImage.Source = null;
                    IsCameraPreviewing = false;
                }
                capture.Dispose();
                capture = null;
                IsCameraInitialized = false;
                CameraButton.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// Is run when the on-screen camera button is tapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CaptureImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsCameraInitialized)
            {
                
                //Get Image
                IRandomAccessStream imageStream = await CaptureImage();
                await RotateImage(imageStream, BitmapRotation.Clockwise90Degrees);//Needs to be rotated 90 degrees when in portrait mode

                //Show image
                await ShowPreview(imageStream);

                //Turn off camera
                await StopCamera(captureManager);
            }
        }

        private async Task ShowPreview(IRandomAccessStream imageStream)
        {
            BitmapImage bmp = new BitmapImage();
            currentImageStream = imageStream;
            await bmp.SetSourceAsync(imageStream);
            ScanImage.Source = bmp;
            AcceptPictureButton.Visibility = Visibility.Visible;
            RedoPictureButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Captures a JPEG image from the camera
        /// </summary>
        /// <returns>JPEG stream</returns>
        private async Task<IRandomAccessStream> CaptureImage()
        {
            ImageEncodingProperties format = ImageEncodingProperties.CreateJpeg();
            IRandomAccessStream imageStream = new InMemoryRandomAccessStream();
            await captureManager.CapturePhotoToStreamAsync(format, imageStream);
            return imageStream;
        }

        /// <summary>
        /// Rotates an image
        /// </summary>
        /// <param name="imageStream">Stream to be rotated</param>
        /// <param name="rotation">Rotation to be applied</param>
        /// <returns>Rotated image</returns>
        private async Task RotateImage(IRandomAccessStream imageStream, BitmapRotation rotation)
        {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(imageStream);
            BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(imageStream, decoder);

            encoder.BitmapTransform.Rotation = rotation;

            await encoder.FlushAsync();
        }

        /// <summary>
        /// Is run when the user taps the RedoPictureButton
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RedoPictureButton_Click(object sender, RoutedEventArgs e)
        {
            AcceptPictureButton.Visibility = Visibility.Collapsed;
            RedoPictureButton.Visibility = Visibility.Collapsed;
            ScanImage.Source = null;
            await StartCamera();
        }

        /// <summary>
        /// Is called by App.xaml.cs in OnActivated if the current session is a continuation after a FileOpenPicker was used
        /// </summary>
        /// <param name="args"></param>
        public void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args)
        {
            if (args.ContinuationData.ContainsKey(ContinuationAction.ContinuationAction.ToString()))
            {
                ContinuationAction action;
                if (Enum.TryParse<ContinuationAction>((string)args.ContinuationData[ContinuationAction.ContinuationAction.ToString()], out action))
                {
                    switch (action)
                    {
                        case ContinuationAction.ScanImageFromLibrary:
                        {
                            if (args != null && args.Files.Count > 0)
                            {
                                ContinueImageChosen(args.Files[0]);
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void AcceptPictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentImageStream != null)
            {
                Ocr.BeginGetOcrRequest(currentImageStream.AsStream());
            }
        }

        private void Ocr_ReceivedOcrData(object sender, string data)
        {
            //TODO: Extract price and name information from ocr data
        }

    }
}

