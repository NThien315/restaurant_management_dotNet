using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Nhom13_QLNhaHang
{
    public partial class FormCategoryManager : Form
    {
        string connectionString = @"Data Source=.;Initial Catalog=Nhom13_QLNhaHang;Integrated Security=True";

        public FormCategoryManager()
        {
            InitializeComponent();
            dgvCategory.AutoGenerateColumns = false;
        }

        private void FormCategoryManager_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Lấy tất cả danh mục
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Categories", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvCategory.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void dgvCategory_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dgvCategory.Rows[e.RowIndex];
                txtID.Text = row.Cells["CategoryID"].Value.ToString();
                txtName.Text = row.Cells["CategoryName"].Value.ToString();

                // Xử lý ảnh
                if (row.Cells["ImagePath"].Value != DBNull.Value)
                {
                    string fileName = row.Cells["ImagePath"].Value.ToString();

                    // 1. Load ảnh lên PictureBox
                    picCategory.Image = LoadImageSafe(fileName);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", fileName);
                        picCategory.Tag = fullPath;
                    }
                }
                else
                {
                    picCategory.Image = null;
                    picCategory.Tag = "";
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) return;

            try
            {
                string savedPath = SaveImageToFolder((picCategory.Tag ?? "").ToString());

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Categories (CategoryName, ImagePath) VALUES (@name, @img)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@img", savedPath);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Đã thêm danh mục!");
                LoadData();
                ResetInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;

            try
            {
                string savedPath = SaveImageToFolder((picCategory.Tag ?? "").ToString());

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Categories SET CategoryName = @name, ImagePath = @img WHERE CategoryID = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@img", savedPath);
                    cmd.Parameters.AddWithValue("@id", txtID.Text);
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Đã sửa danh mục!");
                LoadData();
                ResetInput();
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtID.Text)) return;

            if (MessageBox.Show("Bạn có chắc xóa danh mục này?\n(Lưu ý: Nếu danh mục đang có món ăn thì sẽ không xóa được)", "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        // Dùng try-catch để bắt lỗi Khóa Ngoại
                        SqlCommand cmd = new SqlCommand("DELETE FROM Categories WHERE CategoryID = @id", conn);
                        cmd.Parameters.AddWithValue("@id", txtID.Text);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("Đã xóa!");
                    LoadData();
                    ResetInput();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa danh mục này vì đang có món ăn thuộc về nó.\nHãy xóa hoặc chuyển các món ăn sang danh mục khác trước!");
                }
            }
        }

        private void btnChonAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (open.ShowDialog() == DialogResult.OK)
            {
                picCategory.Image = Image.FromFile(open.FileName);
                // Lưu đường dẫn gốc (ví dụ: C:\Users\Desktop\anh.jpg) vào Tag
                picCategory.Tag = open.FileName;
            }
        }

        // --- CÁC HÀM HỖ TRỢ (GIỐNG FORM MAIN) ---
        private string SaveImageToFolder(string sourcePath)
        {
            try
            {
                if (string.IsNullOrEmpty(sourcePath)) return "";

                // 1. Xác định đường dẫn thư mục bin\Debug\Images 
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

        private Image LoadImageSafe(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName)) return null;

                // Luôn load từ bin\Debug\Images
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                string absolutePath = Path.Combine(folderPath, fileName);

                if (File.Exists(absolutePath))
                {
                    byte[] buffer = File.ReadAllBytes(absolutePath);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        return Image.FromStream(ms);
                    }
                }
                return null;
            }
            catch { return null; }
        }

        void ResetInput()
        {
            txtID.Clear(); txtName.Clear(); picCategory.Image = null; picCategory.Tag = null;
        }
    }
}