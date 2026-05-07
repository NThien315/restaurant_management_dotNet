namespace Nhom13_QLNhaHang
{
    partial class OrderItemCard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderItemCard));
            this.picItemImage = new Guna.UI2.WinForms.Guna2PictureBox();
            this.lblItemName = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblItemPrice = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblQuantity = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.lblTotalPrice = new Guna.UI2.WinForms.Guna2HtmlLabel();
            this.btnXoaMon = new Guna.UI2.WinForms.Guna2ImageButton();
            this.guna2ShadowPanel1 = new Guna.UI2.WinForms.Guna2ShadowPanel();
            this.guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            ((System.ComponentModel.ISupportInitialize)(this.picItemImage)).BeginInit();
            this.guna2ShadowPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // picItemImage
            // 
            this.picItemImage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(245)))), ((int)(((byte)(230)))));
            this.picItemImage.BorderRadius = 10;
            this.picItemImage.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(245)))), ((int)(((byte)(230)))));
            this.picItemImage.ImageRotate = 0F;
            this.picItemImage.Location = new System.Drawing.Point(16, 15);
            this.picItemImage.Name = "picItemImage";
            this.picItemImage.Size = new System.Drawing.Size(45, 45);
            this.picItemImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picItemImage.TabIndex = 0;
            this.picItemImage.TabStop = false;
            // 
            // lblItemName
            // 
            this.lblItemName.BackColor = System.Drawing.Color.Transparent;
            this.lblItemName.Font = new System.Drawing.Font("#9Slide03 Quicksand", 9.749999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.lblItemName.Location = new System.Drawing.Point(70, 10);
            this.lblItemName.Name = "lblItemName";
            this.lblItemName.Size = new System.Drawing.Size(42, 22);
            this.lblItemName.TabIndex = 1;
            this.lblItemName.Text = "Món a";
            // 
            // lblItemPrice
            // 
            this.lblItemPrice.BackColor = System.Drawing.Color.Transparent;
            this.lblItemPrice.Font = new System.Drawing.Font("#9Slide03 Quicksand", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblItemPrice.ForeColor = System.Drawing.Color.Gray;
            this.lblItemPrice.Location = new System.Drawing.Point(70, 41);
            this.lblItemPrice.Name = "lblItemPrice";
            this.lblItemPrice.Size = new System.Drawing.Size(35, 21);
            this.lblItemPrice.TabIndex = 2;
            this.lblItemPrice.Text = "15000";
            // 
            // lblQuantity
            // 
            this.lblQuantity.BackColor = System.Drawing.Color.Transparent;
            this.lblQuantity.Font = new System.Drawing.Font("#9Slide03 Quicksand", 8.999999F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblQuantity.ForeColor = System.Drawing.Color.Black;
            this.lblQuantity.Location = new System.Drawing.Point(143, 41);
            this.lblQuantity.Name = "lblQuantity";
            this.lblQuantity.Size = new System.Drawing.Size(16, 21);
            this.lblQuantity.TabIndex = 3;
            this.lblQuantity.Text = "x2";
            // 
            // lblTotalPrice
            // 
            this.lblTotalPrice.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalPrice.BackColor = System.Drawing.Color.Transparent;
            this.lblTotalPrice.Font = new System.Drawing.Font("#9Slide03 Quicksand Bold", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalPrice.Location = new System.Drawing.Point(170, 39);
            this.lblTotalPrice.Name = "lblTotalPrice";
            this.lblTotalPrice.Size = new System.Drawing.Size(47, 25);
            this.lblTotalPrice.TabIndex = 4;
            this.lblTotalPrice.Text = "30000";
            this.lblTotalPrice.TextAlignment = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnXoaMon
            // 
            this.btnXoaMon.CheckedState.ImageSize = new System.Drawing.Size(30, 30);
            this.btnXoaMon.HoverState.ImageSize = new System.Drawing.Size(30, 30);
            this.btnXoaMon.Image = ((System.Drawing.Image)(resources.GetObject("btnXoaMon.Image")));
            this.btnXoaMon.ImageOffset = new System.Drawing.Point(0, 0);
            this.btnXoaMon.ImageRotate = 0F;
            this.btnXoaMon.ImageSize = new System.Drawing.Size(20, 20);
            this.btnXoaMon.Location = new System.Drawing.Point(211, 10);
            this.btnXoaMon.Name = "btnXoaMon";
            this.btnXoaMon.PressedState.ImageSize = new System.Drawing.Size(30, 30);
            this.btnXoaMon.Size = new System.Drawing.Size(28, 29);
            this.btnXoaMon.TabIndex = 8;
            this.btnXoaMon.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // guna2ShadowPanel1
            // 
            this.guna2ShadowPanel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2ShadowPanel1.Controls.Add(this.btnXoaMon);
            this.guna2ShadowPanel1.Controls.Add(this.picItemImage);
            this.guna2ShadowPanel1.Controls.Add(this.guna2Panel1);
            this.guna2ShadowPanel1.Controls.Add(this.lblTotalPrice);
            this.guna2ShadowPanel1.Controls.Add(this.lblItemName);
            this.guna2ShadowPanel1.Controls.Add(this.lblQuantity);
            this.guna2ShadowPanel1.Controls.Add(this.lblItemPrice);
            this.guna2ShadowPanel1.FillColor = System.Drawing.Color.White;
            this.guna2ShadowPanel1.Location = new System.Drawing.Point(0, 6);
            this.guna2ShadowPanel1.Name = "guna2ShadowPanel1";
            this.guna2ShadowPanel1.Radius = 10;
            this.guna2ShadowPanel1.ShadowColor = System.Drawing.Color.Black;
            this.guna2ShadowPanel1.ShadowDepth = 30;
            this.guna2ShadowPanel1.ShadowShift = 4;
            this.guna2ShadowPanel1.Size = new System.Drawing.Size(254, 75);
            this.guna2ShadowPanel1.TabIndex = 9;
            // 
            // guna2Panel1
            // 
            this.guna2Panel1.BackColor = System.Drawing.Color.Transparent;
            this.guna2Panel1.BorderRadius = 10;
            this.guna2Panel1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(245)))), ((int)(((byte)(230)))));
            this.guna2Panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.guna2Panel1.Location = new System.Drawing.Point(11, 10);
            this.guna2Panel1.Name = "guna2Panel1";
            this.guna2Panel1.Size = new System.Drawing.Size(55, 55);
            this.guna2Panel1.TabIndex = 9;
            // 
            // OrderItemCard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.guna2ShadowPanel1);
            this.Name = "OrderItemCard";
            this.Size = new System.Drawing.Size(254, 87);
            ((System.ComponentModel.ISupportInitialize)(this.picItemImage)).EndInit();
            this.guna2ShadowPanel1.ResumeLayout(false);
            this.guna2ShadowPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox picItemImage;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblItemName;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblItemPrice;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblQuantity;
        private Guna.UI2.WinForms.Guna2HtmlLabel lblTotalPrice;
        private Guna.UI2.WinForms.Guna2ImageButton btnXoaMon;
        private Guna.UI2.WinForms.Guna2ShadowPanel guna2ShadowPanel1;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
    }
}
