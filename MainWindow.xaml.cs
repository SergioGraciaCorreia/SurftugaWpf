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
using System.Windows.Threading;

namespace SurftugaWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private DispatcherTimer animationTimer; // Temporizador para la animación
		private bool isIdleFrame1 = true;       // Controla qué frame mostrar

		public MainWindow()
        {
            InitializeComponent();

			// Configurar el temporizador
			animationTimer = new DispatcherTimer();
			animationTimer.Interval = TimeSpan.FromMilliseconds(600); // Cambiar cada 500ms
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();

			try
			{
				// Inicializar el MediaPlayer
				mediaPlayer = new MediaPlayer();
				mediaPlayer.Open(new Uri("assets/seaSound.mp3", UriKind.Relative));
				mediaPlayer.Volume = 1;
				mediaPlayer.Play();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al reproducir el sonido: {ex.Message}");
			}
			// Configurar el sonido en bucle
			mediaPlayer.MediaEnded += (sender, e) =>
			{
				mediaPlayer.Position = TimeSpan.Zero; // Reiniciar el sonido
				mediaPlayer.Play();
			};

			// Iniciar el sonido
			mediaPlayer.Play();
		}
		private MediaPlayer mediaPlayer;
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				// Cambiar entre las escenas
				MenuScene.Visibility = Visibility.Hidden; // Oculta el menú
				GameScene.Visibility = Visibility.Visible; // Muestra el fondo del juego
				GameScene.UpdateLayout();
				TortugaImage.UpdateLayout();
			}
		}
		// Alternar imágenes en cada tick del temporizador
		private void AnimationTimer_Tick(object sender, EventArgs e)
		{
			if (isIdleFrame1)
			{
				TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga idle 2.png", UriKind.Relative));
			}
			else
			{
				TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga idle.png", UriKind.Relative));
			}

			isIdleFrame1 = !isIdleFrame1; // Cambiar el estado
		}
	}
}