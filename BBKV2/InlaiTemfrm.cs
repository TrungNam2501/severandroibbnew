using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Data;
using BBKV2.BienTam;
using System.Threading.Tasks;
using Android.Content;

namespace BBKV2
{
    [Activity(Label = "In Lại Tem")]
    public class InlaiTemfrm : AppCompatActivity
    {
        private Button btnInLai;
        private Button btnBack;
        private Spinner cbMes;
        private Spinner cbPrinter;
        private EditText txtRecipe;
        private EditText txtWgt;
        private Spinner cbBarcode;

        //Servicebb.ServiceBBintem web1 = new Servicebb.ServiceBBintem();
        ServiceBBOK.ServiceBBintem web1 = new ServiceBBOK.ServiceBBintem();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.InlaiForm);


            btnInLai = FindViewById<Button>(Resource.Id.btnInlai);
            btnBack = FindViewById<Button>(Resource.Id.btnQuaylai1);
            cbMes = FindViewById<Spinner>(Resource.Id.cbMES);
            cbPrinter = FindViewById<Spinner>(Resource.Id.cbPrinter);
            cbBarcode = FindViewById<Spinner>(Resource.Id.cbBarcode);
            txtRecipe = FindViewById<EditText>(Resource.Id.txtRecipe);
            txtWgt = FindViewById<EditText>(Resource.Id.txtWgt);

            DataTable dtMes = web1.Get_Mesid_list_Inlai(BT.machno).Tables[0];

            List<string> MES = new List<string>();
            foreach (DataRow item in dtMes.Rows)
            {
                MES.Add(item["Plan_Id"].ToString().Trim() + " - " + item["Recipe_Code"].ToString().Trim());
            }

            cbMes.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, MES.ToArray());
            cbMes.ItemSelected += (sender, e) =>
            {
                txtRecipe.Text = cbMes.SelectedItem.ToString().Substring(cbMes.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                string mes = cbMes.SelectedItem.ToString().Substring(0, cbMes.SelectedItem.ToString().IndexOf('-')).Trim();
                DataTable dtWgt_Barcode = web1.Get_Barcode(mes, BT.machno).Tables[0];
                List<string> barcode_wgt = new List<string>();
                foreach (DataRow item in dtWgt_Barcode.Rows)
                {
                    barcode_wgt.Add(item["barcode"].ToString().Trim() + " - " + item["weight"].ToString().Trim() + "KG");
                }
                cbBarcode.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, barcode_wgt.ToArray());
            };

            string[] result = web1.Printer();
            List<string> printer = new List<string>();
            foreach (string item in result)
            {
                if (item.Trim() != "")
                {
                    printer.Add(item.Trim());
                }
            }

            cbPrinter = FindViewById<Spinner>(Resource.Id.cbPrinter);
            cbPrinter.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, printer.ToArray());

            btnBack.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(MenuFrm));
                StartActivity(intent);
            };
            btnInLai.Click += async (sender, e) =>
            {
                string err = "";
                var mDialog = new ProgressDialog(this);
                mDialog.SetMessage("Đang In Tem...");
                mDialog.SetCancelable(false);
                mDialog.Show();

                await Task.Run((() =>
                {
                    try
                    {
                        string printer = cbPrinter.SelectedItem.ToString().Substring(cbPrinter.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                        string recipe = cbMes.SelectedItem.ToString().Substring(cbMes.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                        string mes = cbMes.SelectedItem.ToString().Substring(0, cbMes.SelectedItem.ToString().IndexOf('-')).Trim();
                        string wgt = cbBarcode.SelectedItem.ToString().Substring(cbBarcode.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                        wgt = wgt.Remove(wgt.Length - 2);
                        string barcode = cbBarcode.SelectedItem.ToString().Substring(0, cbBarcode.SelectedItem.ToString().IndexOf('-')).Trim();

                        string result = web1.Print_BB_Again(BT.name, BT.machno, barcode, mes, wgt, printer, recipe);
                        if (result.ToString() != "")
                        {
                            err = "Lỗi không in lại tem được!";
                            return;
                        }
                        else
                        {
                            err = "In lại tem thành công!";
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        err = "Không có mã tem của MES này";
                        return;
                    }
                }
                ));

                mDialog.Dismiss();
                var answer = plVoid.Show(this, "Thông Báo", err); Console.WriteLine(answer);
            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}