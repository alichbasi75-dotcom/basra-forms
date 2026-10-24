using System;
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
        private ProgressBar progressBar;
        private Label lblStatus;

        // Independent Version Tracking for Each Program
        private string cloudCentersVersion = "2.0.0";
        private string cloudAdminVersion = "2.0.2";

        private string CLOUD_CENTERS_URL = "https://basra-forms.vercel.app/version-centers.json";
        private string CLOUD_ADMIN_URL = "https://basra-forms.vercel.app/version-admin.json";
        private string CLOUD_CENTERS_BACKUP = "https://alichbasi75-dotcom.github.io/basra-forms/version-centers.json";
        private string CLOUD_ADMIN_BACKUP = "https://alichbasi75-dotcom.github.io/basra-forms/version-admin.json";

        private string workspaceDir = @"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system";
        private string selectedFilePath = "";

        public MainForm()
        {
            InitializeComponent();
            FetchCloudVersion();
        }

        private void InitializeComponent()
        {
            this.Text = "أداة تحديث ورفع منظومة استمارات البصرة (المراكز والمسؤول بشكل منفصل)";
            this.Size = new Size(670, 750);
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
            btnPublish.Location = new Point(20, 563);
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
            btnOpenFolder.Location = new Point(460, 563);
            btnOpenFolder.Size = new Size(170, 46);
            btnOpenFolder.BackColor = Color.FromArgb(241, 245, 249);
            btnOpenFolder.ForeColor = Color.FromArgb(51, 65, 85);
            btnOpenFolder.Cursor = Cursors.Hand;
            btnOpenFolder.Click += (s, e) => {
                if (Directory.Exists(workspaceDir)) Process.Start("explorer.exe", workspaceDir);
            };
            this.Controls.Add(btnOpenFolder);

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 617);
            progressBar.Size = new Size(610, 16);
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "اختر البرنامج أولاً (المراكز أو المسؤول)، ثم اختر الملف المطابق لتفعيل الرفع.";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
            lblStatus.Location = new Point(20, 638);
            lblStatus.Size = new Size(610, 60);
            this.Controls.Add(lblStatus);

            OnTargetChanged();
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

                    // 1. Fetch Centers Version
                    try
                    {
                        string jsonC = wc.DownloadString(CLOUD_CENTERS_URL + "?t=" + DateTime.Now.Ticks);
                        Match mC = Regex.Match(jsonC, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                        if (mC.Success) cloudCentersVersion = mC.Groups[1].Value;
                    }
                    catch
                    {
                        try
                        {
                            string jsonC = wc.DownloadString(CLOUD_CENTERS_BACKUP + "?t=" + DateTime.Now.Ticks);
                            Match mC = Regex.Match(jsonC, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (mC.Success) cloudCentersVersion = mC.Groups[1].Value;
                        }
                        catch {}
                    }

                    // 2. Fetch Admin Version
                    try
                    {
                        string jsonA = wc.DownloadString(CLOUD_ADMIN_URL + "?t=" + DateTime.Now.Ticks);
                        Match mA = Regex.Match(jsonA, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                        if (mA.Success) cloudAdminVersion = mA.Groups[1].Value;
                    }
                    catch
                    {
                        try
                        {
                            string jsonA = wc.DownloadString(CLOUD_ADMIN_BACKUP + "?t=" + DateTime.Now.Ticks);
                            Match mA = Regex.Match(jsonA, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (mA.Success) cloudAdminVersion = mA.Groups[1].Value;
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
                "ملاحظة: سيتم تحديث هذا البرنامج وإصداره فقط دون المساس بإصدار البرنامج الآخر إطلاقاً.",
                targetName, fname, newVer
            );

            DialogResult res = MessageBox.Show(confirmMsg, "تأكيد النشر السحابي المنفصل", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res != DialogResult.Yes) return;

            btnPublish.Enabled = false;
            btnBrowseFile.Enabled = false;
            rbCenters.Enabled = false;
            rbAdmin.Enabled = false;
            txtNewVer.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lblStatus.Text = "جاري معالجة ونسخ الملف المطابق...";
            lblStatus.ForeColor = Color.FromArgb(2, 132, 199);

            try
            {
                string gitAddArgs = "";
                string nowIso = DateTime.UtcNow.ToString("o");

                // Case 1: Updating ADMIN ONLY (admin.html)
                if (rbAdmin.Checked)
                {
                    SafeCopy(selectedFilePath, Path.Combine(workspaceDir, "admin.html"));
                    SafeCopy(selectedFilePath, Path.Combine(workspaceDir, "public", "admin.html"));
                    cloudAdminVersion = newVer;

                    // Write version-admin.json (ROOT and PUBLIC)
                    string adminJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"admin\"\n}}", newVer, nowIso);
                    File.WriteAllText(Path.Combine(workspaceDir, "version-admin.json"), adminJson);
                    File.WriteAllText(Path.Combine(workspaceDir, "public", "version-admin.json"), adminJson);

                    gitAddArgs = "admin.html public/admin.html version-admin.json public/version-admin.json";
                }
                // Case 2: Updating CENTERS ONLY (index.html)
                else
                {
                    SafeCopy(selectedFilePath, Path.Combine(workspaceDir, "index.html"));
                    SafeCopy(selectedFilePath, Path.Combine(workspaceDir, "public", "index.html"));
                    SafeCopy(selectedFilePath, Path.Combine(workspaceDir, "public", "center.html"));
                    cloudCentersVersion = newVer;

                    // Write version-centers.json (ROOT and PUBLIC)
                    string centersJson = string.Format("{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"system\": \"centers\"\n}}", newVer, nowIso);
                    File.WriteAllText(Path.Combine(workspaceDir, "version-centers.json"), centersJson);
                    File.WriteAllText(Path.Combine(workspaceDir, "public", "version-centers.json"), centersJson);

                    gitAddArgs = "index.html public/index.html public/center.html version-centers.json public/version-centers.json";
                }

                // 2. Also keep combined version.json updated for compatibility
                string combinedJson = string.Format(
                    "{{\n  \"centersVersion\": \"{0}\",\n  \"adminVersion\": \"{1}\",\n  \"version\": \"{2}\",\n  \"updatedAt\": \"{3}\",\n  \"target\": \"{4}\",\n  \"file\": \"{5}\"\n}}",
                    cloudCentersVersion, cloudAdminVersion, newVer, nowIso, targetName, fname
                );
                File.WriteAllText(Path.Combine(workspaceDir, "version.json"), combinedJson);
                File.WriteAllText(Path.Combine(workspaceDir, "public", "version.json"), combinedJson);
                gitAddArgs += " version.json public/version.json";

                // 3. Git Add (Specific files only!)
                lblStatus.Text = "جاري إضافة الملفات المحددة للتحديث (Git Add)...";
                ProcessStartInfo psiAdd = new ProcessStartInfo("git", "add " + gitAddArgs);
                psiAdd.WorkingDirectory = workspaceDir;
                psiAdd.CreateNoWindow = true;
                psiAdd.UseShellExecute = false;
                Process.Start(psiAdd).WaitForExit();

                // 4. Git Commit
                lblStatus.Text = "جاري توثيق التحديث المنفصل (Git Commit)...";
                string commitMsg = string.Format("تحديث {0} إلى الإصدار {1}: {2}", targetName, newVer, fname);
                ProcessStartInfo psiCommit = new ProcessStartInfo("git", string.Format("commit -m \"{0}\"", commitMsg));
                psiCommit.WorkingDirectory = workspaceDir;
                psiCommit.CreateNoWindow = true;
                psiCommit.UseShellExecute = false;
                Process.Start(psiCommit).WaitForExit();

                // 5. Git Pull Rebase before Push
                lblStatus.Text = "جاري مزامنة الفرع السحابي...";
                ProcessStartInfo psiPull = new ProcessStartInfo("git", "pull --rebase origin main");
                psiPull.WorkingDirectory = workspaceDir;
                psiPull.CreateNoWindow = true;
                psiPull.UseShellExecute = false;
                Process.Start(psiPull).WaitForExit();

                // 6. Git Push
                lblStatus.Text = "جاري رفع التحديث السحابي إلى GitHub و Vercel...";
                ProcessStartInfo psiPush = new ProcessStartInfo("git", "push origin main");
                psiPush.WorkingDirectory = workspaceDir;
                psiPush.CreateNoWindow = true;
                psiPush.UseShellExecute = false;
                Process pPush = Process.Start(psiPush);
                pPush.WaitForExit();

                progressBar.Visible = false;

                if (pPush.ExitCode == 0)
                {
                    lblStatus.Text = string.Format("اكتمل الرفع بنجاح! تم تحديث {0} إلى الإصدار ({1}) أونلاين 🚀", targetName, newVer);
                    lblStatus.ForeColor = Color.FromArgb(16, 185, 129);

                    MessageBox.Show(
                        string.Format(
                            "تم رفع ونشر التحديث السحابي بنجاح تام! 🚀\n\n" +
                            "• النظام المحدث: {0}\n" +
                            "• الملف المرفوع: {1}\n" +
                            "• رقم الإصدار الجديد: {2}\n\n" +
                            "تم تحديث هذا البرنامج أونلاين دون تغيير إصدار البرنامج الآخر نهائياً.",
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
                    MessageBox.Show("تعذر إتمام عملية الرفع السحابي. يرجى التأكد من اتصال الإنترنت وإعادة المحاولة.", "خطأ في الرفع", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                progressBar.Visible = false;
                lblStatus.Text = "خطأ: " + ex.Message;
                lblStatus.ForeColor = Color.FromArgb(225, 29, 72);
                MessageBox.Show("حدث خطأ أثناء تنفيذ التحديث:\n" + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnBrowseFile.Enabled = true;
                rbCenters.Enabled = true;
                rbAdmin.Enabled = true;
                txtNewVer.Enabled = true;
                ValidateAll();
            }
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
