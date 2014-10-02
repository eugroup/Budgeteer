using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Budgeteer;
using Android.Hardware;
using System.IO;

namespace BudgeteerAndroid
{
	[Activity (Label = "Budgeteer", MainLauncher = true, Icon = "@drawable/icon",
		ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, Theme="@android:style/Theme.Holo.Light")]
	public class MainActivity : Activity, TextureView.ISurfaceTextureListener, Camera.IAutoFocusCallback,
		Camera.IPictureCallback, Camera.IShutterCallback
	{
		/// <summary>
		/// The main controller from the core part.
		/// </summary>
		private MainController mainController;
		public MainController MainController {
			get {
				return mainController;
			}
		}
			
		private Camera camera;

		/// <summary>
		/// The texture view for previewing the camera image.
		/// </summary>
		private TextureView textureView;

		public MainActivity()
		{
			mainController = new MainController ();
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			textureView = (TextureView)FindViewById (Resource.Id.cameraPreview);
			textureView.SurfaceTextureListener = this;

			Button snapButton = (Button)FindViewById (Resource.Id.buttonTakePhoto);
			snapButton.Click += (object sender, EventArgs e) => {
				camera.TakePicture(this, null, this);
			};
		}
			
		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h)
		{

			//textureView.LayoutParameters = new FrameLayout.LayoutParams (w, h);

			camera = Camera.Open ();

			Camera.Parameters p = camera.GetParameters ();
			p.PictureFormat = Android.Graphics.ImageFormatType.Jpeg;
			Camera.Size previewSize = p.PreviewSize;
			camera.SetParameters (p);
			camera.SetDisplayOrientation (90);

			Android.Graphics.Matrix m = new Android.Graphics.Matrix ();
			m.SetScale(1,  (float)(previewSize.Width) / h);
			textureView.SetTransform (m);

			camera.SetPreviewTexture (surface);
			camera.StartPreview ();
			camera.AutoFocus (this);
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
		{
			camera.StopPreview ();
			camera.Release ();

			return true;
		}
			
		public void OnSurfaceTextureSizeChanged (Android.Graphics.SurfaceTexture surface, int width, int height)
		{

		}

		public void OnSurfaceTextureUpdated (Android.Graphics.SurfaceTexture surface)
		{

		}

		public void OnAutoFocus (bool success, Camera camera)
		{
			camera.AutoFocus (this);
		}
			
		public void OnPictureTaken (byte[] data, Camera camera)
		{
			Toast.MakeText (this, "Photo taken. size: " + data.Length, ToastLength.Short).Show();
			camera.StartPreview ();

			Stream imageStream = new MemoryStream(data);

			mainController.OnPictureTaken (imageStream);
		}

		public void OnShutter ()
		{

		}
	}
		
}
