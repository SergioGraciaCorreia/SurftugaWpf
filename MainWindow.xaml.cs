using System.Diagnostics;
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
		private DispatcherTimer cooldownTimer;  // Temporizador para manejar el enfriamiento
		private double velocidadFondo = 1; // Velocidad de desplazamiento del fondo
		private double incrementoVelocidad = 0.01; // Incremento de velocidad por cada tick
		private bool isIdleFrame1 = true;        // Controla qué frame mostrar
		private bool canJump = true;            // Indica si se puede realizar un salto
		private bool isGameRunning = false; // Controla si el juego está en ejecución
		private double originalYPosition;       // Posición original de la tortuga
		private MediaPlayer backgroundMediaPlayer; // MediaPlayer para el sonido de fondo
		private MediaPlayer jumpMediaPlayer;     // MediaPlayer para el sonido de salto
		private MediaPlayer songMediaPlayer;     // MediaPlayer para la canción de fondo
		private Random random = new Random(); // Generador de números aleatorios
		private int puntuacion = 0; // Variable para almacenar la puntuación
		private double tiempoAcumuladoPuntuacion = 0; // Variable para acumular el tiempo
		private int tiempoEntreObstaculosMin = 750; // Tiempo mínimo entre obstáculos (en ms)
		private int tiempoEntreObstaculosMax = 1500; // Tiempo máximo entre obstáculos (en ms)
		private int tiempoEntreObstaculos; // Tiempo actual entre obstáculos
		private double tiempoTranscurrido = 0; // Tiempo transcurrido desde el último obstáculo
		private List<string> obstaculosDisponibles = new List<string>
		{
			"assets/Pulpo idle.png",
			"assets/Tiburon idle.png",
			"assets/Pajaro idle.png"
		};

		public MainWindow()
		{
			InitializeComponent();


			// Habilitar doble búfer para reducir parpadeos
			RenderOptions.SetBitmapScalingMode(FondoMovimiento, BitmapScalingMode.HighQuality);
			RenderOptions.SetEdgeMode(FondoMovimiento, EdgeMode.Aliased);
			RenderOptions.SetBitmapScalingMode(GameCanvas, BitmapScalingMode.HighQuality);
			RenderOptions.SetEdgeMode(GameCanvas, EdgeMode.Aliased);
		

			// Configurar el temporizador de animación
			animationTimer = new DispatcherTimer();
			animationTimer.Interval = TimeSpan.FromMilliseconds(500); // Cambiar cada 500ms
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();

			// Usar CompositionTarget.Rendering para animaciones suaves
			CompositionTarget.Rendering += OnRendering;

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

		private void OnRendering(object sender, EventArgs e)
		{
			if (!isGameRunning) return; // No ejecutar la lógica del juego si no está en ejecución

			// Obtener el tiempo transcurrido desde el último fotograma
			double deltaTime = 16; // Aproximadamente 16ms por fotograma (60 FPS)

			// Mover el fondo
			MoverFondo(deltaTime);

			// Generar obstáculos
			GenerarObstaculos(deltaTime);

			// Mover obstáculos
			MoverObstaculos(deltaTime);

			// Incrementar la puntuación cada segundo (1000ms)
			tiempoAcumuladoPuntuacion += deltaTime;
			if (tiempoAcumuladoPuntuacion >= 1000) // Cada 1000ms (1 segundo)
			{
				puntuacion += 1; // Aumentar la puntuación en 5 puntos cada segundo
				ActualizarPuntuacion();
				tiempoAcumuladoPuntuacion = 0; // Reiniciar el contador
			}
		}

		private void ActualizarPuntuacion()
		{
			// Actualizar el TextBlock con la puntuación actual
			PuntuacionText.Text = $"SCORE: {puntuacion}";
		}

		private void MoverFondo(double deltaTime)
		{
			// Incrementar la velocidad del fondo
			velocidadFondo += incrementoVelocidad;

			// Mover ambas imágenes
			double newLeft1 = Canvas.GetLeft(FondoImage1) - velocidadFondo;
			double newLeft2 = Canvas.GetLeft(FondoImage2) - velocidadFondo;

			Canvas.SetLeft(FondoImage1, newLeft1);
			Canvas.SetLeft(FondoImage2, newLeft2);

			// Obtener el ancho real de las imágenes
			double imageWidth = FondoImage1.ActualWidth;

			// Reposicionar las imágenes cuando salgan completamente de la pantalla
			if (newLeft1 + imageWidth <= 0)
			{
				// Colocar FondoImage1 a la derecha de FondoImage2
				Canvas.SetLeft(FondoImage1, newLeft2 + imageWidth);
			}
			if (newLeft2 + imageWidth <= 0)
			{
				// Colocar FondoImage2 a la derecha de FondoImage1
				Canvas.SetLeft(FondoImage2, newLeft1 + imageWidth);
			}
		}

		private void GenerarObstaculos(double deltaTime)
		{
			tiempoTranscurrido += deltaTime;

			if (tiempoTranscurrido >= tiempoEntreObstaculos)
			{
				// Seleccionar un obstáculo aleatorio de la lista
				string obstaculoAleatorio = obstaculosDisponibles[random.Next(obstaculosDisponibles.Count)];
				SpawnObstaculo(obstaculoAleatorio);

				// Generar un nuevo tiempo aleatorio entre obstáculos
				tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

				// Reiniciar el contador
				tiempoTranscurrido = 0;
			}
		}

		private void MoverObstaculos(double deltaTime)
		{
			// Obtener la posición X de la tortuga
			double tortugaX = Canvas.GetLeft(TortugaImage);

			// Mover todos los obstáculos en el Canvas
			foreach (var child in GameCanvas.Children.OfType<Image>().ToList())
			{
				if (child.Source.ToString().Contains("Pulpo") || child.Source.ToString().Contains("Tiburon") || child.Source.ToString().Contains("Pajaro"))
				{
					double left = Canvas.GetLeft(child);
					Canvas.SetLeft(child, left - 10); // Mover x píxeles a la izquierda

					// Cambiar la imagen si el obstáculo está cerca de la tortuga
					if (left < tortugaX + 400) // Umbral de proximidad
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

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && !isGameRunning) // Solo iniciar si el juego no está en ejecución
			{
				// Reiniciar la puntuación
				puntuacion = 0;
				ActualizarPuntuacion();

				// Cambiar entre las escenas
				MenuScene.Visibility = Visibility.Hidden; // Oculta el menú
				GameScene.Visibility = Visibility.Visible; // Muestra el fondo del juego

				// Iniciar el juego
				isGameRunning = true;
				animationTimer.Start(); // Iniciar el temporizador de animación

				songMediaPlayer.Play(); // Reproducir la canción de fondo
				// Configurar la canción en bucle
				songMediaPlayer.MediaEnded += (sender, e) =>
				{
					songMediaPlayer.Position = TimeSpan.Zero; // Reiniciar la canción
					songMediaPlayer.Play();
				};
				// Generar el primer obstáculo manualmente
				SpawnObstaculo("assets/Pulpo idle.png");

				// Reiniciar el contador de tiempo
				tiempoTranscurrido = 0;

				// Generar un nuevo tiempo aleatorio entre obstáculos
				tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

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

		private void StartCooldown()
		{
			cooldownTimer = new DispatcherTimer();
			cooldownTimer.Interval = TimeSpan.FromMilliseconds(6); // Enfriamiento de 0.6 segundo
			cooldownTimer.Tick += (sender, e) =>
			{
				canJump = true; // Permitir saltar de nuevo
				cooldownTimer.Stop();
			};
			cooldownTimer.Start();
		}

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
				Width = 170,
				Height = 170
			};

			// Posicionar el obstáculo
			double posicionY = 345; // Posición por defecto (obstáculos submarinos)
			if (imagenLejana.Contains("Pajaro"))
			{
				posicionY = 100; // Posición más alta para los pájaros
			}

			Canvas.SetLeft(obstaculo, 900);
			Canvas.SetTop(obstaculo, posicionY);
			GameCanvas.Children.Add(obstaculo);
		}

		private void FondoImage_Loaded(object sender, RoutedEventArgs e)
		{
			// Posicionar las imágenes del fondo una al lado de la otra
			Canvas.SetLeft(FondoImage1, 0); // FondoImage1 comienza en la posición 0
			Canvas.SetLeft(FondoImage2, FondoImage1.ActualWidth); // FondoImage2 comienza justo después de FondoImage1
		}
	}
}

