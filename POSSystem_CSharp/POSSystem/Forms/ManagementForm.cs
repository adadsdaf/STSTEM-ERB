using POSSystem.Models;
using POSSystem.Services;

namespace POSSystem.Forms
{
    public class ManagementForm : Form
    {
        private readonly User _currentUser;
        private TabControl _tabs = null!;

        public ManagementForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            this.RightToLeft = RightToLeft.Yes;
            this.RightToLeftLayout = true;
        }

        private void InitializeComponent()
        {
            this.Text = "إدارة النظام";
            this.Size = new Size(950, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);

            _tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Arial", 11) };

            _tabs.TabPages.Add(BuildItemsTab());
            _tabs.TabPages.Add(BuildCategoriesTab());
            _tabs.TabPages.Add(BuildCustomersTab());
            if (_currentUser.Role == "admin")
            {
                _tabs.TabPages.Add(BuildUsersTab());
                _tabs.TabPages.Add(BuildSettingsTab());
            }
            _tabs.TabPages.Add(BuildReportsTab());

            this.Controls.Add(_tabs);
        }

        // ─── ITEMS TAB ─────────────────────────────────────────────────────
        private DataGridView _itemsGrid = null!;
        private TabPage BuildItemsTab()
        {
            var page = new TabPage("الأصناف");
            _itemsGrid = MakeGrid();
            _itemsGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "الاسم", DataPropertyName = "Name", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "السعر", DataPropertyName = "Price", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } },
                new DataGridViewTextBoxColumn { HeaderText = "الفئة", DataPropertyName = "CategoryName", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "الباركود", DataPropertyName = "Barcode", Width = 110 },
                new DataGridViewCheckBoxColumn { HeaderText = "مفضل", DataPropertyName = "IsFavorite", Width = 65 },
                new DataGridViewCheckBoxColumn { HeaderText = "متاح", DataPropertyName = "IsAvailable", Width = 60 }
            );

            var toolbar = MakeToolbar(
                ("+ إضافة", Color.FromArgb(34,139,34), (s,e) => { using var f=new ItemEditForm(); if(f.ShowDialog()==DialogResult.OK && f.ResultItem!=null){ItemService.Create(f.ResultItem);LoadItems();} }),
                ("تعديل", Color.FromArgb(30,100,200), (s,e) => EditSelectedItem()),
                ("حذف", Color.FromArgb(200,50,50), (s,e) => DeleteSelectedItem(_itemsGrid, id => { ItemService.Delete(id); LoadItems(); }))
            );
            LoadItems();
            page.Controls.AddRange(new Control[] { toolbar, _itemsGrid });
            toolbar.Dock = DockStyle.Top; _itemsGrid.Dock = DockStyle.Fill;
            return page;
        }

        private void LoadItems()
        {
            _itemsGrid.DataSource = ItemService.GetItems();
        }

        private void EditSelectedItem()
        {
            if (_itemsGrid.CurrentRow?.DataBoundItem is not Item item) return;
            using var f = new ItemEditForm(item);
            if (f.ShowDialog() == DialogResult.OK && f.ResultItem != null) { ItemService.Update(f.ResultItem); LoadItems(); }
        }

        // ─── CATEGORIES TAB ───────────────────────────────────────────────
        private DataGridView _catGrid = null!;
        private TabPage BuildCategoriesTab()
        {
            var page = new TabPage("الفئات");
            _catGrid = MakeGrid();
            _catGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "الاسم", DataPropertyName = "Name", Width = 200 },
                new DataGridViewTextBoxColumn { HeaderText = "اللون", DataPropertyName = "Color", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "الترتيب", DataPropertyName = "SortOrder", Width = 80 }
            );

            var toolbar = MakeToolbar(
                ("+ إضافة", Color.FromArgb(34,139,34), (s,e) => AddCategory()),
                ("تعديل", Color.FromArgb(30,100,200), (s,e) => EditCategory()),
                ("حذف", Color.FromArgb(200,50,50), (s,e) => DeleteSelectedItem(_catGrid, id => { CategoryService.Delete(id); LoadCategories(); }))
            );
            LoadCategories();
            page.Controls.AddRange(new Control[] { toolbar, _catGrid });
            toolbar.Dock = DockStyle.Top; _catGrid.Dock = DockStyle.Fill;
            return page;
        }

        private void LoadCategories() => _catGrid.DataSource = CategoryService.GetAll();

        private void AddCategory()
        {
            var name = InputDialog("اسم الفئة الجديدة:");
            if (!string.IsNullOrEmpty(name)) { CategoryService.Create(new Category { Name=name, Color="#4CAF50" }); LoadCategories(); }
        }

        private void EditCategory()
        {
            if (_catGrid.CurrentRow?.DataBoundItem is not Category cat) return;
            var name = InputDialog("اسم الفئة:", cat.Name);
            if (!string.IsNullOrEmpty(name)) { cat.Name=name; CategoryService.Update(cat); LoadCategories(); }
        }

        // ─── CUSTOMERS TAB ────────────────────────────────────────────────
        private DataGridView _custGrid = null!;
        private TabPage BuildCustomersTab()
        {
            var page = new TabPage("العملاء");
            _custGrid = MakeGrid();
            _custGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "الاسم", DataPropertyName = "Name", Width = 180 },
                new DataGridViewTextBoxColumn { HeaderText = "الجوال", DataPropertyName = "Phone", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "البريد", DataPropertyName = "Email", Width = 180 },
                new DataGridViewTextBoxColumn { HeaderText = "إجمالي المشتريات", DataPropertyName = "TotalPurchases", Width = 140, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } }
            );
            var toolbar = MakeToolbar(
                ("+ إضافة", Color.FromArgb(34,139,34), (s,e) => AddCustomer()),
                ("حذف", Color.FromArgb(200,50,50), (s,e) => DeleteSelectedItem(_custGrid, id => { CustomerService.Delete(id); LoadCustomers(); }))
            );
            LoadCustomers();
            page.Controls.AddRange(new Control[] { toolbar, _custGrid });
            toolbar.Dock = DockStyle.Top; _custGrid.Dock = DockStyle.Fill;
            return page;
        }

        private void LoadCustomers() => _custGrid.DataSource = CustomerService.GetAll();

        private void AddCustomer()
        {
            var name = InputDialog("اسم العميل:");
            if (!string.IsNullOrEmpty(name)) { CustomerService.Create(new Customer { Name=name }); LoadCustomers(); }
        }

        // ─── USERS TAB ───────────────────────────────────────────────────
        private DataGridView _usersGrid = null!;
        private TabPage BuildUsersTab()
        {
            var page = new TabPage("المستخدمون");
            _usersGrid = MakeGrid();
            _usersGrid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "الاسم", DataPropertyName = "Name", Width = 180 },
                new DataGridViewTextBoxColumn { HeaderText = "الدور", DataPropertyName = "Role", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "PIN", DataPropertyName = "Pin", Width = 80 },
                new DataGridViewCheckBoxColumn { HeaderText = "نشط", DataPropertyName = "IsActive", Width = 60 }
            );
            var toolbar = MakeToolbar(
                ("+ إضافة", Color.FromArgb(34,139,34), (s,e) => AddUser()),
                ("حذف", Color.FromArgb(200,50,50), (s,e) => DeleteSelectedItem(_usersGrid, id => { UserService.Delete(id); LoadUsers(); }))
            );
            LoadUsers();
            page.Controls.AddRange(new Control[] { toolbar, _usersGrid });
            toolbar.Dock = DockStyle.Top; _usersGrid.Dock = DockStyle.Fill;
            return page;
        }

        private void LoadUsers() => _usersGrid.DataSource = UserService.GetAll();

        private void AddUser()
        {
            var name = InputDialog("اسم المستخدم:");
            var pin = InputDialog("رمز PIN:");
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(pin))
            { UserService.Create(new User { Name=name, Pin=pin, Role="cashier" }); LoadUsers(); }
        }

        // ─── SETTINGS TAB ────────────────────────────────────────────────
        private TabPage BuildSettingsTab()
        {
            var page = new TabPage("الإعدادات");
            var s = SettingsService.Get();
            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(20) };

            var y = 20;
            var storeNameBox = AddSetting(panel, ref y, "اسم المحل:", s.StoreName);
            var addressBox = AddSetting(panel, ref y, "العنوان:", s.Address ?? "");
            var phoneBox = AddSetting(panel, ref y, "الهاتف:", s.Phone ?? "");
            var taxBox = AddSetting(panel, ref y, "نسبة الضريبة %:", s.TaxRate.ToString());
            var svcBox = AddSetting(panel, ref y, "نسبة الخدمة %:", s.ServiceRate.ToString());
            var footerBox = AddSetting(panel, ref y, "تذييل الفاتورة:", s.ReceiptFooter ?? "");

            var btnSave = new Button { Text = "حفظ الإعدادات", Location = new Point(20, y+10), Size = new Size(200, 42),
                BackColor = Color.FromArgb(34,139,34), ForeColor = Color.White,
                Font = new Font("Arial", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (sender, e) =>
            {
                s.StoreName = storeNameBox.Text; s.Address = addressBox.Text; s.Phone = phoneBox.Text;
                if (decimal.TryParse(taxBox.Text, out var tax)) s.TaxRate = tax;
                if (decimal.TryParse(svcBox.Text, out var svc)) s.ServiceRate = svc;
                s.ReceiptFooter = footerBox.Text;
                SettingsService.Save(s);
                MessageBox.Show("تم حفظ الإعدادات", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(btnSave);
            page.Controls.Add(panel);
            return page;
        }

        private TextBox AddSetting(Panel panel, ref int y, string label, string value)
        {
            panel.Controls.Add(new Label { Text = label, Location = new Point(20, y), Size = new Size(160, 26), Font = new Font("Arial", 11), TextAlign = ContentAlignment.MiddleRight });
            var box = new TextBox { Text = value, Location = new Point(190, y), Size = new Size(350, 28), Font = new Font("Arial", 11) };
            panel.Controls.Add(box); y += 42; return box;
        }

        // ─── REPORTS TAB ─────────────────────────────────────────────────
        private TabPage BuildReportsTab()
        {
            var page = new TabPage("التقارير");
            var grid = MakeGrid();
            grid.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "رقم الفاتورة", DataPropertyName = "InvoiceNumber", Width = 160 },
                new DataGridViewTextBoxColumn { HeaderText = "الحالة", DataPropertyName = "Status", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "العميل", DataPropertyName = "CustomerName", Width = 130 },
                new DataGridViewTextBoxColumn { HeaderText = "الكاشير", DataPropertyName = "CashierName", Width = 130 },
                new DataGridViewTextBoxColumn { HeaderText = "الإجمالي", DataPropertyName = "Total", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } },
                new DataGridViewTextBoxColumn { HeaderText = "طريقة الدفع", DataPropertyName = "PaymentMethod", Width = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "التاريخ", DataPropertyName = "CreatedAt", Width = 150, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd/MM/yyyy HH:mm" } }
            );

            var toolbar = MakeToolbar(("تحديث", Color.FromArgb(30,100,200), (s,e) => grid.DataSource = InvoiceService.GetAll()));
            grid.DataSource = InvoiceService.GetAll();
            page.Controls.AddRange(new Control[] { toolbar, grid });
            toolbar.Dock = DockStyle.Top; grid.Dock = DockStyle.Fill;
            return page;
        }

        // ─── HELPERS ─────────────────────────────────────────────────────
        private static DataGridView MakeGrid() => new DataGridView
        {
            AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
            Font = new Font("Arial", 10), RowHeadersVisible = false,
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245,248,255) },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Arial", 10, FontStyle.Bold), BackColor = Color.FromArgb(60,90,160), ForeColor = Color.White },
            EnableHeadersVisualStyles = false
        };

        private static Panel MakeToolbar(params (string text, Color color, EventHandler click)[] buttons)
        {
            var panel = new Panel { Height = 48, BackColor = Color.FromArgb(230, 235, 245), Padding = new Padding(8, 6, 8, 6) };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            foreach (var (text, color, click) in buttons)
            {
                var btn = new Button { Text = text, Size = new Size(110, 36), BackColor = color, ForeColor = Color.White,
                    Font = new Font("Arial", 11, FontStyle.Bold), FlatStyle = FlatStyle.Flat, Margin = new Padding(4,0,0,0) };
                btn.FlatAppearance.BorderSize = 0; btn.Click += click;
                flow.Controls.Add(btn);
            }
            panel.Controls.Add(flow);
            return panel;
        }

        private static void DeleteSelectedItem(DataGridView grid, Action<int> deleteAction)
        {
            if (grid.CurrentRow == null) return;
            var id = (int)grid.CurrentRow.Cells["Id"]?.Value! ;
            if (id <= 0) return;
            if (MessageBox.Show("هل تريد حذف هذا العنصر؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { deleteAction(id); }
            catch (Exception ex) { MessageBox.Show($"لا يمكن الحذف: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static string? InputDialog(string prompt, string defaultValue = "")
        {
            var form = new Form { Text = prompt, Size = new Size(380, 160), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            var lbl = new Label { Text = prompt, Location = new Point(10, 15), Size = new Size(350, 25), Font = new Font("Arial", 11) };
            var box = new TextBox { Text = defaultValue, Location = new Point(10, 45), Size = new Size(350, 28), Font = new Font("Arial", 11) };
            var ok = new Button { Text = "موافق", DialogResult = DialogResult.OK, Location = new Point(200, 85), Size = new Size(80, 30) };
            var cancel = new Button { Text = "إلغاء", DialogResult = DialogResult.Cancel, Location = new Point(290, 85), Size = new Size(70, 30) };
            form.Controls.AddRange(new Control[] { lbl, box, ok, cancel });
            form.AcceptButton = ok; form.CancelButton = cancel;
            return form.ShowDialog() == DialogResult.OK ? box.Text.Trim() : null;
        }
    }
}
