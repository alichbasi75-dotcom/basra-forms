using System;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BasraUpdater
{
    public class MainForm : Form
    {
        // Controls
        private Label lblTitle;
        private Label lblSubTitle;

        // Cloud Version Bar
        private Panel pnlCloud;
        private Label lblCurrentCloudVer;
        private Label lblCloudVerVal;
        private Button btnCheckCloud;

        // Program Choice GroupBox
        private GroupBox grpTarget;
        private RadioButton rbCenters;
        private RadioButton rbAdmin;
        private Label lblTargetNotice;

        // File Selection GroupBox
        private GroupBox grpFile;
        private Label lblFilePath;
        private TextBox txtFilePath;
        private Button btnBrowseFile;
        private Panel pnlDropZone;
        private Label lblDropHint;
        private Label lblFileValidation;

        // Version GroupBox
        private GroupBox grpVer;
        private Label lblNewVer;
        private TextBox txtNewVer;
        private Label lblVerHint;
        private Label lblVersionValidation;

        // Publish Actions
        private Button btnPublish;
        private Button btnOpenFolder;

        // Rollback GroupBox (Two Previous Versions)
        private GroupBox grpRollback;
        private Button btnRollback1;
        private Button btnRollback2;
        private Label lblRollbackHint;

        private ProgressBar progressBar;
        private Label lblStatus;

        // Independent Version Tracking for Each Program
        private string cloudCentersVersion = "2.0.0";
        private string cloudAdminVersion = "2.0.8";

        private string CLOUD_CENTERS_URL = "https://raw.githubusercontent.com/alichbasi75-dotcom/basra-forms/main/version-centers.json";
        private string CLOUD_ADMIN_URL = "https://raw.githubusercontent.com/alichbasi75-dotcom/basra-forms/main/version-admin.json";
        private string CLOUD_CENTERS_BACKUP = "https://alichbasi75-dotcom.github.io/basra-forms/version-centers.json";
        private string CLOUD_ADMIN_BACKUP = "https://alichbasi75-dotcom.github.io/basra-forms/version-admin.json";
        private string CLOUD_CENTERS_VERCEL = "https://basra-forms.vercel.app/version-centers.json";
        private string CLOUD_ADMIN_VERCEL = "https://basra-forms.vercel.app/version-admin.json";

        private string workspaceDir = @"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system";
        private string desktopDir = @"C:\Users\ali\Desktop\منظومة_استمارات_مكتب_انتخابات_البصرة";
        private string selectedFilePath = "";

        public MainForm()
        {
            InitializeComponent();
            FetchCloudVersion();
        }

        private void InitializeComponent()
        {
            this.Text = "أداة تحديث ورفع منظومة استمارات البصرة (المراكز والمسؤول بشكل منفصل)";
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            this.Size = new Size(670, 845);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.AllowDrop = true;
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;

            // 1. Header
            lblTitle = new Label();
            lblTitle.Text = "منظومة استمارات البصرة — أداة التحديث السحابي المنفصل";
            lblTitle.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(20, 12);
            lblTitle.Size = new Size(610, 26);
            this.Controls.Add(lblTitle);

            lblSubTitle = new Label();
            lblSubTitle.Text = "تحديث واجهة المراكز (index.html) أو واجهة المسؤول (admin.html) بملفات إصدار مفصولة تماماً";
            lblSubTitle.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblSubTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubTitle.Location = new Point(20, 38);
            lblSubTitle.Size = new Size(610, 20);
            this.Controls.Add(lblSubTitle);

            // 2. Cloud Version Bar
            pnlCloud = new Panel();
            pnlCloud.Location = new Point(20, 62);
            pnlCloud.Size = new Size(610, 48);
            pnlCloud.BackColor = Color.FromArgb(241, 245, 249);
            pnlCloud.BorderStyle = BorderStyle.FixedSingle;

            lblCurrentCloudVer = new Label();
            lblCurrentCloudVer.Text = "إصدار بوابة المراكز الحالي على المنصة:";
            lblCurrentCloudVer.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblCurrentCloudVer.Location = new Point(310, 12);
            lblCurrentCloudVer.Size = new Size(280, 24);
            pnlCloud.Controls.Add(lblCurrentCloudVer);

            lblCloudVerVal = new Label();
            lblCloudVerVal.Text = "جاري الفحص...";
            lblCloudVerVal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCloudVerVal.ForeColor = Color.FromArgb(2, 132, 199);
            lblCloudVerVal.Location = new Point(100, 11);
            lblCloudVerVal.Size = new Size(205, 24);
            pnlCloud.Controls.Add(lblCloudVerVal);

            btnCheckCloud = new Button();
            btnCheckCloud.Text = "تحديث 🔄";
            btnCheckCloud.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnCheckCloud.Location = new Point(10, 8);
            btnCheckCloud.Size = new Size(80, 28);
            btnCheckCloud.BackColor = Color.White;
            btnCheckCloud.Cursor = Cursors.Hand;
            btnCheckCloud.Click += (s, e) => FetchCloudVersion();
            pnlCloud.Controls.Add(btnCheckCloud);

            this.Controls.Add(pnlCloud);

            // 3. Step 1: Program Choice (Strictly Separate: Centers OR Admin)
            grpTarget = new GroupBox();
            grpTarget.Text = "1. حدد البرنامج المراد تحديثه حصراً (كل برنامج له ملف إصدار مستقل):";
            grpTarget.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpTarget.ForeColor = Color.FromArgb(30, 41, 59);
            grpTarget.Location = new Point(20, 118);
            grpTarget.Size = new Size(610, 115);
            grpTarget.BackColor = Color.White;

            rbCenters = new RadioButton();
            rbCenters.Text = "تحديث برنامج مراكز التسجيل فقط (بوابة المراكز الـ 76 — ملف index.html حصراً)";
            rbCenters.Location = new Point(20, 25);
            rbCenters.Size = new Size(570, 24);
            rbCenters.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            rbCenters.ForeColor = Color.FromArgb(3, 105, 161);
            rbCenters.Checked = true;
            rbCenters.CheckedChanged += (s, e) => OnTargetChanged();
            grpTarget.Controls.Add(rbCenters);

            rbAdmin = new RadioButton();
            rbAdmin.Text = "تحديث برنامج المسؤول فقط (لوحة تحكم الإدارة السحابية — ملف admin.html حصراً)";
            rbAdmin.Location = new Point(20, 52);
            rbAdmin.Size = new Size(570, 24);
            rbAdmin.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            rbAdmin.ForeColor = Color.FromArgb(180, 83, 9);
            rbAdmin.CheckedChanged += (s, e) => OnTargetChanged();
            grpTarget.Controls.Add(rbAdmin);

            lblTargetNotice = new Label();
            lblTargetNotice.Text = "⚠️ إشعار مهم: تحديث أحد البرنامجين لا يغير إصدار البرنامج الآخر إطلاقاً، وتحديث الاستمارات يتم من لوحة المسؤول أونلاين.";
            lblTargetNotice.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblTargetNotice.ForeColor = Color.FromArgb(180, 83, 9);
            lblTargetNotice.Location = new Point(20, 80);
            lblTargetNotice.Size = new Size(570, 28);
            grpTarget.Controls.Add(lblTargetNotice);

            this.Controls.Add(grpTarget);

            // 4. Step 2: File Selection & Matching Validation
            grpFile = new GroupBox();
            grpFile.Text = "2. اختيار الملف المطلوب وشرط المطابقة الإلزامي:";
            grpFile.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpFile.ForeColor = Color.FromArgb(30, 41, 59);
            grpFile.Location = new Point(20, 240);
            grpFile.Size = new Size(610, 210);
            grpFile.BackColor = Color.White;

            lblFilePath = new Label();
            lblFilePath.Text = "مسار الملف:";
            lblFilePath.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblFilePath.Location = new Point(510, 28);
            lblFilePath.Size = new Size(80, 20);
            grpFile.Controls.Add(lblFilePath);

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(140, 25);
            txtFilePath.Size = new Size(365, 26);
            txtFilePath.Font = new Font("Segoe UI", 9f);
            txtFilePath.ReadOnly = true;
            txtFilePath.BackColor = Color.FromArgb(248, 250, 252);
            grpFile.Controls.Add(txtFilePath);

            btnBrowseFile = new Button();
            btnBrowseFile.Text = "استعراض واختيار 📂";
            btnBrowseFile.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnBrowseFile.Location = new Point(15, 23);
            btnBrowseFile.Size = new Size(120, 30);
            btnBrowseFile.BackColor = Color.FromArgb(224, 242, 254);
            btnBrowseFile.ForeColor = Color.FromArgb(3, 105, 161);
            btnBrowseFile.Cursor = Cursors.Hand;
            btnBrowseFile.Click += BtnBrowseFile_Click;
            grpFile.Controls.Add(btnBrowseFile);

            // Drag and drop zone
            pnlDropZone = new Panel();
            pnlDropZone.Location = new Point(15, 62);
            pnlDropZone.Size = new Size(575, 75);
            pnlDropZone.BackColor = Color.FromArgb(248, 250, 252);
            pnlDropZone.BorderStyle = BorderStyle.FixedSingle;
            pnlDropZone.AllowDrop = true;
            pnlDropZone.DragEnter += MainForm_DragEnter;
            pnlDropZone.DragDrop += MainForm_DragDrop;
            pnlDropZone.Cursor = Cursors.Hand;
            pnlDropZone.Click += (s, e) => BtnBrowseFile_Click(s, e);

            lblDropHint = new Label();
            lblDropHint.Text = "📥 اسحب وأفلت ملف برنامج المراكز حصراً:\n(index.html)";
            lblDropHint.TextAlign = ContentAlignment.MiddleCenter;
            lblDropHint.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDropHint.ForeColor = Color.FromArgb(100, 116, 139);
            lblDropHint.Dock = DockStyle.Fill;
            lblDropHint.AllowDrop = true;
            lblDropHint.DragEnter += MainForm_DragEnter;
            lblDropHint.DragDrop += MainForm_DragDrop;
            lblDropHint.Click += (s, e) => BtnBrowseFile_Click(s, e);
            pnlDropZone.Controls.Add(lblDropHint);
            grpFile.Controls.Add(pnlDropZone);

            // Validation Message Display Box
            lblFileValidation = new Label();
            lblFileValidation.Text = "⚠️ يرجى اختيار الملف المطابق للبرنامج المحدد أعلاه لتفعيل زر النشر.";
            lblFileValidation.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblFileValidation.ForeColor = Color.FromArgb(217, 119, 6);
            lblFileValidation.Location = new Point(15, 145);
            lblFileValidation.Size = new Size(575, 58);
            grpFile.Controls.Add(lblFileValidation);

            this.Controls.Add(grpFile);

            // 5. Step 3: New Version
            grpVer = new GroupBox();
            grpVer.Text = "3. تعيين رقم الإصدار الجديد للبرنامج المختار:";
            grpVer.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpVer.ForeColor = Color.FromArgb(30, 41, 59);
            grpVer.Location = new Point(20, 458);
            grpVer.Size = new Size(610, 95);
            grpVer.BackColor = Color.White;

            lblNewVer = new Label();
            lblNewVer.Text = "رقم الإصدار الجديد المراد رفعه:";
            lblNewVer.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblNewVer.Location = new Point(390, 26);
            lblNewVer.Size = new Size(200, 22);
            grpVer.Controls.Add(lblNewVer);

            txtNewVer = new TextBox();
            txtNewVer.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            txtNewVer.Location = new Point(230, 23);
            txtNewVer.Size = new Size(150, 28);
            txtNewVer.TextAlign = HorizontalAlignment.Center;
            txtNewVer.RightToLeft = RightToLeft.No;
            txtNewVer.TextChanged += (s, e) => ValidateAll();
            grpVer.Controls.Add(txtNewVer);

            lblVerHint = new Label();
            lblVerHint.Text = "(أكبر من إصدار البرنامج الحالي)";
            lblVerHint.Font = new Font("Segoe UI", 8f);
            lblVerHint.ForeColor = Color.FromArgb(148, 163, 184);
            lblVerHint.Location = new Point(15, 28);
            lblVerHint.Size = new Size(205, 20);
            grpVer.Controls.Add(lblVerHint);

            lblVersionValidation = new Label();
            lblVersionValidation.Text = "جاري التحقق من رقم الإصدار...";
            lblVersionValidation.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblVersionValidation.ForeColor = Color.FromArgb(217, 119, 6);
            lblVersionValidation.Location = new Point(15, 60);
            lblVersionValidation.Size = new Size(575, 24);
            grpVer.Controls.Add(lblVersionValidation);

            this.Controls.Add(grpVer);

            // 6. Action Buttons
            btnPublish = new Button();
            btnPublish.Text = "🚀 رفع ونشر التحديث إلى السحاب (مقفول حتى مطابقة الملف)";
            btnPublish.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnPublish.Location = new Point(20, 560);
            btnPublish.Size = new Size(430, 46);
            btnPublish.BackColor = Color.FromArgb(148, 163, 184);
            btnPublish.ForeColor = Color.White;
            btnPublish.Cursor = Cursors.Default;
            btnPublish.FlatStyle = FlatStyle.Flat;
            btnPublish.FlatAppearance.BorderSize = 0;
            btnPublish.Enabled = false;
            btnPublish.Click += BtnPublish_Click;
            this.Controls.Add(btnPublish);

            btnOpenFolder = new Button();
            btnOpenFolder.Text = "📂 مجلد المنظومة";
            btnOpenFolder.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnOpenFolder.Location = new Point(460, 560);
            btnOpenFolder.Size = new Size(170, 46);
            btnOpenFolder.BackColor = Color.FromArgb(241, 245, 249);
            btnOpenFolder.ForeColor = Color.FromArgb(51, 65, 85);
            btnOpenFolder.Cursor = Cursors.Hand;
            btnOpenFolder.Click += (s, e) => {
                if (Directory.Exists(workspaceDir)) Process.Start("explorer.exe", workspaceDir);
            };
            this.Controls.Add(btnOpenFolder);

            // 7. Step 4: Rollback to Two Previous Versions
            grpRollback = new GroupBox();
            grpRollback.Text = "4. الرجوع لنسختين سابقتين بعد التحديث (للبرنامج المحدد أعلاه):";
            grpRollback.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpRollback.ForeColor = Color.FromArgb(30, 41, 59);
            grpRollback.Location = new Point(20, 614);
            grpRollback.Size = new Size(610, 95);
            grpRollback.BackColor = Color.White;

            btnRollback1 = new Button();
            btnRollback1.Text = "⏪ رجوع للنسخة السابقة (1)";
            btnRollback1.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRollback1.Location = new Point(310, 25);
            btnRollback1.Size = new Size(285, 38);
            btnRollback1.BackColor = Color.FromArgb(254, 243, 199);
            btnRollback1.ForeColor = Color.FromArgb(146, 64, 14);
            btnRollback1.FlatStyle = FlatStyle.Flat;
            btnRollback1.FlatAppearance.BorderColor = Color.FromArgb(245, 158, 11);
            btnRollback1.Cursor = Cursors.Hand;
            btnRollback1.Click += (s, e) => RollbackToSlot(1);
            grpRollback.Controls.Add(btnRollback1);

            btnRollback2 = new Button();
            btnRollback2.Text = "⏮️ رجوع للنسخة الأقدم (2)";
            btnRollback2.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRollback2.Location = new Point(15, 25);
            btnRollback2.Size = new Size(285, 38);
            btnRollback2.BackColor = Color.FromArgb(224, 231, 255);
            btnRollback2.ForeColor = Color.FromArgb(55, 48, 163);
            btnRollback2.FlatStyle = FlatStyle.Flat;
            btnRollback2.FlatAppearance.BorderColor = Color.FromArgb(99, 102, 241);
            btnRollback2.Cursor = Cursors.Hand;
            btnRollback2.Click += (s, e) => RollbackToSlot(2);
            grpRollback.Controls.Add(btnRollback2);

            lblRollbackHint = new Label();
            lblRollbackHint.Text = "💡 يتم حفظ آخر نسختين سابقتين تلقائياً عند كل تحديث، ويمكنك استرجاع أي منهما ورفعها للمنصة فوراً.";
            lblRollbackHint.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblRollbackHint.ForeColor = Color.FromArgb(100, 116, 139);
            lblRollbackHint.Location = new Point(15, 68);
            lblRollbackHint.Size = new Size(580, 20);
            grpRollback.Controls.Add(lblRollbackHint);

            this.Controls.Add(grpRollback);

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 717);
            progressBar.Size = new Size(610, 16);
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "اختر البرنامج أولاً (المراكز أو المسؤول)، ثم اختر الملف المطابق لتفعيل الرفع.";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
            lblStatus.Location = new Point(20, 738);
            lblStatus.Size = new Size(610, 60);
            this.Controls.Add(lblStatus);

            OnTargetChanged();
        }

        private void RunGitCommand(string args, int timeoutMs)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    string lockFile = Path.Combine(workspaceDir, ".git", "index.lock");
                    if (File.Exists(lockFile)) { try { File.Delete(lockFile); } catch { } }
                    ProcessStartInfo psi = new ProcessStartInfo("git", args);
                    psi.WorkingDirectory = workspaceDir;
                    psi.EnvironmentVariables["HOME"] = workspaceDir;
                    psi.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psi.EnvironmentVariables["GIT_EDITOR"] = "true";
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    psi.RedirectStandardError = true;
                    Process p = Process.Start(psi);
                    string err = p.StandardError.ReadToEnd();
                    if (!p.WaitForExit(timeoutMs)) { try { p.Kill(); } catch { } }
                    if (p.ExitCode == 0) break;
                    if (err.IndexOf("Could not resolve host", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        err.IndexOf("Failed to connect", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        System.Threading.Thread.Sleep(1500);
                        continue;
                    }
                    break;
                }
                catch { break; }
            }
        }

        private bool ExecuteGitPushWithRetry(Action<string> updateStatus, out string finalErr)
        {
            finalErr = "";
            int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt > 1 && updateStatus != null)
                {
                    updateStatus(string.Format("جاري إعادة محاولة الاتصال بالسحاب ({0} من {1})...", attempt, maxAttempts));
                }

                try
                {
                    string lockFile = Path.Combine(workspaceDir, ".git", "index.lock");
                    if (File.Exists(lockFile)) { try { File.Delete(lockFile); } catch { } }

                    ProcessStartInfo psiPush = new ProcessStartInfo("git", "push origin main");
                    psiPush.WorkingDirectory = workspaceDir;
                    psiPush.EnvironmentVariables["HOME"] = workspaceDir;
                    psiPush.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psiPush.CreateNoWindow = true;
                    psiPush.UseShellExecute = false;
                    psiPush.RedirectStandardError = true;

                    using (Process pPush = Process.Start(psiPush))
                    {
                        finalErr = pPush.StandardError.ReadToEnd();
                        if (!pPush.WaitForExit(35000)) { try { pPush.Kill(); } catch { } }
                        if (pPush.ExitCode == 0)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    finalErr = ex.Message;
                }

                // If transient network / DNS error, pause briefly and flush DNS before next attempt
                if (attempt < maxAttempts)
                {
                    try
                    {
                        ProcessStartInfo psiFlush = new ProcessStartInfo("ipconfig", "/flushdns");
                        psiFlush.CreateNoWindow = true;
                        psiFlush.UseShellExecute = false;
                        Process pF = Process.Start(psiFlush);
                        if (pF != null) pF.WaitForExit(3000);
                    }
                    catch { }
                    System.Threading.Thread.Sleep(2500);
                }
            }
            return false;
        }

        private string GetBackupPrefix(bool isAdmin)
        {
            return isAdmin ? "admin" : "centers";
        }

        private string GetBackupHtmlPath(bool isAdmin, int slot)
        {
            string dir = Path.Combine(workspaceDir, "backups");
            return Path.Combine(dir, string.Format("{0}_prev{1}.html", GetBackupPrefix(isAdmin), slot));
        }

        private string GetBackupMetaPath(bool isAdmin, int slot)
        {
            string dir = Path.Combine(workspaceDir, "backups");
            return Path.Combine(dir, string.Format("{0}_prev{1}.txt", GetBackupPrefix(isAdmin), slot));
        }

        private string GetBackupVersionLabel(bool isAdmin, int slot)
        {
            try
            {
                string metaPath = GetBackupMetaPath(isAdmin, slot);
                string htmlPath = GetBackupHtmlPath(isAdmin, slot);
                if (File.Exists(metaPath) && File.Exists(htmlPath))
                {
                    string raw = File.ReadAllText(metaPath).Trim();
                    string[] parts = raw.Split('|');
                    if (parts.Length >= 2)
                        return string.Format("الإصدار {0} ({1})", parts[0], parts[1]);
                    if (parts.Length == 1 && parts[0].Length > 0)
                        return "الإصدار " + parts[0];
                }
            }
            catch { }
            return "";
        }

        private string GetBackupVersionNumber(bool isAdmin, int slot)
        {
            try
            {
                string metaPath = GetBackupMetaPath(isAdmin, slot);
                if (File.Exists(metaPath))
                {
                    string raw = File.ReadAllText(metaPath).Trim();
                    string[] parts = raw.Split('|');
                    if (parts.Length >= 1 && parts[0].Trim().Length > 0)
                        return parts[0].Trim();
                }
            }
            catch { }
            return isAdmin ? "2.0.2" : "2.0.7";
        }

        private void UpdateRollbackButtonsUI()
        {
            if (btnRollback1 == null || btnRollback2 == null) return;
            bool isAdmin = rbAdmin.Checked;

            string lbl1 = GetBackupVersionLabel(isAdmin, 1);
            bool has1 = File.Exists(GetBackupHtmlPath(isAdmin, 1)) && !string.IsNullOrEmpty(lbl1);
            btnRollback1.Enabled = has1;
            btnRollback1.Text = has1
                ? "⏪ رجوع للنسخة السابقة (1): " + lbl1
                : "⏪ النسخة السابقة (1): غير متوفرة بعد";

            string lbl2 = GetBackupVersionLabel(isAdmin, 2);
            bool has2 = File.Exists(GetBackupHtmlPath(isAdmin, 2)) && !string.IsNullOrEmpty(lbl2);
            btnRollback2.Enabled = has2;
            btnRollback2.Text = has2
                ? "⏮️ رجوع للنسخة الأقدم (2): " + lbl2
                : "⏮️ النسخة الأقدم (2): غير متوفرة بعد";
        }

        private void SaveBackupBeforeUpdate(bool isAdmin, string currentVerBeforeUpdate)
        {
            try
            {
                string bDir = Path.Combine(workspaceDir, "backups");
                if (!Directory.Exists(bDir)) Directory.CreateDirectory(bDir);

                string currentActiveHtml = Path.Combine(workspaceDir, isAdmin ? "admin.html" : "index.html");
                if (!File.Exists(currentActiveHtml)) return;

                string p1Html = GetBackupHtmlPath(isAdmin, 1);
                string p1Meta = GetBackupMetaPath(isAdmin, 1);
                string p2Html = GetBackupHtmlPath(isAdmin, 2);
                string p2Meta = GetBackupMetaPath(isAdmin, 2);

                // Shift prev1 -> prev2
                if (File.Exists(p1Html))
                {
                    SafeCopy(p1Html, p2Html);
                    if (File.Exists(p1Meta)) SafeCopy(p1Meta, p2Meta);
                }

                // Save current active -> prev1
                SafeCopy(currentActiveHtml, p1Html);
                string datePart = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                File.WriteAllText(p1Meta, string.Format("{0}|{1}", currentVerBeforeUpdate, datePart));

                // Mirror backups to Desktop folder if present
                if (Directory.Exists(desktopDir))
                {
                    string dBackups = Path.Combine(desktopDir, "backups");
                    if (!Directory.Exists(dBackups)) Directory.CreateDirectory(dBackups);
                    foreach (string f in Directory.GetFiles(bDir))
                    {
                        SafeCopy(f, Path.Combine(dBackups, Path.GetFileName(f)));
                    }
                }
            }
            catch { }
        }

        private void RollbackToSlot(int slot)
        {
            bool isAdminTarget = rbAdmin.Checked;
            string htmlBackup = GetBackupHtmlPath(isAdminTarget, slot);
            if (!File.Exists(htmlBackup))
            {
                MessageBox.Show("نسخة الاسترجاع المطلوبة غير متوفرة حالياً.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string targetName = isAdminTarget ? "برنامج المسؤول فقط (لوحة الإدارة)" : "برنامج مراكز التسجيل فقط (بوابة المراكز)";
            string fname = isAdminTarget ? "admin.html" : "index.html";
            string restoredVer = GetBackupVersionNumber(isAdminTarget, slot);
            string slotTitle = slot == 1 ? "النسخة السابقة الأولى (1)" : "النسخة السابقة الأقدم (2)";

            string confirmMsg = string.Format(
                "هل أنت متأكد من رغبتك في الرجوع إلى {0} ورفعها إلى المنصة السحابية؟\n\n" +
                "• البرنامج المحدد: {1}\n" +
                "• الملف المسترجع: {2}\n" +
                "• رقم إصدار النسخة المسترجعة: {3}\n\n" +
                "سيتم استرجاع هذه النسخة محلياً ورفعها مباشرة إلى المنصة السحابية دون المساس بالبرنامج الآخر.",
                slotTitle, targetName, fname, restoredVer
            );

            DialogResult res = MessageBox.Show(confirmMsg, "تأكيد الرجوع لنسخة سابقة", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            btnPublish.Enabled = false;
            btnBrowseFile.Enabled = false;
            btnRollback1.Enabled = false;
            btnRollback2.Enabled = false;
            rbCenters.Enabled = false;
            rbAdmin.Enabled = false;
            txtNewVer.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lblStatus.Text = "جاري استرجاع النسخة السابقة ومزامنتها مع المنصة...";
            lblStatus.ForeColor = Color.FromArgb(180, 83, 9);

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    string lockFile = Path.Combine(workspaceDir, ".git", "index.lock");
                    if (File.Exists(lockFile)) { try { File.Delete(lockFile); } catch { } }
                    string rebaseDir = Path.Combine(workspaceDir, ".git", "rebase-merge");
                    if (Directory.Exists(rebaseDir)) { try { Directory.Delete(rebaseDir, true); } catch { } }

                    // 1. Sync with remote first (immune to unstaged local changes)
                    RunGitCommand("fetch origin main", 20000);
                    RunGitCommand("checkout origin/main -- data/ public/data/", 10000);
                    RunGitCommand("reset --soft origin/main", 10000);
                    RunGitCommand("reset", 10000);

                    // 2. Restore backup HTML file to workspace and Desktop
                    string gitAddArgs = "";
                    string nowIso = DateTime.UtcNow.ToString("o");

                    if (isAdminTarget)
                    {
                        SafeCopy(htmlBackup, Path.Combine(workspaceDir, "admin.html"));
                        SafeCopy(htmlBackup, Path.Combine(workspaceDir, "public", "admin.html"));
                        if (Directory.Exists(desktopDir))
                        {
                            SafeCopy(htmlBackup, Path.Combine(desktopDir, "admin.html"));
                            SafeCopy(htmlBackup, Path.Combine(desktopDir, "public", "admin.html"));
                        }
                        cloudAdminVersion = restoredVer;

                        string adminJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"admin\"\n}}", restoredVer, nowIso);
                        File.WriteAllText(Path.Combine(workspaceDir, "version-admin.json"), adminJson);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "version-admin.json"), adminJson);

                        gitAddArgs = "admin.html public/admin.html version-admin.json public/version-admin.json";
                    }
                    else
                    {
                        SafeCopy(htmlBackup, Path.Combine(workspaceDir, "index.html"));
                        SafeCopy(htmlBackup, Path.Combine(workspaceDir, "public", "index.html"));
                        SafeCopy(htmlBackup, Path.Combine(workspaceDir, "public", "center.html"));
                        if (Directory.Exists(desktopDir))
                        {
                            SafeCopy(htmlBackup, Path.Combine(desktopDir, "index.html"));
                            SafeCopy(htmlBackup, Path.Combine(desktopDir, "public", "index.html"));
                            SafeCopy(htmlBackup, Path.Combine(desktopDir, "public", "center.html"));
                        }
                        cloudCentersVersion = restoredVer;

                        string centersJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"centers\"\n}}", restoredVer, nowIso);
                        File.WriteAllText(Path.Combine(workspaceDir, "version-centers.json"), centersJson);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "version-centers.json"), centersJson);

                        gitAddArgs = "index.html public/index.html public/center.html version-centers.json public/version-centers.json";
                    }

                    string combinedJson = string.Format(
                        "{{\n  \"centersVersion\": \"{0}\",\n  \"adminVersion\": \"{1}\",\n  \"version\": \"{2}\",\n  \"updatedAt\": \"{3}\",\n  \"target\": \"{4}\",\n  \"file\": \"{5}\"\n}}",
                        cloudCentersVersion, cloudAdminVersion, restoredVer, nowIso, targetName, fname
                    );
                    File.WriteAllText(Path.Combine(workspaceDir, "version.json"), combinedJson);
                    File.WriteAllText(Path.Combine(workspaceDir, "public", "version.json"), combinedJson);
                    gitAddArgs += " version.json public/version.json";

                    // 3. Git Add, Commit, Push
                    ProcessStartInfo psiAdd = new ProcessStartInfo("git", "add " + gitAddArgs);
                    psiAdd.WorkingDirectory = workspaceDir;
                    psiAdd.EnvironmentVariables["HOME"] = workspaceDir;
                    psiAdd.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psiAdd.CreateNoWindow = true;
                    psiAdd.UseShellExecute = false;
                    Process pAdd = Process.Start(psiAdd);
                    if (!pAdd.WaitForExit(15000)) { try { pAdd.Kill(); } catch { } }

                    string commitMsg = string.Format("استرجاع {0} إلى {1} (الإصدار {2})", targetName, slotTitle, restoredVer);
                    ProcessStartInfo psiCommit = new ProcessStartInfo("git", string.Format("commit -m \"{0}\"", commitMsg));
                    psiCommit.WorkingDirectory = workspaceDir;
                    psiCommit.EnvironmentVariables["HOME"] = workspaceDir;
                    psiCommit.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psiCommit.CreateNoWindow = true;
                    psiCommit.UseShellExecute = false;
                    Process pCommit = Process.Start(psiCommit);
                    if (!pCommit.WaitForExit(15000)) { try { pCommit.Kill(); } catch { } }

                    string pushErr = "";
                    bool pushSuccess = ExecuteGitPushWithRetry(delegate(string statusText) {
                        this.Invoke((MethodInvoker)delegate { lblStatus.Text = statusText; });
                    }, out pushErr);

                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Visible = false;
                        if (pushSuccess)
                        {
                            lblStatus.Text = string.Format("تم الرجوع بنجاح إلى {0} (الإصدار {1}) ورفعها للمنصة السحابية! ✅", slotTitle, restoredVer);
                            lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                            OnTargetChanged();
                            MessageBox.Show(
                                string.Format(
                                    "تم استرجاع النسخة السابقة ورفعها للمنصة بنجاح! ✅\n\n" +
                                    "• البرنامج: {0}\n" +
                                    "• النسخة المسترجعة: {1}\n" +
                                    "• رقم الإصدار الفعال الآن: {2}",
                                    targetName, slotTitle, restoredVer
                                ),
                                "تم استرجاع النسخة بنجاح",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                        else
                        {
                            lblStatus.Text = "حدث خطأ أثناء رفع النسخة المسترجعة للسحاب.";
                            lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                            MessageBox.Show("تعذر رفع النسخة المسترجعة للسحاب:\n" + pushErr, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Visible = false;
                        lblStatus.Text = "خطأ أثناء الاسترجاع: " + ex.Message;
                        lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                        MessageBox.Show("حدث خطأ أثناء استرجاع النسخة:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                finally
                {
                    this.Invoke((MethodInvoker)delegate {
                        btnBrowseFile.Enabled = true;
                        rbCenters.Enabled = true;
                        rbAdmin.Enabled = true;
                        txtNewVer.Enabled = true;
                        UpdateRollbackButtonsUI();
                        ValidateAll();
                    });
                }
            });
        }

        private string GetCurrentTargetCloudVersion()
        {
            return rbAdmin.Checked ? cloudAdminVersion : cloudCentersVersion;
        }

        private void OnTargetChanged()
        {
            string curVer = GetCurrentTargetCloudVersion();

            if (rbAdmin.Checked)
            {
                lblCurrentCloudVer.Text = "إصدار لوحة المسؤول الحالي على المنصة:";
                lblCloudVerVal.Text = "الإصدار " + cloudAdminVersion;
                lblCloudVerVal.ForeColor = Color.FromArgb(180, 83, 9);
                lblDropHint.Text = "📥 اسحب وأفلت ملف لوحة المسؤول حصراً:\n(admin.html)";
                lblDropHint.ForeColor = Color.FromArgb(180, 83, 9);
                SuggestNextVersion(cloudAdminVersion);
            }
            else
            {
                lblCurrentCloudVer.Text = "إصدار بوابة المراكز الحالي على المنصة:";
                lblCloudVerVal.Text = "الإصدار " + cloudCentersVersion;
                lblCloudVerVal.ForeColor = Color.FromArgb(3, 105, 161);
                lblDropHint.Text = "📥 اسحب وأفلت ملف برنامج المراكز حصراً:\n(index.html)";
                lblDropHint.ForeColor = Color.FromArgb(3, 105, 161);
                SuggestNextVersion(cloudCentersVersion);
            }

            UpdateRollbackButtonsUI();
            ValidateAll();
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                SetSelectedFile(files[0]);
            }
        }

        private void BtnBrowseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            if (rbAdmin.Checked)
            {
                ofd.Title = "اختر ملف لوحة المسؤول حصراً (admin.html)";
                ofd.Filter = "ملف لوحة المسؤول (admin.html)|admin.html";
            }
            else
            {
                ofd.Title = "اختر ملف برنامج المراكز حصراً (index.html)";
                ofd.Filter = "ملف برنامج المراكز (index.html)|index.html";
            }

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SetSelectedFile(ofd.FileName);
            }
        }

        private void SetSelectedFile(string path)
        {
            selectedFilePath = path;
            txtFilePath.Text = path;
            ValidateAll();
        }

        private bool ValidateFileMatch()
        {
            if (string.IsNullOrEmpty(selectedFilePath) || !File.Exists(selectedFilePath))
            {
                lblFileValidation.Text = "⚠️ يرجى اختيار الملف المطلوب رفعه أولاً.";
                lblFileValidation.ForeColor = Color.FromArgb(217, 119, 6);
                return false;
            }

            FileInfo fi = new FileInfo(selectedFilePath);
            string fname = fi.Name.ToLower();
            string ext = fi.Extension.ToLower();

            // Case A: User chose to update ADMIN only
            if (rbAdmin.Checked)
            {
                if (fname == "admin.html")
                {
                    lblFileValidation.Text = string.Format("✅ تطابق تام ومؤكد: تم اختيار ملف لوحة المسؤول ({0}) بنجاح.\nسيتم تحديث كود لوحة المسؤول السحابية حصراً دون لمس برنامج المراكز أو الاستمارات.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
                else if (fname == "index.html")
                {
                    lblFileValidation.Text = "🚫 غير مسموح وعدم تطابق! لقد اخترت (تحديث برنامج المسؤول فقط)،\nولكن الملف المختار هو ملف برنامج المراكز (index.html).\nيجب اختيار ملف (admin.html) حصراً.";
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
                else
                {
                    lblFileValidation.Text = string.Format("🚫 غير مسموح وعدم تطابق! لقد اخترت (تحديث برنامج المسؤول فقط)،\nولكن الملف المختار هو: \"{0}\".\nيجب عليك حصراً اختيار ملف لوحة المسؤول (admin.html).", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
            }

            // Case B: User chose to update CENTERS only
            if (rbCenters.Checked)
            {
                if (fname == "index.html" || fname == "center.html")
                {
                    lblFileValidation.Text = string.Format("✅ تطابق تام ومؤكد: تم اختيار صفحة بوابة مراكز التسجيل الـ 76 ({0}).\nسيتم تحديث واجهة المراكز أونلاين حصراً دون لمس لوحة المسؤول أو الاستمارات.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
                else if (fname == "admin.html")
                {
                    lblFileValidation.Text = "🚫 غير مسموح وعدم تطابق! لقد اخترت (تحديث برنامج المراكز فقط)،\nولكن الملف المختار هو ملف المسؤول (admin.html).\nإذا كنت تريد تحديث المسؤول يرجى تحديد خيار المسؤول أعلاه أولاً.";
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
                else if (fname == "forms.json" || ext == ".xlsx" || ext == ".docx" || ext == ".pdf")
                {
                    lblFileValidation.Text = "🚫 غير مسموح! تحديث الاستمارات والنماذج متوقف تماماً على برنامج المسؤول حصراً أونلاين.\nبرنامج التحديث مخصص لتحديث واجهة برنامج المراكز (ملف index.html حصراً).";
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
                else
                {
                    lblFileValidation.Text = string.Format("🚫 غير مسموح وعدم تطابق! الملف \"{0}\" غير مسموح به هنا.\nيجب عليك حصراً اختيار ملف واجهة المراكز (index.html).", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
            }

            return false;
        }

        private void FetchCloudVersion()
        {
            lblCloudVerVal.Text = "جاري الفحص...";
            lblCloudVerVal.ForeColor = Color.FromArgb(2, 132, 199);

            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; // Tls12
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    // 1. Fetch Centers Version (Vercel first for zero-cache live updates)
                    string[] centersUrls = new string[]
                    {
                        CLOUD_CENTERS_VERCEL,
                        CLOUD_CENTERS_URL,
                        CLOUD_CENTERS_BACKUP
                    };
                    foreach (string url in centersUrls)
                    {
                        try
                        {
                            string jsonC = wc.DownloadString(url + "?t=" + DateTime.UtcNow.Ticks);
                            Match mC = Regex.Match(jsonC, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (mC.Success && !string.IsNullOrEmpty(mC.Groups[1].Value))
                            {
                                string fetchedVer = mC.Groups[1].Value.Trim();
                                if (CompareVersions(fetchedVer, cloudCentersVersion) >= 0)
                                {
                                    cloudCentersVersion = fetchedVer;
                                }
                                break;
                            }
                        }
                        catch {}
                    }

                    // 2. Fetch Admin Version (Vercel first for zero-cache live updates)
                    string[] adminUrls = new string[]
                    {
                        CLOUD_ADMIN_VERCEL,
                        CLOUD_ADMIN_URL,
                        CLOUD_ADMIN_BACKUP
                    };
                    foreach (string url in adminUrls)
                    {
                        try
                        {
                            string jsonA = wc.DownloadString(url + "?t=" + DateTime.UtcNow.Ticks);
                            Match mA = Regex.Match(jsonA, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (mA.Success && !string.IsNullOrEmpty(mA.Groups[1].Value))
                            {
                                string fetchedVer = mA.Groups[1].Value.Trim();
                                if (CompareVersions(fetchedVer, cloudAdminVersion) >= 0)
                                {
                                    cloudAdminVersion = fetchedVer;
                                }
                                break;
                            }
                        }
                        catch {}
                    }
                }

                OnTargetChanged();
            }
            catch
            {
                lblCloudVerVal.Text = "تعذر الاتصال (محلياً)";
                lblCloudVerVal.ForeColor = Color.FromArgb(225, 29, 72);
                OnTargetChanged();
            }

            ValidateAll();
        }

        private void SuggestNextVersion(string cur)
        {
            try
            {
                string[] parts = cur.Trim().Split('.');
                if (parts.Length >= 2)
                {
                    int last = int.Parse(parts[parts.Length - 1]);
                    parts[parts.Length - 1] = (last + 1).ToString();
                    txtNewVer.Text = string.Join(".", parts);
                }
                else
                {
                    txtNewVer.Text = "2.0.1";
                }
            }
            catch
            {
                txtNewVer.Text = "2.0.1";
            }
        }

        private bool ValidateVersionNumber()
        {
            string newVer = txtNewVer.Text.Trim();
            string curTargetVer = GetCurrentTargetCloudVersion();
            string targetName = rbAdmin.Checked ? "لوحة المسؤول" : "بوابة المراكز";

            if (string.IsNullOrEmpty(newVer))
            {
                lblVersionValidation.Text = "❌ يرجى إدخال رقم الإصدار الجديد لـ " + targetName;
                lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                return false;
            }

            if (!Regex.IsMatch(newVer, @"^\d+(\.\d+)+$"))
            {
                lblVersionValidation.Text = "❌ تنسيق رقم الإصدار غير صالح. استخدم مثلاً: 2.0.1 أو 2.1.0";
                lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                return false;
            }

            int cmp = CompareVersions(newVer, curTargetVer);
            if (cmp <= 0)
            {
                lblVersionValidation.Text = string.Format("❌ يجب أن يكون الإصدار الجديد ({0}) أكبر من إصدار {1} الحالي ({2}).", newVer, targetName, curTargetVer);
                lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                return false;
            }

            lblVersionValidation.Text = string.Format("✅ رقم الإصدار معتمد لـ {0}: {1} (أكبر من الإصدار الحالي {2})", targetName, newVer, curTargetVer);
            lblVersionValidation.ForeColor = Color.FromArgb(16, 185, 129);
            return true;
        }

        private int CompareVersions(string v1, string v2)
        {
            try
            {
                string[] p1 = v1.Trim().Split('.');
                string[] p2 = v2.Trim().Split('.');
                int len = Math.Max(p1.Length, p2.Length);

                for (int i = 0; i < len; i++)
                {
                    int num1 = i < p1.Length ? int.Parse(p1[i]) : 0;
                    int num2 = i < p2.Length ? int.Parse(p2[i]) : 0;
                    if (num1 > num2) return 1;
                    if (num1 < num2) return -1;
                }
                return 0;
            }
            catch
            {
                return string.Compare(v1, v2, StringComparison.OrdinalIgnoreCase);
            }
        }

        private void ValidateAll()
        {
            bool fileOk = ValidateFileMatch();
            bool verOk = ValidateVersionNumber();

            if (fileOk && verOk)
            {
                string targetName = rbAdmin.Checked ? "برنامج المسؤول فقط" : "برنامج المراكز فقط";
                btnPublish.Enabled = true;
                btnPublish.BackColor = Color.FromArgb(16, 185, 129);
                btnPublish.Cursor = Cursors.Hand;
                btnPublish.Text = "🚀 رفع ونشر التحديث الآن (" + targetName + ")";
                lblStatus.Text = "جاهز للنشر: الملف مطابق تماماً للبرنامج المختار ورقم الإصدار معتمد.";
                lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
            }
            else
            {
                btnPublish.Enabled = false;
                btnPublish.BackColor = Color.FromArgb(148, 163, 184);
                btnPublish.Cursor = Cursors.Default;
                if (!fileOk)
                {
                    btnPublish.Text = "🚫 زر الرفع مقفول (الملف المختار غير مطابق للبرنامج المحدد)";
                    lblStatus.Text = "لا يمكن المتابعة: يجب اختيار الملف المطابق للبرنامج المحدد أعلاه حصراً.";
                    lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                }
                else
                {
                    btnPublish.Text = "🚫 زر الرفع مقفول (رقم الإصدار غير صالح أو ليس أكبر من إصدار هذا البرنامج)";
                    lblStatus.Text = "يرجى تحديد رقم إصدار أكبر من إصدار هذا البرنامج الحالي للمتابعة.";
                    lblStatus.ForeColor = Color.FromArgb(217, 119, 6);
                }
            }
        }

        private void SafeCopy(string src, string dst)
        {
            try
            {
                string fullSrc = Path.GetFullPath(src);
                string fullDst = Path.GetFullPath(dst);

                if (string.Equals(fullSrc, fullDst, StringComparison.OrdinalIgnoreCase))
                {
                    return; // الملف هو نفسه بالفعل، لا حاجة لنسخه فوق نفسه
                }

                string dir = Path.GetDirectoryName(fullDst);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.Copy(fullSrc, fullDst, true);
            }
            catch
            {
                byte[] bytes = File.ReadAllBytes(src);
                File.WriteAllBytes(dst, bytes);
            }
        }

        private void BtnPublish_Click(object sender, EventArgs e)
        {
            if (!ValidateFileMatch() || !ValidateVersionNumber()) return;

            string targetName = rbAdmin.Checked ? "برنامج المسؤول فقط (لوحة الإدارة)" : "برنامج مراكز التسجيل فقط (بوابة المراكز)";
            string newVer = txtNewVer.Text.Trim();
            string fname = Path.GetFileName(selectedFilePath);

            string confirmMsg = string.Format(
                "هل أنت متأكد من رغبتك في رفع ونشر هذا التحديث السحابي؟\n\n" +
                "• الهدف المعتمد: {0}\n" +
                "• الملف المختار: {1}\n" +
                "• رقم الإصدار الجديد لهذا البرنامج: {2}\n\n" +
                "ملاحظة: سيتم حفظ النسخة الحالية في سجل الاسترجاع وتحديث هذا البرنامج فقط دون المساس بالبرنامج الآخر.",
                targetName, fname, newVer
            );

            DialogResult res = MessageBox.Show(confirmMsg, "تأكيد النشر السحابي المنفصل", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            btnPublish.Enabled = false;
            btnBrowseFile.Enabled = false;
            btnRollback1.Enabled = false;
            btnRollback2.Enabled = false;
            rbCenters.Enabled = false;
            rbAdmin.Enabled = false;
            txtNewVer.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lblStatus.Text = "جاري معالجة ونسخ الملف المطابق...";
            lblStatus.ForeColor = Color.FromArgb(2, 132, 199);

            bool isAdminTarget = rbAdmin.Checked;
            string curVerBeforeUpdate = isAdminTarget ? cloudAdminVersion : cloudCentersVersion;

            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    // 0. Clean any stale git lock or broken rebase folder
                    string lockFile = Path.Combine(workspaceDir, ".git", "index.lock");
                    if (File.Exists(lockFile)) { try { File.Delete(lockFile); } catch { } }
                    string rebaseDir = Path.Combine(workspaceDir, ".git", "rebase-merge");
                    if (Directory.Exists(rebaseDir)) { try { Directory.Delete(rebaseDir, true); } catch { } }

                    // 1. Sync with remote FIRST before applying our file (immune to unstaged local changes)
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "جاري مزامنة الفرع السحابي...";
                    });
                    RunGitCommand("fetch origin main", 20000);
                    RunGitCommand("checkout origin/main -- data/ public/data/", 10000);
                    RunGitCommand("reset --soft origin/main", 10000);
                    RunGitCommand("reset", 10000);

                    // 1b. Save current version into the 2-version rollback history before overwriting!
                    SaveBackupBeforeUpdate(isAdminTarget, curVerBeforeUpdate);

                    // 2. Now copy the selected file and update version JSONs on top of latest branch
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "جاري معالجة ونسخ الملف المطابق...";
                    });
                    string gitAddArgs = "";
                    string nowIso = DateTime.UtcNow.ToString("o");

                    if (isAdminTarget)
                    {
                        string adminHtml = File.ReadAllText(selectedFilePath, Encoding.UTF8);
                        adminHtml = Regex.Replace(adminHtml, @"id=""adminVersionVal""[^>]*>[^<]+<", string.Format("id=\"adminVersionVal\">الإصدار {0}<", newVer));
                        adminHtml = Regex.Replace(adminHtml, @"const\s+ver\s*=\s*vData\.version\s*\|\|\s*""[^""]+""", string.Format("const ver = vData.version || \"{0}\"", newVer));

                        File.WriteAllText(Path.Combine(workspaceDir, "admin.html"), adminHtml, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "admin.html"), adminHtml, Encoding.UTF8);

                        cloudAdminVersion = newVer;

                        string adminJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"admin\"\n}}", newVer, nowIso);
                        File.WriteAllText(Path.Combine(workspaceDir, "version-admin.json"), adminJson, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "version-admin.json"), adminJson, Encoding.UTF8);

                        string[] localAdminTargets = new string[] {
                            @"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_exe_system",
                            @"D:\منظومة_استمارات_البصرة\برنامج_المسؤول",
                            @"D:\منظومة_استمارات_البصرة\برنامج_المسؤول\AppStorage\AdminData",
                            @"C:\Users\ali\Desktop\منظومة_استمارات_البصرة_النسخة_التنفيذية_EXE"
                        };
                        foreach (string lt in localAdminTargets)
                        {
                            try {
                                if (Directory.Exists(lt)) {
                                    File.WriteAllText(Path.Combine(lt, "admin.html"), adminHtml, Encoding.UTF8);
                                    File.WriteAllText(Path.Combine(lt, "version-admin.json"), adminJson, Encoding.UTF8);
                                }
                            } catch { }
                        }

                        gitAddArgs = "admin.html public/admin.html version-admin.json public/version-admin.json sheets/ public/sheets/ data/forms.json public/data/forms.json";
                    }
                    else
                    {
                        string centerHtml = File.ReadAllText(selectedFilePath, Encoding.UTF8);
                        centerHtml = Regex.Replace(centerHtml, @"id=""centerVersionVal""[^>]*>[^<]+<", string.Format("id=\"centerVersionVal\">الإصدار {0}<", newVer));

                        File.WriteAllText(Path.Combine(workspaceDir, "index.html"), centerHtml, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "index.html"), centerHtml, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "center.html"), centerHtml, Encoding.UTF8);

                        cloudCentersVersion = newVer;

                        string centersJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"centers\"\n}}", newVer, nowIso);
                        File.WriteAllText(Path.Combine(workspaceDir, "version-centers.json"), centersJson, Encoding.UTF8);
                        File.WriteAllText(Path.Combine(workspaceDir, "public", "version-centers.json"), centersJson, Encoding.UTF8);

                        string[] localCenterTargets = new string[] {
                            @"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_exe_system",
                            @"D:\منظومة_استمارات_البصرة\برنامج_المراكز",
                            @"D:\منظومة_استمارات_البصرة\برنامج_المراكز\AppStorage\CenterData",
                            @"C:\Users\ali\Desktop\منظومة_استمارات_البصرة_النسخة_التنفيذية_EXE"
                        };
                        foreach (string lt in localCenterTargets)
                        {
                            try {
                                if (Directory.Exists(lt)) {
                                    File.WriteAllText(Path.Combine(lt, "index.html"), centerHtml, Encoding.UTF8);
                                    File.WriteAllText(Path.Combine(lt, "version-centers.json"), centersJson, Encoding.UTF8);
                                }
                            } catch { }
                        }

                        gitAddArgs = "index.html public/index.html public/center.html version-centers.json public/version-centers.json sheets/ public/sheets/ data/forms.json public/data/forms.json";
                    }

                    string combinedJson = string.Format(
                        "{{\n  \"centersVersion\": \"{0}\",\n  \"adminVersion\": \"{1}\",\n  \"version\": \"{2}\",\n  \"updatedAt\": \"{3}\",\n  \"target\": \"{4}\",\n  \"file\": \"{5}\"\n}}",
                        cloudCentersVersion, cloudAdminVersion, newVer, nowIso, targetName, fname
                    );
                    File.WriteAllText(Path.Combine(workspaceDir, "version.json"), combinedJson);
                    File.WriteAllText(Path.Combine(workspaceDir, "public", "version.json"), combinedJson);
                    gitAddArgs += " version.json public/version.json";

                    // 3. Git Add
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "جاري إضافة الملفات المحددة للتحديث (Git Add)...";
                    });
                    ProcessStartInfo psiAdd = new ProcessStartInfo("git", "add " + gitAddArgs);
                    psiAdd.WorkingDirectory = workspaceDir;
                    psiAdd.EnvironmentVariables["HOME"] = workspaceDir;
                    psiAdd.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psiAdd.CreateNoWindow = true;
                    psiAdd.UseShellExecute = false;
                    Process pAdd = Process.Start(psiAdd);
                    if (!pAdd.WaitForExit(15000)) { try { pAdd.Kill(); } catch { } }

                    // 4. Git Commit
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "جاري توثيق التحديث المنفصل (Git Commit)...";
                    });
                    string commitMsg = string.Format("تحديث {0} إلى الإصدار {1}: {2}", targetName, newVer, fname);
                    ProcessStartInfo psiCommit = new ProcessStartInfo("git", string.Format("commit -m \"{0}\"", commitMsg));
                    psiCommit.WorkingDirectory = workspaceDir;
                    psiCommit.EnvironmentVariables["HOME"] = workspaceDir;
                    psiCommit.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
                    psiCommit.CreateNoWindow = true;
                    psiCommit.UseShellExecute = false;
                    Process pCommit = Process.Start(psiCommit);
                    if (!pCommit.WaitForExit(15000)) { try { pCommit.Kill(); } catch { } }

                    // 5. Git Push
                    this.Invoke((MethodInvoker)delegate {
                        lblStatus.Text = "جاري رفع التحديث السحابي إلى GitHub و Vercel...";
                    });
                    string pushErr = "";
                    bool pushSuccess = ExecuteGitPushWithRetry(delegate(string statusText) {
                        this.Invoke((MethodInvoker)delegate { lblStatus.Text = statusText; });
                    }, out pushErr);

                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Visible = false;
                        if (pushSuccess)
                        {
                            if (isAdminTarget)
                            {
                                cloudAdminVersion = newVer;
                            }
                            else
                            {
                                cloudCentersVersion = newVer;
                            }
                            OnTargetChanged();

                            lblStatus.Text = string.Format("اكتمل الرفع بنجاح! تم تحديث {0} إلى الإصدار ({1}) أونلاين 🚀", targetName, newVer);
                            lblStatus.ForeColor = Color.FromArgb(16, 185, 129);
                            UpdateRollbackButtonsUI();

                            MessageBox.Show(
                                string.Format(
                                    "تم رفع ونشر التحديث السحابي بنجاح تام! 🚀\n\n" +
                                    "• النظام المحدث: {0}\n" +
                                    "• الملف المرفوع: {1}\n" +
                                    "• رقم الإصدار الجديد: {2}\n\n" +
                                    "تم حفظ النسخة السابقة تلقائياً في أزرار الاسترجاع بالأسفل في حال رغبتك بالرجوع إليها.",
                                    targetName, fname, newVer
                                ),
                                "اكتمل التحديث السحابي بنجاح",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );

                            FetchCloudVersion();
                        }
                        else
                        {
                            lblStatus.Text = "حدث خطأ أثناء الاتصال بالسحاب. تأكد من اتصال الإنترنت.";
                            lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                            string msg = "تعذر إتمام عملية الرفع السحابي. يرجى التأكد من اتصال الإنترنت وإعادة المحاولة.";
                            if (!string.IsNullOrEmpty(pushErr) && pushErr.Trim().Length > 0)
                            {
                                msg += "\n\nتفاصيل الخطأ:\n" + pushErr.Trim();
                            }
                            MessageBox.Show(msg, "خطأ في الرفع", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate {
                        progressBar.Visible = false;
                        lblStatus.Text = "خطأ: " + ex.Message;
                        lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                        MessageBox.Show("حدث خطأ أثناء تنفيذ التحديث:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
                finally
                {
                    this.Invoke((MethodInvoker)delegate {
                        btnBrowseFile.Enabled = true;
                        rbCenters.Enabled = true;
                        rbAdmin.Enabled = true;
                        txtNewVer.Enabled = true;
                        UpdateRollbackButtonsUI();
                        ValidateAll();
                    });
                }
            });
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
