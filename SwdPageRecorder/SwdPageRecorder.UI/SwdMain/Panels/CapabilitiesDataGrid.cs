using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SwdPageRecorder.UI
{
    public partial class CapabilitiesDataGridView : UserControl, IView
    {
        public CapabilitiesDataGridPresenter Presenter { get; private set; }
        public CapabilitiesDataGridView()
        {
            InitializeComponent();
            Presenter = Presenters.CapabilitiesDataGridPresenter;
            Presenter.InitWithView(this);
        }

    }
}
