using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;

namespace ConsoleMemoryLeak
{
    public class NotificationManager
    {
        public void SendSusNotify(Process process, double speed)
        {
            var builder = new ToastContentBuilder();

            AppendProcessIcon(builder, process);
            builder.AddText("Suspicion of Memory Leak");
            builder.AddText(process.ProcessName + " leak speed " + speed + " MB/sec");
            builder.Show();
        }
        public void SendKillNotify(Process process)
        {
            var builder = new ToastContentBuilder();

            AppendProcessIcon(builder, process);
            builder.AddText("Ran out memory");
            builder.AddText(process.ProcessName + " has been killed");
            builder.Show();
        }

        private void AppendProcessIcon(ToastContentBuilder builder, Process process)
        {
            string filepath = Environment.CurrentDirectory + "/temp.jpg";
            var icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
            icon.ToBitmap().Save(filepath);

            builder.AddAppLogoOverride(new Uri(filepath));
        }
    }
}
