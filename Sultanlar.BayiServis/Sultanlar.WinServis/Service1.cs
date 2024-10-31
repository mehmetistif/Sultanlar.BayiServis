using Sultanlar.ClassLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace Sultanlar.WinServis
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        Timer tmr;
        EventLog ev;

        protected override void OnStart(string[] args)
        {
            ev = new EventLog();
            ev.Source = "Sultanlar Bayi In Servis";

            tmr = new Timer(3600000);
            tmr.Elapsed += Tmr_Elapsed;
            tmr.Enabled = true;
            tmr.Start();
            Yap();
        }

        private void Tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now.Hour == 10 || DateTime.Now.Hour == 12 || DateTime.Now.Hour == 14 || DateTime.Now.Hour == 16 || DateTime.Now.Hour == 18 || DateTime.Now.Hour == 20)
            {
                Yap();
            }
        }

        private void Yap()
        {
            /*Class1 cls = new Class1(ev, "1071593");
            cls.KaanGonder();
            cls.KaanStokGonder();*/

            Class1 cls1 = new Class1(ev, "1052689");
            cls1.PekerGonder();
            cls1.PekerStokGonder();

            Class1 cls = new Class1(ev, "1018538");
            cls.YilmazGonder();
            cls.YilmazStokGonder();
        }

        protected override void OnStop()
        {
        }
    }
}
