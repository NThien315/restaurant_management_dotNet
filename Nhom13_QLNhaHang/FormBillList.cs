using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace Nhom13_QLNhaHang
{
    public partial class FormBillList : Form
    {
        public int SelectedOrderID { get; private set; } = 0;
        public int SelectedTableID { get; private set; } = 0;

        string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";

        private Guna2Button currentBtn = null;

        public FormBillList()
        {
            InitializeComponent();

            // 1. Cấu hình bảng
            dgvChiTiet.AutoGenerateColumns = true;
            ConfigureGrid();
            StyleGrid(); 

            // 2. Làm đẹp Form 
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Danh sách đơn đang phục vụ";
            this.BackColor = Color.WhiteSmoke; 

            // 3. Load dữ liệu
            LoadActiveTables();
        }

        // --- 1. LOAD DANH SÁCH BÀN (BÊN TRÁI) ---
        void LoadActiveTables()
        {
            flpBanCoKhach.Controls.Clear();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT o.OrderID, b.TableName, o.OrderTime, o.TableID
                                   FROM Orders o
                                   LEFT JOIN BanAn b ON o.TableID = b.TableID
                                   WHERE o.TrangThai = N'Chờ thanh toán'
                                   ORDER BY o.OrderTime DESC"; 

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Guna2Button btn = new Guna2Button();

                        // Hiển thị Tên Bàn + Giờ đặt
                        string tableName = reader["TableName"] != DBNull.Value
                                           ? reader["TableName"].ToString()
                                           : "Mang Về";

                        DateTime time = Convert.ToDateTime(reader["OrderTime"]);
                        // Định dạng: 10:30 (dd/MM)
                        string timeStr = time.ToString("HH:mm") + " (" + time.ToString("dd/MM") + ")";

                        btn.Text = $"{tableName}\n{timeStr}";

                        // --- STYLE NÚT BÀN ---
                        btn.Width = 140;
                        btn.Height = 80;
                        btn.BorderRadius = 12;
                        btn.FillColor = Color.White; 
                        btn.ForeColor = Color.Black;
                        btn.Font = new Font("Segoe UI", 9, FontStyle.Regular);

                        btn.BorderColor = Color.Silver;
                        btn.BorderThickness = 1;

                        // Hiệu ứng khi di chuột
                        btn.HoverState.FillColor = Color.FromArgb(224, 224, 224);

                        // Lưu thông tin vào Tag
                        btn.Tag = new OrderInfo
                        {
                            OrderID = Convert.ToInt32(reader["OrderID"]),
                            TableID = reader["TableID"] != DBNull.Value ? Convert.ToInt32(reader["TableID"]) : 0
                        };

                        btn.Click += BtnTable_Click;
                        flpBanCoKhach.Controls.Add(btn);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        private void BtnTable_Click(object sender, EventArgs e)
        {
            Guna2Button clickedBtn = (Guna2Button)sender;

            // Đổi màu hiệu ứng chọn
            if (currentBtn != null) currentBtn.FillColor = Color.Teal;
            clickedBtn.FillColor = Color.OrangeRed;
            currentBtn = clickedBtn;

            // Lấy ID để load chi tiết
            OrderInfo info = (OrderInfo)clickedBtn.Tag;
            SelectedOrderID = info.OrderID;
            SelectedTableID = info.TableID;

            LoadOrderDetails(SelectedOrderID);
        }

        // --- 2. LOAD CHI TIẾT (BÊN PHẢI) ---
        void LoadOrderDetails(int orderID)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = @"SELECT 
                            d.ItemID AS [ID], 
                            f.ItemName AS [Tên Món], 
                            d.Quantity AS [SL], 
                            (d.Quantity * d.PriceAtTime) AS [Thành Tiền]
                           FROM OrderDetails d 
                           JOIN FoodItems f ON d.ItemID = f.ItemID 
                           WHERE d.OrderID = @id";

                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", orderID);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Gán dữ liệu
                    dgvChiTiet.DataSource = dt;

                    // --- CẤU HÌNH CỘT (DATA SPECIFIC) ---

                    dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    if (dgvChiTiet.Columns.Count > 0)
                    {
                        // Ẩn cột ID
                        if (dgvChiTiet.Columns.Contains("ID")) dgvChiTiet.Columns["ID"].Visible = false;

                        // Chỉnh độ rộng (Tỉ lệ)
                        if (dgvChiTiet.Columns.Contains("Tên Món")) dgvChiTiet.Columns["Tên Món"].FillWeight = 50;

                        if (dgvChiTiet.Columns.Contains("SL"))
                        {
                            dgvChiTiet.Columns["SL"].FillWeight = 15;
                            dgvChiTiet.Columns["SL"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                        }

                        if (dgvChiTiet.Columns.Contains("Thành Tiền"))
                        {
                            dgvChiTiet.Columns["Thành Tiền"].FillWeight = 35;
                            dgvChiTiet.Columns["Thành Tiền"].DefaultCellStyle.Format = "N0";
                            dgvChiTiet.Columns["Thành Tiền"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        void ConfigureGrid()
        {
            // 1. CHẶN CHỈNH SỬA 
            dgvChiTiet.ReadOnly = true;              // Chỉ cho xem, không cho sửa
            dgvChiTiet.AllowUserToAddRows = false;   // Bỏ dòng trống cuối cùng
            dgvChiTiet.AllowUserToDeleteRows = false;// Không cho bấm Delete để xóa dòng
            dgvChiTiet.AllowUserToResizeRows = false;// Không cho kéo giãn dòng

            // 2. CHỌN CẢ DÒNG
            dgvChiTiet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvChiTiet.MultiSelect = false;          // Chỉ được chọn 1 dòng

            // 3. ẨN TIÊU ĐỀ DÒNG
            dgvChiTiet.RowHeadersVisible = false;
        }

        void StyleGrid()
        {
            // --- 1. MÀU SẮC & VIỀN (Visual) ---
            dgvChiTiet.BorderStyle = BorderStyle.None;
            dgvChiTiet.BackgroundColor = Color.WhiteSmoke;
            dgvChiTiet.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvChiTiet.GridColor = Color.FromArgb(224, 224, 224);

            // --- 2. HEADER (TIÊU ĐỀ) ---
            dgvChiTiet.EnableHeadersVisualStyles = false;

            dgvChiTiet.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 150, 136); 
            dgvChiTiet.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvChiTiet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Chỉnh chiều cao Header
            dgvChiTiet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvChiTiet.ColumnHeadersHeight = 45;

            // --- 3. DÒNG DỮ LIỆU (ROWS) ---
            dgvChiTiet.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvChiTiet.DefaultCellStyle.ForeColor = Color.Black;
            dgvChiTiet.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 224, 192);
            dgvChiTiet.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Chiều cao dòng (Chuyển từ code cũ sang đây)
            dgvChiTiet.RowTemplate.Height = 40;
        }

        // --- 3. CÁC NÚT CHỨC NĂNG ---

        // Nút "Chọn đơn này" (OK)
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (SelectedOrderID == 0)
            {
                MessageBox.Show("Vui lòng chọn một bàn bên trái!");
                return;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Nút "Hủy" (Đóng form)
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public class OrderInfo
        {
            public int OrderID { get; set; }
            public int TableID { get; set; }
        }
    }
}