using MaterialDesignThemes.Wpf;
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
using System.Windows.Shapes;
using Haley.Services;
using Haley.Utils;
using Haley.Abstractions;
using Haley.Events;
using Haley.Enums;
using Haley.Models;
using Haley.MVVM;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows.Threading;



namespace Projeto_Adriana___Desenho_Vetorial
{
    /// <summary>
    /// Lógica interna para Janela_Desenho.xaml
    /// </summary>
    public partial class Janela_Desenho : Window
    {
        private List<Point> points = new List<Point>();
        private List<Polyline> lines = new List<Polyline>();


        private bool Desenhando = true; // Inicialmente, está no modo de desenho
        private bool erasing = false; // Inicialmente, está no modo de desenho
        private bool Desenho_Livre = false;

        private int Medidas;
        private int diametro;
        private Brush brushcolor = Brushes.Black;

        private Rectangle? selectionRect; // Retângulo de seleção
        private Point startPoint; // Ponto inicial da seleção
        private Point endPoint; // Ponto final da seleção

        private bool formas = false;
        private bool isResizing = false;
        private double initialWidth;
        private double initialHeight;

        public Janela_Desenho()
        {
            InitializeComponent();
            Medidas = 4;
            this.PreviewMouseWheel += Janela_Desenho_PreviewMouseWheel;
            // Certifique-se de que o Canvas tenha uma transformação inicial
            if (canvas.LayoutTransform == null)
            {
                canvas.LayoutTransform = new ScaleTransform();
            }
        }

        private void Janela_Desenho_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double normalizedDelta = e.Delta / SystemParameters.WheelScrollLines;
                double scale = normalizedDelta > 0 ? 1.1 : 0.9;

                canvas.LayoutTransform = new ScaleTransform(canvas.LayoutTransform.Value.M11 * scale, canvas.LayoutTransform.Value.M22 * scale);

                List<Point> newPoints = new List<Point>();

                foreach (Point point in points)
                {
                    Point newPoint = new Point(point.X * scale, point.Y * scale);
                    newPoints.Add(newPoint);

                    Ellipse ellipse = new Ellipse
                    {
                        Width = Medidas,
                        Height = Medidas,
                        Fill = brushcolor
                    };
                    Canvas.SetLeft(ellipse, newPoint.X - Medidas / 2);
                    Canvas.SetTop(ellipse, newPoint.Y - Medidas / 2);
                    canvas.Children.Add(ellipse);
                }

                points = newPoints;

                foreach (Polyline line in lines)
                {
                    line.Points = new PointCollection(line.Points.Select(p => new Point(p.X * scale, p.Y * scale)));
                }

