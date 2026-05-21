using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BBKV2.BienTam
{
    public class plVoid
    {
        public static void closeApplication(Activity activity)
        {
            activity.FinishAndRemoveTask();
        }
       
        public static void HideKeyboard(Activity context)
        {
            var imm = (InputMethodManager)context.GetSystemService(Context.InputMethodService);
            int sdk = (int)Build.VERSION.SdkInt;

            if (sdk < 11)
            {
                imm.HideSoftInputFromWindow(context.Window.CurrentFocus.WindowToken, 0);
            }
            else
            {
                imm.HideSoftInputFromWindow(context.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
            }
        }

        public enum MessageBoxResult
        {
            Negative, Ignore, Cancel, Closed
        };

        private static MessageBoxResult yesNoDialogResult;

        public static async Task<MessageBoxResult> Show(Context context, String title, String message)
        {
            yesNoDialogResult = MessageBoxResult.Closed;
            var alert = new AlertDialog.Builder(context)
               .SetTitle(title).SetMessage(message)
               .SetCancelable(false)
               .SetIcon(Android.Resource.Drawable.IcDialogAlert);

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);


            alert.SetNegativeButton("OK", (senderAlert, args) =>
            {
                yesNoDialogResult = MessageBoxResult.Negative;
                waitHandle.Set();
            });

            alert.Show();
            await Task.Run(() => waitHandle.WaitOne());
            return yesNoDialogResult;
        }
    }
}