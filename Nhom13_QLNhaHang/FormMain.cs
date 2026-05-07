using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Collections.Generic;

namespace Nhom13_QLNhaHang
{
    public partial class FormMain : Form
    {
        private readonly string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";
        public int CurrentStaffID { get; set; } = 1;
        public string CurrentStaffName { get; set; } = "Admin";
        public string CurrentStaffRole { get; set; } = "Admin";

        private int _currentOrderID = 0;
        private int _currentTableID = 0;

        private bool isDragging = false;
        private Point dragCursorPoint;
        private Point dragControlPoint;
        private Control activeTableButton = null;
        private ToolTip mainToolTip = new ToolTip();
        public FormMain()
        {
            InitializeComponent();
            StyleGrid();
            UpdateOrderSummary();
            btnHome.Checked = true;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            pnlHome.BringToFront();
        }

        private void btnFoodManager_Click(object sender, EventArgs e)
        {
            pnlFoodManager.BringToFront();

            // 1. Load lại danh sách món
            LoadFoodData();
            UpdateButtonState(false);

            // 2. Chỉ tải danh sách Danh mục (ComboBox) nếu nó đang trống
            if (cboCategory.Items.Count == 0)
            {
                LoadCategoryComboBox();
            }
        }

        private void btnTableManager_Click(object sender, EventArgs e)
        {
            pnlTableManager.BringToFront();
            LoadTableMap();
        }

        private void btnStatistics_Click(object sender, EventArgs e)
        {
            pnlStatistics.BringToFront();
            // Gọi hàm load dữ liệu theo ngày đang hiển thị trên ô chọn
            LoadBillListByDate(dtpFromDate.Value, dtpToDate.Value);
        }

        private void LoadCategories()
        {
            flowLayoutPanelCategories.Controls.Clear();
            string query = "SELECT CategoryID, CategoryName, ImagePath FROM Categories";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    // Lấy đường dẫn ảnh
                    object imgObj = reader["ImagePath"];
                    string imgPath = (imgObj == DBNull.Value) ? "" : imgObj.ToString();

                    Guna.UI2.WinForms.Guna2Button btn = new Guna.UI2.WinForms.Guna2Button();

                    // 1. Dữ liệu
                    btn.Text = reader["CategoryName"].ToString();
                    btn.Tag = Convert.ToInt32(reader["CategoryID"]);

                    // 2. Hình ảnh (Load ảnh bằng hàm GetImageFromPath)
                    btn.Image = GetImageFromPath(imgPath);
                    btn.ImageSize = new Size(25, 25);
                    btn.ImageAlign = HorizontalAlignment.Left;
                    btn.TextAlign = HorizontalAlignment.Left; 
                    btn.TextOffset = new Point(10, 0);

                    // 3. Giao diện (Style)
                    btn.Size = new Size(140, 45); // Kích thước nút
                    btn.BorderRadius = 10; // Bo góc
                    btn.FillColor = Color.White; // Màu nền
                    btn.ForeColor = Color.Black; // Màu chữ
                    btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                    // --- HIỆU ỨNG TAB ---
                    btn.ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton;
                    // Khi được chọn (Checked) sẽ đổi màu
                    btn.CheckedState.FillColor = Color.FromArgb(255, 128, 128); // Màu đỏ nhạt/hồng khi chọn
                    btn.CheckedState.ForeColor = Color.White;
                    btn.CheckedState.Image = btn.Image; 

                    // 4. Sự kiện
                    btn.Click += CategoryButton_Click;

                    // Thêm vào Panel
                    flowLayoutPanelCategories.Controls.Add(btn);
                }
            }

