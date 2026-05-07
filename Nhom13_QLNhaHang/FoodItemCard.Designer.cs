namespace Nhom13_QLNhaHang
{
    partial class FoodItemCard
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelBackground = new Guna.UI2.WinForms.Guna2Panel();
            this.lblFoodPrice = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblFoodName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.picFoodImage = new Guna.UI2.WinForms.Guna2PictureBox();
            this.guna2ShadowPanel1 = new Guna.UI2.WinForms.Guna2ShadowPanel();
            this.panelBackground.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picFoodImage)).BeginInit();
            this.guna2ShadowPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelBackground
            // 
            this.panelBackground.BorderRadius = 15;
            this.panelBackground.Controls.Add(this.guna2ShadowPanel1);
            this.panelBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBackground.FillColor = System.Drawing.Color.White;
            this.panelBackground.Location = new System.Drawing.Point(0, 0);
            this.panelBackground.Name = "panelBackground";
            this.panelBackground.Size = new System.Drawing.Size(120, 180);
            this.panelBackground.TabIndex = 0;
            // 
            // lblFoodPrice
            // 
            this.lblFoodPrice.AutoSize = false;
            this.lblFoodPrice.BackColor = System.Drawing.Color.Transparent;
            this.lblFoodPrice.Font = new System.Drawing.Font("#9Slide03 Quicksand", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFoodPrice.Location = new System.Drawing.Point(8, 150);
            this.lblFoodPrice.Name = "lblFoodPrice";
            this.lblFoodPrice.Size = new System.Drawing.Size(105, 15);
            this.lblFoodPrice.TabIndex = 2;
            this.lblFoodPrice.Text = "15000";
            this.lblFoodPrice.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFoodName
            // 
            this.lblFoodName.AutoSize = false;
            this.lblFoodName.BackColor = System.Drawing.Color.Transparent;
            this.lblFoodName.Font = new System.Drawing.Font("#9Slide03 Quicksand Bold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFoodName.Location = new System.Drawing.Point(8, 105);
            this.lblFoodName.Name = "lblFoodName";
            this.lblFoodName.Size = new System.Drawing.Size(105, 35);
            this.lblFoodName.TabIndex = 1;
            this.lblFoodName.Text = "Món A";
            this.lblFoodName.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // picFoodImage
            // 
            this.picFoodImage.BackColor = System.Drawing.Color.Transparent;
            this.picFoodImage.FillColor = System.Drawing.Color.Transparent;
            this.picFoodImage.ImageRotate = 0F;
            this.picFoodImage.Location = new System.Drawing.Point(8, 12);
            this.picFoodImage.Name = "picFoodImage";
            this.picFoodImage.Size = new System.Drawing.Size(105, 87);
            this.picFoodImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picFoodImage.TabIndex = 0;
            this.picFoodImage.TabStop = false;
            // 
            // guna2ShadowPanel1
            // 
            this.guna2ShadowPanel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2ShadowPanel1.Controls.Add(this.lblFoodPrice);
            this.guna2ShadowPanel1.Controls.Add(this.lblFoodName);
            this.guna2ShadowPanel1.Controls.Add(this.picFoodImage);
            this.guna2ShadowPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.guna2ShadowPanel1.FillColor = System.Drawing.Color.White;
            this.guna2ShadowPanel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.guna2ShadowPanel1.Location = new System.Drawing.Point(0, 0);
            this.guna2ShadowPanel1.Name = "guna2ShadowPanel1";
            this.guna2ShadowPanel1.Radius = 5;
            this.guna2ShadowPanel1.ShadowColor = System.Drawing.Color.Black;
            this.guna2ShadowPanel1.ShadowDepth = 30;
            this.guna2ShadowPanel1.ShadowShift = 1;
            this.guna2ShadowPanel1.Size = new System.Drawing.Size(120, 180);
            this.guna2ShadowPanel1.TabIndex = 3;
            // 
            // FoodItemCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panelBackground);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "FoodItemCard";
            this.Size = new System.Drawing.Size(120, 180);
            this.panelBackground.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picFoodImage)).EndInit();
            this.guna2ShadowPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2Panel panelBackground;
        private Guna.UI2.WinForms.Guna2PictureBox picFoodImage;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblFoodPrice;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblFoodName;
        private Guna.UI2.WinForms.Guna2ShadowPanel guna2ShadowPanel1;
    }
}
