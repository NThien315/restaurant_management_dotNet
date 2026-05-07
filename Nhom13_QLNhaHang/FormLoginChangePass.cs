using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Nhom13_QLNhaHang
{
    public partial class FormLoginChangePass : Form
    {
        private readonly string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";

        public FormLoginChangePass()
        {
            InitializeComponent();
            this.Text = "Đổi mật khẩu hệ thống";
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void btnDongY_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(txtUser.Text) ||
                string.IsNullOrWhiteSpace(txtOldPass.Text) ||
                string.IsNullOrWhiteSpace(txtNewPass.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin!", "Cảnh báo");
                return;
            }

            // 2. Kiểm tra khớp mật khẩu mới
            if (txtNewPass.Text != txtConfirm.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 3. Kiểm tra xem Username và Password cũ có đúng không
                    string checkSql = "SELECT COUNT(*) FROM Staff WHERE Username = @u AND PasswordHash = @p";
                    SqlCommand cmdCheck = new SqlCommand(checkSql, conn);
                    cmdCheck.Parameters.AddWithValue("@u", txtUser.Text);
                    cmdCheck.Parameters.AddWithValue("@p", txtOldPass.Text);

                    int count = (int)cmdCheck.ExecuteScalar();

                    if (count == 0)
                    {
                        MessageBox.Show("Tên đăng nhập hoặc Mật khẩu cũ không đúng!", "Thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // 4. Nếu đúng thì cập nhật mật khẩu mới
                    string updateSql = "UPDATE Staff SET PasswordHash = @newP WHERE Username = @u";
                    SqlCommand cmdUpdate = new SqlCommand(updateSql, conn);
                    cmdUpdate.Parameters.AddWithValue("@newP", txtNewPass.Text);
                    cmdUpdate.Parameters.AddWithValue("@u", txtUser.Text);

                    cmdUpdate.ExecuteNonQuery();

                    MessageBox.Show("Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "Thông báo");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}