using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lissajous
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Lissascoupe());
            */
            Sharpscope scope;
            using (scope = new Sharpscope(1024, 1024, "Sharpscope"))
            {
                scope.Run(120);
            }
        }
    }
}
