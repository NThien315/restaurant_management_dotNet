using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nhom13_QLNhaHang
{
    public partial class FoodItemCard : UserControl
    {
        public FoodItemCard()
        {
            InitializeComponent();
            guna2ShadowPanel1.MouseEnter += HoverEffect_Enter;
            guna2ShadowPanel1.MouseLeave += HoverEffect_Leave;

            picFoodImage.MouseEnter += HoverEffect_Enter;
            lblFoodName.MouseEnter += HoverEffect_Enter;
            lblFoodPrice.MouseEnter += HoverEffect_Enter;

            if (this.panelBackground != null) this.panelBackground.Click += Child_Click;
            if (this.picFoodImage != null) this.picFoodImage.Click += Child_Click;
            if (this.lblFoodName != null) this.lblFoodName.Click += Child_Click;
            if (this.lblFoodPrice != null) this.lblFoodPrice.Click += Child_Click;
        }

        private void HoverEffect_Enter(object sender, EventArgs e)
        {
            // Đổi màu nền của PANEL
            guna2ShadowPanel1.FillColor = Color.FromArgb(230, 240, 255);

            this.Cursor = Cursors.Hand;
        }

        private void HoverEffect_Leave(object sender, EventArgs e)
        {
            guna2ShadowPanel1.FillColor = Color.White;

            this.Cursor = Cursors.Default;
        }

        private void Child_Click(object sender, EventArgs e)
        {
            this.InvokeOnClick(this, EventArgs.Empty);
        }

        // --- Thuộc tính (Properties) ---

        private string _itemName;
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                if (lblFoodName != null) lblFoodName.Text = value;
            }
        }

        private double _itemPrice;
        public double ItemPrice
        {
            get { return _itemPrice; }
            set
            {
                _itemPrice = value;
                // Hiển thị tiền Việt: 150.000 đ
                if (lblFoodPrice != null) lblFoodPrice.Text = value.ToString("N0") + " đ";
            }
        }

        private Image _itemImage;
        public Image ItemImage
        {
            get { return _itemImage; }
            set
            {
                _itemImage = value;
                if (picFoodImage != null) picFoodImage.Image = value;
            }
        }

        public void SetTooltipInfo(ToolTip toolTip, string text)
        {
            // Gắn cho bản thân cái thẻ
            toolTip.SetToolTip(this, text);

            // Gắn cho các thành phần con (để di chuột vào ảnh cũng hiện)
            if (this.picFoodImage != null) toolTip.SetToolTip(this.picFoodImage, text);
            if (this.lblFoodName != null) toolTip.SetToolTip(this.lblFoodName, text);
            if (this.lblFoodPrice != null) toolTip.SetToolTip(this.lblFoodPrice, text);
            if (this.panelBackground != null) toolTip.SetToolTip(this.panelBackground, text);
        }
    }
}