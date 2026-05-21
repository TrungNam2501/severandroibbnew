using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BBKV2.BienTam;

namespace BBKV2
{
    [Activity(Label = "Form Chức năng")]
    public class MenuFrm : AppCompatActivity
    {
        private Button btnBack;
        private Button btnScan;
        private Button btnInlai;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.MenuForm);

            btnScan = FindViewById<Button>(Resource.Id.btnScan);
            btnInlai = FindViewById<Button>(Resource.Id.btnInlai);
            btnBack = FindViewById<Button>(Resource.Id.back);

            btnScan.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(ScanFrm));
                StartActivity(intent);
            };

            btnInlai.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(InlaiTemfrm));
                StartActivity(intent);
            };

            btnBack.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                //plVoid.closeApplication(this);
            };
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}