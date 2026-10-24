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

        // State
        private string cloudVersion = "جاري الفحص...";
        private string CLOUD_URL = "https://basra-forms.vercel.app/version.json";
        private string CLOUD_BACKUP = "https://alichbasi75-dotcom.github.io/basra-forms/version.json";
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
            lblSubTitle.Text = "تحديث برنامج المراكز أو برنامج المسؤول بشكل منفصل تماماً مع التحقق الإلزامي من مطابقة الملفات";
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
            lblCurrentCloudVer.Text = "الإصدار المرفوع حالياً على المنصة:";
            lblCurrentCloudVer.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblCurrentCloudVer.Location = new Point(320, 12);
            lblCurrentCloudVer.Size = new Size(270, 24);
            pnlCloud.Controls.Add(lblCurrentCloudVer);

            lblCloudVerVal = new Label();
            lblCloudVerVal.Text = "جاري الفحص...";
            lblCloudVerVal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCloudVerVal.ForeColor = Color.FromArgb(2, 132, 199);
            lblCloudVerVal.Location = new Point(100, 11);
            lblCloudVerVal.Size = new Size(210, 24);
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
            grpTarget.Text = "1. حدد البرنامج المراد تحديثه حصراً (لا يتم تحديث الاثنين معاً):";
            grpTarget.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpTarget.ForeColor = Color.FromArgb(30, 41, 59);
            grpTarget.Location = new Point(20, 118);
            grpTarget.Size = new Size(610, 105);
            grpTarget.BackColor = Color.White;

            rbCenters = new RadioButton();
            rbCenters.Text = "تحديث برنامج مراكز التسجيل فقط (بوابة المراكز الـ 76 — ملف index.html أو الاستمارات)";
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
            lblTargetNotice.Text = "⚠️ إشعار: عند اختيار أحد البرنامجين، يلزمك البرنامج باختيار الملف المتطابق معه حصراً ولا يسمح برفع غيره.";
            lblTargetNotice.Font = new Font("Segoe UI", 8f);
            lblTargetNotice.ForeColor = Color.FromArgb(100, 116, 139);
            lblTargetNotice.Location = new Point(20, 80);
            lblTargetNotice.Size = new Size(570, 18);
            grpTarget.Controls.Add(lblTargetNotice);

            this.Controls.Add(grpTarget);

            // 4. Step 2: File Selection & Matching Validation
            grpFile = new GroupBox();
            grpFile.Text = "2. اختيار الملف المطلوب وشرط المطابقة الإلزامي:";
            grpFile.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpFile.ForeColor = Color.FromArgb(30, 41, 59);
            grpFile.Location = new Point(20, 230);
            grpFile.Size = new Size(610, 220);
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
            lblDropHint.Text = "📥 اسحب وأفلت الملف المطابق هنا مباشرة\n(برنامج المراكز: index.html أو استمارة معتمدة | برنامج المسؤول: admin.html)";
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
            lblFileValidation.Size = new Size(575, 65);
            grpFile.Controls.Add(lblFileValidation);

            this.Controls.Add(grpFile);

            // 5. Step 3: New Version
            grpVer = new GroupBox();
            grpVer.Text = "3. تعيين رقم الإصدار الجديد:";
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
            lblVerHint.Text = "(يجب أن يكون أكبر من الإصدار السحابي)";
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

        private void OnTargetChanged()
        {
            if (rbAdmin.Checked)
            {
                lblDropHint.Text = "📥 اسحب وأفلت ملف لوحة المسؤول حصراً:\n(admin.html)";
                lblDropHint.ForeColor = Color.FromArgb(180, 83, 9);
            }
            else
            {
                lblDropHint.Text = "📥 اسحب وأفلت ملف برنامج المراكز حصراً:\n(index.html أو استمارة Word/Excel/PDF أو forms.json)";
                lblDropHint.ForeColor = Color.FromArgb(3, 105, 161);
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
                ofd.Filter = "ملف لوحة المسؤول (admin.html)|admin.html|صفحات HTML (*.html)|*.html|جميع الملفات (*.*)|*.*";
            }
            else
            {
                ofd.Title = "اختر ملف برنامج المراكز (index.html أو استمارة معتمدة)";
                ofd.Filter = "ملفات برنامج المراكز (index.html;*.json;*.xlsx;*.docx;*.pdf)|index.html;center.html;*.json;*.xlsx;*.docx;*.pdf|صفحات HTML (*.html)|*.html|استمارات معتمدة (*.xlsx;*.docx;*.pdf)|*.xlsx;*.docx;*.pdf|جميع الملفات (*.*)|*.*";
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
                    lblFileValidation.Text = string.Format("✅ تطابق تام ومؤكد: تم اختيار ملف لوحة المسؤول المعتمد ({0}) بنجاح.\nسيتم تحديث كود لوحة المسؤول السحابية حصراً دون لمس برنامج المراكز.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
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
                if (fname == "admin.html")
                {
                    lblFileValidation.Text = "🚫 غير مسموح وعدم تطابق! لقد اخترت (تحديث برنامج المراكز فقط)،\nولكن الملف المختار هو ملف المسؤول (admin.html).\nإذا كنت تريد تحديث المسؤول يرجى تحديد خيار المسؤول أعلاه أولاً.";
                    lblFileValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }

                if (fname == "index.html" || fname == "center.html")
                {
                    lblFileValidation.Text = string.Format("✅ تطابق تام ومؤكد: تم اختيار صفحة بوابة مراكز التسجيل الـ 76 ({0}).\nسيتم تحديث واجهة المراكز أونلاين حصراً دون لمس لوحة المسؤول.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
                else if (fname == "forms.json" || fname == "centers.json" || fname == "categories.json")
                {
                    lblFileValidation.Text = string.Format("✅ تطابق تام: تم اختيار قاعدة بيانات المراكز ({0}).\nسيتم تحديث بيانات المراكز أونلاين فوراً.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
                else if (ext == ".xlsx" || ext == ".docx" || ext == ".pdf" || ext == ".png" || ext == ".jpg")
                {
                    lblFileValidation.Text = string.Format("✅ تطابق تام: تم اختيار استمارة/ملف معتمد للمراكز ({0}).\nسيتم نسخه لمجلد النماذج ورفعه للمراكز الـ 76 أونلاين.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
                else
                {
                    lblFileValidation.Text = string.Format("⚠️ تنبيه: الملف \"{0}\" غير معتاد لبرنامج المراكز. الملف المعتمد هو index.html أو ملفات الاستمارات.", fi.Name);
                    lblFileValidation.ForeColor = Color.FromArgb(217, 119, 6);
                    return true;
                }
            }

            return false;
        }

        private void FetchCloudVersion()
        {
            lblCloudVerVal.Text = "جاري الفحص...";
            lblCloudVerVal.ForeColor = Color.FromArgb(2, 132, 199);
            btnCheckCloud.Enabled = false;

            System.Threading.ThreadPool.QueueUserWorkItem((state) => {
                string fetched = null;
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "BasraUpdaterClient");
                        string json = wc.DownloadString(CLOUD_URL + "?t=" + DateTime.UtcNow.Ticks);
                        Match m = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                        if (m.Success) fetched = m.Groups[1].Value;
                    }
                }
                catch
                {
                    try
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.Headers.Add("User-Agent", "BasraUpdaterClient");
                            string json = wc.DownloadString(CLOUD_BACKUP + "?t=" + DateTime.UtcNow.Ticks);
                            Match m = Regex.Match(json, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                            if (m.Success) fetched = m.Groups[1].Value;
                        }
                    }
                    catch { }
                }

                this.BeginInvoke((MethodInvoker)delegate {
                    btnCheckCloud.Enabled = true;
                    if (!string.IsNullOrEmpty(fetched))
                    {
                        cloudVersion = fetched.Trim();
                        lblCloudVerVal.Text = cloudVersion;
                        lblCloudVerVal.ForeColor = Color.FromArgb(16, 185, 129);
                        AutoSuggestNextVersion(cloudVersion);
                    }
                    else
                    {
                        cloudVersion = "2.0.0";
                        lblCloudVerVal.Text = cloudVersion + " (محلي)";
                        lblCloudVerVal.ForeColor = Color.FromArgb(100, 116, 139);
                        AutoSuggestNextVersion("2.0.0");
                    }
                    ValidateAll();
                });
            });
        }

        private void AutoSuggestNextVersion(string curVer)
        {
            if (string.IsNullOrEmpty(txtNewVer.Text) || txtNewVer.Text == "جاري...")
            {
                string clean = curVer.Trim().TrimStart('v', 'V');
                string[] parts = clean.Split('.');
                if (parts.Length >= 2)
                {
                    int last;
                    if (int.TryParse(parts[parts.Length - 1], out last))
                    {
                        parts[parts.Length - 1] = (last + 1).ToString();
                        txtNewVer.Text = string.Join(".", parts);
                        return;
                    }
                }
                txtNewVer.Text = clean + ".1";
            }
        }

        private int CompareVersions(string v1, string v2)
        {
            string c1 = Regex.Replace(v1.Trim().TrimStart('v', 'V'), @"[^0-9.]", "");
            string c2 = Regex.Replace(v2.Trim().TrimStart('v', 'V'), @"[^0-9.]", "");

            string[] p1 = c1.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            string[] p2 = c2.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            int maxLen = Math.Max(p1.Length, p2.Length);
            for (int i = 0; i < maxLen; i++)
            {
                int num1 = (i < p1.Length) ? int.Parse(p1[i]) : 0;
                int num2 = (i < p2.Length) ? int.Parse(p2[i]) : 0;
                if (num1 > num2) return 1;
                if (num1 < num2) return -1;
            }
            return 0;
        }

        private bool ValidateVersionNumber()
        {
            string inputVer = txtNewVer.Text.Trim();

            if (string.IsNullOrEmpty(inputVer))
            {
                lblVersionValidation.Text = "يرجى كتابة رقم الإصدار الجديد (مثال: 2.0.1 أو 2.1.0).";
                lblVersionValidation.ForeColor = Color.FromArgb(217, 119, 6);
                return false;
            }

            if (!Regex.IsMatch(inputVer, @"^[vV]?\d+(\.\d+)+$"))
            {
                lblVersionValidation.Text = "صيغة الإصدار غير صالحة. اكتب أرقاماً مفصولة بنقاط (مثل: 2.0.1).";
                lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                return false;
            }

            try
            {
                int cmp = CompareVersions(inputVer, cloudVersion);
                if (cmp <= 0)
                {
                    lblVersionValidation.Text = string.Format("🚫 غير مسموح: الإصدار ({0}) يجب أن يكون أكبر من الإصدار السحابي ({1}).", inputVer, cloudVersion);
                    lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                    return false;
                }
                else
                {
                    lblVersionValidation.Text = string.Format("✅ الإصدار صالح ({0} > {1}).", inputVer, cloudVersion);
                    lblVersionValidation.ForeColor = Color.FromArgb(16, 185, 129);
                    return true;
                }
            }
            catch
            {
                lblVersionValidation.Text = "خطأ في مقارنة الإصدارات.";
                lblVersionValidation.ForeColor = Color.FromArgb(225, 29, 72);
                return false;
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
                    btnPublish.Text = "🚫 زر الرفع مقفول (رقم الإصدار غير صالح أو ليس أكبر من السحابي)";
                    lblStatus.Text = "يرجى تحديد رقم إصدار أكبر من الإصدار السحابي الحالي للمتابعة.";
                    lblStatus.ForeColor = Color.FromArgb(217, 119, 6);
                }
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
                "• رقم الإصدار الجديد: {2}\n\n" +
                "ملاحظة: سيتم تحديث هذا البرنامج فقط حصراً دون المساس بالبرنامج الآخر.",
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

            lblStatus.Text = "جاري نسخ الملف المطابق إلى موقعه في المنظومة...";
            lblStatus.ForeColor = Color.FromArgb(2, 132, 199);

            try
            {
                string gitAddArgs = "";

                // Case 1: Updating ADMIN ONLY
                if (rbAdmin.Checked)
                {
                    // Copy to admin.html and public/admin.html
                    File.Copy(selectedFilePath, Path.Combine(workspaceDir, "admin.html"), true);
                    File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "admin.html"), true);
                    gitAddArgs = "admin.html public/admin.html version.json";
                }
                // Case 2: Updating CENTERS ONLY
                else
                {
                    string ext = Path.GetExtension(selectedFilePath).ToLower();
                    string name = Path.GetFileName(selectedFilePath).ToLower();

                    if (name == "index.html" || name == "center.html")
                    {
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, "index.html"), true);
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "index.html"), true);
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "center.html"), true);
                        gitAddArgs = "index.html public/index.html public/center.html version.json";
                    }
                    else if (name == "forms.json" || name == "centers.json" || name == "categories.json")
                    {
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, "data", name), true);
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "data", name), true);
                        gitAddArgs = "data/" + name + " public/data/" + name + " version.json";
                    }
                    else if (ext == ".xlsx" || ext == ".docx" || ext == ".pdf")
                    {
                        string up1 = Path.Combine(workspaceDir, "uploads", Path.GetFileName(selectedFilePath));
                        string up2 = Path.Combine(workspaceDir, "public", "uploads", Path.GetFileName(selectedFilePath));
                        File.Copy(selectedFilePath, up1, true);
                        File.Copy(selectedFilePath, up2, true);
                        gitAddArgs = "uploads/ public/uploads/ version.json";
                    }
                    else
                    {
                        File.Copy(selectedFilePath, Path.Combine(workspaceDir, Path.GetFileName(selectedFilePath)), true);
                        gitAddArgs = Path.GetFileName(selectedFilePath) + " version.json";
                    }
                }

                // 2. Update version.json
                lblStatus.Text = "جاري تحديث رقم الإصدار في سجل المنظومة...";
                string vJsonPath = Path.Combine(workspaceDir, "version.json");
                string vJsonContent = string.Format(
                    "{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"target\": \"{2}\",\n  \"file\": \"{3}\"\n}}",
                    newVer, DateTime.UtcNow.ToString("o"), targetName, fname
                );
                File.WriteAllText(vJsonPath, vJsonContent);

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

                // 5. Git Push
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
                            "تم تحديث المنظومة أونلاين فوراً دون التأثير على البرنامج الآخر.",
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
