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
using Android.Views;
using Android.Graphics;
using Android.Views.InputMethods;

using static Android.Provider.Settings;
using Android.Util;

namespace BBKV2
{
    [Activity(WindowSoftInputMode = SoftInput.StateAlwaysHidden, Label = "Quét Tem")]


    public class ScanFrm : AppCompatActivity
    {
        private EditText txtWgt;
        private EditText txtRecipe;
        private Spinner cbMES;
        private EditText txtPalet;
        private Button btnReset;
        private Button btnIntem;
        private Button btnEnd;
        private LinearLayout layout;
        private Spinner cbPrinter;


        //Servicebb.ServiceBBintem web1 = new Servicebb.ServiceBBintem();
        ServiceBBOK.ServiceBBintem web1 = new ServiceBBOK.ServiceBBintem();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.ScanForm);
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);


            txtPalet = FindViewById<EditText>(Resource.Id.txtPalet);
            txtRecipe = FindViewById<EditText>(Resource.Id.txtRecipe);
            txtWgt = FindViewById<EditText>(Resource.Id.txtWgt);
            btnIntem = FindViewById<Button>(Resource.Id.btnIntem);
            btnEnd = FindViewById<Button>(Resource.Id.btnEnd);
            cbMES = FindViewById<Spinner>(Resource.Id.cbMES);
            txtPalet.ShowSoftInputOnFocus = false;
            txtWgt.ShowSoftInputOnFocus = false;
            txtRecipe.ShowSoftInputOnFocus = false;
            txtPalet.RequestFocus();

            RadioButton radioButton1 = FindViewById<RadioButton>(Resource.Id.candao);
            RadioButton radioButton2 = FindViewById<RadioButton>(Resource.Id.sanxuat);
            CheckBox cbOEM = FindViewById<CheckBox>(Resource.Id.cbOEM);
            txtPalet.ShowSoftInputOnFocus = false;
            txtWgt.ShowSoftInputOnFocus = false;

            DataTable dtMES = web1.Get_Mesid_list(BT.machno).Tables[0];
            if (dtMES.Rows.Count == 0)
            {
                string err = "Vui lòng tạo mes ";
                var answer = plVoid.Show(this, "Thông Báo", err); Console.WriteLine(answer);
                btnEnd.Enabled = false;
                btnIntem.Enabled = false;

                return;

            }
            List<string> mes = new List<string>();
            foreach (DataRow item in dtMES.Rows)
            {
                mes.Add(item["Plan_Id"].ToString().Trim() + " - " + item["Recipe_Code"].ToString().Trim());
            }
            cbMES.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mes.ToArray());
            txtPalet.Touch += (s, e) =>
            {
                e.Handled = false;
                ShowKeyboard(txtPalet);
            };

            txtWgt.Touch += (s, e) =>
            {
                e.Handled = false;
                ShowKeyboard(txtWgt);
            };


            cbMES.ItemSelected += (s, e) =>
            {
                RunOnUiThread(() => {

                    txtRecipe.Text = cbMES.SelectedItem.ToString().Substring(cbMES.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                    if (txtRecipe.Text.Trim().Length == 7 || txtRecipe.Text.Trim().Length == 8)
                    {

                        if (txtRecipe.Text.Substring(6, 1).Trim() == "9" || txtRecipe.Text.Substring(txtRecipe.Text.Trim().Length - 2, 2).Trim() == "RM")  //Update 22-11-2021  bỏ KT rub != "", chỉ cho phép keo -9 chọn sản xuất hay cán đảo --- Lợi
                        {
                            radioButton1.Visibility = ViewStates.Visible;
                            radioButton2.Visibility = ViewStates.Visible;

                            radioButton1.Checked = false;
                            radioButton2.Checked = false;
                        }
                        else
                        {
                            radioButton1.Visibility = ViewStates.Gone;
                            radioButton2.Visibility = ViewStates.Gone;

                        }
                    }
                    else
                    {
                        radioButton1.Visibility = ViewStates.Gone;
                        radioButton2.Visibility = ViewStates.Gone;
                    }
                    txtPalet.RequestFocus();
                });

                   
                
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
            txtPalet.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (txtPalet.Text.Length >= 6)
                {
                    txtPalet.Enabled = false;
                    txtWgt.RequestFocus();
                }
            };

            radioButton1.CheckedChange += (s, e) =>
            {
                if (radioButton1.Checked == true)
                {
                    radioButton2.Checked = false;


                }

            };

            radioButton2.CheckedChange += (s, e) =>
            {
                if (radioButton2.Checked == true)
                {
                    radioButton1.Checked = false;


                }

            };
           



            btnIntem.Click += async (sender, e) =>
            {
                string err = "";
                var mDialog = new ProgressDialog(this);
                mDialog.SetMessage("Đang In Tem...");
                mDialog.SetCancelable(false);
                mDialog.Show();

                await Task.Run((() =>
                {

                    if (txtPalet.Text == "")
                    {
                        err = "Vui lòng quét mã PALET!";
                        return;
                    }
                    string sMac_pda = GetMacAddress();

                    string candao1 = "";//(sanxuat.Checked == true) || (candao.Checked == false) ? "N" : "Y";
                    if (radioButton2.Checked == true)
                    {
                        candao1 = "N";
                    }
                    if (radioButton1.Checked == true)
                    {
                        candao1 = "Y";
                    }
                    if (radioButton1.Checked == false && radioButton2.Checked == false)
                    {
                        candao1 = "Y";
                    }
                    string OEM = cbOEM.Checked ? "OEM" : "";

                    if (txtWgt.Text == "")
                    {
                        err = "Vui lòng nhập trọng lượng KEO!";
                        return;
                    }
                    try
                    {
                        //string mayin = cbPrinter.SelectedItem.ToString().Substring(cbPrinter.SelectedItem.ToString().IndexOf('-') + 1).Trim();
                        //string mesID = cbMES.SelectedItem.ToString().Substring(0, cbMES.SelectedItem.ToString().IndexOf('-')).Trim();

                        string mayin = cbPrinter.SelectedItem.ToString();
                        string mesID = cbMES.SelectedItem.ToString().Substring(0, cbMES.SelectedItem.ToString().IndexOf('-')).Trim();
                        string recipe = txtRecipe.Text.Trim();
                        string weight = txtWgt.Text.Trim();
                        string pallet = txtPalet.Text.Trim();

                        string Result = web1.Print_BB(recipe, BT.machno, mayin, "", BT.empno, candao1.Trim(), weight, mesID, pallet, sMac_pda,OEM,BT.depno);
                        if (Result.ToString().Trim() == "")
                        {
                            RunOnUiThread(() =>
                            {
                                txtPalet.Text = "";
                                txtWgt.Text = "";
                                txtPalet.RequestFocus();
                                err = "In Tem Thành Công!";
                            });

                            return;
                        }
                        else
                        {
                            err = Result.ToString().Trim();
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        err = "Không kết nối được Server!";
                        return;
                    }
                }
                ));

                mDialog.Dismiss();
                var answer = plVoid.Show(this, "Thông Báo", err); Console.WriteLine(answer);
            };

           

            btnEnd.Click += (sender, e) =>
             {
                 AndroidX.AppCompat.App.AlertDialog.Builder b = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
                 b.SetTitle("Xác Nhận");
                 b.SetMessage("Bạn có chắc chắn kết thúc mẻ keo này?");
                 b.SetPositiveButton("Có", async (s, a) =>
                 {
                     try
                     {
                         string mesID = cbMES.SelectedItem.ToString().Substring(0, cbMES.SelectedItem.ToString().IndexOf('-')).Trim();
                         string recipe = txtRecipe.Text.Trim();
                         string result = web1.Finish(recipe, mesID, BT.machno);
                         if (result.ToString() == "")
                         {
                             DataTable dtMES = web1.Get_Mesid_list(BT.machno).Tables[0];
                             List<string> mes = new List<string>();
                            
                             foreach (DataRow item in dtMES.Rows)
                             {
                                 mes.Add(item["Plan_Id"].ToString().Trim() + " - " + item["Recipe_Code"].ToString().Trim());
                             }
                             cbMES.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, mes.ToArray());
                             txtPalet.Text = "";
                             txtWgt.Text = "";
                             var answer = await plVoid.Show(this, "Thông Báo", "Kết thúc mẻ thành công!"); Console.WriteLine(answer);
                             return;
                         }
                         else
                         {
                             var answer = await plVoid.Show(this, "Thông Báo", "Lỗi Rồi!"); Console.WriteLine(answer);
                             return;
                         }
                     }
                     catch (Exception ex)
                     {
                         var answer = await plVoid.Show(this, "Thông Báo", "Không kết nối được Server!"); Console.WriteLine(answer);
                         return;
                     }
                 });
                 b.SetNegativeButton("Không", (s, a) =>
                 {
                     b.Dispose();
                 });
                 AndroidX.AppCompat.App.AlertDialog ad = b.Create();
                 ad.Show();
             };


            btnReset = FindViewById<Button>(Resource.Id.btnReset);
            btnReset.Click += (s, e) =>
            {
                txtPalet.Enabled = true;
                txtWgt.Text = "";
                txtPalet.Text = "";
                txtPalet.RequestFocus();
            };

            layout = FindViewById<LinearLayout>(Resource.Id.layoutScan);
            layout.Click += (sender, e) =>
            {
                plVoid.HideKeyboard(this);
            };
        }
        private void ShowKeyboard(View view)
        {
            var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
            if (inputMethodManager != null)
            {
                inputMethodManager.ShowSoftInput(view, ShowFlags.Forced);
            }
            else
            {
                // Xử lý trường hợp inputMethodManager là null
                Log.Error("InputMethodManager", "Không thể khởi tạo InputMethodManager.");
            }
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public static string GetMacAddress()
        {
            try
            {
                System.Collections.IList all = Java.Util.Collections.List(Java.Net.NetworkInterface.NetworkInterfaces);
                foreach (Java.Net.NetworkInterface nif in all)
                {
                    if (nif.Name != "wlan0") continue;

                    byte[] macBytes = nif.GetHardwareAddress();
                    if (macBytes == null)
                    {
                        return "";
                    }

                    var res1 = new Java.Lang.StringBuilder();
                    foreach (byte b in macBytes)
                    {
                        //string ss = b.ToString();
                        string s = Java.Lang.Integer.ToHexString(b & 0xFF);
                        try
                        {
                            string ss = int.Parse(Java.Lang.Integer.ToHexString(b & 0xFF)).ToString("00");
                            res1.Append(ss + ":");
                        }
                        catch
                        {
                            res1.Append(s + ":");
                        }


                    }

                    if (res1.Length() > 0)
                    {
                        res1.DeleteCharAt(res1.Length() - 1);
                    }
                    return res1.ToString();
                }

            }
            catch (Java.Lang.Exception ex)
            {
            }
            return "02:00:00:00:00:00";
        }
    }
}