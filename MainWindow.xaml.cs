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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            /*Esse elemento vai carregar a página meus projetos
             para o programa iniciar com ela carregada.*/

            Loaded += Carregamento_pag_meus_projetos;
        }

        /*Com isso se cria uma classe com o carregamento que criei
         e assim nessa classe a página meus projetos será ja iniciada
        com o prgrama e aparecerá no frame.*/

        private void Carregamento_pag_meus_projetos(object sender, RoutedEventArgs e)
        {
            // Carregar a página "Meus Projetos" no Frame quando iniciar o Programa.

            Area_Exib.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Area_Exib.Navigate(new Uri("Meus_Projetos.xaml", UriKind.Relative));
        }

        private void Btn_Meus_Projetos_Click(object sender, RoutedEventArgs e)
        {
            Area_Exib.Navigate(new Uri("Meus_Projetos.xaml", UriKind.Relative));
        }

        private void Btn_Criar_Novo_Projeto_Click(object sender, RoutedEventArgs e)
        {
            Area_Exib.Navigate(new Uri("Novo_Projeto.xaml", UriKind.Relative));

        }

        private void Txt_Pesquisar_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Txt_Pesquisar_GotFocus(object sender, RoutedEventArgs e)
        {
            Txt_Pesquisar.Visibility = System.Windows.Visibility.Collapsed;
            Txt_Pesquisar2.Visibility = System.Windows.Visibility.Visible;
            Txt_Pesquisar.Focus();
        }

        private void Txt_Pesquisar2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Txt_Pesquisar2.Text)) 
            {
                Txt_Pesquisar2.Visibility = System.Windows.Visibility.Collapsed;
                Txt_Pesquisar.Visibility = System.Windows.Visibility.Visible;
            }

        }

        private void btn_Fechar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_Minimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Area_Exib_Navigated(object sender, NavigationEventArgs e)
        {

        }
    }
}
