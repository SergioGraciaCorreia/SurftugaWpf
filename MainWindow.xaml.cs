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
	public partial class MainWindow : Window
	{
		private DispatcherTimer animationTimer;  // Temporizador para la animación
		private DispatcherTimer obstaculoTimer;  // Temporizador para spawnear obstáculos
		private DispatcherTimer cooldownTimer;  // Temporizador para manejar el enfriamiento
		private bool isIdleFrame1 = true;        // Controla qué frame mostrar
		private bool canJump = true;            // Indica si se puede realizar un salto
		private double originalYPosition;       // Posición original de la tortuga
		private MediaPlayer backgroundMediaPlayer; // MediaPlayer para el sonido de fondo
		private MediaPlayer jumpMediaPlayer;     // MediaPlayer para el sonido de salto
		private MediaPlayer songMediaPlayer;     // MediaPlayer para la canción de fondo
		private Random random = new Random(); // Generador de números aleatorios
		private int tiempoEntreObstaculosMin = 25; // Tiempo mínimo entre obstáculos (en ticks)
		private int tiempoEntreObstaculosMax = 50; // Tiempo máximo entre obstáculos (en ticks)
		private int tiempoEntreObstaculos; // Tiempo actual entre obstáculos
		private int contadorTicks = 0; // Contador de ticks para spawnear obstáculos
		private List<string> obstaculosDisponibles = new List<string>
			{
				"assets/Pulpo idle.png",
				"assets/Tiburon idle.png",
				"assets/Pajaro idle.png"
			};




		public MainWindow()
		{
			InitializeComponent();

			// Inicializar el temporizador para mover el obstáculo
			obstaculoTimer = new DispatcherTimer();
			obstaculoTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
			obstaculoTimer.Tick += ObstaculoTimer_Tick;


			// Configurar el temporizador de animación
			animationTimer = new DispatcherTimer();
			animationTimer.Interval = TimeSpan.FromMilliseconds(600); // Cambiar cada 600ms
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();

			try
			{
				// Inicializar MediaPlayer para el sonido de fondo
				backgroundMediaPlayer = new MediaPlayer();
				backgroundMediaPlayer.Open(new Uri("assets/seaSound.mp3", UriKind.Relative));
				backgroundMediaPlayer.Volume = 1;
				backgroundMediaPlayer.Play();

				// Configurar el sonido en bucle
				backgroundMediaPlayer.MediaEnded += (sender, e) =>
				{
					backgroundMediaPlayer.Position = TimeSpan.Zero; // Reiniciar el sonido
					backgroundMediaPlayer.Play();
				};

				// Inicializar MediaPlayer para el sonido de salto
				jumpMediaPlayer = new MediaPlayer();
				jumpMediaPlayer.Open(new Uri("assets/tortugaSalto.mp3", UriKind.Relative));
				jumpMediaPlayer.Volume = 0.2; // Ajusta el volumen si es necesario

				// Inicializar MediaPlayer para la canción de fondo
				songMediaPlayer = new MediaPlayer();
				songMediaPlayer.Open(new Uri("assets/song.mp3", UriKind.Relative));
				songMediaPlayer.Volume = 0.2; // Ajusta el volumen si es necesario
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al reproducir un sonido: {ex.Message}");
			}
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				// Cambiar entre las escenas
				MenuScene.Visibility = Visibility.Hidden; // Oculta el menú
				GameScene.Visibility = Visibility.Visible; // Muestra el fondo del juego
				songMediaPlayer.Play(); // Reproducir la canción de fondo

				// Detener el temporizador antes de generar el primer obstáculo
				obstaculoTimer.Stop();

				// Generar el primer obstáculo manualmente
				SpawnObstaculo("assets/Pulpo idle.png");

				// Reiniciar el contador de ticks
				contadorTicks = 0;

				// Generar un nuevo tiempo aleatorio entre obstáculos
				tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

				// Reactivar el temporizador
				obstaculoTimer.Start();

				// Configurar el sonido en bucle
				songMediaPlayer.MediaEnded += (sender, e) =>
				{
					songMediaPlayer.Position = TimeSpan.Zero; // Reiniciar el sonido
					songMediaPlayer.Play();
				};

				GameScene.UpdateLayout();
				TortugaImage.UpdateLayout();

				// Capturar la posición de la tortuga una vez visible
				originalYPosition = Canvas.GetTop(TortugaImage);
			}

			// Detectar el salto
			if (e.Key == Key.Space && canJump)
			{
				PerformJump();
			}
		}

		// Método para realizar un salto
		private void PerformJump()
		{
			canJump = false; // No permitir otro salto hasta terminar el enfriamiento

			// Reproducir sonido de salto
			jumpMediaPlayer.Position = TimeSpan.Zero; // Reiniciar el sonido en caso de que esté en uso
			jumpMediaPlayer.Play();

			// Cambiar la imagen a la de salto

			TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga salto.png", UriKind.Relative));

			// Mover la tortuga hacia arriba (salto normal)
			Canvas.SetTop(TortugaImage, originalYPosition - 160);

			// Regresar después de 0.8 segundos
			DispatcherTimer jumpTimer = new DispatcherTimer();
			jumpTimer.Interval = TimeSpan.FromMilliseconds(800);
			jumpTimer.Tick += (sender, e) =>
			{
				Canvas.SetTop(TortugaImage, originalYPosition); // Regresa a la posición inicial
				// Restaurar la imagen a la de reposo
				TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga idle.png", UriKind.Relative));
				jumpTimer.Stop();
				StartCooldown(); // Iniciar enfriamiento
			};
			jumpTimer.Start();
		}

		// Iniciar el temporizador de enfriamiento 
		private void StartCooldown()
		{
			cooldownTimer = new DispatcherTimer();
			cooldownTimer.Interval = TimeSpan.FromMilliseconds(10); // Enfriamiento de 0.1 segundo
			cooldownTimer.Tick += (sender, e) =>
			{
				canJump = true; // Permitir saltar de nuevo
				cooldownTimer.Stop();
			};
			cooldownTimer.Start();
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

		private void SpawnObstaculo(string imagenLejana)
		{
			Image obstaculo = new Image
			{
				Source = new BitmapImage(new Uri($"pack://application:,,,/{imagenLejana}", UriKind.Absolute)),
				Width = 160,
				Height = 160
			};

			// Posicionar el obstáculo
			double posicionY = 345; // Posición por defecto (obstáculos submarinos)
			if (imagenLejana.Contains("Pajaro"))
			{
				posicionY = 100; // Posición más alta para los pájaros
			}

			Canvas.SetLeft(obstaculo, 800);
			Canvas.SetTop(obstaculo, posicionY);
			GameCanvas.Children.Add(obstaculo);
		}

		private void ObstaculoTimer_Tick(object sender, EventArgs e)
		{
			// Obtener la posición X de la tortuga
			double tortugaX = Canvas.GetLeft(TortugaImage);

			// Generar un nuevo obstáculo cada cierto tiempo
			contadorTicks++;
			if (contadorTicks >= tiempoEntreObstaculos)
			{
				// Seleccionar un obstáculo aleatorio de la lista
				string obstaculoAleatorio = obstaculosDisponibles[random.Next(obstaculosDisponibles.Count)];
				SpawnObstaculo(obstaculoAleatorio);

				// Generar un nuevo tiempo aleatorio entre obstáculos
				tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

				// Reiniciar el contador
				contadorTicks = 0;
			}

			// Mover todos los obstáculos en el Canvas
			foreach (var child in GameCanvas.Children.OfType<Image>().ToList())
			{
				if (child.Source.ToString().Contains("Pulpo") || child.Source.ToString().Contains("Tiburon") || child.Source.ToString().Contains("Pajaro"))
				{
					double left = Canvas.GetLeft(child);
					Canvas.SetLeft(child, left - 20); // Mover 20 píxeles a la izquierda

					// Cambiar la imagen si el obstáculo está cerca de la tortuga
					if (left < tortugaX + 250) // Umbral de proximidad
					{
						if (child.Source.ToString().Contains("Pulpo idle.png"))
						{
							child.Source = new BitmapImage(new Uri("pack://application:,,,/assets/Pulpo ataque.png", UriKind.Absolute));
						}
						else if (child.Source.ToString().Contains("Tiburon idle.png"))
						{
							child.Source = new BitmapImage(new Uri("pack://application:,,,/assets/Tiburon ataque.png", UriKind.Absolute));
						}
						else if (child.Source.ToString().Contains("Pajaro idle.png"))
						{
							child.Source = new BitmapImage(new Uri("pack://application:,,,/assets/Pajaro ataque.png", UriKind.Absolute));
						}
					}

					// Si el obstáculo sale de la pantalla, eliminarlo
					if (left + child.Width < 0)
					{
						GameCanvas.Children.Remove(child);
					}
				}
			}
		}
	}



}

