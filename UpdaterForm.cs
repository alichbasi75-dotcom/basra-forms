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

        // Cloud Version Panel
        private Panel pnlCloud;
        private Label lblCurrentCloudVer;
        private Label lblCloudVerVal;
        private Button btnCheckCloud;

        // File Selection Box
        private GroupBox grpFile;
        private Label lblFilePath;
        private TextBox txtFilePath;
        private Button btnBrowseFile;
        private Button btnBrowseFolder;
        private Label lblDestType;
        private ComboBox cmbDestType;
        private Label lblFileInfo;
        private Panel pnlDropZone;
        private Label lblDropHint;

        // Version & Target Box
        private GroupBox grpVer;
        private Label lblNewVer;
        private TextBox txtNewVer;
        private Label lblVerHint;
        private Label lblValidationMsg;

        // Publish Action
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
        private bool isDirectory = false;

        public MainForm()
        {
            InitializeComponent();
            FetchCloudVersion();
        }

        private void InitializeComponent()
        {
            this.Text = "أداة رفع وتحديث منظومة استمارات البصرة - الإصدارات المعتمدة";
            this.Size = new Size(650, 710);
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
            lblTitle.Text = "منظومة استمارات البصرة — مركز التحديث والنشر السحابي";
            lblTitle.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(20, 12);
            lblTitle.Size = new Size(590, 26);
            this.Controls.Add(lblTitle);

            lblSubTitle = new Label();
            lblSubTitle.Text = "اختر الملف أو الاستمارة المراد تحديثها، وحدد الإصدار الجديد لرفع التعديلات للسحاب فوراً";
            lblSubTitle.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblSubTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubTitle.Location = new Point(20, 38);
            lblSubTitle.Size = new Size(590, 20);
            this.Controls.Add(lblSubTitle);

            // 2. Cloud Version Bar
            pnlCloud = new Panel();
            pnlCloud.Location = new Point(20, 65);
            pnlCloud.Size = new Size(590, 52);
            pnlCloud.BackColor = Color.FromArgb(241, 245, 249);
            pnlCloud.BorderStyle = BorderStyle.FixedSingle;

            lblCurrentCloudVer = new Label();
            lblCurrentCloudVer.Text = "الإصدار المرفوع حالياً على المنصة:";
            lblCurrentCloudVer.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblCurrentCloudVer.Location = new Point(310, 14);
            lblCurrentCloudVer.Size = new Size(260, 25);
            pnlCloud.Controls.Add(lblCurrentCloudVer);

            lblCloudVerVal = new Label();
            lblCloudVerVal.Text = "جاري الفحص...";
            lblCloudVerVal.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblCloudVerVal.ForeColor = Color.FromArgb(2, 132, 199);
            lblCloudVerVal.Location = new Point(100, 13);
            lblCloudVerVal.Size = new Size(200, 25);
            pnlCloud.Controls.Add(lblCloudVerVal);

            btnCheckCloud = new Button();
            btnCheckCloud.Text = "تحديث 🔄";
            btnCheckCloud.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnCheckCloud.Location = new Point(10, 10);
            btnCheckCloud.Size = new Size(80, 30);
            btnCheckCloud.BackColor = Color.White;
            btnCheckCloud.Cursor = Cursors.Hand;
            btnCheckCloud.Click += (s, e) => FetchCloudVersion();
            pnlCloud.Controls.Add(btnCheckCloud);

            this.Controls.Add(pnlCloud);

            // 3. File Selection GroupBox
            grpFile = new GroupBox();
            grpFile.Text = "1. اختيار الملف أو التحديث المطلوب رفعه";
            grpFile.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpFile.ForeColor = Color.FromArgb(30, 41, 59);
            grpFile.Location = new Point(20, 128);
            grpFile.Size = new Size(590, 260);
            grpFile.BackColor = Color.White;

            lblFilePath = new Label();
            lblFilePath.Text = "مسار الملف المختار:";
            lblFilePath.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblFilePath.Location = new Point(460, 28);
            lblFilePath.Size = new Size(115, 20);
            grpFile.Controls.Add(lblFilePath);

            txtFilePath = new TextBox();
            txtFilePath.Location = new Point(210, 25);
            txtFilePath.Size = new Size(245, 26);
            txtFilePath.Font = new Font("Segoe UI", 9f);
            txtFilePath.ReadOnly = true;
            txtFilePath.BackColor = Color.FromArgb(248, 250, 252);
            grpFile.Controls.Add(txtFilePath);

            btnBrowseFile = new Button();
            btnBrowseFile.Text = "اختيار ملف 📂";
            btnBrowseFile.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnBrowseFile.Location = new Point(110, 23);
            btnBrowseFile.Size = new Size(95, 30);
            btnBrowseFile.BackColor = Color.FromArgb(224, 242, 254);
            btnBrowseFile.ForeColor = Color.FromArgb(3, 105, 161);
            btnBrowseFile.Cursor = Cursors.Hand;
            btnBrowseFile.Click += BtnBrowseFile_Click;
            grpFile.Controls.Add(btnBrowseFile);

            btnBrowseFolder = new Button();
            btnBrowseFolder.Text = "مجلد كامل 📁";
            btnBrowseFolder.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnBrowseFolder.Location = new Point(15, 23);
            btnBrowseFolder.Size = new Size(90, 30);
            btnBrowseFolder.BackColor = Color.FromArgb(241, 245, 249);
            btnBrowseFolder.ForeColor = Color.FromArgb(51, 65, 85);
            btnBrowseFolder.Cursor = Cursors.Hand;
            btnBrowseFolder.Click += BtnBrowseFolder_Click;
            grpFile.Controls.Add(btnBrowseFolder);

            // Drag and drop dropzone
            pnlDropZone = new Panel();
            pnlDropZone.Location = new Point(15, 62);
            pnlDropZone.Size = new Size(560, 75);
            pnlDropZone.BackColor = Color.FromArgb(248, 250, 252);
            pnlDropZone.BorderStyle = BorderStyle.FixedSingle;
            pnlDropZone.AllowDrop = true;
            pnlDropZone.DragEnter += MainForm_DragEnter;
            pnlDropZone.DragDrop += MainForm_DragDrop;
            pnlDropZone.Cursor = Cursors.Hand;
            pnlDropZone.Click += (s, e) => BtnBrowseFile_Click(s, e);

            lblDropHint = new Label();
            lblDropHint.Text = "📥 اسحب وأفلت أي ملف (استمارة Word / Excel / PDF / صفحة HTML) هنا مباشرة\nأو انقر هنا لتصفح واختيار الملف من حاسبتك";
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

            // File Info Label
            lblFileInfo = new Label();
            lblFileInfo.Text = "لم يتم اختيار أي ملف بعد (اختياري: يمكنك النشر بدون ملفات جديدة إذا أردت تحديث الإصدار فقط).";
            lblFileInfo.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblFileInfo.ForeColor = Color.FromArgb(100, 116, 139);
            lblFileInfo.Location = new Point(15, 142);
            lblFileInfo.Size = new Size(560, 36);
            grpFile.Controls.Add(lblFileInfo);

            // Destination Dropdown
            lblDestType = new Label();
            lblDestType.Text = "الوجهة المعتمدة للملف داخل المنظومة:";
            lblDestType.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblDestType.Location = new Point(310, 185);
            lblDestType.Size = new Size(265, 20);
            grpFile.Controls.Add(lblDestType);

            cmbDestType = new ComboBox();
            cmbDestType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDestType.Font = new Font("Segoe UI", 9f);
            cmbDestType.Location = new Point(15, 210);
            cmbDestType.Size = new Size(560, 28);
            cmbDestType.Items.Add("استمارة معتمدة للمراكز — تُنسخ إلى مجلد uploads/ وتتاح للتحميل والطباعة فوراً");
            cmbDestType.Items.Add("بوابة مراكز التسجيل الـ 76 — ملف index.html و center.html");
            cmbDestType.Items.Add("لوحة تحكم المسؤول السحابية — ملف admin.html");
            cmbDestType.Items.Add("قاعدة بيانات النماذج المعتمدة — ملف data/forms.json");
            cmbDestType.Items.Add("قاعدة بيانات مراكز التسجيل الـ 76 — ملف data/centers.json");
            cmbDestType.Items.Add("تصنيفات النماذج — ملف data/categories.json");
            cmbDestType.Items.Add("صور النماذج الممسوحة — مجلد sheets/");
            cmbDestType.Items.Add("ملف رئيسي في مجلد المنظومة الأساسي");
            cmbDestType.SelectedIndex = 0;
            grpFile.Controls.Add(cmbDestType);

            this.Controls.Add(grpFile);

            // 4. Version GroupBox
            grpVer = new GroupBox();
            grpVer.Text = "2. تعيين رقم الإصدار الجديد";
            grpVer.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grpVer.ForeColor = Color.FromArgb(30, 41, 59);
            grpVer.Location = new Point(20, 396);
            grpVer.Size = new Size(590, 108);
            grpVer.BackColor = Color.White;

            lblNewVer = new Label();
            lblNewVer.Text = "رقم الإصدار الجديد المراد رفعه:";
            lblNewVer.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            lblNewVer.Location = new Point(380, 28);
            lblNewVer.Size = new Size(195, 22);
            grpVer.Controls.Add(lblNewVer);

            txtNewVer = new TextBox();
            txtNewVer.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            txtNewVer.Location = new Point(220, 25);
            txtNewVer.Size = new Size(150, 30);
            txtNewVer.TextAlign = HorizontalAlignment.Center;
            txtNewVer.RightToLeft = RightToLeft.No;
            txtNewVer.TextChanged += (s, e) => ValidateVersion();
            grpVer.Controls.Add(txtNewVer);

            lblVerHint = new Label();
            lblVerHint.Text = "(يجب أن يكون أكبر من الإصدار السحابي)";
            lblVerHint.Font = new Font("Segoe UI", 8f);
            lblVerHint.ForeColor = Color.FromArgb(148, 163, 184);
            lblVerHint.Location = new Point(15, 30);
            lblVerHint.Size = new Size(195, 20);
            grpVer.Controls.Add(lblVerHint);

            lblValidationMsg = new Label();
            lblValidationMsg.Text = "جاري التحقق من رقم الإصدار...";
            lblValidationMsg.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblValidationMsg.ForeColor = Color.FromArgb(217, 119, 6);
            lblValidationMsg.Location = new Point(15, 65);
            lblValidationMsg.Size = new Size(560, 25);
            grpVer.Controls.Add(lblValidationMsg);

            this.Controls.Add(grpVer);

            // 5. Actions Area
            btnPublish = new Button();
            btnPublish.Text = "🚀 رفع ونشر التحديث إلى السحاب (Vercel & GitHub)";
            btnPublish.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnPublish.Location = new Point(20, 515);
            btnPublish.Size = new Size(410, 48);
            btnPublish.BackColor = Color.FromArgb(16, 185, 129);
            btnPublish.ForeColor = Color.White;
            btnPublish.Cursor = Cursors.Hand;
            btnPublish.FlatStyle = FlatStyle.Flat;
            btnPublish.FlatAppearance.BorderSize = 0;
            btnPublish.Enabled = false;
            btnPublish.Click += BtnPublish_Click;
            this.Controls.Add(btnPublish);

            btnOpenFolder = new Button();
            btnOpenFolder.Text = "📂 مجلد المنظومة";
            btnOpenFolder.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnOpenFolder.Location = new Point(440, 515);
            btnOpenFolder.Size = new Size(170, 48);
            btnOpenFolder.BackColor = Color.FromArgb(241, 245, 249);
            btnOpenFolder.ForeColor = Color.FromArgb(51, 65, 85);
            btnOpenFolder.Cursor = Cursors.Hand;
            btnOpenFolder.Click += (s, e) => {
                if (Directory.Exists(workspaceDir)) Process.Start("explorer.exe", workspaceDir);
            };
            this.Controls.Add(btnOpenFolder);

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Location = new Point(20, 575);
            progressBar.Size = new Size(590, 18);
            progressBar.Visible = false;
            this.Controls.Add(progressBar);

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "جاهز. حدد الملف المطلوب ورقم الإصدار ثم اضغط رفع ونشر التحديث.";
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(71, 85, 105);
            lblStatus.Location = new Point(20, 600);
            lblStatus.Size = new Size(590, 50);
            this.Controls.Add(lblStatus);
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
                SetSelectedFileOrFolder(files[0]);
            }
        }

        private void BtnBrowseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "اختر ملف التحديث المطلوب رفعه للمنظومة";
            ofd.Filter = "كافة ملفات التحديث (*.html;*.json;*.xlsx;*.docx;*.pdf;*.png;*.exe)|*.html;*.json;*.xlsx;*.docx;*.pdf;*.png;*.exe|استمارات ونماذج (*.xlsx;*.docx;*.pdf)|*.xlsx;*.docx;*.pdf|صفحات وبرمجة المنظومة (*.html;*.json;*.js)|*.html;*.json;*.js|جميع الملفات (*.*)|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                SetSelectedFileOrFolder(ofd.FileName);
            }
        }

        private void BtnBrowseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "اختر المجلد الذي يحتوي على التحديثات";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SetSelectedFileOrFolder(fbd.SelectedPath);
            }
        }

        private void SetSelectedFileOrFolder(string path)
        {
            selectedFilePath = path;
            txtFilePath.Text = path;

            if (Directory.Exists(path))
            {
                isDirectory = true;
                lblFileInfo.Text = "📁 تم اختيار مجلد تحديثات كامل: " + Path.GetFileName(path) + " (سيتم نسخ محتوياته إلى مجلد المنظومة).";
                lblFileInfo.ForeColor = Color.FromArgb(3, 105, 161);
                cmbDestType.SelectedIndex = 7; // Root workspace
            }
            else if (File.Exists(path))
            {
                isDirectory = false;
                FileInfo fi = new FileInfo(path);
                string ext = fi.Extension.ToLower();
                string name = fi.Name.ToLower();

                double sizeKb = fi.Length / 1024.0;
                string sizeStr = sizeKb > 1024 ? string.Format("{0:F1} MB", sizeKb / 1024.0) : string.Format("{0:F1} KB", sizeKb);

                // Smart Destination Detection
                if (name == "admin.html")
                {
                    cmbDestType.SelectedIndex = 2; // admin.html
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: صفحة لوحة تحكم المسؤول ({0}) — سيتم تحديث رابط المسؤول أونلاين فوراً.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (name == "index.html" || name == "center.html")
                {
                    cmbDestType.SelectedIndex = 1; // index.html
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: بوابة مراكز التسجيل الـ 76 ({0}) — سيتم تحديث واجهة المراكز أونلاين فوراً.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (name == "forms.json")
                {
                    cmbDestType.SelectedIndex = 3; // forms.json
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: قاعدة بيانات النماذج ({0}) — سيتم تحديث الاستمارات المعروضة لكافة المراكز.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (name == "centers.json")
                {
                    cmbDestType.SelectedIndex = 4; // centers.json
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: قاعدة بيانات المراكز الـ 76 ({0}) — سيتم تحديث كودات وبيانات المراكز.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (name == "categories.json")
                {
                    cmbDestType.SelectedIndex = 5; // categories.json
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: تصنيفات النماذج ({0}) — سيتم تحديث تصنيفات الاستمارات.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (ext == ".xlsx" || ext == ".xls" || ext == ".docx" || ext == ".doc" || ext == ".pdf")
                {
                    cmbDestType.SelectedIndex = 0; // uploads/
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: استمارة رسمية معتمدة ({0}) — سيتم نسخها إلى uploads/ وإتاحتها للتحميل فوراً للمراكز.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                {
                    cmbDestType.SelectedIndex = 6; // sheets/
                    lblFileInfo.Text = string.Format("✅ تم التعرف التلقائي: صورة معاينة استمارة ({0}) — سيتم نسخها إلى sheets/.", sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(16, 185, 129);
                }
                else
                {
                    cmbDestType.SelectedIndex = 7; // Root
                    lblFileInfo.Text = string.Format("📄 ملف تم اختياره: {0} ({1}). يرجى التأكد من الوجهة المحددة أدناه.", fi.Name, sizeStr);
                    lblFileInfo.ForeColor = Color.FromArgb(51, 65, 85);
                }
            }
            ValidateVersion();
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
                    ValidateVersion();
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

        private void ValidateVersion()
        {
            string inputVer = txtNewVer.Text.Trim();

            if (string.IsNullOrEmpty(inputVer))
            {
                lblValidationMsg.Text = "يرجى كتابة رقم الإصدار الجديد (مثال: 2.0.1 أو 2.1.0).";
                lblValidationMsg.ForeColor = Color.FromArgb(217, 119, 6);
                btnPublish.Enabled = false;
                return;
            }

            if (!Regex.IsMatch(inputVer, @"^[vV]?\d+(\.\d+)+$"))
            {
                lblValidationMsg.Text = "صيغة الإصدار غير صالحة. اكتب أرقاماً مفصولة بنقاط (مثل: 2.0.1).";
                lblValidationMsg.ForeColor = Color.FromArgb(225, 29, 72);
                btnPublish.Enabled = false;
                return;
            }

            try
            {
                int cmp = CompareVersions(inputVer, cloudVersion);
                if (cmp <= 0)
                {
                    lblValidationMsg.Text = string.Format("🚫 غير مسموح: الإصدار ({0}) يجب أن يكون أكبر من الإصدار السحابي الحالي ({1}).", inputVer, cloudVersion);
                    lblValidationMsg.ForeColor = Color.FromArgb(225, 29, 72);
                    btnPublish.Enabled = false;
                }
                else
                {
                    lblValidationMsg.Text = string.Format("✅ الإصدار صالح ({0} > {1}). النظام جاهز للرفع والتحديث.", inputVer, cloudVersion);
                    lblValidationMsg.ForeColor = Color.FromArgb(16, 185, 129);
                    btnPublish.Enabled = true;
                }
            }
            catch
            {
                lblValidationMsg.Text = "خطأ في مقارنة الإصدارات.";
                lblValidationMsg.ForeColor = Color.FromArgb(225, 29, 72);
                btnPublish.Enabled = false;
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSub = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSub);
            }
        }

        private void BtnPublish_Click(object sender, EventArgs e)
        {
            string newVer = txtNewVer.Text.Trim();
            string destText = cmbDestType.SelectedItem != null ? cmbDestType.SelectedItem.ToString() : "";
            string fileNameOnly = !string.IsNullOrEmpty(selectedFilePath) ? Path.GetFileName(selectedFilePath) : "بدون ملفات إضافية";

            string msg = string.Format(
                "هل أنت متأكد من رغبتك في رفع ونشر هذا التحديث إلى السحاب؟\n\n" +
                "• رقم الإصدار الجديد: {0}\n" +
                "• الملف المختار: {1}\n" +
                "• الوجهة المحددة: {2}\n\n" +
                "سيتم نشر التحديث تلقائياً وتحديث بوابة المراكز الـ 76 ولوحة المسؤول أونلاين فوراً.",
                newVer, fileNameOnly, destText
            );

            DialogResult confirm = MessageBox.Show(msg, "تأكيد رفع ونشر التحديث السحابي", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            btnPublish.Enabled = false;
            btnBrowseFile.Enabled = false;
            btnBrowseFolder.Enabled = false;
            txtNewVer.Enabled = false;
            cmbDestType.Enabled = false;
            progressBar.Visible = true;
            progressBar.Style = ProgressBarStyle.Marquee;

            lblStatus.Text = "جاري معالجة الملفات المحددة...";
            lblStatus.ForeColor = Color.FromArgb(2, 132, 199);

            try
            {
                // 1. Process File/Folder Copy if selected
                if (!string.IsNullOrEmpty(selectedFilePath))
                {
                    if (isDirectory && Directory.Exists(selectedFilePath))
                    {
                        lblStatus.Text = "جاري نسخ مجلد التحديثات إلى مجلد المنظومة...";
                        CopyDirectory(selectedFilePath, workspaceDir);
                    }
                    else if (File.Exists(selectedFilePath))
                    {
                        string fname = Path.GetFileName(selectedFilePath);
                        lblStatus.Text = "جاري نسخ الملف: " + fname + " إلى موقعه المعتمد...";

                        int destIdx = cmbDestType.SelectedIndex;
                        switch (destIdx)
                        {
                            case 0: // uploads/
                                string upDir = Path.Combine(workspaceDir, "uploads");
                                string pubUpDir = Path.Combine(workspaceDir, "public", "uploads");
                                if (!Directory.Exists(upDir)) Directory.CreateDirectory(upDir);
                                if (!Directory.Exists(pubUpDir)) Directory.CreateDirectory(pubUpDir);
                                File.Copy(selectedFilePath, Path.Combine(upDir, fname), true);
                                File.Copy(selectedFilePath, Path.Combine(pubUpDir, fname), true);
                                break;

                            case 1: // index.html / center.html
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, "index.html"), true);
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "index.html"), true);
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "center.html"), true);
                                break;

                            case 2: // admin.html
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, "admin.html"), true);
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, "public", "admin.html"), true);
                                break;

                            case 3: // data/forms.json
                                string dataDir = Path.Combine(workspaceDir, "data");
                                string pubDataDir = Path.Combine(workspaceDir, "public", "data");
                                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                                if (!Directory.Exists(pubDataDir)) Directory.CreateDirectory(pubDataDir);
                                File.Copy(selectedFilePath, Path.Combine(dataDir, "forms.json"), true);
                                File.Copy(selectedFilePath, Path.Combine(pubDataDir, "forms.json"), true);
                                break;

                            case 4: // data/centers.json
                                string dDir = Path.Combine(workspaceDir, "data");
                                string pdDir = Path.Combine(workspaceDir, "public", "data");
                                File.Copy(selectedFilePath, Path.Combine(dDir, "centers.json"), true);
                                File.Copy(selectedFilePath, Path.Combine(pdDir, "centers.json"), true);
                                break;

                            case 5: // data/categories.json
                                string catDir = Path.Combine(workspaceDir, "data");
                                string pubCatDir = Path.Combine(workspaceDir, "public", "data");
                                File.Copy(selectedFilePath, Path.Combine(catDir, "categories.json"), true);
                                File.Copy(selectedFilePath, Path.Combine(pubCatDir, "categories.json"), true);
                                break;

                            case 6: // sheets/
                                string shDir = Path.Combine(workspaceDir, "sheets");
                                string pubShDir = Path.Combine(workspaceDir, "public", "sheets");
                                if (!Directory.Exists(shDir)) Directory.CreateDirectory(shDir);
                                if (!Directory.Exists(pubShDir)) Directory.CreateDirectory(pubShDir);
                                File.Copy(selectedFilePath, Path.Combine(shDir, fname), true);
                                File.Copy(selectedFilePath, Path.Combine(pubShDir, fname), true);
                                break;

                            default: // Root
                                File.Copy(selectedFilePath, Path.Combine(workspaceDir, fname), true);
                                break;
                        }
                    }
                }

                // 2. Update version.json
                lblStatus.Text = "جاري تحديث رقم الإصدار في المنظومة...";
                string vJsonPath = Path.Combine(workspaceDir, "version.json");
                string vJsonContent = string.Format(
                    "{{\n  \"version\": \"{0}\",\n  \"updatedAt\": \"{1}\",\n  \"file\": \"{2}\"\n}}",
                    newVer, DateTime.UtcNow.ToString("o"), fileNameOnly
                );
                File.WriteAllText(vJsonPath, vJsonContent);

                // 3. Git Add, Commit and Push
                lblStatus.Text = "جاري تجهيز التحديثات وإضافتها إلى مستودع السحاب (Git Add)...";

                ProcessStartInfo psiAdd = new ProcessStartInfo("git", "add -A");
                psiAdd.WorkingDirectory = workspaceDir;
                psiAdd.CreateNoWindow = true;
                psiAdd.UseShellExecute = false;
                Process.Start(psiAdd).WaitForExit();

                lblStatus.Text = "جاري توثيق التحديث (Git Commit)...";
                ProcessStartInfo psiCommit = new ProcessStartInfo("git", string.Format("commit -m \"تحديث سحابي إصدار {0}: {1}\"", newVer, fileNameOnly));
                psiCommit.WorkingDirectory = workspaceDir;
                psiCommit.CreateNoWindow = true;
                psiCommit.UseShellExecute = false;
                Process.Start(psiCommit).WaitForExit();

                lblStatus.Text = "جاري إرسال التحديث إلى GitHub و Vercel (Git Push)...";
                ProcessStartInfo psiPush = new ProcessStartInfo("git", "push origin main");
                psiPush.WorkingDirectory = workspaceDir;
                psiPush.CreateNoWindow = true;
                psiPush.UseShellExecute = false;
                Process pPush = Process.Start(psiPush);
                pPush.WaitForExit();

                progressBar.Visible = false;

                if (pPush.ExitCode == 0)
                {
                    lblStatus.Text = string.Format("اكتمل الرفع بنجاح! الإصدار الجديد ({0}) منشور الآن لكافة المراكز أونلاين 🚀", newVer);
                    lblStatus.ForeColor = Color.FromArgb(16, 185, 129);

                    MessageBox.Show(
                        string.Format(
                            "تم رفع ونشر التحديث السحابي بنجاح تام! 🚀\n\n" +
                            "• الإصدار الجديد: {0}\n" +
                            "• الملف المرفوع: {1}\n\n" +
                            "تم تحديث منصة السحاب أونلاين لجميع مراكز التسجيل الـ 76 ولوحة المسؤول.",
                            newVer, fileNameOnly
                        ),
                        "تم التحديث السحابي بنجاح",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    FetchCloudVersion();
                }
                else
                {
                    lblStatus.Text = "حدث خطأ أثناء الاتصال بالسحاب. يرجى التحقق من الإنترنت.";
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
                btnBrowseFolder.Enabled = true;
                txtNewVer.Enabled = true;
                cmbDestType.Enabled = true;
                ValidateVersion();
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
