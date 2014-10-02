using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Budgeteer;
using Android.Hardware;

namespace BudgeteerAndroid
{
	[Activity (Label = "Budgeteer", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : Activity, TextureView.ISurfaceTextureListener, Camera.IAutoFocusCallback
	{

		private MainController mainController;

		public MainController MainController {
			get {
				return mainController;
			}
		}

		private Camera camera;
		private TextureView textureView;

		public MainActivity()
		{
			mainController = new MainController ();
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			textureView = new TextureView (this);
			textureView.SurfaceTextureListener = this;

			SetContentView (textureView);
		}

		public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int w, int h)
		{
			textureView.LayoutParameters = new FrameLayout.LayoutParams (w, h);

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
	}
		
}
