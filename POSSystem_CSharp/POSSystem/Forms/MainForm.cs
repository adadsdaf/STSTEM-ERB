using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem.Forms
{
    public class MainForm : Form
    {
        private User _currentUser;
        private Invoice _currentInvoice;
        private AppSettings _settings;
        private List<Item> _allItems = new();
        private List<Category> _categories = new();
        private int? _selectedCategoryId = null;

        // Controls
        private Panel _itemsPanel = null!;
        private DataGridView _invoiceGrid = null!;
        private Label _subtotalLbl = null!, _taxLbl = null!, _discountLbl = null!, _totalLbl = null!;
        private Label _invoiceNumLbl = null!, _cashierLbl = null!, _statusBar = null!;
        private TextBox _searchBox = null!;
        private Panel _categoriesPanel = null!;
        private NumericUpDown _discountInput = null!;
        private ComboBox _orderTypeCombo = null!, _tableCombo = null!, _customerCombo = null!;

        public MainForm(User user)
        {
            _currentUser = user;
            _settings = SettingsService.Get();
            _currentInvoice = InvoiceService.CreateNew(user.Id);
            InitializeComponent();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
            LoadData();
            UpdateInvoiceDisplay();
        }

        private void InitializeComponent()
        {
            this.Text = $"نقطة المبيعات - {_settings.StoreName}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            BuildTopBar();
            BuildLeftPanel();
            BuildCenterPanel();
            BuildRightPanel();
            BuildBottomBar();
        }

        // ──────────────── TOP BAR ────────────────────────────────────────
        private void BuildTopBar()
        {
            var topBar = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = Color.FromArgb(30, 35, 60) };

            var titleLbl = new Label { Text = _settings.StoreName, Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(topBar.Width - 250, 0), Size = new Size(240, 52), Anchor = AnchorStyles.Top | AnchorStyles.Right };

            _invoiceNumLbl = new Label { Font = new Font("Arial", 11), ForeColor = Color.FromArgb(180, 200, 255),
                TextAlign = ContentAlignment.MiddleCenter, Location = new Point(300, 0), Size = new Size(280, 52) };

            _cashierLbl = new Label { Text = $"الكاشير: {_currentUser.Name}", Font = new Font("Arial", 11),
                ForeColor = Color.FromArgb(180,220,180), TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(10, 0), Size = new Size(280, 52) };

            var timeLbl = new Label { Font = new Font("Arial", 11), ForeColor = Color.FromArgb(200, 200, 220),
                TextAlign = ContentAlignment.MiddleLeft, Location = new Point(600, 0), Size = new Size(200, 52) };
            var clock = new System.Windows.Forms.Timer { Interval = 1000 };
            clock.Tick += (s, e) => timeLbl.Text = DateTime.Now.ToString("HH:mm:ss  dd/MM/yyyy");
            clock.Start();

            var btnManage = MakeTopBtn("الإدارة", Color.FromArgb(70, 80, 150));
            btnManage.Location = new Point(topBar.Width - 520, 8);
            btnManage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnManage.Click += (s, e) => { using var f = new ManagementForm(_currentUser); f.ShowDialog(); LoadData(); };

            var btnLogout = MakeTopBtn("تسجيل خروج", Color.FromArgb(150, 60, 60));
            btnLogout.Location = new Point(topBar.Width - 400, 8);
            btnLogout.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogout.Click += (s, e) => { this.Close(); };

            topBar.Controls.AddRange(new Control[] { titleLbl, _invoiceNumLbl, _cashierLbl, timeLbl, btnManage, btnLogout });
            this.Controls.Add(topBar);
        }

        private Button MakeTopBtn(string text, Color bg)
        {
            var btn = new Button { Text = text, Size = new Size(110, 36), BackColor = bg, ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ──────────────── LEFT PANEL (Invoice + Totals) ───────────────────
        private void BuildLeftPanel()
        {
            var leftPanel = new Panel { Dock = DockStyle.Left, Width = 380, BackColor = Color.FromArgb(250, 252, 255), Padding = new Padding(8) };

            // Invoice header controls
            var headerPanel = new Panel { Dock = DockStyle.Top, Height = 145, Padding = new Padding(4) };

            _orderTypeCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 10),
                Location = new Point(5, 5), Size = new Size(170, 28) };
            _orderTypeCombo.Items.AddRange(new object[] { "صالة - Dine In", "تيك اوي", "توصيل" });
            _orderTypeCombo.SelectedIndex = 0;

            _tableCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 10),
                Location = new Point(185, 5), Size = new Size(180, 28) };
            _tableCombo.Items.Add("(بدون طاولة)");
            for (int i = 1; i <= 20; i++) _tableCombo.Items.Add($"طاولة {i}");
            _tableCombo.SelectedIndex = 0;

            var custLbl = new Label { Text = "العميل:", Location = new Point(5, 40), Size = new Size(60, 25), Font = new Font("Arial", 10), TextAlign = ContentAlignment.MiddleRight };
            _customerCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Arial", 10),
                Location = new Point(70, 38), Size = new Size(295, 28) };
            _customerCombo.Items.Add(new Customer { Id = 0, Name = "(بدون عميل)" });
            _customerCombo.DisplayMember = "Name"; _customerCombo.ValueMember = "Id";
            _customerCombo.SelectedIndex = 0;

            var discLbl = new Label { Text = "خصم:", Location = new Point(5, 75), Size = new Size(55, 28), Font = new Font("Arial", 10), TextAlign = ContentAlignment.MiddleRight };
            _discountInput = new NumericUpDown { Location = new Point(65, 74), Size = new Size(110, 28), Font = new Font("Arial", 10),
                DecimalPlaces = 2, Maximum = 9999999, ThousandsSeparator = true };
            _discountInput.ValueChanged += (s, e) => { _currentInvoice.DiscountAmount = _discountInput.Value; UpdateTotals(); };

            var notesBox = new TextBox { PlaceholderText = "ملاحظات الفاتورة...", Location = new Point(5, 108), Size = new Size(360, 28), Font = new Font("Arial", 10) };

            headerPanel.Controls.AddRange(new Control[] { _orderTypeCombo, _tableCombo, custLbl, _customerCombo, discLbl, _discountInput, notesBox });

            // Invoice grid
            _invoiceGrid = new DataGridView
            {
                Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = false,
                AllowUserToAddRows = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, Font = new Font("Arial", 10), RowHeadersVisible = false,
                BackgroundColor = Color.White, GridColor = Color.FromArgb(220, 225, 235),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(50, 80, 160), ForeColor = Color.White, Font = new Font("Arial", 10, FontStyle.Bold) },
                EnableHeadersVisualStyles = false, RowTemplate = { Height = 34 }
            };

            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "الصنف", DataPropertyName = "Name", Width = 130, ReadOnly = true });
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "السعر", DataPropertyName = "Price", Width = 70, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            var qtyCol = new DataGridViewTextBoxColumn { HeaderText = "الكمية", DataPropertyName = "Quantity", Width = 60 };
            _invoiceGrid.Columns.Add(qtyCol);
            _invoiceGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "الإجمالي", DataPropertyName = "Total", Width = 80, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });

            _invoiceGrid.CellValueChanged += InvoiceGrid_CellValueChanged;
            _invoiceGrid.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveSelectedItem(); };

            // Totals panel
            var totalsPanel = new Panel { Dock = DockStyle.Bottom, Height = 175, BackColor = Color.FromArgb(240, 242, 248), Padding = new Padding(6) };
            _subtotalLbl = MakeTotalLabel("المجموع:", "", totalsPanel, 5);
            _taxLbl = MakeTotalLabel("الضريبة:", "", totalsPanel, 35);
            _discountLbl = MakeTotalLabel("الخصم:", "", totalsPanel, 65);
            _totalLbl = new Label { Font = new Font("Arial", 16, FontStyle.Bold), ForeColor = Color.FromArgb(220, 50, 50),
                TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Bottom, Height = 45,
                BackColor = Color.FromArgb(230, 235, 248), Padding = new Padding(8, 0, 0, 0) };
            totalsPanel.Controls.Add(_totalLbl);

            // Action buttons
            var actionsPanel = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = Color.FromArgb(240, 242, 248) };
            var btnPay = MakeActionBtn("دفع", Color.FromArgb(34, 139, 34), new Point(8, 8), new Size(175, 45));
            var btnNew = MakeActionBtn("فاتورة جديدة", Color.FromArgb(50, 100, 200), new Point(190, 8), new Size(175, 45));
            var btnCancel = MakeActionBtn("إلغاء الفاتورة", Color.FromArgb(200, 50, 50), new Point(8, 58), new Size(175, 35));
            var btnPrint = MakeActionBtn("طباعة", Color.FromArgb(100, 100, 100), new Point(190, 58), new Size(175, 35));

            btnPay.Click += BtnPay_Click;
            btnNew.Click += (s, e) => NewInvoice();
            btnCancel.Click += (s, e) => CancelInvoice();
            btnPrint.Click += (s, e) => PrintInvoice();

            actionsPanel.Controls.AddRange(new Control[] { btnPay, btnNew, btnCancel, btnPrint });

            leftPanel.Controls.AddRange(new Control[] { totalsPanel, actionsPanel, _invoiceGrid, headerPanel });
            this.Controls.Add(leftPanel);
        }

        private Label MakeTotalLabel(string caption, string value, Panel parent, int y)
        {
            parent.Controls.Add(new Label { Text = caption, Font = new Font("Arial", 11), Location = new Point(6, y), Size = new Size(80, 25), TextAlign = ContentAlignment.MiddleRight });
            var lbl = new Label { Text = value, Font = new Font("Arial", 11, FontStyle.Bold), Location = new Point(90, y), Size = new Size(270, 25), TextAlign = ContentAlignment.MiddleLeft };
            parent.Controls.Add(lbl);
            return lbl;
        }

        private Button MakeActionBtn(string text, Color bg, Point loc, Size size)
        {
            var btn = new Button { Text = text, Location = loc, Size = size, BackColor = bg, ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // ──────────────── CENTER PANEL (Categories + Items Grid) ──────────
        private void BuildCenterPanel()
        {
            var centerPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };

            // Search bar
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 46, Padding = new Padding(2) };
            var searchLbl = new Label { Text = "بحث:", Dock = DockStyle.Right, Width = 55, Font = new Font("Arial", 11), TextAlign = ContentAlignment.MiddleCenter };
            _searchBox = new TextBox { Dock = DockStyle.Fill, Font = new Font("Arial", 12), PlaceholderText = "ابحث بالاسم، الباركود، رقم الصنف..." };
            _searchBox.TextChanged += (s, e) => LoadItems();
            var barcodeBtn = new Button { Text = "باركود", Dock = DockStyle.Left, Width = 80, BackColor = Color.FromArgb(70, 90, 150), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 10) };
            barcodeBtn.FlatAppearance.BorderSize = 0;
            barcodeBtn.Click += (s, e) => _searchBox.Focus();
            searchPanel.Controls.AddRange(new Control[] { _searchBox, searchLbl, barcodeBtn });

            // Categories
            _categoriesPanel = new Panel { Dock = DockStyle.Top, Height = 52, AutoScroll = true, BackColor = Color.FromArgb(230, 235, 245) };

            // Items grid
            _itemsPanel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(245, 247, 252), Padding = new Padding(4) };

            centerPanel.Controls.AddRange(new Control[] { _itemsPanel, _categoriesPanel, searchPanel });
            this.Controls.Add(centerPanel);
        }

        // ──────────────── RIGHT PANEL (Stats) ────────────────────────────
        private Label _todaySalesLbl = null!, _openInvoicesLbl = null!;
        private void BuildRightPanel()
        {
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 200, BackColor = Color.FromArgb(30, 35, 60), Padding = new Padding(10) };

            var statsTitle = new Label { Text = "إحصائيات اليوم", Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Top, Height = 40 };

            _todaySalesLbl = MakeStatLabel(rightPanel, "مبيعات اليوم", "جاري التحميل...", 60);
            _openInvoicesLbl = MakeStatLabel(rightPanel, "فواتير مفتوحة", "-", 140);

            var refreshBtn = new Button { Text = "تحديث", Location = new Point(10, 210), Size = new Size(175, 36),
                BackColor = Color.FromArgb(60, 80, 150), ForeColor = Color.White, Font = new Font("Arial", 11), FlatStyle = FlatStyle.Flat };
            refreshBtn.FlatAppearance.BorderSize = 0;
            refreshBtn.Click += (s, e) => UpdateStats();

            rightPanel.Controls.AddRange(new Control[] { statsTitle, refreshBtn });
            this.Controls.Add(rightPanel);
            UpdateStats();
        }

        private Label MakeStatLabel(Panel parent, string caption, string value, int y)
        {
            parent.Controls.Add(new Label { Text = caption, Location = new Point(10, y), Size = new Size(175, 22),
                Font = new Font("Arial", 10), ForeColor = Color.FromArgb(160, 180, 220), TextAlign = ContentAlignment.MiddleCenter });
            var lbl = new Label { Text = value, Location = new Point(10, y + 24), Size = new Size(175, 32),
                Font = new Font("Arial", 13, FontStyle.Bold), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            parent.Controls.Add(lbl);
            return lbl;
        }

        // ──────────────── STATUS BAR ─────────────────────────────────────
        private void BuildBottomBar()
        {
            _statusBar = new Label { Dock = DockStyle.Bottom, Height = 26, BackColor = Color.FromArgb(50, 60, 90),
                ForeColor = Color.FromArgb(200, 210, 240), Font = new Font("Arial", 9), TextAlign = ContentAlignment.MiddleRight, Padding = new Padding(8, 0, 8, 0) };
            _statusBar.Text = "جاهز";
            this.Controls.Add(_statusBar);
        }

        // ──────────────── DATA LOADING ────────────────────────────────────
        private void LoadData()
        {
            _settings = SettingsService.Get();
            _categories = CategoryService.GetAll();
            _allItems = ItemService.GetItems();
            LoadCustomersCombo();
            BuildCategoryButtons();
            LoadItems();
        }

        private void LoadCustomersCombo()
        {
            _customerCombo.Items.Clear();
            _customerCombo.Items.Add(new Customer { Id = 0, Name = "(بدون عميل)" });
            foreach (var c in CustomerService.GetAll()) _customerCombo.Items.Add(c);
            _customerCombo.SelectedIndex = 0;
        }

        private void BuildCategoryButtons()
        {
            _categoriesPanel.Controls.Clear();
            int x = 4;

            var allBtn = new Button { Text = "الكل", Size = new Size(80, 40), Location = new Point(x, 6),
                BackColor = _selectedCategoryId == null ? Color.FromArgb(50, 80, 200) : Color.FromArgb(100, 110, 140),
                ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 10) };
            allBtn.FlatAppearance.BorderSize = 0;
            allBtn.Click += (s, e) => { _selectedCategoryId = null; LoadItems(); BuildCategoryButtons(); };
            _categoriesPanel.Controls.Add(allBtn);
            x += 86;

            var favBtn = new Button { Text = "⭐ مفضل", Size = new Size(90, 40), Location = new Point(x, 6),
                BackColor = Color.FromArgb(200, 150, 0), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 10) };
            favBtn.FlatAppearance.BorderSize = 0;
            favBtn.Click += (s, e) => { _selectedCategoryId = -1; LoadItems(); };
            _categoriesPanel.Controls.Add(favBtn);
            x += 98;

            foreach (var cat in _categories)
            {
                var catLocal = cat;
                var isSelected = _selectedCategoryId == cat.Id;
                var catColor = ParseColor(cat.Color, Color.FromArgb(60, 120, 60));
                var btn = new Button { Text = cat.Name, Size = new Size(100, 40), Location = new Point(x, 6),
                    BackColor = isSelected ? AdjustBrightness(catColor, 0.8f) : catColor,
                    ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 10), Tag = catLocal };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => { _selectedCategoryId = catLocal.Id; LoadItems(); BuildCategoryButtons(); };
                _categoriesPanel.Controls.Add(btn);
                x += 106;
            }
        }

        private void LoadItems()
        {
            _itemsPanel.Controls.Clear();
            var search = _searchBox?.Text.Trim() ?? "";
            List<Item> items;

            if (_selectedCategoryId == -1) items = ItemService.GetItems(favorites: true);
            else items = ItemService.GetItems(_selectedCategoryId, string.IsNullOrEmpty(search) ? null : search);

            int col = 0, row = 0;
            int btnW = 140, btnH = 72, gap = 6, startX = 6, startY = 6;
            int perRow = Math.Max(1, (_itemsPanel.Width - startX * 2) / (btnW + gap));

            foreach (var item in items)
            {
                var itemLocal = item;
                var catColor = ParseColor(item.CategoryColor ?? "#4CAF50", Color.FromArgb(60, 120, 60));
                var btn = new Button
                {
                    Size = new Size(btnW, btnH),
                    Location = new Point(startX + col * (btnW + gap), startY + row * (btnH + gap)),
                    BackColor = catColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat,
                    Tag = itemLocal, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = AdjustBrightness(catColor, 0.85f);

                // Multi-line text: item number + name + price
                var numPart = item.ItemNumber != null ? $"({item.ItemNumber})" : "";
                btn.Text = $"{numPart}\n{item.Name}\n{item.Price:N0} {_settings.CurrencySymbol}";
                btn.Font = new Font("Arial", 9);

                btn.Click += (s, e) => AddItemToInvoice(itemLocal);
                _itemsPanel.Controls.Add(btn);

                col++;
                if (col >= perRow) { col = 0; row++; }
            }

            _statusBar.Text = $"عدد الأصناف: {items.Count}";
        }

        // ──────────────── INVOICE OPERATIONS ─────────────────────────────
        private void AddItemToInvoice(Item item)
        {
            var existing = _currentInvoice.Items.FirstOrDefault(i => i.ItemId == item.Id);
            if (existing != null) { existing.Quantity++; }
            else
            {
                _currentInvoice.Items.Add(new InvoiceItem
                {
                    ItemId = item.Id, Name = item.Name, Price = item.Price,
                    Quantity = 1, TaxRate = item.TaxRate, DiscountRate = item.DiscountRate
                });
            }
            UpdateInvoiceDisplay();
            _statusBar.Text = $"تم إضافة: {item.Name}";
        }

        private void RemoveSelectedItem()
        {
            if (_invoiceGrid.CurrentRow == null || _invoiceGrid.CurrentRow.Index < 0) return;
            _currentInvoice.Items.RemoveAt(_invoiceGrid.CurrentRow.Index);
            UpdateInvoiceDisplay();
        }

        private void InvoiceGrid_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _currentInvoice.Items.Count) return;
            if (_invoiceGrid.Columns[e.ColumnIndex].DataPropertyName == "Quantity")
            {
                if (decimal.TryParse(_invoiceGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out var qty) && qty > 0)
                {
                    _currentInvoice.Items[e.RowIndex].Quantity = qty;
                    UpdateTotals();
                }
            }
        }

        private void UpdateInvoiceDisplay()
        {
            _invoiceGrid.DataSource = null;
            _invoiceGrid.DataSource = _currentInvoice.Items;
            _invoiceNumLbl.Text = $"فاتورة: {_currentInvoice.InvoiceNumber}  |  {_currentInvoice.CreatedAt:dd/MM/yyyy HH:mm}";
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            var sym = _settings.CurrencySymbol;
            _subtotalLbl.Text = $"{_currentInvoice.Subtotal:N2} {sym}";
            _taxLbl.Text = $"{_currentInvoice.TaxAmount:N2} {sym}";
            _discountLbl.Text = $"{_currentInvoice.DiscountAmount:N2} {sym}";
            _totalLbl.Text = $"  الإجمالي: {_currentInvoice.Total:N2} {sym}";
        }

        private void UpdateStats()
        {
            try
            {
                var (sales, count, open) = InvoiceService.GetTodaySummary();
                _todaySalesLbl.Text = $"{sales:N2} {_settings.CurrencySymbol}";
                _openInvoicesLbl.Text = open.ToString();
            }
            catch { }
        }

        // ──────────────── PAYMENT ────────────────────────────────────────
        private void BtnPay_Click(object? sender, EventArgs e)
        {
            if (!_currentInvoice.Items.Any()) { MessageBox.Show("الفاتورة فارغة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Save items first
            InvoiceService.SaveItems(_currentInvoice.Id, _currentInvoice.Items);
            UpdateInvoiceHeader();

            using var payForm = new PaymentForm(_currentInvoice, _settings.CurrencySymbol);
            if (payForm.ShowDialog() == DialogResult.OK)
            {
                InvoiceService.ProcessPayment(_currentInvoice.Id, payForm.SelectedMethod, payForm.AmountPaid);
                var change = payForm.AmountPaid - _currentInvoice.Total;
                MessageBox.Show($"تم الدفع بنجاح!\nالمبلغ المدفوع: {payForm.AmountPaid:N2} {_settings.CurrencySymbol}\nالباقي: {change:N2} {_settings.CurrencySymbol}",
                    "تم الدفع", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStats();
                NewInvoice();
            }
        }

        private void UpdateInvoiceHeader()
        {
            _currentInvoice.DiscountAmount = _discountInput.Value;
            var custItem = _customerCombo.SelectedItem as Customer;
            _currentInvoice.CustomerId = custItem?.Id == 0 ? null : custItem?.Id;
            var types = new[] { "dine_in", "takeaway", "delivery" };
            _currentInvoice.OrderType = types[_orderTypeCombo.SelectedIndex];
            _currentInvoice.TableNumber = _tableCombo.SelectedIndex > 0 ? $"{_tableCombo.SelectedIndex}" : null;
            InvoiceService.UpdateHeader(_currentInvoice);
        }

        private void NewInvoice()
        {
            if (_currentInvoice.Items.Any() && _currentInvoice.Status == "open")
                InvoiceService.SaveItems(_currentInvoice.Id, _currentInvoice.Items);
            _currentInvoice = InvoiceService.CreateNew(_currentUser.Id);
            _discountInput.Value = 0;
            _customerCombo.SelectedIndex = 0;
            _orderTypeCombo.SelectedIndex = 0;
            _tableCombo.SelectedIndex = 0;
            UpdateInvoiceDisplay();
            _statusBar.Text = "فاتورة جديدة";
        }

        private void CancelInvoice()
        {
            if (MessageBox.Show("هل تريد إلغاء الفاتورة الحالية؟", "تأكيد", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            InvoiceService.Cancel(_currentInvoice.Id);
            _currentInvoice = InvoiceService.CreateNew(_currentUser.Id);
            UpdateInvoiceDisplay();
            _statusBar.Text = "تم إلغاء الفاتورة";
        }

        private void PrintInvoice()
        {
            if (!_currentInvoice.Items.Any()) { MessageBox.Show("الفاتورة فارغة!", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            var receipt = BuildReceiptText();
            var dlg = new Form { Text = "معاينة الفاتورة", Size = new Size(400, 600), StartPosition = FormStartPosition.CenterParent };
            var tb = new RichTextBox { Dock = DockStyle.Fill, Text = receipt, Font = new Font("Courier New", 11), ReadOnly = true, BackColor = Color.White };
            var printBtn = new Button { Text = "طباعة", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(34,139,34), ForeColor = Color.White, Font = new Font("Arial", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            printBtn.Click += (s, e) =>
            {
                var pd = new System.Drawing.Printing.PrintDocument();
                pd.PrintPage += (ps, pe) =>
                {
                    pe.Graphics!.DrawString(receipt, new Font("Courier New", 9), Brushes.Black, new System.Drawing.PointF(20, 20));
                };
                var prd = new PrintDialog { Document = pd };
                if (prd.ShowDialog() == DialogResult.OK) pd.Print();
                dlg.Close();
            };
            dlg.Controls.AddRange(new Control[] { tb, printBtn });
            dlg.ShowDialog();
        }

        private string BuildReceiptText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(Center(_settings.StoreName));
            if (!string.IsNullOrEmpty(_settings.Address)) sb.AppendLine(Center(_settings.Address));
            if (!string.IsNullOrEmpty(_settings.Phone)) sb.AppendLine(Center(_settings.Phone));
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"فاتورة: {_currentInvoice.InvoiceNumber}");
            sb.AppendLine($"التاريخ: {_currentInvoice.CreatedAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"الكاشير: {_currentUser.Name}");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine($"{"الصنف",-20} {"الكمية",6} {"السعر",8}");
            sb.AppendLine(new string('-', 40));
            foreach (var item in _currentInvoice.Items)
                sb.AppendLine($"{item.Name,-20} {item.Quantity,6:N1} {item.Total,8:N2}");
            sb.AppendLine(new string('=', 40));
            sb.AppendLine($"{"المجموع:",-25} {_currentInvoice.Subtotal,10:N2}");
            sb.AppendLine($"{"الضريبة:",-25} {_currentInvoice.TaxAmount,10:N2}");
            if (_currentInvoice.DiscountAmount > 0) sb.AppendLine($"{"الخصم:",-25} {_currentInvoice.DiscountAmount,10:N2}");
            sb.AppendLine($"{"الإجمالي:",-25} {_currentInvoice.Total,10:N2}");
            sb.AppendLine(new string('=', 40));
            if (!string.IsNullOrEmpty(_settings.ReceiptFooter)) { sb.AppendLine(); sb.AppendLine(Center(_settings.ReceiptFooter)); }
            return sb.ToString();
        }

        private static string Center(string text, int width = 40) => text.PadLeft((width + text.Length) / 2).PadRight(width);

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { _searchBox.Focus(); _searchBox.SelectAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) NewInvoice();
            else if (e.KeyCode == Keys.F12) BtnPay_Click(sender, e);
            else if (e.KeyCode == Keys.Delete) RemoveSelectedItem();
        }

        // ──────────────── HELPERS ─────────────────────────────────────────
        private static Color ParseColor(string hex, Color fallback)
        {
            try { return ColorTranslator.FromHtml(hex); } catch { return fallback; }
        }

        private static Color AdjustBrightness(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                (int)Math.Min(255, color.R * factor),
                (int)Math.Min(255, color.G * factor),
                (int)Math.Min(255, color.B * factor));
        }
    }
}
