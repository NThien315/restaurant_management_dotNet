using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nhom13_QLNhaHang
{
    public partial class Login : Form
    {
        private readonly string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";

        public Login()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text;
            string pass = txtPassword.Text;

            // 1. SỬA CÂU TRUY VẤN CHO KHỚP SQL: ChucVu, PasswordHash
            string query = "SELECT StaffID, FullName, ChucVu FROM Staff WHERE Username = @user AND PasswordHash = @pass";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@user", user);
                    cmd.Parameters.AddWithValue("@pass", pass);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // --- ĐĂNG NHẬP THÀNH CÔNG ---

                        FormMain fMain = new FormMain();

                        // 2. TRUYỀN DỮ LIỆU SANG FORM MAIN

                        // Lấy StaffID
                        object idObj = reader["StaffID"];
                        if (idObj != DBNull.Value)
                        {
                            fMain.CurrentStaffID = Convert.ToInt32(idObj);
                        }

                        // Lấy Tên hiển thị
                        fMain.CurrentStaffName = reader["FullName"].ToString();

                        // --- QUAN TRỌNG: Lấy từ cột 'ChucVu' ---
                        fMain.CurrentStaffRole = reader["ChucVu"].ToString();
                        // ---------------------------------------

                        // 3. Chuyển Form
                        this.Hide();
                        fMain.ShowDialog();
                        this.Show();

                        txtPassword.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }
        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            // Mở form đổi mật khẩu dành riêng cho Login
            FormLoginChangePass frm = new FormLoginChangePass();
            frm.ShowDialog();
        }
    }
}