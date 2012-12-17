using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace AaltoWindraw
{
    public class ColorButton : System.Windows.Forms.Button
    {
        #region properties
        private Color buttonColor;

        public Color ButtonColor
        {
            get
            {
                return this.buttonColor;
            }
            set
            {
                this.buttonColor = value;
            }
        }
        #endregion

        public ColorButton()
            : base()
        {
            this.buttonColor = System.Windows.Media.Colors.AliceBlue; 
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
        }

        protected override void OnMouseEnter(System.EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        public void SetStateActive()
        {

        }

        public void SetStateNormal()
        {

        }

	}
}