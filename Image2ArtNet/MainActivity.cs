using System;
using System.Net;
using System.Net.Sockets;
//using System.Drawing;
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
        int count = 0;
        MemoryStream ms;
        UdpClient client;
        NetworkStream ns;
        BinaryWriter br;
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
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);
            Button button1 = FindViewById<Button>(Resource.Id.button1);
            ImageView image = FindViewById<ImageView>(Resource.Id.imageView1);
            Button button2 = FindViewById<Button>(Resource.Id.button2);
            Button button3 = FindViewById<Button>(Resource.Id.button3);
            serpentine = FindViewById<Switch>(Resource.Id.switch1);
            fliphor = FindViewById<Switch>(Resource.Id.switch2);
            flipver = FindViewById<Switch>(Resource.Id.switch3);
            IP = FindViewById<EditText>(Resource.Id.editText1);
            xpoints = FindViewById<EditText>(Resource.Id.editText2);
            ypoints = FindViewById<EditText>(Resource.Id.editText3);
            xdim = int.Parse(xpoints.Text);
            ydim = int.Parse(ypoints.Text);



            Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner1);

            // spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.number_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;

            using (ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context))
            {

                serpentine.Checked = prefs.GetBoolean("serpentine", true);
                flipver.Checked = prefs.GetBoolean("flipver", false);
                fliphor.Checked = prefs.GetBoolean("fliphor", false);
                IP.Text = prefs.GetString("IP", "2.0.0.5");
                xpoints.Text = prefs.GetString("xpoints", "12");
                ypoints.Text = prefs.GetString("ypoints", "12");

            }
            


            button.Click += delegate
            {

                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                    Intent.CreateChooser(imageIntent, "Select image"), 0);



            };
            button1.Click += delegate
           {
               count++;
               //image.SetImageResource(Resource.Drawable.testpattern);
               image.BuildDrawingCache(true);
               Bitmap bmp = Bitmap.CreateBitmap(image.GetDrawingCache(true));

               byte[] buffer = new byte[512];
               byte[] pix;

               //System.Buffer.BlockCopy(artnet.ToCharArray(), 0, buffer, 0, artnet.Length);

               pix = BitConverter.GetBytes(bmp.GetPixel(1, 1));

               image.BuildDrawingCache(false);
               //buffer[100] = pix[0];
               buffer[count] = 255;
               //buffer[30] = 255;
               count++;
               //image.SetImageResource(Resource.Drawable.Icon);
               int xmax = image.Width;
               int ymax = image.Height;

               int pos;
               int x;
               int y;


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
                      pix = BitConverter.GetBytes(bmp.GetPixel(1 + xn*(xmax/xdim), 1+(ydim-yn-1)*(ymax/ydim)));

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

            button2.Click += delegate
            {

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        // Download data.
                        string value = client.DownloadString("http://" + IP.Text + "/?file=save&nr=" + spinner.SelectedItemPosition.ToString());
                        
                    }
                    catch
                    {

                    }


                }

            };


            button3.Click += delegate
            {

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        // Download data.
                        string value = client.DownloadString("http://" + IP.Text + "/?file=load&nr=" + spinner.SelectedItemPosition.ToString());

                    }
                    catch
                    {

                    }


                }

            };

        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                var imageView = FindViewById<ImageView>(Resource.Id.imageView1);
                EditText xpoints = FindViewById<EditText>(Resource.Id.editText2);
                EditText ypoints = FindViewById<EditText>(Resource.Id.editText3);
                xdim = int.Parse(xpoints.Text);
                ydim = int.Parse(ypoints.Text);
                int y = imageView.Height;
                int x = imageView.Width;

                //imageView.SetImageURI(data.Data);
                using (var bmp = Android.Provider.MediaStore.Images.Media.GetBitmap(ContentResolver, data.Data))
                {
                    using (var bmpScaled = Bitmap.CreateScaledBitmap(bmp, xdim, ydim, false))
                    {
                        using (var bmpScaled12 = Bitmap.CreateScaledBitmap(bmpScaled, x, y, false))
                        {
                            imageView.SetImageBitmap(bmpScaled12);
                        }
                    }


                    //Bitmap bmp = Android.Provider.MediaStore.Images.Media.GetBitmap(ContentResolver, data.Data);


                }
                imageView.BuildDrawingCache(true);
            }
        }


        protected override void OnPause()
        {
            base.OnPause(); // Always call the superclass first

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Android.App.Application.Context);
            ISharedPreferencesEditor editor = prefs.Edit();
            editor.PutBoolean("serpentine", serpentine.Checked);
            editor.PutBoolean("flipver", flipver.Checked);
            editor.PutBoolean("fliphor", fliphor.Checked);
            editor.PutString("IP", IP.Text);
            editor.PutString("xpoints", xpoints.Text);
            editor.PutString("ypoints", ypoints.Text);
            // editor.Commit();    // applies changes synchronously on older APIs
            editor.Apply();        // applies changes asynchronously on newer APIs

        }



    }
}

