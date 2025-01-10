using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Projeto_Adriana___Desenho_Vetorial
{
    public partial class Meus_Projetos : Page
    {
        public Meus_Projetos()
        {
            InitializeComponent();

            try
            {
                // Carregar as últimas imagens salvas na pasta
                CarregarUltimasImagens();
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao carregar imagens", ex);
            }
        }

        private void CarregarUltimasImagens()
        {
            try
            {
                // Caminho para a pasta de DesenhosSalvos 
                string pastaDesenhosSalvos = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DesenhosSalvos");
                pastaDesenhosSalvos = Path.GetFullPath(pastaDesenhosSalvos); // Para garantir que o caminho seja resolvido corretamente
                int numeroDeImagens = 4;

                var imagensMaisRecentes = Directory.GetFiles(pastaDesenhosSalvos, "*.png")
                    .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                    .Take(numeroDeImagens)
                    .ToList();

                int indiceImagem = 0;

                for (int i = 1; i <= numeroDeImagens; i++)
                {
                    Image image = FindName($"Image{i}") as Image;
                    if (image != null && indiceImagem < imagensMaisRecentes.Count)
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(imagensMaisRecentes[indiceImagem], UriKind.Absolute);
                        bitmap.EndInit();

                        image.Source = bitmap;
                        indiceImagem++;
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarErro("Erro ao carregar últimas imagens", ex);
            }
        }

        private void MostrarErro(string mensagem, Exception ex)
        {
            MessageBox.Show($"{mensagem}\n\nDetalhes do erro: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Btn_Novo_Projeto_Click(object sender, RoutedEventArgs e)
        {
            Janela_Desenho janela = new Janela_Desenho();
            janela.Show();
        }
    }
}
