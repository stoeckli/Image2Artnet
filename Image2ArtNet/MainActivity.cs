using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.OS;
using Android.Content.PM;
using Android.Preferences;

namespace Image2ArtNet
{
    [Activity(Label = "12x12", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {
        UdpClient client;
        int xdim = 12;
        int ydim = 12;
        Switch serpentine;
        Switch fliphor;
        Switch flipver;
        EditText IP;
        EditText xpoints;
        EditText ypoints;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            Button btnSelect = FindViewById<Button>(Resource.Id.btnSelect);
            Button btnSend = FindViewById<Button>(Resource.Id.btnSend);
            ImageView image = FindViewById<ImageView>(Resource.Id.imgPicture);
            Button btnStore = FindViewById<Button>(Resource.Id.btnStore);
            Button btnRecall = FindViewById<Button>(Resource.Id.btnRecall);
            serpentine = FindViewById<Switch>(Resource.Id.swiSerpentine);
            fliphor = FindViewById<Switch>(Resource.Id.swiFlipHor);
            flipver = FindViewById<Switch>(Resource.Id.swiFlipVer);
            IP = FindViewById<EditText>(Resource.Id.txtIP);
            xpoints = FindViewById<EditText>(Resource.Id.txtX);
            ypoints = FindViewById<EditText>(Resource.Id.txtY);
            xdim = int.Parse(xpoints.Text);
            ydim = int.Parse(ypoints.Text);

            Spinner spinner = FindViewById<Spinner>(Resource.Id.spnMemory);

            // Load the spinner values from the text resouce
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.number_array, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            // Restore preferences
            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
            {

                serpentine.Checked = prefs.GetBoolean("serpentine", true);
                flipver.Checked = prefs.GetBoolean("flipver", false);
                fliphor.Checked = prefs.GetBoolean("fliphor", false);
                IP.Text = prefs.GetString("IP", "2.0.0.5");
                xpoints.Text = prefs.GetString("xpoints", "12");
                ypoints.Text = prefs.GetString("ypoints", "12");

            }

            // Shows the image selection dialog
            btnSelect.Click += delegate
            {

                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                    Intent.CreateChooser(imageIntent, "Select image"), 0);
                
            };

            // Send the image data to the display
            btnSend.Click += delegate
            {

                image.BuildDrawingCache(true);
                Bitmap bmp = Bitmap.CreateBitmap(image.GetDrawingCache(true));

                byte[] buffer = new byte[512];
                byte[] pix;

                pix = BitConverter.GetBytes(bmp.GetPixel(1, 1));

                image.BuildDrawingCache(false);

                int xmax = image.Width;
                int ymax = image.Height;

                int pos;
                int x;
                int y;

                // imperical gamma values
                byte[] gamma = {0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,
                                0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  1,  1,  1,  1,
                                1,  1,  1,  1,  1,  1,  1,  1,  1,  2,  2,  2,  2,  2,  2,  2,
                                2,  3,  3,  3,  3,  3,  3,  3,  4,  4,  4,  4,  4,  5,  5,  5,
                                5,  6,  6,  6,  6,  7,  7,  7,  7,  8,  8,  8,  9,  9,  9, 10,
                                10, 10, 11, 11, 11, 12, 12, 13, 13, 13, 14, 14, 15, 15, 16, 16,
                                17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 22, 23, 24, 24, 25,
                                25, 26, 27, 27, 28, 29, 29, 30, 31, 32, 32, 33, 34, 35, 35, 36,
                                37, 38, 39, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 50,
                                51, 52, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 66, 67, 68,
                                69, 70, 72, 73, 74, 75, 77, 78, 79, 81, 82, 83, 85, 86, 87, 89,
                                90, 92, 93, 95, 96, 98, 99,101,102,104,105,107,109,110,112,114,
                                115,117,119,120,122,124,126,127,129,131,133,135,137,138,140,142,
                                144,146,148,150,152,154,156,158,160,162,164,167,169,171,173,175,
                                177,180,182,184,186,189,191,193,196,198,200,203,205,208,210,213,
                                215,218,220,223,225,228,231,233,236,239,241,244,247,249,252,255 };
               


                for (int yn = 0; yn < ydim; yn++)
                {
                    for (int xn = 0; xn < xdim; xn++)
                    {
                        pix = BitConverter.GetBytes(bmp.GetPixel(1 + xn * (xmax / xdim), 1 + (ydim - yn - 1) * (ymax / ydim)));

                        if (fliphor.Checked)
                        {
                            x = xdim - 1 - xn;
                        }
                        else
                        {
                            x = xn;
                        }
                        if (flipver.Checked)
                        {
                            y = ydim - 1 - yn;
                        }
                        else
                        {
                            y = yn;
                        }

                        if (serpentine.Checked == true)
                        {
                            if (((int)(y / 2.0) == (y / 2.0)) & (serpentine.Checked == true))
                            {
                                pos = 3 * (x + (xdim * y));
                            }
                            else
                            {
                                pos = 3 * ((xdim * (y + 1)) - 1 - x);
                            }
                        }
                        else
                        {
                            pos = 3 * (x + (xdim * y));
                        }


                        buffer[pos] = gamma[pix[2]]; //red
                        buffer[pos + 1] = gamma[pix[1]]; //green
                        buffer[pos + 2] = gamma[pix[0]]; //blue
                    }
                }


                client = new UdpClient();
                try
                {
                    client.Send(buffer, buffer.Length, IP.Text, 6454);
                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            };

            // store data to display memory
            btnStore.Click += delegate
            {

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string value = client.DownloadString("http://" + IP.Text + "/?file=save&nr=" + spinner.SelectedItemPosition.ToString());                        
                    }
                    catch
                    {

                    }


                }

            };

            // recall data prom display memory
            btnRecall.Click += delegate
            {

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string value = client.DownloadString("http://" + IP.Text + "/?file=load&nr=" + spinner.SelectedItemPosition.ToString());
                    }
                    catch
                    {

                    }

                }

            };

        }

        // image selected, reformat for display
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var imageView = FindViewById<ImageView>(Resource.Id.imgPicture);
                EditText xpoints = FindViewById<EditText>(Resource.Id.txtX);
                EditText ypoints = FindViewById<EditText>(Resource.Id.txtY);
                xdim = int.Parse(xpoints.Text);
                ydim = int.Parse(ypoints.Text);
                int y = imageView.Height;
                int x = imageView.Width;

                using (var bmp = Android.Provider.MediaStore.Images.Media.GetBitmap(ContentResolver, data.Data))
                {
                    using (var bmpScaled = Bitmap.CreateScaledBitmap(bmp, xdim, ydim, false))
                    {
                        using (var bmpScaled12 = Bitmap.CreateScaledBitmap(bmpScaled, x, y, false))
                        {
                            imageView.SetImageBitmap(bmpScaled12);
                        }
                    }

                }

                imageView.BuildDrawingCache(true);
            }
        }

        // save preferences
        protected override void OnPause()
        {
            base.OnPause(); 

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("serpentine", serpentine.Checked);
            editor.PutBoolean("flipver", flipver.Checked);
            editor.PutBoolean("fliphor", fliphor.Checked);
            editor.PutString("IP", IP.Text);
            editor.PutString("xpoints", xpoints.Text);
            editor.PutString("ypoints", ypoints.Text);
            editor.Commit();  
            editor.Apply();  

        }
    }
}

