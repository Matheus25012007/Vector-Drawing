using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Projeto_Adriana___Desenho_Vetorial
{
    /// <summary>
    /// Interação lógica para Novo_Projeto.xam
    /// </summary>
    public partial class Novo_Projeto : Page
    {
        public Novo_Projeto()
        {
            InitializeComponent();
        }

        private void Btn_Novo_Projeto_Click(object sender, RoutedEventArgs e)
        {
            Janela_Desenho janela = new Janela_Desenho();
            janela.Show();
        }
    }
}
