using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


using System.Net.NetworkInformation;
using System.Windows.Threading;
using System.Diagnostics;

namespace shutdownload
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkInterface[] Interfaces;  //vector con interfaces de red
        private long downAnterior = 0;
        private long upAnterior = 0;
        private int segundos = 0; 
        private DispatcherTimer timer;
        private Boolean ini;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            ini = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < Interfaces.Length; i++)
                cbInterfaz.Items.Add(Interfaces[i].Name);
            cbInterfaz.SelectedIndex = 0;

            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            NetworkInterface interfaz = Interfaces[cbInterfaz.SelectedIndex];
            IPv4InterfaceStatistics estadisticas = interfaz.GetIPv4Statistics();

            int download=(int)(estadisticas.BytesReceived - downAnterior) / 1024;
            lbDownSpeed.Text = download.ToString() + " KB/s";
            downAnterior = estadisticas.BytesReceived;

            int upload = (int)(estadisticas.BytesSent - upAnterior) / 1024;
            lbUpSpeed.Text = upload.ToString() + " KB/s";
            upAnterior = estadisticas.BytesSent;

            if (ini)
            {
                lbTiempo.Text = (udTiempo.Value * 60 - segundos).ToString() + "s";

                if (cbApagar.SelectedIndex == 0)
                {
                    Check(download);
                }
                else
                {
                    Check(upload);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            gbParametros.IsEnabled = ini;
            ini = !ini;
            btnIniciar.Content = ini ? "Detener" : "Iniciar";
            segundos = 0;
        }

        private void Check(int updown)
        {
            if (updown < udVelocidad.Value)
            {
                segundos++;
                if (segundos > udTiempo.Value*60)
                {
                    Apagar(cbAccion.SelectedIndex);
                }
            }
            else
            {
                segundos = 0;
            }
        }

        private void Apagar(int x)
        {
            Button_Click(null, null);

            switch (x)
            {
                case 0:
                    System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Suspend, true, true);
                    break;
                case 1:
                    Process.Start("shutdown.exe", "-s -f");
                    break;
                case 2:
                    System.Windows.Forms.Application.SetSuspendState(System.Windows.Forms.PowerState.Hibernate, true, true);
                    break;
            }
        }
    }
}
