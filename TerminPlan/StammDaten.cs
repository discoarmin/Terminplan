using System.Windows.Forms;

namespace Terminplan
{
    using Infragistics.Win.UltraWinToolbars;
    using System.Globalization;

    public partial class StammDaten : Form
    {
        /// <summary>Setzt das Formular für den Terminplan</summary>
        public TerminPlanForm FrmTerminPlan { private get; set; }


        public StammDaten()
        {

            var culture = new CultureInfo("de-DE");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            InitializeComponent();
        }

        private void ultraToolbarsManager1_ToolClick(object sender, ToolClickEventArgs e)
        {

        }
    }
}
