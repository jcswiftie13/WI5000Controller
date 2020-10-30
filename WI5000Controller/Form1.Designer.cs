namespace WI5000Controller
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axWI1 = new AxWILib.AxWI();
            ((System.ComponentModel.ISupportInitialize)(this.axWI1)).BeginInit();
            this.SuspendLayout();
            // 
            // axWI1
            // 
            this.axWI1.Enabled = true;
            this.axWI1.Location = new System.Drawing.Point(776, 12);
            this.axWI1.Name = "axWI1";
            this.axWI1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWI1.OcxState")));
            this.axWI1.Size = new System.Drawing.Size(12, 9);
            this.axWI1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.axWI1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axWI1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxWILib.AxWI axWI1;
    }
}