            // Tự động bấm vào danh mục đầu tiên (nếu có)
            if (flowLayoutPanelCategories.Controls.Count > 0)
            {
                var firstBtn = (Guna.UI2.WinForms.Guna2Button)flowLayoutPanelCategories.Controls[0];
                firstBtn.Checked = true;
                LoadFoodItems((int)firstBtn.Tag);
            }
        }

        private void CategoryButton_Click(object sender, EventArgs e)
        {
            var btn = (Guna.UI2.WinForms.Guna2Button)sender;
            int catID = (int)btn.Tag;
            LoadFoodItems(catID);
        }

        private void LoadFoodItems(int categoryID)
        {
            // 1. TẠM DỪNG VẼ GIAO DIỆN
            flowLayoutPanelChooseOrder.SuspendLayout();

            try
            {
                // 2. GIẢI PHÓNG BỘ NHỚ CŨ
                foreach (Control c in flowLayoutPanelChooseOrder.Controls)
                {
                    if (c is FoodItemCard item)
                    {
                        if (item.ItemImage != null) item.ItemImage.Dispose();
                        item.Dispose();
                    }
                }
                flowLayoutPanelChooseOrder.Controls.Clear();

                // 3. LẤY DỮ LIỆU TỪ SQL
                // Chỉ hiện những món đang bán (chưa bị xóa)
                string query = "SELECT ItemID, ItemName, Price, ImagePath FROM FoodItems WHERE CategoryID = @ID AND IsActive = 1";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@ID", categoryID);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        // Lấy tên ảnh
                        object resourceNameObj = reader["ImagePath"];
                        string resourceName = (resourceNameObj == DBNull.Value) ? null : resourceNameObj.ToString();

                        // Tạo thẻ món ăn
                        FoodItemCard foodCard = new FoodItemCard();

                        // Gán dữ liệu
                        int id = Convert.ToInt32(reader["ItemID"]);
                        foodCard.Tag = id;
                        foodCard.ItemName = reader["ItemName"].ToString();
                        foodCard.ItemPrice = (double)Convert.ToDecimal(reader["Price"]);

                        // Tooltip
                        string fullName = reader["ItemName"].ToString();
                        foodCard.SetTooltipInfo(mainToolTip, fullName);

                        // Load ảnh
                        foodCard.ItemImage = GetImageFromPath(resourceName);

                        // Gắn sự kiện Click
                        foodCard.Click += FoodItem_Click;

                        ToolTip tip = new ToolTip();
                        tip.SetToolTip(foodCard, fullName);

                        // Thêm vào Panel
                        flowLayoutPanelChooseOrder.Controls.Add(foodCard);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải món ăn: " + ex.Message);
            }
            finally
            {
                // 4. CHO PHÉP VẼ LẠI GIAO DIỆN
                flowLayoutPanelChooseOrder.ResumeLayout();
            }
        }

        private void FoodItem_Click(object sender, EventArgs e)
        {
            // 1. Lấy thẻ món ăn vừa bị click
            FoodItemCard clickedItem = (FoodItemCard)sender;

            // 2. Lấy đủ 4 thông tin từ thẻ đó
            int id = Convert.ToInt32(clickedItem.Tag); // Lấy ID
            string name = clickedItem.ItemName;        // Lấy Tên
            double price = clickedItem.ItemPrice;      // Lấy Giá
            Image originalImg = clickedItem.ItemImage;
            Image imgClone = null;

            // Tạo bản sao của ảnh để đưa vào giỏ hàng
            // Giúp giỏ hàng không bị ảnh hưởng khi menu bên ngoài bị xóa
            if (originalImg != null)
            {
                imgClone = new Bitmap(originalImg);
            }

            // Truyền bản sao (imgClone) vào thay vì ảnh gốc
            AddOrUpdateItemInOrderList(id, name, price, imgClone);
        }

        private void FoodCard_Clicked(object sender, EventArgs e)
        {
            // 1. Ép kiểu 'sender' (đối tượng đã gửi sự kiện) về đúng loại FoodItemCard
            FoodItemCard clickedCard = (FoodItemCard)sender;

            // 2. Lấy thông tin từ các thuộc tính công khai (public properties) của card đó
            int id = Convert.ToInt32(clickedCard.Tag);
            string name = clickedCard.ItemName;
            double price = clickedCard.ItemPrice;
            Image image = clickedCard.ItemImage;

            // 3. Gọi hàm bên dưới để thêm/cập nhật món ăn vào giỏ hàng (panel bên phải)
            AddOrUpdateItemInOrderList(id, name, price, image);
        }

        private void AddOrUpdateItemInOrderList(int itemID, string name, double price, Image image)
        {
            var existingItem = flowLayoutPanelOrder.Controls.OfType<OrderItemCard>()
                                                     .FirstOrDefault(item => item.ItemName == name);

            if (existingItem != null)
            {
                // TRƯỜNG HỢP 1: Món đã có -> Tăng số lượng
                existingItem.ItemQuantity += 1;
            }
            else
            {
                // TRƯỜNG HỢP 2: Món chưa có -> Tạo mới
                OrderItemCard newItem = new OrderItemCard();

                newItem.Tag = itemID;

                newItem.ItemName = name;
                newItem.ItemPrice = price;
                newItem.ItemImage = image;
                newItem.ItemQuantity = 1;

                // Căn chỉnh giao diện
                newItem.Width = flowLayoutPanelOrder.Width - 25;
                newItem.Margin = new Padding(0, 0, 0, 5);

                // Gắn sự kiện xóa
                newItem.OnDelete += OrderItem_OnDelete;

                flowLayoutPanelOrder.Controls.Add(newItem);
            }

            // Gọi hàm tính tiền
            UpdateOrderSummary();
        }

        private void OrderItem_OnDelete(object sender, EventArgs e)
        {
            // Lấy ra món ăn đang đòi xóa
            OrderItemCard itemToRemove = (OrderItemCard)sender;

            // Xóa nó khỏi giao diện giỏ hàng
            flowLayoutPanelOrder.Controls.Remove(itemToRemove);

            // Giải phóng bộ nhớ 
            itemToRemove.Dispose();

            // Tính lại tổng tiền sau khi xóa
            UpdateOrderSummary();
        }

        private Image GetImageFromPath(string dbPath)
        {
            try
            {
                if (string.IsNullOrEmpty(dbPath)) return null;

                // BƯỚC 1: Xử lý đường dẫn đầu vào
                string fileName = Path.GetFileName(dbPath);

                // BƯỚC 2: Tự động trỏ vào thư mục bin\Debug\Images của phần mềm
                string folderImages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string absolutePath = Path.Combine(folderImages, fileName);

                // BƯỚC 3: Load ảnh (Logic Deep Copy chống lỗi parameter)
                if (!File.Exists(absolutePath)) return null;

                byte[] fileBytes = File.ReadAllBytes(absolutePath);
                if (fileBytes.Length == 0) return null;

                using (MemoryStream ms = new MemoryStream(fileBytes))
                {
                    using (Image tempImage = Image.FromStream(ms))
                    {
                        return new Bitmap(tempImage);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load ảnh: {ex.Message}");
                return null;
            }
        }

        private void CheckEmptyCart()
        {
            // Kiểm tra xem có món nào trong giỏ không
            bool isEmpty = flowLayoutPanelOrder.Controls.Count == 0;

            // 1. Xử lý Ẩn/Hiện hình ảnh Empty
            if (picEmptyCart != null) picEmptyCart.Visible = isEmpty; // Trống thì hiện, có món thì ẩn
            if (lblEmptyCart != null) lblEmptyCart.Visible = isEmpty;

            // 2. Xử lý danh sách món ăn
            if (!isEmpty)
            {
                // Nếu có món: Phải đảm bảo danh sách hiện ra và nằm trên cùng
                flowLayoutPanelOrder.Visible = true;
                flowLayoutPanelOrder.BringToFront(); // ĐẨY DANH SÁCH LÊN TRÊN HÌNH
            }
            else
            {
                // Nếu trống: Đưa hình lên trên (hoặc ẩn danh sách đi cũng được)
                if (picEmptyCart != null) picEmptyCart.BringToFront();
                if (lblEmptyCart != null) lblEmptyCart.BringToFront();
            }
        }

        private void UpdateOrderSummary()
        {
            double subTotal = 0;
            foreach (Control c in flowLayoutPanelOrder.Controls)
            {
                if (c is OrderItemCard item)
                {
                    subTotal += item.TotalPrice;
                }
            }

            // 1. Kiểm tra giỏ trống (Gọi hàm vừa viết ở trên)
            CheckEmptyCart();

            // 2. Tính thuế
            double tax = subTotal * 0.1;

            // 3. Tính giảm giá (Lấy từ txtDiscount)
            double discountPercent = 0;
            double discountAmount = 0;

            // Thử chuyển text thành số, nếu người dùng nhập chữ linh tinh thì coi như là 0
            if (txtDiscount != null && double.TryParse(txtDiscount.Text, out double d))
            {
                discountPercent = d;
                // Giới hạn giảm giá tối đa 100% 
                if (discountPercent > 100) discountPercent = 100;
            }

            discountAmount = subTotal * (discountPercent / 100);

            // 4. Tổng cuối
            double grandTotal = subTotal + tax - discountAmount;

            // 5. Hiển thị ra màn hình
            lblSubTotal.Text = subTotal.ToString("N0");
            lblTax.Text = tax.ToString("N0");

            lblGrandTotal.Text = grandTotal.ToString("N0") + " đ";
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower();

            // Tạm dừng vẽ giao diện để tìm kiếm mượt hơn (không bị nháy)
            flowLayoutPanelChooseOrder.SuspendLayout();

            foreach (Control c in flowLayoutPanelChooseOrder.Controls)
            {
                if (c is FoodItemCard item)
                {
                    // Nếu tên món chứa từ khóa -> Hiện, ngược lại -> Ẩn
                    bool isMatch = item.ItemName.ToLower().Contains(keyword);
                    item.Visible = isMatch;
                }
            }

            flowLayoutPanelChooseOrder.ResumeLayout();
        }

        private void btnThanhToan_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra giỏ hàng
            if (flowLayoutPanelOrder.Controls.Count == 0) return;

            // 2. Lấy tổng tiền từ Label
            decimal totalAmount = 0;
            decimal.TryParse(lblGrandTotal.Text.Replace(".", "").Replace(" đ", "").Trim(), out totalAmount);

            int orderIdToPrint = 0; // Biến quan trọng để lưu ID hóa đơn cần in

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ---------------------------------------------------------
                    // TRƯỜNG HỢP 1: KHÁCH MANG VỀ (Tạo mới -> Lưu -> Lấy ID)
                    // ---------------------------------------------------------
                    if (chkMangVe.Checked)
                    {
                        // Tạo hóa đơn "Đã thanh toán"
                        string insertOrder = @"INSERT INTO Orders (TableID, OrderTime, TotalAmount, TrangThai, StaffID) 
                                       VALUES (NULL, GETDATE(), @Total, N'Đã thanh toán', @StaffID);
                                       SELECT SCOPE_IDENTITY();"; // Trả về ID vừa tạo

                        SqlCommand cmd = new SqlCommand(insertOrder, conn);
                        cmd.Parameters.AddWithValue("@Total", totalAmount);
                        cmd.Parameters.AddWithValue("@StaffID", CurrentStaffID);

                        // --- Lấy ID vừa tạo lưu vào biến ---
                        orderIdToPrint = Convert.ToInt32(cmd.ExecuteScalar());

                        // Lưu chi tiết món ăn (Vòng lặp)
                        foreach (Control c in flowLayoutPanelOrder.Controls)
                        {
                            if (c is OrderItemCard item)
                            {
                                string sqlDetail = "INSERT INTO OrderDetails VALUES (@oid, @iid, @qty, @price, 0)"; // 0 là IsPrinted (nếu có)
                                SqlCommand cmdDetail = new SqlCommand(sqlDetail, conn);
                                cmdDetail.Parameters.AddWithValue("@oid", orderIdToPrint); // Dùng ID vừa lấy
                                cmdDetail.Parameters.AddWithValue("@iid", item.Tag);
                                cmdDetail.Parameters.AddWithValue("@qty", item.ItemQuantity);
                                cmdDetail.Parameters.AddWithValue("@price", item.ItemPrice);
                                cmdDetail.ExecuteNonQuery();
                            }
                        }
                        MessageBox.Show("Thanh toán Mang Về thành công!");
                    }
                    // ---------------------------------------------------------
                    // TRƯỜNG HỢP 2: KHÁCH TẠI BÀN (Lấy ID cũ -> Cập nhật)
                    // ---------------------------------------------------------
                    else
                    {
                        if (_currentOrderID == 0)
                        {
                            MessageBox.Show("Vui lòng bấm GỌI MÓN hoặc LẤY ĐƠN trước khi thanh toán!");
                            return;
                        }

                        // --- ID cần in chính là ID đang treo ---
                        orderIdToPrint = _currentOrderID;

                        // Update trạng thái Hóa đơn
                        string updateOrder = "UPDATE Orders SET TrangThai = N'Đã thanh toán', TotalAmount = @total, OrderTime = GETDATE() WHERE OrderID = @oid";
                        SqlCommand cmd = new SqlCommand(updateOrder, conn);
                        cmd.Parameters.AddWithValue("@oid", _currentOrderID);
                        cmd.Parameters.AddWithValue("@total", totalAmount);
                        cmd.ExecuteNonQuery();

                        // Update Bàn -> Trống
                        // (Dùng _currentTableID vì bạn đang dùng biến này cho nút Lấy Đơn)
                        if (_currentTableID > 0)
                        {
                            SqlCommand cmdTable = new SqlCommand("UPDATE BanAn SET TrangThai = N'Trống' WHERE TableID = @tid", conn);
                            cmdTable.Parameters.AddWithValue("@tid", _currentTableID);
                            cmdTable.ExecuteNonQuery();
                        }

                        MessageBox.Show("Thanh toán bàn thành công!");
                    }
                } // Kết thúc using (đóng kết nối SQL)

                // ---------------------------------------------------------
                // BƯỚC 3: HỎI IN HÓA ĐƠN (Thực hiện sau khi đã lưu xong xuôi)
                // ---------------------------------------------------------
                if (orderIdToPrint > 0)
                {
                    ExportAndPrintBill(orderIdToPrint);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi thanh toán: " + ex.Message);
                return; // Nếu lỗi thì không Reset giao diện để nhân viên kiểm tra lại
            }

            // 4. Dọn dẹp giao diện
            ResetOrderUI();
        }

        void StyleGrid()
        {
            // Kiểm tra xem bạn đã đặt tên bảng là dgvFood chưa nhé!
            dgvFood.BorderStyle = BorderStyle.None;
            dgvFood.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFood.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Màu Header (Tiêu đề cột)
            dgvFood.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 50, 80);
            dgvFood.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvFood.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Màu dòng (Rows)
            dgvFood.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 200, 200);
            dgvFood.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvFood.RowTemplate.Height = 40;

            // Thêm chút padding cho dễ nhìn
            dgvFood.DefaultCellStyle.Padding = new Padding(5);
        }

        void LoadFoodData()
        {
            try
            {
                dgvFood.AutoGenerateColumns = false;

                string query = @"SELECT f.ItemID, f.ItemName, c.CategoryName, f.Price, f.ImagePath, f.CategoryID, f.IsActive 
                         FROM FoodItems f
                         INNER JOIN Categories c ON f.CategoryID = c.CategoryID
                         WHERE 1=1";

                if (chkHienThiDaXoa.Checked)
                {
                    // Nếu đang tích: Xem thùng rác (IsActive = 0)
                    query += " AND f.IsActive = 0";

                    // Đổi màu tiêu đề bảng sang màu Xám để người dùng biết đang ở thùng rác
                    dgvFood.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;

                    // Đổi tên nút Xóa thành "Khôi phục" 
                    btnXoa.Text = "Khôi phục";
                    btnXoa.FillColor = Color.SeaGreen; // Đổi màu xanh
                    btnXoa.FillColor2 = Color.OliveDrab;
                }
                else
                {
                    // Nếu không tích: Xem bình thường (IsActive = 1)
                    query += " AND f.IsActive = 1";

                    // Trả lại màu gốc (Màu đỏ cam của bạn)
                    dgvFood.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 50, 80);

                    // Trả lại nút Xóa
                    btnXoa.Text = "Xoá";
                    btnXoa.FillColor = Color.Firebrick;
                    btnXoa.FillColor2 = Color.Maroon;
                }
                // -----------------------

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvFood.DataSource = dt;

                    // A. Định dạng cột GIÁ TIỀN (Ví dụ: 25000 -> 25,000)
                    if (dgvFood.Columns.Contains("Price"))
                    {
                        dgvFood.Columns["Price"].DefaultCellStyle.Format = "N0"; // N0: Số nguyên, có dấu phẩy
                        dgvFood.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; // Căn phải
                    }

                    // B. Ẩn cột đường dẫn ảnh
                    if (dgvFood.Columns.Contains("ImagePath"))
                    {
                        dgvFood.Columns["ImagePath"].Visible = false;
                    }

                    // C. Ẩn cột ID Danh mục (Nếu lỡ hiện ra)
                    if (dgvFood.Columns.Contains("CategoryID"))
                    {
                        dgvFood.Columns["CategoryID"].Visible = false;
                    }

                    // D. Ẩn cột IsActive (Trạng thái)
                    if (dgvFood.Columns.Contains("IsActive"))
                    {
                        dgvFood.Columns["IsActive"].Visible = false;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        void LoadCategoryComboBox()
        {
            try
            {
                string query = "SELECT CategoryID, CategoryName FROM Categories";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Đổ dữ liệu vào ComboBox
                    cboCategory.DataSource = dt;
                    cboCategory.DisplayMember = "CategoryName"; // Cái hiện lên cho người dùng thấy
                    cboCategory.ValueMember = "CategoryID";     // Cái giá trị ẩn bên dưới (để lưu vào DB)

                    // Đặt mặc định không chọn gì cả (hoặc chọn cái đầu tiên tùy bạn)
                    cboCategory.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục: " + ex.Message);
            }
        }

        private void dgvFood_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvFood.Rows[e.RowIndex];

                txtFoodID.Text = row.Cells["ItemID"].Value.ToString();
                txtFoodName.Text = row.Cells["ItemName"].Value.ToString();
                txtFoodPrice.Text = row.Cells["Price"].Value.ToString();
                cboCategory.Text = row.Cells["CategoryName"].Value.ToString();

                // Xử lý ảnh
                if (dgvFood.Columns.Contains("ImagePath") && row.Cells["ImagePath"].Value != DBNull.Value)
                {
                    string fileName = row.Cells["ImagePath"].Value.ToString();

                    // 1. Hiển thị ảnh
                    picFood.Image = LoadImageSafe(fileName);

                    // 2. Lưu đường dẫn tuyệt đối vào Tag (ĐỂ NÚT SỬA HOẠT ĐỘNG)
                    // Nếu không có dòng này, khi bấm Lưu nó sẽ không tìm thấy ảnh cũ đâu
                    string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
                    picFood.Tag = fullPath;
                }
                else
                {
                    picFood.Image = null;
                    picFood.Tag = "";
                }

                // Gọi hàm cập nhật trạng thái nút bấm
                UpdateButtonState(true);
            }
        }

        private void txtSearchFood_TextChanged(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu gốc từ DataGridView ra
            DataTable dt = dgvFood.DataSource as DataTable;

            if (dt != null)
            {
                // 2. Lấy từ khóa
                string keyword = txtSearchFoodList.Text.Trim();

                // 3. XỬ LÝ KÝ TỰ ĐẶC BIỆT
                // Bước A: Xử lý dấu nháy đơn (để tránh lỗi cú pháp chuỗi)
                // Ví dụ: Món 'Ngon' -> Món ''Ngon''
                keyword = keyword.Replace("'", "''");

                // Bước B: Xử lý các ký tự có ý nghĩa riêng trong RowFilter ([, %, *)
                // Phải thay thế dấu [ đầu tiên để tránh lỗi chồng chéo
                keyword = keyword.Replace("[", "[[]");
                keyword = keyword.Replace("%", "[%]");
                keyword = keyword.Replace("*", "[*]");

                // 4. Áp dụng bộ lọc
                if (string.IsNullOrEmpty(keyword))
                {
                    dt.DefaultView.RowFilter = "";
                }
                else
                {
                    // Cú pháp: Tìm trong Tên Món HOẶC Tên Danh Mục
                    dt.DefaultView.RowFilter = string.Format("ItemName LIKE '%{0}%' OR CategoryName LIKE '%{0}%'", keyword);
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            // 1. Validation đầu vào
            if (string.IsNullOrWhiteSpace(txtFoodName.Text)) { MessageBox.Show("Chưa nhập tên món!"); return; }
            if (cboCategory.SelectedIndex == -1) { MessageBox.Show("Vui lòng chọn danh mục!"); return; }
            if (!decimal.TryParse(txtFoodPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Giá tiền không hợp lệ!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 2. KIỂM TRA TRÙNG TÊN (Kể cả món đã ẩn)
                    // Lấy cả IsActive để biết đường xử lý
                    string checkSQL = "SELECT ItemID, IsActive FROM FoodItems WHERE ItemName = @name";
                    SqlCommand cmdCheck = new SqlCommand(checkSQL, conn);
                    cmdCheck.Parameters.AddWithValue("@name", txtFoodName.Text.Trim());

                    SqlDataReader reader = cmdCheck.ExecuteReader();

                    if (reader.Read()) // Nếu tìm thấy tên trùng
                    {
                        int existingID = Convert.ToInt32(reader["ItemID"]);
                        bool isActive = Convert.ToBoolean(reader["IsActive"]);
                        reader.Close(); // Đóng reader để chạy lệnh khác

                        if (isActive)
                        {
                            // TRƯỜNG HỢP A: Tên trùng với món ĐANG BÁN
                            MessageBox.Show("Tên món này đã tồn tại trong thực đơn!", "Trùng lặp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            // TRƯỜNG HỢP B: Tên trùng với món ĐÃ ẨN (Ngừng kinh doanh)
                            DialogResult r = MessageBox.Show(
                                "Món ăn này đã từng tồn tại nhưng đang bị ẩn (Ngừng kinh doanh).\nBạn có muốn khôi phục và cập nhật giá mới cho nó không?",
                                "Phát hiện món cũ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (r == DialogResult.Yes)
                            {
                                // Thực hiện KHÔI PHỤC (Restore) + CẬP NHẬT thông tin mới
                                string sourcePath = (picFood.Tag != null) ? picFood.Tag.ToString() : "";
                                string savedPath = SaveImageToFolder(sourcePath);

                                string restoreSQL = @"UPDATE FoodItems 
                                              SET Price = @price, 
                                                  CategoryID = @catID, 
                                                  ImagePath = @img, 
                                                  IsActive = 1 
                                              WHERE ItemID = @id";

                                SqlCommand cmdRestore = new SqlCommand(restoreSQL, conn);
                                cmdRestore.Parameters.AddWithValue("@price", price);
                                cmdRestore.Parameters.AddWithValue("@catID", cboCategory.SelectedValue);
                                cmdRestore.Parameters.AddWithValue("@img", savedPath);
                                cmdRestore.Parameters.AddWithValue("@id", existingID);
                                cmdRestore.ExecuteNonQuery();

                                MessageBox.Show("Đã khôi phục món ăn thành công!");

                                // Refresh UI
                                LoadFoodData();
                                if (cboCategory.SelectedValue != null)
                                    LoadFoodItems(Convert.ToInt32(cboCategory.SelectedValue));
                                btnLamMoi_Click(null, null);
                                return; // Kết thúc hàm luôn
                            }
                            else
                            {
                                return; // Người dùng chọn No -> Không làm gì cả
                            }
                        }
                    }
                    reader.Close(); // Đóng reader nếu không tìm thấy trùng

                    // 3. THÊM MỚI HOÀN TOÀN (Nếu không trùng ai cả)
                    string newImgPath = SaveImageToFolder((picFood.Tag != null) ? picFood.Tag.ToString() : "");

                    string insertSQL = @"INSERT INTO FoodItems (ItemName, CategoryID, Price, ImagePath, IsActive) 
                                 VALUES (@name, @catID, @price, @img, 1)";

                    SqlCommand cmd = new SqlCommand(insertSQL, conn);
                    cmd.Parameters.AddWithValue("@name", txtFoodName.Text.Trim());
                    cmd.Parameters.AddWithValue("@catID", cboCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@img", newImgPath);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Thêm thành công!");
                LoadFoodData();
                if (cboCategory.SelectedValue != null)
                    LoadFoodItems(Convert.ToInt32(cboCategory.SelectedValue));
                btnLamMoi_Click(null, null);
                UpdateButtonState(false);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFoodID.Text))
            {
                MessageBox.Show("Vui lòng chọn món cần sửa!");
                return;
            }

            try
            {
                // 1. Lưu ảnh mới (hoặc giữ ảnh cũ)
                string sourcePath = (picFood.Tag != null) ? picFood.Tag.ToString() : "";
                string savedPath = SaveImageToFolder(sourcePath);

                // 2. Update SQL
                string query = @"UPDATE FoodItems 
                         SET ItemName=@name, CategoryID=@catID, Price=@price, ImagePath=@img 
                         WHERE ItemID=@id";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtFoodID.Text);
                    cmd.Parameters.AddWithValue("@name", txtFoodName.Text);
                    cmd.Parameters.AddWithValue("@catID", cboCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@price", Convert.ToDecimal(txtFoodPrice.Text));
                    cmd.Parameters.AddWithValue("@img", savedPath);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cập nhật thành công!");

                // --- CÁC BƯỚC CẬP NHẬT GIAO DIỆN  ---

                // A. Load lại bảng quản lý (để thấy dòng dữ liệu mới)
                LoadFoodData();

                // B. Load lại Menu bán hàng ngoài trang chủ (để thấy ảnh mới đổi)
                if (cboCategory.SelectedValue != null)
                {
                    // Lấy ID danh mục đang chọn để load lại đúng trang đó
                    int currentCatID = Convert.ToInt32(cboCategory.SelectedValue);
                    LoadFoodItems(currentCatID);
                }

                // C. Xóa trắng các ô nhập
                btnLamMoi_Click(null, null);
                UpdateButtonState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFoodID.Text)) return;

            // TRƯỜNG HỢP 1: ĐANG Ở CHẾ ĐỘ THÙNG RÁC -> THỰC HIỆN KHÔI PHỤC
            if (chkHienThiDaXoa.Checked)
            {
                if (MessageBox.Show("Bạn muốn khôi phục món ăn này bán lại?", "Khôi phục", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();
                            // Update lại IsActive = 1
                            string sql = "UPDATE FoodItems SET IsActive = 1 WHERE ItemID = @id";
                            SqlCommand cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("@id", txtFoodID.Text);
                            cmd.ExecuteNonQuery();
                        }
                        MessageBox.Show("Đã khôi phục món ăn!");
                        LoadFoodData(); // Refresh list

                        // Đồng bộ sang menu bán hàng
                        if (cboCategory.SelectedValue != null)
                            LoadFoodItems(Convert.ToInt32(cboCategory.SelectedValue));
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
                return; // Kết thúc, không chạy phần xóa bên dưới
            }

            // TRƯỜNG HỢP 2: ĐANG Ở CHẾ ĐỘ BÌNH THƯỜNG -> THỰC HIỆN XÓA 
            if (MessageBox.Show("Bạn có chắc muốn xóa món này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra đã bán chưa
                    string checkSql = "SELECT COUNT(*) FROM OrderDetails WHERE ItemID = @id";
                    SqlCommand cmdCheck = new SqlCommand(checkSql, conn);
                    cmdCheck.Parameters.AddWithValue("@id", txtFoodID.Text);
                    int count = (int)cmdCheck.ExecuteScalar();

                    if (count == 0)
                    {
                        // Chưa bán -> Xóa vĩnh viễn
                        string deleteSql = "DELETE FROM FoodItems WHERE ItemID = @id";
                        SqlCommand cmdDelete = new SqlCommand(deleteSql, conn);
                        cmdDelete.Parameters.AddWithValue("@id", txtFoodID.Text);
                        cmdDelete.ExecuteNonQuery();
                        MessageBox.Show("Đã xóa vĩnh viễn!");
                    }
                    else
                    {
                        // Đã bán -> Xóa mềm
                        string updateSql = "UPDATE FoodItems SET IsActive = 0 WHERE ItemID = @id";
                        SqlCommand cmdUpdate = new SqlCommand(updateSql, conn);
                        cmdUpdate.Parameters.AddWithValue("@id", txtFoodID.Text);
                        cmdUpdate.ExecuteNonQuery();
                        MessageBox.Show($"Đã chuyển vào thùng rác (Do đã bán {count} lần).");
                    }
                }

                LoadFoodData();
                if (cboCategory.SelectedValue != null)
                    LoadFoodItems(Convert.ToInt32(cboCategory.SelectedValue));
                btnLamMoi_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtFoodID.Clear();
            txtFoodName.Clear();
            txtFoodPrice.Clear();
            cboCategory.SelectedIndex = -1;
            UpdateButtonState(false);

            // Reset ảnh về ảnh gốc hoặc trống
            picFood.Image = null;
            picFood.Tag = null;

            // Nếu có ảnh placeholder thì dùng dòng dưới:
            // picFood.Image = Properties.Resources.placeholder;
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (open.ShowDialog() == DialogResult.OK)
            {
                // Hiển thị ảnh lên PictureBox để xem trước
                picFood.Image = Image.FromFile(open.FileName);

                // Lưu đường dẫn file gốc vào Tag để lát nữa dùng
                picFood.Tag = open.FileName;
            }
        }

        private string SaveImageToFolder(string sourcePath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourcePath)) return "";

                // 1. Xác định đường dẫn thư mục bin\Debug\Images (Nơi phần mềm đang chạy)
                string debugFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                if (!Directory.Exists(debugFolder)) Directory.CreateDirectory(debugFolder);

                // 2. Xác định đường dẫn thư mục GỐC Project\Images (Nơi lưu trữ an toàn)
                // Logic: Từ bin\Debug đi lùi ra 2 cấp cha sẽ về thư mục Project
                // Lưu ý: Cách này chỉ chạy đúng khi đang Dev trong Visual Studio
                string projectFolder = "";
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                    if (dirInfo.Parent != null && dirInfo.Parent.Parent != null)
                    {
                        projectFolder = Path.Combine(dirInfo.Parent.Parent.FullName, "Images");
                        if (!Directory.Exists(projectFolder)) Directory.CreateDirectory(projectFolder);
                    }
                }
                catch { /* Bỏ qua nếu không tìm thấy thư mục Project (ví dụ khi đã đóng gói exe) */ }

                // 3. Kiểm tra xem ảnh nguồn có phải là ảnh đã nằm trong kho không
                // Nếu chọn ảnh từ chính thư mục Debug hoặc Project thì không cần copy, lấy tên luôn
                if (sourcePath.Contains(debugFolder) || (!string.IsNullOrEmpty(projectFolder) && sourcePath.Contains(projectFolder)))
                {
                    return Path.GetFileName(sourcePath);
                }

                // 4. Tạo tên file mới (Dùng Guid để không trùng)
                string fileExtension = Path.GetExtension(sourcePath);
                string newFileName = Guid.NewGuid().ToString() + fileExtension;

                // 5. COPY VÀO bin\Debug (Để hiện lên ngay lập tức)
                string destDebug = Path.Combine(debugFolder, newFileName);
                File.Copy(sourcePath, destDebug, true);

                // 6. COPY VÀO Project\Images (Để sao lưu vĩnh viễn)
                if (!string.IsNullOrEmpty(projectFolder))
                {
                    string destProject = Path.Combine(projectFolder, newFileName);
                    File.Copy(sourcePath, destProject, true);
                }

                // Trả về tên file để lưu xuống SQL
                return newFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu ảnh: " + ex.Message);
                return "";
            }
        }

        private Image LoadImageSafe(string fileNameOrPath)
        {
            try
            {
                if (string.IsNullOrEmpty(fileNameOrPath)) return null;

                // 1. Chỉ lấy phần TÊN FILE 
                // Ví dụ: Đầu vào là "Images\ga.png" -> Lấy ra "ga.png"
                string cleanFileName = Path.GetFileName(fileNameOrPath);

                // 2. Tạo đường dẫn tuyệt đối vào thư mục Images
                string folderImages = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string absolutePath = Path.Combine(folderImages, cleanFileName);

                // 3. Kiểm tra file có tồn tại không
                if (File.Exists(absolutePath))
                {
                    // Load ảnh theo cách không khóa file (để sau này còn xóa/sửa được)
                    byte[] buffer = File.ReadAllBytes(absolutePath);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        return Image.FromStream(ms);
                    }
                }
                return null; // Không thấy file
            }
            catch (Exception ex)
            {
                // Debug để biết lỗi gì (ví dụ file ảnh bị hỏng)
                System.Diagnostics.Debug.WriteLine("Lỗi load ảnh: " + ex.Message);
                return null;
            }
        }

        void LoadTableMap()
        {
            pnlMap.Controls.Clear(); // Xóa bản đồ cũ

            try
            {
                string query = "SELECT * FROM BanAn";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        Guna.UI2.WinForms.Guna2CircleButton btnTable = new Guna.UI2.WinForms.Guna2CircleButton();

                        btnTable.Width = 60;
                        btnTable.Height = 60;
                        btnTable.Text = row["TableName"].ToString();
                        btnTable.Tag = row["TableID"].ToString();

                        // --- CODE MỚI: Xử lý màu sắc cho 3 trạng thái ---
                        string status = row["TrangThai"].ToString();
                        switch (status)
                        {
                            case "Có Khách":
                                btnTable.FillColor = Color.Crimson; 
                                break;
                            case "Đặt Trước":
                                btnTable.FillColor = Color.Orange; 
                                break;
                            default:
                                btnTable.FillColor = Color.Teal;  
                                break;
                        }
                        // ------------------------------------------------

                        // Lấy tọa độ
                        int x = (row["PositionX"] != DBNull.Value) ? Convert.ToInt32(row["PositionX"]) : 10;
                        int y = (row["PositionY"] != DBNull.Value) ? Convert.ToInt32(row["PositionY"]) : 10;
                        btnTable.Location = new Point(x, y);

                        // Gắn sự kiện
                        btnTable.MouseDown += BtnTable_MouseDown;
                        btnTable.MouseMove += BtnTable_MouseMove;
                        btnTable.MouseUp += BtnTable_MouseUp;
                        btnTable.Click += BtnTable_Click;

                        pnlMap.Controls.Add(btnTable);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // Khi bắt đầu nhấn chuột xuống bàn
        private void BtnTable_MouseDown(object sender, MouseEventArgs e)
        {
            isDragging = true;
            // Lưu vị trí con trỏ chuột và vị trí bàn hiện tại
            dragCursorPoint = Cursor.Position;
            dragControlPoint = ((Control)sender).Location;

            // Đánh dấu đây là cái bàn đang thao tác
            activeTableButton = (Control)sender;
            activeTableButton.BringToFront(); // Đưa lên lớp trên cùng
        }

        // Khi di chuyển chuột
        private void BtnTable_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // Tính toán vị trí mới
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                activeTableButton.Location = Point.Add(dragControlPoint, new Size(dif));
            }
        }

        // Khi nhả chuột ra
        private void BtnTable_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            // Lúc này cái bàn đã nằm ở vị trí mới, nhưng chưa lưu vào SQL
            // Phải bấm nút "Lưu Vị Trí" mới lưu.
        }

        // Khi Click vào bàn (Để hiện thông tin sang bên phải sửa)
        private void BtnTable_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2CircleButton btn = (Guna.UI2.WinForms.Guna2CircleButton)sender;
            txtTableID.Text = btn.Tag.ToString();
            txtTableName.Text = btn.Text;

            if (btn.FillColor == Color.Crimson)
            {
                cboTableStatus.Text = "Có Khách";

                _currentTableID = int.Parse(btn.Tag.ToString());
                LoadOrderFromTable(_currentTableID);
                lblBanDangChon.Text = btn.Text;
            }
            else if (btn.FillColor == Color.Orange)
                cboTableStatus.Text = "Đặt Trước";
            else
                cboTableStatus.Text = "Trống";
        }

        private void btnSaveLayout_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    foreach (Control c in pnlMap.Controls)
                    {
                        if (c is Guna.UI2.WinForms.Guna2CircleButton btn)
                        {
                            // --- Update bảng BanAn ---
                            string query = "UPDATE BanAn SET PositionX = @x, PositionY = @y WHERE TableID = @id";

                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@x", btn.Location.X);
                            cmd.Parameters.AddWithValue("@y", btn.Location.Y);
                            cmd.Parameters.AddWithValue("@id", btn.Tag.ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                MessageBox.Show("Đã lưu sơ đồ nhà hàng!");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnAddTable_Click(object sender, EventArgs e)
        {
            // Kiểm tra nhập liệu
            if (string.IsNullOrWhiteSpace(txtTableName.Text))
            {
                MessageBox.Show("Vui lòng nhập tên bàn!");
                return;
            }

            try
            {
                // --- CODE MỚI: Random vị trí để bàn không đè lên nhau ---
                Random r = new Random();
                int x = r.Next(20, 300); // Random chiều ngang từ 20 đến 300
                int y = r.Next(20, 200); // Random chiều dọc từ 20 đến 200

                string query = "INSERT INTO BanAn (TableName, TrangThai, PositionX, PositionY) VALUES (@name, N'Trống', @x, @y)";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", txtTableName.Text);
                    cmd.Parameters.AddWithValue("@x", x);
                    cmd.Parameters.AddWithValue("@y", y);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Thêm bàn mới thành công!");

                // Cập nhật lại bản đồ
                LoadTableMap();

                // Xóa trắng ô nhập
                btnNewTable_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnEditTable_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTableID.Text))
            {
                MessageBox.Show("Vui lòng chọn một bàn trên bản đồ để sửa!");
                return;
            }

            try
            {
                string query = "UPDATE BanAn SET TableName = @name, TrangThai = @status WHERE TableID = @id";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", txtTableID.Text);
                    cmd.Parameters.AddWithValue("@name", txtTableName.Text);

                    // Nếu người dùng không chọn trạng thái thì mặc định là Trống
                    string status = cboTableStatus.SelectedItem != null ? cboTableStatus.SelectedItem.ToString() : "Trống";
                    cmd.Parameters.AddWithValue("@status", status);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cập nhật thông tin bàn thành công!");
                LoadTableMap(); // Vẽ lại màu sắc/tên mới
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnDeleteTable_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTableID.Text)) return;

            if (MessageBox.Show("Bạn chắc chắn muốn xóa bàn này?\n(Lưu ý: Không thể xóa nếu bàn đã từng có hóa đơn)",
                                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string query = "DELETE FROM BanAn WHERE TableID = @id";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@id", txtTableID.Text);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đã xóa bàn!");
                    LoadTableMap(); // Vẽ lại bản đồ (bàn đó sẽ biến mất)
                    btnNewTable_Click(null, null); // Reset ô nhập
                }
                catch (Exception ex)
                {
                    // Lỗi này thường do khóa ngoại (Foreign Key) bên bảng Orders
                    MessageBox.Show("Không thể xóa bàn này vì dữ liệu ràng buộc (Đã từng có khách ngồi).\n" + ex.Message);
                }
            }
        }

        private void btnNewTable_Click(object sender, EventArgs e)
        {
            txtTableID.Clear();
            txtTableName.Clear();
            cboTableStatus.SelectedIndex = -1;
            txtTableName.Focus(); // Đưa con trỏ chuột vào ô tên để nhập ngay
        }

        void LoadBillListByDate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                dgvBillList.AutoGenerateColumns = false;

                // 1. Reset biểu đồ
                chartRevenue.Series["Doanh thu"].Points.Clear();

                string query = @"
            SELECT 
                o.OrderID, 
                b.TableName, 
                s.FullName, 
                o.OrderTime, 
                o.TotalAmount
            FROM Orders o
            LEFT JOIN BanAn b ON o.TableID = b.TableID
            LEFT JOIN Staff s ON o.StaffID = s.StaffID
            WHERE o.OrderTime >= @fromDate AND o.OrderTime <= @toDate
            AND o.TrangThai = N'Đã thanh toán'
            ORDER BY o.OrderTime ASC"; // Sắp xếp theo ngày để vẽ biểu đồ cho đẹp

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fromDate", fromDate.Date);
                    cmd.Parameters.AddWithValue("@toDate", toDate.Date.AddDays(1).AddSeconds(-1));

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvBillList.DataSource = dt;

                    // --- TÍNH TOÁN & VẼ BIỂU ĐỒ ---
                    double totalRevenue = 0;

                    // Dictionary để gom nhóm doanh thu theo ngày (Ví dụ: Ngày 10 bán 3 đơn thì cộng dồn lại)
                    var revenueByDate = new Dictionary<string, double>();

                    foreach (DataRow row in dt.Rows)
                    {
                        double amount = Convert.ToDouble(row["TotalAmount"]);
                        DateTime time = Convert.ToDateTime(row["OrderTime"]);

                        // Cộng tổng doanh thu
                        totalRevenue += amount;

                        // Gom nhóm cho biểu đồ (Key là ngày tháng dạng chuỗi)
                        string dateKey = time.ToString("dd/MM");
                        if (revenueByDate.ContainsKey(dateKey))
                            revenueByDate[dateKey] += amount;
                        else
                            revenueByDate.Add(dateKey, amount);
                    }

                    // Hiển thị tổng tiền
                    lblTotalRevenue.Text = totalRevenue.ToString("N0") + " đ";

                    // Đẩy dữ liệu vào Chart
                    foreach (var item in revenueByDate)
                    {
                        // item.Key là ngày, item.Value là tiền
                        chartRevenue.Series["Doanh thu"].Points.AddXY(item.Key, item.Value);
                    }

                    // Format cột tiền trên biểu đồ hiển thị dạng số cho dễ nhìn
                    chartRevenue.Series["Doanh thu"].LabelFormat = "#,##0";
                    chartRevenue.Series["Doanh thu"].IsValueShownAsLabel = true; // Hiện số tiền trên đầu cột
                }

                // Gọi thêm hàm Top món ăn
                LoadTopSellingFood(fromDate, toDate);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void btnViewBill_Click(object sender, EventArgs e)
        {
            LoadBillListByDate(dtpFromDate.Value, dtpToDate.Value);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (lblWelcome != null)
            {
                lblWelcome.Text = $"Xin chào, <b>{CurrentStaffName}</b>";
            }
            ApplyAuthorization();
            LoadCategories();
            LoadTableComboBox();
            LoadCategories();
            DateTime today = DateTime.Now;
            DateTime firstDay = new DateTime(today.Year, today.Month, 1);

            cboTableStatus.Items.Clear();
            cboTableStatus.Items.Add("Trống");
            cboTableStatus.Items.Add("Có Khách");
            cboTableStatus.Items.Add("Đặt Trước");

            dtpFromDate.Value = firstDay;
            dtpToDate.Value = today;
            LoadTableComboBox();// Load danh sách bàn

            // --- CẤU HÌNH LƯỚI THỐNG KÊ ---
            dgvBillList.ReadOnly = true; // Chặn sửa dữ liệu
            dgvBillList.AllowUserToAddRows = false; // Chặn dòng trống ở cuối
            dgvBillList.AllowUserToDeleteRows = false; // Chặn phím Delete trên bàn phím
            dgvBillList.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Chọn là chọn cả dòng cho đẹp
            dgvBillList.MultiSelect = false; // Chỉ cho chọn 1 dòng mỗi lần để tránh lỗi khi xóa
            dgvBillList.RowHeadersVisible = false; // Ẩn cột header bên trái ngoài cùng
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanelOrder.Controls.Count == 0) return;

            DialogResult result = MessageBox.Show("Bạn có chắc muốn xóa toàn bộ giỏ hàng?",
                                                  "Xác nhận",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // 1. Dọn dẹp bộ nhớ (Image)
                foreach (Control c in flowLayoutPanelOrder.Controls)
                {
                    if (c is OrderItemCard item)
                    {
                        if (item.ItemImage != null) item.ItemImage.Dispose();
                        item.Dispose();
                    }
                }

                // 2. Xóa sạch controls
                flowLayoutPanelOrder.Controls.Clear();

                // 3. Tính lại tiền (về 0) & hiện Empty State
                UpdateOrderSummary();
            }
        }

        private void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            // 1. Nếu ô trống -> Coi như là 0
            if (string.IsNullOrEmpty(txtDiscount.Text))
            {
                UpdateOrderSummary(); // Tính lại tiền với mức giảm 0%
                return;
            }

            // 2. Ràng buộc giá trị
            if (int.TryParse(txtDiscount.Text, out int discount))
            {
                if (discount > 100)
                {
                    MessageBox.Show("Giảm giá tối đa là 100%!", "Cảnh báo");
                    txtDiscount.Text = "100";
                    txtDiscount.SelectionStart = txtDiscount.Text.Length; // Đưa con trỏ về cuối
                }
                // Logic tính tiền
                UpdateOrderSummary();
            }
            else
            {
                // Trường hợp copy-paste chữ vào thì xóa đi
                txtDiscount.Text = "0";
            }
        }

        private void txtDiscount_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho phép nhập số (Digit) và phím điều khiển (Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Chặn không cho nhập
            }
        }

        private void timerClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        // Hàm load danh sách bàn vào ComboBox
        private void LoadTableComboBox()
        {
            try
            {
                // Chỉ lấy bàn có trạng thái là 'Trống'
                // Các bàn 'Có Khách' hoặc 'Đặt Trước' sẽ TỰ ĐỘNG BỊ LOẠI BỎ
                string query = "SELECT TableID, TableName FROM BanAn WHERE TrangThai = N'Trống'";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboBanTrong.DataSource = dt;
                    cboBanTrong.DisplayMember = "TableName";
                    cboBanTrong.ValueMember = "TableID";
                    cboBanTrong.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load danh sách bàn: " + ex.Message);
            }
        }

        private void txtFoodPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho phép nhập số (Digit) và phím điều khiển (như Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Chặn ký tự đó lại
            }
        }

        // Hàm này sẽ chạy ngay khi Form mở lên
        private void ApplyAuthorization()
        {
            // Nếu là Admin thì mở hết (return luôn)
            if (CurrentStaffRole == "Admin") return;

            // --- NẾU LÀ STAFF (NHÂN VIÊN) ---

            // 1. SIDEBAR (THANH BÊN)
            // Yêu cầu: Hiển thị lại nút Bill và Setting
            btnBill.Visible = true;
            btnSetting.Visible = true;

            // 2. QUẢN LÝ MÓN ĂN (pnlFoodManager) -> GIỮ NGUYÊN KHÓA
            btnThem.Enabled = false;
            btnSua.Enabled = false;
            btnXoa.Enabled = false;
            btnLamMoi.Enabled = false;
            btnChonAnh.Enabled = false;
            btnQuanLyDanhMuc.Enabled = false;
            txtFoodName.ReadOnly = true;
            txtFoodPrice.ReadOnly = true;
            cboCategory.Enabled = false;
            chkHienThiDaXoa.Visible = false;

            // 3. QUẢN LÝ BÀN (pnlTableManager) -> ĐIỀU CHỈNH
            // Cấm thay đổi cấu trúc (Thêm, Xóa, Lưu vị trí Map)
            btnAddTable.Enabled = false;
            btnDeleteTable.Enabled = false;
            btnNewTable.Enabled = false;
            btnSaveLayout.Enabled = false; // Cấm nhân viên nghịch làm hỏng sơ đồ

            // CHO PHÉP: Điều chỉnh trạng thái bàn
            btnEditTable.Enabled = true;   // Mở nút Sửa
            cboTableStatus.Enabled = true; // Mở ComboBox chọn trạng thái
            txtTableName.ReadOnly = false; // Phải mở ô này thì nút Sửa mới hoạt động được

            // 4. THỐNG KÊ (pnlStatistics) -> ĐIỀU CHỈNH
            // Được xem (đã mở nút btnBill ở trên), nhưng CẤM XÓA BILL
            btnXoaBill.Enabled = false;    // Khóa nút xóa hóa đơn
                                           // Nút "Xem chi tiết" vẫn hoạt động bình thường (mặc định Enabled = true)

            // 5. CÀI ĐẶT (pnlSetting) -> ĐIỀU CHỈNH
            // Được vào xem danh sách, nhưng KHÔNG ĐƯỢC THAO TÁC dữ liệu nhân viên
            btnStaffAdd.Enabled = false;
            btnStaffEdit.Enabled = false;
            btnStaffDelete.Enabled = false;
            btnStaffClear.Enabled = false;

            // Khóa các ô nhập liệu nhân viên
            txtStaffName.ReadOnly = true;
            txtStaffUser.ReadOnly = true;
            txtStaffPass.ReadOnly = true;
            cboRole.Enabled = false;
        }

        private void chkMangVe_CheckedChanged(object sender, EventArgs e)
        {
            // Nếu chọn Mang về -> Khóa chọn bàn, Xóa chọn bàn
            if (chkMangVe.Checked)
            {
                cboBanTrong.SelectedIndex = -1;
                cboBanTrong.Enabled = false;
            }
            else
            {
                cboBanTrong.Enabled = true;
            }
        }

        private void btnGoiMon_Click(object sender, EventArgs e)
        {
            // Ràng buộc: Mang về không cần gọi món
            if (chkMangVe.Checked)
            {
                MessageBox.Show("Khách mang về vui lòng bấm THANH TOÁN luôn!", "Sai thao tác", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ràng buộc: Giỏ hàng trống
            if (flowLayoutPanelOrder.Controls.Count == 0)
            {
                MessageBox.Show("Chưa chọn món nào cả!", "Giỏ trống", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // TRƯỜNG HỢP 1: TẠO ĐƠN MỚI (Từ bàn trống)
                if (_currentOrderID == 0)
                {
                    if (cboBanTrong.SelectedIndex == -1)
                    {
                        MessageBox.Show("Vui lòng chọn bàn trống để gọi món!", "Thiếu thông tin");
                        return;
                    }

                    int newTableID = Convert.ToInt32(cboBanTrong.SelectedValue);

                    // Tạo Header hóa đơn
                    string insertOrder = @"INSERT INTO Orders (TableID, OrderTime, TotalAmount, TrangThai, StaffID) 
                                   VALUES (@TableID, GETDATE(), 0, N'Chờ thanh toán', @StaffID);
                                   SELECT SCOPE_IDENTITY();";

                    SqlCommand cmd = new SqlCommand(insertOrder, conn);
                    cmd.Parameters.AddWithValue("@TableID", newTableID);
                    cmd.Parameters.AddWithValue("@StaffID", CurrentStaffID);

                    _currentOrderID = Convert.ToInt32(cmd.ExecuteScalar());
                    _currentTableID = newTableID; // Lưu lại để dùng tiếp

                    // Cập nhật bàn -> Có Khách
                    SqlCommand cmdTable = new SqlCommand("UPDATE BanAn SET TrangThai = N'Có Khách' WHERE TableID = @id", conn);
                    cmdTable.Parameters.AddWithValue("@id", newTableID);
                    cmdTable.ExecuteNonQuery();
                    SaveOrderDetails(_currentOrderID, conn);
                }

                // TRƯỜNG HỢP 2: CẬP NHẬT ĐƠN CŨ (Đã có _currentOrderID từ nút Lấy Đơn hoặc vừa tạo ở trên)

                SqlCommand cmdDel = new SqlCommand("DELETE FROM OrderDetails WHERE OrderID = @id", conn);
                cmdDel.Parameters.AddWithValue("@id", _currentOrderID);
                cmdDel.ExecuteNonQuery();

                foreach (Control c in flowLayoutPanelOrder.Controls)
                {
                    if (c is OrderItemCard item)
                    {
                        string insertDetail = @"INSERT INTO OrderDetails (OrderID, ItemID, Quantity, PriceAtTime) 
                                        VALUES (@oid, @iid, @qty, @price)";
                        SqlCommand cmdDetail = new SqlCommand(insertDetail, conn);
                        cmdDetail.Parameters.AddWithValue("@oid", _currentOrderID);
                        cmdDetail.Parameters.AddWithValue("@iid", item.Tag);
                        cmdDetail.Parameters.AddWithValue("@qty", item.ItemQuantity);
                        cmdDetail.Parameters.AddWithValue("@price", item.ItemPrice);
                        cmdDetail.ExecuteNonQuery();
                    }
                }
            }

            MessageBox.Show("Đã gửi xuống bếp thành công!");

            // Reset sau khi gọi món xong
            ResetOrderUI();
        }

        // Viết hàm riêng để load lại đơn
        private void LoadOrderFromTable(int tableID)
        {
            // 1. Xóa sạch giỏ hàng hiện tại
            flowLayoutPanelOrder.Controls.Clear();
            _currentOrderID = 0; // Reset tạm

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 2. Tìm xem bàn này có đơn nào đang "Chờ thanh toán" không
                string findOrder = "SELECT OrderID FROM Orders WHERE TableID = @tid AND TrangThai = N'Chờ thanh toán'";
                SqlCommand cmdFind = new SqlCommand(findOrder, conn);
                cmdFind.Parameters.AddWithValue("@tid", tableID);

                object result = cmdFind.ExecuteScalar();

                if (result != null) // A ha! Bàn này đang có khách
                {
                    _currentOrderID = Convert.ToInt32(result); // Lưu lại ID để lát nữa update

                    // 3. Load chi tiết món ăn của đơn này đổ vào giỏ hàng
                    string getDetails = @"SELECT d.ItemID, f.ItemName, d.PriceAtTime, d.Quantity, f.ImagePath 
                                  FROM OrderDetails d 
                                  JOIN FoodItems f ON d.ItemID = f.ItemID 
                                  WHERE d.OrderID = @oid";

                    SqlCommand cmdDet = new SqlCommand(getDetails, conn);
                    cmdDet.Parameters.AddWithValue("@oid", _currentOrderID);

                    SqlDataReader reader = cmdDet.ExecuteReader();
                    while (reader.Read())
                    {
                        // Gọi lại hàm thêm món vào giao diện (nhớ sửa hàm này để nhận ImagePath hoặc load lại ảnh)
                        int id = Convert.ToInt32(reader["ItemID"]);
                        string name = reader["ItemName"].ToString();
                        double price = Convert.ToDouble(reader["PriceAtTime"]);
                        int qty = Convert.ToInt32(reader["Quantity"]);

                        // Tự viết hàm load ảnh từ đường dẫn
                        string imgPath = reader["ImagePath"] != DBNull.Value ? reader["ImagePath"].ToString() : "";
                        Image img = LoadImageSafe(imgPath);

                        // Thêm vào UI (Bạn cần sửa hàm AddOrUpdate một chút để hỗ trợ set số lượng > 1 ngay từ đầu)
                        // Hoặc code thủ công:
                        OrderItemCard item = new OrderItemCard();
                        item.Tag = id;
                        item.ItemName = name;
                        item.ItemPrice = price;
                        item.ItemQuantity = qty;
                        item.ItemImage = img;
                        // ... gắn các sự kiện ...
                        flowLayoutPanelOrder.Controls.Add(item);
                    }
                }
            }
            UpdateOrderSummary(); // Tính lại tổng tiền
        }

        private void cboBan_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra an toàn
            if (cboBanTrong.SelectedIndex == -1 || cboBanTrong.SelectedValue == null) return;

            int tableID;
            if (int.TryParse(cboBanTrong.SelectedValue.ToString(), out tableID))
            {
                // Reset trạng thái
                _currentOrderID = 0;
                flowLayoutPanelOrder.Controls.Clear();
                UpdateOrderSummary();

                // Kiểm tra xem bàn này có đơn treo không
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy ID hóa đơn đang "Chờ thanh toán" của bàn này
                    string sql = "SELECT OrderID FROM Orders WHERE TableID = @tid AND TrangThai = N'Chờ thanh toán'";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@tid", tableID);
                    object result = cmd.ExecuteScalar();

                    if (result != null) // Bàn ĐANG CÓ KHÁCH
                    {
                        _currentOrderID = Convert.ToInt32(result);

                        // Load lại món ăn lên giao diện
                        string sqlDetail = @"SELECT d.ItemID, f.ItemName, d.PriceAtTime, d.Quantity, f.ImagePath 
                                     FROM OrderDetails d JOIN FoodItems f ON d.ItemID = f.ItemID 
                                     WHERE d.OrderID = @oid";
                        SqlCommand cmdDet = new SqlCommand(sqlDetail, conn);
                        cmdDet.Parameters.AddWithValue("@oid", _currentOrderID);
                        SqlDataReader reader = cmdDet.ExecuteReader();

                        while (reader.Read())
                        {
                            // Thêm thẻ món ăn vào lại list (Code cũ của bạn)
                            AddOrUpdateItemInOrderList(
                                Convert.ToInt32(reader["ItemID"]),
                                reader["ItemName"].ToString(),
                                Convert.ToDouble(reader["PriceAtTime"]),
                                LoadImageSafe(reader["ImagePath"]?.ToString()) // Nhớ dùng hàm load ảnh an toàn
                            );

                            // Cập nhật số lượng đúng như trong DB
                            // (Lưu ý: Hàm AddOrUpdate mặc định sl=1, nên bạn cần chỉnh lại sl thủ công ở đây hoặc sửa hàm đó)
                            var item = (OrderItemCard)flowLayoutPanelOrder.Controls[flowLayoutPanelOrder.Controls.Count - 1];
                            item.ItemQuantity = Convert.ToInt32(reader["Quantity"]);
                        }
                    }
                }
            }
        }

        private void btnHuyDon_Click(object sender, EventArgs e)
        {
            if (_currentOrderID == 0) return;

            if (MessageBox.Show("Hủy đơn này và trả bàn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Xóa chi tiết
                    new SqlCommand("DELETE FROM OrderDetails WHERE OrderID = " + _currentOrderID, conn).ExecuteNonQuery();
                    // Xóa Order
                    new SqlCommand("DELETE FROM Orders WHERE OrderID = " + _currentOrderID, conn).ExecuteNonQuery();
                    // Trả bàn về Trống
                    if (_currentTableID > 0)
                    {
                        SqlCommand cmd = new SqlCommand("UPDATE BanAn SET TrangThai = N'Trống' WHERE TableID = @tid", conn);
                        cmd.Parameters.AddWithValue("@tid", _currentTableID);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Đã hủy đơn!");

                // Gọi hàm Reset đã sửa ở trên
                ResetOrderUI();
            }
        }

        private void btnLayDon_Click(object sender, EventArgs e)
        {
            FormBillList frm = new FormBillList();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                // 1. Nhận dữ liệu từ form con
                _currentOrderID = frm.SelectedOrderID;
                _currentTableID = frm.SelectedTableID;

                // 2. Hiển thị thông tin lên giao diện
                // Truy vấn lấy tên bàn để hiển thị cho đẹp
                string tableName = GetTableName(_currentTableID);
                lblBanDangChon.Text = tableName; // Hiển thị lên Label

                // 3. Load món ăn của đơn này vào giỏ
                LoadOrderItemsFromSQL(_currentOrderID);

                // 4. Khóa các control không liên quan để tránh thao tác sai
                cboBanTrong.SelectedIndex = -1;
                cboBanTrong.Enabled = false; // Đang sửa đơn cũ thì không được chọn bàn trống khác
                chkMangVe.Enabled = false;   // Đang sửa đơn bàn thì không được chuyển sang mang về
            }
        }

        // Hàm bổ trợ: Load món từ SQL vào UI
        private void LoadOrderItemsFromSQL(int orderID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = @"SELECT d.ItemID, f.ItemName, d.PriceAtTime, d.Quantity, f.ImagePath 
                       FROM OrderDetails d JOIN FoodItems f ON d.ItemID = f.ItemID 
                       WHERE d.OrderID = @oid";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@oid", orderID);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    AddOrUpdateItemInOrderList(
                        Convert.ToInt32(reader["ItemID"]),
                        reader["ItemName"].ToString(),
                        Convert.ToDouble(reader["PriceAtTime"]),
                        LoadImageSafe(reader["ImagePath"]?.ToString())
                    );

                    // Cập nhật số lượng
                    var item = (OrderItemCard)flowLayoutPanelOrder.Controls[flowLayoutPanelOrder.Controls.Count - 1];
                    item.ItemQuantity = Convert.ToInt32(reader["Quantity"]);
                }
            }
            UpdateOrderSummary();

            // Khóa cboBanTrong lại để tránh nhầm lẫn
            cboBanTrong.Enabled = false;
        }

        // Hàm reset giao diện về trạng thái ban đầu
        private void ResetOrderUI()
        {
            flowLayoutPanelOrder.Controls.Clear();
            UpdateOrderSummary();

            _currentOrderID = 0;
            _currentTableID = 0;

            lblBanDangChon.Text = "Giỏ hàng"; // Reset label thông báo
            cboBanTrong.SelectedIndex = -1;
            cboBanTrong.Enabled = true; // Mở lại chọn bàn
            chkMangVe.Checked = false;
            chkMangVe.Enabled = true;   // Mở lại mang về

            LoadTableComboBox(); // Load lại ds bàn trống mới nhất
            LoadTableMap();      // Vẽ lại màu bản đồ
        }

        // Hàm lấy tên bàn từ ID (để hiện lên Label)
        private string GetTableName(int tableID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT TableName FROM BanAn WHERE TableID = " + tableID, conn);
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "Bàn " + tableID;
            }
        }

        // Hàm lưu chi tiết (Rút gọn code cho nút Thanh Toán)
        private void SaveOrderDetails(int orderID, SqlConnection conn)
        {
            foreach (Control c in flowLayoutPanelOrder.Controls)
            {
                if (c is OrderItemCard item)
                {
                    string sql = "INSERT INTO OrderDetails VALUES (@oid, @iid, @qty, @price)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@oid", orderID);
                    cmd.Parameters.AddWithValue("@iid", item.Tag);
                    cmd.Parameters.AddWithValue("@qty", item.ItemQuantity);
                    cmd.Parameters.AddWithValue("@price", item.ItemPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void ExportAndPrintBill(int orderID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy dữ liệu từ View
                    string query = "SELECT * FROM View_InHoaDon WHERE OrderID = @id";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", orderID);

                    dsBill ds = new dsBill();
                    da.Fill(ds, "dtHoaDon");

                    if (ds.Tables["dtHoaDon"].Rows.Count == 0) return;

                    // Nạp dữ liệu vào Report
                    rptBill report = new rptBill();
                    report.SetDataSource(ds);

                    // --- XUẤT RA FILE PDF ---
                    // Tạo thư mục "HoaDon" ngay cạnh file .exe
                    string folderPath = Path.Combine(Application.StartupPath, "HoaDon");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    // Đặt tên file: Bill_MãĐơn_GiờPhútGiây.pdf
                    string fileName = $"Bill_{orderID}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    string fullPath = Path.Combine(folderPath, fileName);

                    // Xuất file
                    report.ExportToDisk(ExportFormatType.PortableDocFormat, fullPath);

                    // --- TỰ ĐỘNG MỞ FILE LÊN ---
                    // Lệnh này sẽ mở file PDF bằng trình xem mặc định (Chrome, Edge, Acrobat...)
                    // Từ đó bạn bấm Ctrl+P để in rất nhanh và đẹp
                    System.Diagnostics.Process.Start(fullPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất hóa đơn: " + ex.Message);
            }
        }

        private void btnXemChiTiet_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem người dùng đã chọn dòng nào chưa
            if (dgvBillList.SelectedRows.Count == 0 || dgvBillList.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn trong danh sách để xem!", "Chưa chọn hóa đơn");
                return;
            }

            try
            {
                // 2. Lấy OrderID từ dòng đang chọn
                // Lưu ý: Đảm bảo tên cột "OrderID" đúng với tên bạn đặt trong Edit Columns của DataGridView
                if (dgvBillList.CurrentRow.Cells["OrderID"].Value != null)
                {
                    int orderID = Convert.ToInt32(dgvBillList.CurrentRow.Cells["OrderID"].Value);

                    // 3. Mở Form in hóa đơn (FormPrintBill)
                    FormPrintBill frm = new FormPrintBill(orderID);
                    frm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi mở hóa đơn: " + ex.Message);
            }
        }

        void UpdateButtonState(bool isEditing)
        {
            // Nếu đang là nhân viên (không phải Admin) thì chặn hết, không cần check tiếp
            if (CurrentStaffRole != "Admin") return;

            if (isEditing)
            {
                // Đang chọn dòng để sửa: Khóa Thêm, Mở Sửa/Xóa
                btnThem.Enabled = false;
                btnSua.Enabled = true;
                btnXoa.Enabled = true;

                // (Optional) Đổi màu nút Thêm cho xám đi để người dùng biết
                btnThem.FillColor = Color.LightGray;
            }
            else
            {
                // Đang nhập mới: Mở Thêm, Khóa Sửa/Xóa
                btnThem.Enabled = true;
                btnSua.Enabled = false;
                btnXoa.Enabled = false;

                btnThem.FillColor = Color.DarkGreen; // Trả lại màu xanh
            }
        }

        private void chkHienThiDaXoa_CheckedChanged(object sender, EventArgs e)
        {
            LoadFoodData();
            btnLamMoi_Click(null, null);
        }

        private void btnQuanLyDanhMuc_Click(object sender, EventArgs e)
        {
            // 1. Mở Form quản lý danh mục dưới dạng Dialog (Cửa sổ con)
            FormCategoryManager frm = new FormCategoryManager();
            frm.ShowDialog();

            // 2. SAU KHI FORM ĐÓ ĐÓNG LẠI -> LOAD LẠI COMBOBOX NGAY
            // Để danh mục vừa thêm mới xuất hiện ngay lập tức
            LoadCategoryComboBox();

            // (Optional) Nếu bạn đang ở trang Bán hàng (Home) cũng nên load lại sidebar danh mục
            LoadCategories();
        }

        private void LoadTopSellingFood(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Query: Tính tổng số lượng bán của từng món, sắp xếp giảm dần, lấy dòng đầu tiên
                string query = @"
            SELECT TOP 1 f.ItemName, SUM(d.Quantity) as TotalQty
            FROM OrderDetails d
            JOIN Orders o ON d.OrderID = o.OrderID
            JOIN FoodItems f ON d.ItemID = f.ItemID
            WHERE o.OrderTime >= @from AND o.OrderTime <= @to
            AND o.TrangThai = N'Đã thanh toán'
            GROUP BY f.ItemName
            ORDER BY TotalQty DESC";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@from", fromDate.Date);
                    cmd.Parameters.AddWithValue("@to", toDate.Date.AddDays(1).AddSeconds(-1));

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        string foodName = reader["ItemName"].ToString();
                        int qty = Convert.ToInt32(reader["TotalQty"]);

                        // Giả sử bạn có 1 label tên lblBestSeller để hiển thị
                        // lblBestSeller.Text = $"Món bán chạy nhất: {foodName} ({qty} lần)";

                        // Nếu chưa có label thì hiện MessageBox test thử
                        // MessageBox.Show($"Top 1: {foodName}");
                    }
                    else
                    {
                        // lblBestSeller.Text = "Chưa có dữ liệu";
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi Top món: " + ex.Message); }
        }

        private void btnXoaBill_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra xem có dòng nào đang được chọn không
            if (dgvBillList.SelectedRows.Count == 0 || dgvBillList.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn trong danh sách để xóa!", "Thông báo");
                return;
            }

            // 2. Lấy OrderID từ dòng đang chọn
            // (Đảm bảo tên cột "OrderID" khớp với thiết kế cột trong DataGridView của bạn)
            int orderID = Convert.ToInt32(dgvBillList.CurrentRow.Cells["OrderID"].Value);

            // Lấy thêm tổng tiền hoặc ngày để hiển thị thông báo cho chắc chắn
            string orderDate = Convert.ToDateTime(dgvBillList.CurrentRow.Cells["OrderTime"].Value).ToString("dd/MM/yyyy HH:mm");

            // 3. Hỏi xác nhận (Rất quan trọng vì xóa là mất vĩnh viễn)
            DialogResult result = MessageBox.Show(
                $"Bạn có chắc chắn muốn XÓA VĨNH VIỄN hóa đơn số {orderID}?\n(Ngày: {orderDate})\n\nLưu ý: Hành động này không thể hoàn tác và sẽ làm giảm doanh thu tổng.",
                "Xác nhận xóa nguy hiểm",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        // BƯỚC 1: Xóa chi tiết hóa đơn trước (OrderDetails)
                        string sqlDetails = "DELETE FROM OrderDetails WHERE OrderID = @id";
                        SqlCommand cmdDetails = new SqlCommand(sqlDetails, conn);
                        cmdDetails.Parameters.AddWithValue("@id", orderID);
                        cmdDetails.ExecuteNonQuery();

                        // BƯỚC 2: Xóa hóa đơn chính (Orders)
                        string sqlOrder = "DELETE FROM Orders WHERE OrderID = @id";
                        SqlCommand cmdOrder = new SqlCommand(sqlOrder, conn);
                        cmdOrder.Parameters.AddWithValue("@id", orderID);
                        cmdOrder.ExecuteNonQuery();
                    }

                    MessageBox.Show("Đã xóa hóa đơn thành công!");

                    // 4. Load lại dữ liệu để cập nhật danh sách và biểu đồ
                    LoadBillListByDate(dtpFromDate.Value, dtpToDate.Value);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa: " + ex.Message);
                }
            }
        }

        void LoadStaffList()
        {
            try
            {
                string query = "SELECT StaffID, FullName, Username, PasswordHash, ChucVu FROM Staff";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvStaff.DataSource = dt;

                    // Đặt tên hiển thị (nếu cần)
                    if (dgvStaff.Columns["StaffID"] != null) dgvStaff.Columns["StaffID"].HeaderText = "Mã NV";
                    if (dgvStaff.Columns["FullName"] != null) dgvStaff.Columns["FullName"].HeaderText = "Họ Tên";
                    if (dgvStaff.Columns["Username"] != null) dgvStaff.Columns["Username"].HeaderText = "Tài khoản";
                    if (dgvStaff.Columns["ChucVu"] != null) dgvStaff.Columns["ChucVu"].HeaderText = "Chức vụ";

                    // Ẩn cột mật khẩu đi để nhìn cho gọn (nhưng dữ liệu vẫn còn đó để code lấy được)
                    if (dgvStaff.Columns["PasswordHash"] != null) dgvStaff.Columns["PasswordHash"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load nhân viên: " + ex.Message); }
        }

        private void dgvStaff_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvStaff.CurrentRow != null)
            {
                DataGridViewRow row = dgvStaff.CurrentRow;

                btnStaffEdit.Tag = row.Cells["StaffID"].Value.ToString();

                txtStaffName.Text = row.Cells["FullName"].Value.ToString();
                txtStaffUser.Text = row.Cells["Username"].Value.ToString();
                // SỬA: Lấy từ PasswordHash
                txtStaffPass.Text = row.Cells["PasswordHash"].Value.ToString();

                // SỬA: Lấy từ ChucVu
                string role = row.Cells["ChucVu"].Value.ToString();
                cboRole.SelectedItem = role;
            }
        }

        private void btnStaffAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStaffUser.Text) || string.IsNullOrWhiteSpace(txtStaffPass.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên đăng nhập và Mật khẩu!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Kiểm tra trùng Username
                    SqlCommand cmdCheck = new SqlCommand("SELECT COUNT(*) FROM Staff WHERE Username = @user", conn);
                    cmdCheck.Parameters.AddWithValue("@user", txtStaffUser.Text);
                    int count = (int)cmdCheck.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Tên đăng nhập này đã tồn tại!");
                        return;
                    }

                    // SỬA: Insert đúng cột (Bỏ Phone, dùng ChucVu, PasswordHash)
                    string query = "INSERT INTO Staff (FullName, Username, PasswordHash, ChucVu) VALUES (@name, @user, @pass, @role)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@name", txtStaffName.Text);
                    cmd.Parameters.AddWithValue("@user", txtStaffUser.Text);
                    cmd.Parameters.AddWithValue("@pass", txtStaffPass.Text);
                    cmd.Parameters.AddWithValue("@role", cboRole.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Thêm nhân viên thành công!");
                LoadStaffList();
                btnStaffClear_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnStaffEdit_Click(object sender, EventArgs e)
        {
            if (btnStaffEdit.Tag == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // SỬA: Update đúng cột
                    string query = "UPDATE Staff SET FullName=@name, Username=@user, PasswordHash=@pass, ChucVu=@role WHERE StaffID=@id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", btnStaffEdit.Tag);
                    cmd.Parameters.AddWithValue("@name", txtStaffName.Text);
                    cmd.Parameters.AddWithValue("@user", txtStaffUser.Text);
                    cmd.Parameters.AddWithValue("@pass", txtStaffPass.Text);
                    cmd.Parameters.AddWithValue("@role", cboRole.Text);

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Cập nhật thành công!");
                LoadStaffList();
                btnStaffClear_Click(null, null);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnStaffDelete_Click(object sender, EventArgs e)
        {
            if (btnStaffEdit.Tag == null) return;

            int idToDelete = Convert.ToInt32(btnStaffEdit.Tag);

            // 1. CHẶN: Không cho xóa chính mình
            if (idToDelete == CurrentStaffID) // CurrentStaffID là biến toàn cục bạn đã khai báo đầu form
            {
                MessageBox.Show("Bạn không thể tự xóa tài khoản đang đăng nhập!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Staff WHERE StaffID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", idToDelete);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Đã xóa nhân viên!");
                    LoadStaffList();
                    btnStaffClear_Click(null, null);
                }
                catch (Exception ex)
                {
                    // Lỗi này thường do nhân viên này đã từng lập hóa đơn -> Dính khóa ngoại
                    MessageBox.Show("Không thể xóa nhân viên này vì họ đã có lịch sử giao dịch (Lập hóa đơn).\nBạn chỉ nên đổi mật khẩu hoặc đổi Role của họ thôi.", "Lỗi ràng buộc");
                }
            }
        }

        private void btnStaffClear_Click(object sender, EventArgs e)
        {
            txtStaffName.Clear();
            txtStaffUser.Clear();
            txtStaffPass.Clear();
            // Bỏ dòng xóa Phone
            cboRole.SelectedIndex = 0;
            btnStaffEdit.Tag = null;
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            // Kiểm tra quyền: Chỉ Admin mới được vào đây
            if (CurrentStaffRole != "Admin")
            {
                MessageBox.Show("Bạn không có quyền truy cập vào Quản trị hệ thống!", "Từ chối truy cập");
                return;
            }

            pnlSetting.BringToFront(); // Giả sử bạn đặt tên panel là pnlSetting
            LoadStaffList();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            // 1. Hỏi người dùng có chắc chắn muốn thoát không
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?",
                                                  "Xác nhận",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // 2. Ẩn FormMain hiện tại đi (để trông như nó tắt rồi)
                this.Hide();

                // 3. Mở lại FormLogin
                // (Giả sử form đăng nhập của bạn tên là FormLogin)
                Login login = new Login();
                login.ShowDialog(); // Dùng ShowDialog để code dừng tại đây chờ đăng nhập lại

                // 4. Sau khi FormLogin đóng lại (hoặc đăng nhập thành công để mở form mới)
                // Ta đóng hoàn toàn FormMain cũ này để giải phóng bộ nhớ
                this.Close();
            }
        }

        // --- KHU VỰC BACKUP & RESTORE DATABASE ---

        // 1. NÚT SAO LƯU (BACKUP)
        private void btnBackup_Click(object sender, EventArgs e)
        {
            // 1. Cấu hình đường dẫn mong muốn
            string backupFolder = @"C:\BackupData";

            try
            {
                // 2. Kiểm tra nếu thư mục chưa tồn tại thì tạo mới
                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                // 3. Cấu hình hộp thoại SaveFile
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "SQL Server Database Backup files|*.bak";
                sfd.Title = "Sao lưu cơ sở dữ liệu";

                // --- Đặt thư mục mặc định là C:\BackupData ---
                sfd.InitialDirectory = backupFolder;
                sfd.RestoreDirectory = true; // Giữ lại đường dẫn này cho lần sau
                                             // ---------------------------------------------------------

                // Đặt tên file mặc định kèm ngày giờ
                sfd.FileName = "Nhom13_QLNhaHang_" + DateTime.Now.ToString("ddMMyyyy_HHmm") + ".bak";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    // Lấy đường dẫn file người dùng đã chọn (hoặc giữ nguyên mặc định)
                    string backupPath = sfd.FileName;

                    // Câu lệnh SQL Backup
                    // Lưu ý: Đảm bảo tên Database [Nhom13_QLNhaHang] đúng với trong SQL của bạn
                    string query = $"BACKUP DATABASE [Nhom13_QLNhaHang] TO DISK = '{backupPath}'";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Sao lưu dữ liệu thành công vào:\n" + backupPath, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sao lưu: " + ex.Message, "Lỗi");
            }
        }

        // 2. NÚT PHỤC HỒI (RESTORE)
        private void btnRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "SQL Server Database Backup files|*.bak";
            ofd.Title = "Chọn file phục hồi dữ liệu";

            // --- THÊM DÒNG NÀY ĐỂ MỞ LUÔN THƯ MỤC BACKUP ---
            ofd.InitialDirectory = @"C:\BackupData";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string restorePath = ofd.FileName;

                // Cảnh báo người dùng
                if (MessageBox.Show("CẢNH BÁO: Việc phục hồi sẽ ghi đè toàn bộ dữ liệu hiện tại bằng dữ liệu trong file backup.\nBạn có chắc chắn muốn tiếp tục?",
                    "Xác nhận phục hồi nguy hiểm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        // QUAN TRỌNG: Kết nối tới MASTER (Database hệ thống) để Restore
                        // Vì không thể Restore chính Database đang kết nối
                        string masterConnection = @"Data Source=.;Initial Catalog=master;Integrated Security=True";

                        string query = $@"
                    -- 1. Ngắt kết nối tất cả người dùng khác để chiếm quyền (Single User)
                    ALTER DATABASE [Nhom13_QLNhaHang] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    
                    -- 2. Thực hiện Restore (Ghi đè - REPLACE)
                    RESTORE DATABASE [Nhom13_QLNhaHang] FROM DISK = '{restorePath}' WITH REPLACE;
                    
                    -- 3. Mở lại kết nối bình thường (Multi User)
                    ALTER DATABASE [Nhom13_QLNhaHang] SET MULTI_USER;";

                        using (SqlConnection conn = new SqlConnection(masterConnection))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(query, conn);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Phục hồi dữ liệu thành công!\nChương trình sẽ khởi động lại để áp dụng dữ liệu mới.", "Thành công");

                        // Khởi động lại ứng dụng để làm mới dữ liệu
                        Application.Restart();
                        Environment.Exit(0);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi phục hồi: " + ex.Message, "Lỗi");

                        // Phòng trường hợp bị kẹt ở chế độ Single User nếu lỗi
                        try
                        {
                            using (SqlConnection conn = new SqlConnection(@"Data Source=.;Initial Catalog=master;Integrated Security=True"))
                            {
                                conn.Open();
                                new SqlCommand("ALTER DATABASE [Nhom13_QLNhaHang] SET MULTI_USER", conn).ExecuteNonQuery();
                            }
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
