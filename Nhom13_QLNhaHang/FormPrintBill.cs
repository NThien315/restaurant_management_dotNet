using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Nhom13_QLNhaHang
{
    public partial class FormPrintBill : Form
    {
        int _orderID;
        string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";

        public FormPrintBill(int orderID)
        {
            InitializeComponent();
            _orderID = orderID;
        }

        private void FormPrintBill_Load(object sender, EventArgs e)
        {
            // Khi form hiện lên thì load luôn
            LoadReport();
            btnRefresh_Click(sender, e);
        }

        // --- NÚT REFRESH CỦA BẠN ---
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadReport(); // Gọi lại hàm load để lấy dữ liệu mới nhất
        }

        // --- HÀM LOAD DỮ LIỆU RIÊNG ---
        private void LoadReport()
        {
            try
            {
                // Xóa nguồn cũ trước khi nạp mới (Mẹo tránh lỗi lưu cache)
                crystalReportViewer1.ReportSource = null;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Lấy dữ liệu mới nhất từ SQL
                    string query = "SELECT * FROM View_InHoaDon WHERE OrderID = @id";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@id", _orderID);

                    dsBill ds = new dsBill();

                    // QUAN TRỌNG: Tên bảng phải chuẩn
                    da.Fill(ds, "dtHoaDon");

                    // Debug: Kiểm tra xem có dòng nào không?
                    if (ds.Tables["dtHoaDon"].Rows.Count == 0)
                    {
                        MessageBox.Show($"Không tìm thấy dữ liệu cho OrderID: {_orderID}");
                        return;
                    }

                    // 2. Gán vào Report
                    rptBill report = new rptBill();
                    report.SetDataSource(ds);

                    // 3. Đẩy lên Viewer
                    crystalReportViewer1.ReportSource = report;
                    crystalReportViewer1.Refresh(); // Lệnh refresh của control
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải báo cáo: " + ex.Message);
            }
        }
    }
}