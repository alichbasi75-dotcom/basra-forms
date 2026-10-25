using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace BasraCenterApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // 1. Locate local index.html
                string localHtml = Path.Combine(baseDir, "index.html");
                if (!File.Exists(localHtml))
                {
                    string candidate = Path.Combine(baseDir, "public", "index.html");
                    if (File.Exists(candidate)) localHtml = candidate;
                }
                if (!File.Exists(localHtml))
                {
                    string candidate = Path.Combine(baseDir, "بوابة_المراكز.html");
                    if (File.Exists(candidate)) localHtml = candidate;
                }
                if (!File.Exists(localHtml))
                {
                    // Check workspace default path
                    string candidate = @"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system\index.html";
                    if (File.Exists(candidate)) localHtml = candidate;
                }

                // If local file exists, use it; otherwise open cloud portal
                string targetUrl = File.Exists(localHtml)
                    ? new Uri(localHtml).AbsoluteUri
                    : "https://basra-forms.vercel.app/";

                // 2. Find Microsoft Edge for clean app-mode window
                string[] edgePaths = new string[]
                {
                    @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                    @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\Edge\Application\msedge.exe")
                };

                string edgeExe = null;
                foreach (var ep in edgePaths)
                {
                    if (File.Exists(ep)) { edgeExe = ep; break; }
                }

                if (!string.IsNullOrEmpty(edgeExe))
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = edgeExe;
                    psi.Arguments = string.Format("--app=\"{0}\" --window-size=1280,850", targetUrl);
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                }
                else
                {
                    Process.Start(new ProcessStartInfo(targetUrl) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تشغيل برنامج المراكز:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
