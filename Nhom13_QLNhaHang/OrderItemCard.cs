using System;
using System.Windows.Forms;
using System.Drawing;

namespace Nhom13_QLNhaHang
{
    public partial class OrderItemCard : UserControl
    {
        // Sự kiện xóa để FormMain bắt được
        public event EventHandler OnDelete;

        public OrderItemCard()
        {
            InitializeComponent();
        }

        // --- 1. THUỘC TÍNH ĐƠN GIÁ (ItemPrice) ---
        private double _itemPrice;
        public double ItemPrice
        {
            get { return _itemPrice; }
            set
            {
                _itemPrice = value;

                // Cập nhật Label ĐƠN GIÁ (lblItemPrice)
                // Label này chỉ hiện giá gốc (VD: 50.000 đ)
                if (lblItemPrice != null)
                {
                    lblItemPrice.Text = _itemPrice.ToString("N0") + " đ";
                }

                // Khi giá thay đổi, cũng cần tính lại Thành Tiền luôn cho chắc
                UpdateTotalPriceDisplay();
            }
        }

        // --- 2. THUỘC TÍNH SỐ LƯỢNG (ItemQuantity) ---
        private int _quantity = 1;
        public int ItemQuantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;

                // Cập nhật Label SỐ LƯỢNG (lblQuantity)
                if (lblQuantity != null)
                {
                    lblQuantity.Text = _quantity.ToString();
                }

                // Cập nhật Label THÀNH TIỀN (lblTotalPrice)
                // Khi số lượng đổi -> Thành tiền đổi
                UpdateTotalPriceDisplay();
            }
        }

        // --- 3. THUỘC TÍNH TÍNH TOÁN TỔNG TIỀN ---
        public double TotalPrice
        {
            get { return _itemPrice * _quantity; }
        }

        // Hàm phụ trợ để cập nhật Label Thành Tiền (tránh viết lặp code)
        private void UpdateTotalPriceDisplay()
        {
            if (lblTotalPrice != null)
            {
                // Gọi thuộc tính TotalPrice ở trên để lấy kết quả nhân
                lblTotalPrice.Text = TotalPrice.ToString("N0") + " đ";
            }
        }

        // --- CÁC THUỘC TÍNH KHÁC (Giữ nguyên) ---
        public string ItemName
        {
            get { return lblItemName.Text; } // Giả sử label tên là lblName
            set { if (lblItemName != null) lblItemName.Text = value; }
        }

        public Image ItemImage
        {
            get { return picItemImage.Image; } // Giả sử PictureBox tên là picFood
            set { if (picItemImage != null) picItemImage.Image = value; }
        }

        // Sự kiện nút xóa
        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnDelete?.Invoke(this, EventArgs.Empty);
        }
    }
}