                e.Handled = true;
            }
        }

        private void EraseWithinSelectionRect()
        {
            if (selectionRect == null) return;

            double left = Math.Min(startPoint.X, endPoint.X);
            double top = Math.Min(startPoint.Y, endPoint.Y);
            double width = Math.Abs(endPoint.X - startPoint.X);
            double height = Math.Abs(endPoint.Y - startPoint.Y);

            Rect selectionArea = new Rect(left, top, width, height);

            List<UIElement> elementsToRemove = new List<UIElement>();

            foreach (UIElement element in canvas.Children)
            {
                if (element is Polyline polyline)
                {
                    Rect bounds = CalculatePolylineBounds(polyline);
                    if (selectionArea.IntersectsWith(bounds))
                    {
                        elementsToRemove.Add(polyline);
                    }
                }
                else if (element is Ellipse ellipse)
                {
                    Rect bounds = new Rect(Canvas.GetLeft(ellipse), Canvas.GetTop(ellipse), ellipse.Width, ellipse.Height);
                    if (selectionArea.IntersectsWith(bounds))
                    {
                        elementsToRemove.Add(ellipse);
                    }
                }
            }

            foreach (UIElement element in elementsToRemove)
            {
                canvas.Children.Remove(element);
            }

            canvas.Children.Remove(selectionRect);
            selectionRect = null;
        }

        private Rect CalculatePolylineBounds(Polyline polyline)
        {
            double left = double.MaxValue;
            double top = double.MaxValue;
            double right = double.MinValue;
            double bottom = double.MinValue;

            foreach (Point point in polyline.Points)
            {
                left = Math.Min(left, point.X);
                top = Math.Min(top, point.Y);
                right = Math.Max(right, point.X);
                bottom = Math.Max(bottom, point.Y);
            }

            return new Rect(left, top, right - left, bottom - top);
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Desenhando)
            {
                Point newPoint = e.GetPosition(canvas);
                points.Add(newPoint);

                // Atualize o tamanho do ponto
                Ellipse ellipse = new Ellipse
                {
                    Width = Medidas,
                    Height = Medidas,
                    Fill = brushcolor
                };

                Canvas.SetLeft(ellipse, newPoint.X - Medidas / 2);
                Canvas.SetTop(ellipse, newPoint.Y - Medidas / 2);
                canvas.Children.Add(ellipse);
            }
            else if (erasing)
            {
                startPoint = e.GetPosition(canvas);
                endPoint = startPoint;

                selectionRect = new Rectangle
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.5,
                    Fill = Brushes.Transparent
                };
                Canvas.SetLeft(selectionRect, startPoint.X);
                Canvas.SetTop(selectionRect, startPoint.Y);

                canvas.Children.Add(selectionRect);
            }

            else if (formas)
            {
                // Cria um novo retângulo
                Rectangle rectangle = new Rectangle();
                rectangle.Width = 100; //Largura
                rectangle.Height = 100; //Altura
                rectangle.Fill = brushcolor;//Preenchimento
                rectangle.Stroke = brushcolor; //Cor da borda
                rectangle.StrokeThickness = 1; //Espessura da borda

                // Adiciona o retângulo ao canvas
                canvas.Children.Add(rectangle);

                // Obtém a posição do mouse em relação ao canvas
                Point mousePosition = e.GetPosition(canvas);

                // Define a posição inicial do retângulo
                Canvas.SetLeft(rectangle, mousePosition.X - rectangle.Width / 2);
                Canvas.SetTop(rectangle, mousePosition.Y - rectangle.Height / 2);

                formas = false;

                // Define o manipulador de eventos para arrastar o retângulo
                rectangle.MouseMove += Retangulo_MouseMove;

                //Define o manipulador de eventos para permitir que o retângulo possa alterar de tamanho
                rectangle.MouseRightButtonDown += Retangulo_MouseDown;

                isResizing = false;

                // Se o botão esquerdo estiver pressionado, movemos o retângulo
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    startPoint = e.GetPosition(canvas);
                    /*Ele é essencial para rastrear a posição inicial do mouse e 
                     calcular as mudanças seguintes da posição ou tamanho do retângulo.*/
                }
            }
        }

        private void Retangulo_MouseMove(object sender, MouseEventArgs e)
        {
            // Obtém o retângulo que está sendo arrastado
            Rectangle rectangle = sender as Rectangle;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Mover o retângulo
                double deltaX = e.GetPosition(canvas).X - startPoint.X;
                double deltaY = e.GetPosition(canvas).Y - startPoint.Y;

                /*double deltaX = e.GetPosition(canvas).X - startPoint.X (também vale para o eixo Y);
                 * 
                 * e.GetPosition(canvas).X obtém a posição atual do mouse 
                 *longo do eixo X em relação ao elemento canvas.
                 *
                 *startPoint.X é a posição inicial do mouse ao longo do eixo X 
                 *quando o evento de movimento do mouse começou.
                 *
                 *deltaX é a diferença entre a posição atual do mouse e a posição 
                 *inicial ao longo do eixo X. Representa o quanto o mouse se moveu
                 *horizontalmente desde o início do movimento.*/

                Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + deltaX);
                Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + deltaY);

                /*Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + deltaX); (também vale para o eixo Y)
                 * 
                 *Canvas.SetLeft(rectangle, ...) define a nova posição do retângulo 
                 *ao longo do eixo X.
                 *
                 *Canvas.GetLeft(rectangle) obtém a posição atual do retângulo ao 
                 *longo do eixo X em relação ao elemento canvas.
                 *
                 *Adicionamos deltaX à posição atual para mover o retângulo horizontalmente
                 *com base no movimento do mouse.*/
                e.Handled = true;
                startPoint = e.GetPosition(canvas);
            }
            else if (isResizing && (e.RightButton == MouseButtonState.Pressed))
            {
                // Redimensionar o retângulo com o botão direito
                double deltaX = e.GetPosition(canvas).X - startPoint.X;
                double deltaY = e.GetPosition(canvas).Y - startPoint.Y;

                rectangle.Width = Math.Max(0, initialWidth + deltaX);
                rectangle.Height = Math.Max(0, initialHeight + deltaY);

                /*rectangle.Width = Math.Max(0, initialWidth + deltaX); (também vale para o eixo Y)
                 *initialWidth é a largura inicial do retângulo quando o evento começou.
                 *
                 *initialWidth + deltaX representa a nova largura que seria se ajustássemos 
                 *a largura inicial com base no movimento horizontal do mouse.
                 *
                 *Math.Max(0, initialWidth + deltaX) garante que a largura do retângulo não seja 
                 *menor que zero. Isso é feito porque não faz sentido ter uma largura negativa.*/
                e.Handled = true;
                startPoint = e.GetPosition(canvas);
            }

        }
        private void Retangulo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Se o botão direito estiver pressionado, indicamos que vamos redimensionar
            if (e.RightButton == MouseButtonState.Pressed)
            {
                isResizing = true;
                startPoint = e.GetPosition(canvas);
                initialWidth = (sender as Rectangle).Width;
                initialHeight = (sender as Rectangle).Height;

                // Impedimos que o evento propague para o botão direito do mouse
                e.Handled = true;
            }
        }

        //Os parâmetros são:Cor e Posição do Mouse no Canvas
        private void Area_Circulo(Brush Cor_Circulo, Point position)
        {
            //Esse é o método que define a àrea de pincelaem e a cor
            diametro = Medidas;
            Ellipse newellipse = new Ellipse();
            newellipse.Fill = Cor_Circulo;
            newellipse.Width = diametro;
            newellipse.Height = diametro;
            Canvas.SetTop(newellipse, position.Y);
            Canvas.SetLeft(newellipse, position.X);
            canvas.Children.Add(newellipse);
        }

        private void btn_borracha_Click(object sender, RoutedEventArgs e)
        {
            Desenhando = false;
            Desenho_Livre = false;
            erasing = true;

            if (selectionRect != null)//Se o que estiver dentro da àrea do retângulo for diferente de vazio
            {
                canvas.Children.Remove(selectionRect);//Todos os elementos filhos serão removidos dentro da àrea do retângulo
                selectionRect = null;// Atribuí Vazio a variavel
            }
        }

        private void btn_DesenhoVetor_Click(object sender, RoutedEventArgs e)
        {
            Desenhando = true;
            erasing = false;
            Desenho_Livre = false;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (erasing && selectionRect != null)
                {
                    endPoint = e.GetPosition(canvas);
                    double left = Math.Min(startPoint.X, endPoint.X);
                    double top = Math.Min(startPoint.Y, endPoint.Y);
                    double width = Math.Abs(endPoint.X - startPoint.X);
                    double height = Math.Abs(endPoint.Y - startPoint.Y);
                    Canvas.SetLeft(selectionRect, left);
                    Canvas.SetTop(selectionRect, top);
                    selectionRect.Width = width;
                    selectionRect.Height = height;
                }
                else if (Desenhando)
                {
                    Point newPoint = e.GetPosition(canvas);
                    points.Add(newPoint);
                    Ellipse ellipse = new Ellipse
                    {
                        Width = Medidas,
                        Height = Medidas,
                        Fill = brushcolor

                    };
                    Canvas.SetLeft(ellipse, newPoint.X - 1.5);
                    Canvas.SetTop(ellipse, newPoint.Y - 1.5);
                    canvas.Children.Add(ellipse);
                }
                else if (Desenho_Livre && e.LeftButton == MouseButtonState.Pressed)
                {
                    Point mouseposition = e.GetPosition(canvas);
                    Area_Circulo(brushcolor, mouseposition);
                }
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (erasing && selectionRect != null)
            {
                endPoint = e.GetPosition(canvas);
                EraseWithinSelectionRect();
            }
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Desenhando)
            {
                if (points.Count >= 2)//A criação de linhas só acontece se tiver no mínimo 2 pontos na tela
                {
                    Polyline polyline = new Polyline
                    {
                        Stroke = brushcolor, //Variável que define a cor
                        StrokeThickness = Medidas // Variável que define o tamanho
                    };

                    PointCollection pointCollection = new PointCollection(points);
                    polyline.Points = pointCollection; // Basicamente o elemento Polyline recebe as coordenadas dos pontos que são colocados na tela. Os pontos possuem coordenadas bidimensionais, ou seja (X, Y)
                    canvas.Children.Add(polyline);// A linha é adicionada visualmente como elemento filho ao Canvas

                    points.Clear();// Depois que uma linha é criada, os pontos reiniciam
                }
            }
            else if (erasing)
            {
                erasing = false;
                canvas.Children.Remove(selectionRect);
                selectionRect = null;
            }
        }

        private void btn_MudaCor_Click(object sender, RoutedEventArgs e)
        {
            var newdialog = new ColorPickerDialog();//está referenciando um objeto com o elemento ColorPicker
            newdialog.ShowDialog();//Está mostrando o elemento na tela
            ColorUtils.GetSystemColors();//Talvez esteja pegando o as cores que já venha no ColorPicker
            brushcolor = newdialog.SelectedBrush; //Atribuí a cor escolhida na Variável brushcolor, que está sendo usado em outros métodos.
        }

        private void btn_DesenhoLivre_Click(object sender, RoutedEventArgs e)
        {
            Desenho_Livre = true;
            Desenhando = false;
            erasing = false;
        }

        private void btn_Voltar_Menu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_Salvar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON Files (*.json)|*.json";
                if (saveFileDialog.ShowDialog() == true)
                {
                    List<CalculatePolylineBounds> canvasElementList = new List<CalculatePolylineBounds>();

                    foreach (var uiElement in canvas.Children)
                    {
                        if (uiElement is Polyline polyline)
                        {
                            CalculatePolylineBounds element = new CalculatePolylineBounds
                            {
                                X = 0,
                                Y = 0,
                                Type = "Polyline",
                                StrokeColor = ((SolidColorBrush)polyline.Stroke).Color,
                                StrokeThickness = polyline.StrokeThickness,
                                Points = polyline.Points.ToList(),
                            };
                            canvasElementList.Add(element);
                        }
                        else if (uiElement is Ellipse ellipse)
                        {
                            // Adicionar código para lidar com outros tipos de elementos (se necessário)
                        }
                        else if (uiElement is Polyline freehandPolyline)
                        {
                            // Salvar desenho livre
                            CalculatePolylineBounds freehandElement = new CalculatePolylineBounds
                            {
                                X = Canvas.GetLeft(freehandPolyline),
                                Y = Canvas.GetTop(freehandPolyline),
                                Type = "Freehand",
                                StrokeColor = ((SolidColorBrush)freehandPolyline.Stroke).Color,
                                StrokeThickness = freehandPolyline.StrokeThickness,
                                Points = freehandPolyline.Points.ToList(),
                            };
                            canvasElementList.Add(freehandElement);
                        }
                    }

                    string serializedData = JsonConvert.SerializeObject(canvasElementList);
                    File.WriteAllText(saveFileDialog.FileName, serializedData);
                    MessageBox.Show("Desenho salvo em: " + saveFileDialog.FileName, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar o desenho: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btn_Carregar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JSON Files (*.json)|*.json";
                if (openFileDialog.ShowDialog() == true)
                {
                    string serializedData = File.ReadAllText(openFileDialog.FileName);
                    List<CalculatePolylineBounds> loadedCanvasElements = JsonConvert.DeserializeObject<List<CalculatePolylineBounds>>(serializedData);

                    foreach (var element in loadedCanvasElements)
                    {
                        if (element.Type == "Polyline")
                        {
                            Polyline newPolyline = new Polyline
                            {
                                Stroke = new SolidColorBrush(element.StrokeColor),
                                StrokeThickness = element.StrokeThickness,
                            };

                            foreach (var point in element.Points)
                            {
                                newPolyline.Points.Add(point);
                            }

                            canvas.Children.Add(newPolyline);
                        }
                        else if (element.Type == "Freehand")
                        {
                            Polyline newFreehandPolyline = new Polyline
                            {
                                Stroke = new SolidColorBrush(element.StrokeColor),
                                StrokeThickness = element.StrokeThickness,
                            };

                            foreach (var point in element.Points)
                            {
                                newFreehandPolyline.Points.Add(point);
                            }

                            Canvas.SetLeft(newFreehandPolyline, element.X);
                            Canvas.SetTop(newFreehandPolyline, element.Y);

                            canvas.Children.Add(newFreehandPolyline);
                        }
                    }

                    MessageBox.Show("Desenho carregado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Arquivo de desenho não encontrado.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar o desenho: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender; // Cast do sender para o Slider
            Medidas = (int)slider.Value; // Atribui o valor do Slider à variável Medidas
        }


        private void btn_SalvarPNG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define o diretório correto para a pasta "DesenhosSalvos" dentro do diretório do projeto
                string pastaDesejada = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DesenhosSalvos");

                // Verifica se a pasta existe. Se não, cria a pasta
                if (!Directory.Exists(pastaDesejada))
                {
                    Directory.CreateDirectory(pastaDesejada);
                }

                // Cria um nome de arquivo baseado na data e hora atual
                string nomeArquivo = $"Desenho_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.png";
                string caminhoCompleto = System.IO.Path.Combine(pastaDesejada, nomeArquivo);

                // Renderiza o conteúdo do Canvas como uma imagem
                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                    (int)canvas.ActualWidth, (int)canvas.ActualHeight,
                    96d, 96d, PixelFormats.Pbgra32);

                renderBitmap.Render(canvas);

                // Codifica a imagem como PNG e a salva no arquivo
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream fileStream = File.Create(caminhoCompleto))
                {
                    pngEncoder.Save(fileStream);
                }

                MessageBox.Show("Projeto salvo como PNG em: " + caminhoCompleto, "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar o desenho: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void btn_Formas_Click(object sender, RoutedEventArgs e)
        {
            formas = true;
            Desenhando = false;
            erasing = false;
            /*Essas atribuições são para que possa ser possivel criar um
            retângulo. Desativando temporariamente as outras funções para
            que não ocorra colisão de função*/
        }
    }
}
