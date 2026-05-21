using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using BBKV2.BienTam;
using System.Threading.Tasks;

namespace BBKV2
{
    [Activity(WindowSoftInputMode = SoftInput.StateVisible, Label = "In Tem BB KV2", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private Spinner cbMay;
        private EditText empno;
        private TextView Err;
        private Button btnLogin;
        private Button btnExit;
        private LinearLayout layout;

        ServiceBBOK.ServiceBBintem web1 = new ServiceBBOK.ServiceBBintem();

        //Servicebb.ServiceBBintem web1 = new Servicebb.ServiceBBintem(); 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            cbMay = FindViewById<Spinner>(Resource.Id.cbMay);
            string[] items = web1.may();
            cbMay.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, items);
            Err = FindViewById<TextView>(Resource.Id.txtError);

            btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            empno = FindViewById<EditText>(Resource.Id.txtEmpno);
            layout = FindViewById<LinearLayout>(Resource.Id.layoutLN);
            empno.Text = "";
            Err.Text = "";
            empno.RequestFocus();

            btnLogin.Click += (sender, e) =>
            {
                if (empno.Text.Length == 6)
                {
                    try
                    {
                        string[] rs = web1.Login(empno.Text.Trim());
                        if (rs[0].ToString() != "")
                        {
                            Err.Text = "Sai mã số thẻ!! \r\n Vui lòng kiểm tra lại";
                            return;
                        }
                        else
                        {
                            Err.Text = rs[1].ToString() + " - " + rs[2].ToString();
                            BT.machno = cbMay.SelectedItem.ToString().Trim();
                            BT.empno = rs[3].ToString();
                            BT.depno = rs[2].ToString();
                            BT.name = rs[1].ToString();
                            if (BT.machno != "" && BT.depno != "" && BT.empno != "" && BT.name != "")
                            {
                                Intent intent = new Intent(this, typeof(MenuFrm));
                                StartActivity(intent);
                            }
                            else
                            {
                                FinishAndRemoveTask();
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        Err.Text = "Không kết nối được Server!\r\nVui lòng kiểm tra lại";
                        return;
                    }
                }
                else
                {
                    Err.Text = "Sai mã số thẻ!!\r\nVui lòng kiểm tra lại";
                }
            };

            btnExit = FindViewById<Button>(Resource.Id.btnThoat);
            btnExit.Click += (sender, e) =>
            {
                BT.depno = "";
                BT.empno = "";
                BT.machno = "";
                BT.name = "";
                FinishAndRemoveTask();
            };

            empno.KeyPress += async (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (empno.Text.Length == 6)
                {
                    var mDialog = new ProgressDialog(this);
                    mDialog.SetMessage("Vui lòng đợi...");
                    mDialog.SetCancelable(false);
                    mDialog.Show();
                    await Task.Run((() =>
                    {


                  
                    try
                    {
                        string[] rs = web1.Login(empno.Text.Trim());
                        if (rs[0].ToString() != "")
                        {
                            Err.Text = "Sai mã số thẻ!\r\nVui lòng kiểm tra lại";
                            return;
                        }
                        else
                        {
                                RunOnUiThread(() => {
                                    plVoid.HideKeyboard(this);
                                    Err.Text = rs[1].ToString() + " - " + rs[2].ToString();
                                    BT.machno = cbMay.SelectedItem.ToString().Trim();
                                    BT.empno = rs[3].ToString();
                                    BT.depno = rs[2].ToString();
                                    BT.name = rs[1].ToString();
                                });
                        }
                    }
                    catch (System.Exception)
                    {
                        Err.Text = "Không kết nối được Server!\r\nVui lòng kiểm tra lại!";
                        return;
                    }
                    }));
                    mDialog.Dismiss();

                }

            };


            layout.Click += (sender, e) =>
            {
                plVoid.HideKeyboard(this);
            };
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}