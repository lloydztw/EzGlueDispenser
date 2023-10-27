using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Eazy_Project_III
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var frmMain = new MainForm();
            _test(frmMain);
            Application.Run(frmMain);

        }
        static void _test(Form frm)
        {
#if(OPT_DIRECT_TEST_GUI_FORM)
            //> JetEazy.GdxCore3.Model.CoretronicsAPI.updateParams();
            frm.Load += new EventHandler((sender, e) =>
            {
                frm.BeginInvoke(new Action(() =>
                {
                    var dlg = new FormSpace.FormRecipe();
                    {
                        dlg.FormBorderStyle = FormBorderStyle.Sizable;
                        dlg.ControlBox = true;
                        //dlg.ShowDialog(frm);
                        dlg.Show(frm);
                    }
                }));
            });
#endif
        }
    }
}
