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
		// Campos y propiedades
		private DispatcherTimer animationTimer;
		private DispatcherTimer cooldownTimer;
		private double velocidadFondo = 1;
		private double incrementoVelocidad = 0.01;
		private bool isIdleFrame1 = true;
		private bool canJump = true;
		private bool isGameRunning = false;
		private double originalYPosition;
		private MediaPlayer backgroundMediaPlayer;
		private MediaPlayer jumpMediaPlayer;
		private MediaPlayer songMediaPlayer;
		private Random random = new Random();
		private int puntuacion = 0;
		private double tiempoAcumuladoPuntuacion = 0;
		private int tiempoEntreObstaculosMin = 750;
		private int tiempoEntreObstaculosMax = 1500;
		private int tiempoEntreObstaculos;
		private double tiempoTranscurrido = 0;
		private List<string> obstaculosDisponibles = new List<string>
		{
			"assets/Pulpo idle.png",
			"assets/Tiburon idle.png",
			"assets/Pajaro idle.png"
		};

		// Constructor e inicialización
		public MainWindow()
		{
			InitializeComponent();
			ConfigurarJuego();
		}

		private void ConfigurarJuego()
		{
			RenderOptions.SetBitmapScalingMode(FondoMovimiento, BitmapScalingMode.HighQuality);
			RenderOptions.SetEdgeMode(FondoMovimiento, EdgeMode.Aliased);
			RenderOptions.SetBitmapScalingMode(GameCanvas, BitmapScalingMode.HighQuality);
			RenderOptions.SetEdgeMode(GameCanvas, EdgeMode.Aliased);

			animationTimer = new DispatcherTimer();
			animationTimer.Interval = TimeSpan.FromMilliseconds(500);
			animationTimer.Tick += AnimationTimer_Tick;
			animationTimer.Start();

			CompositionTarget.Rendering += OnRendering;

			InicializarSonidos();
		}

		private void InicializarSonidos()
		{
			try
			{
				backgroundMediaPlayer = new MediaPlayer();
				backgroundMediaPlayer.Open(new Uri("assets/seaSound.mp3", UriKind.Relative));
				backgroundMediaPlayer.Volume = 1;
				backgroundMediaPlayer.Play();
				backgroundMediaPlayer.MediaEnded += (sender, e) =>
				{
					backgroundMediaPlayer.Position = TimeSpan.Zero;
					backgroundMediaPlayer.Play();
				};

				jumpMediaPlayer = new MediaPlayer();
				jumpMediaPlayer.Open(new Uri("assets/tortugaSalto.mp3", UriKind.Relative));
				jumpMediaPlayer.Volume = 0.2;

				songMediaPlayer = new MediaPlayer();
				songMediaPlayer.Open(new Uri("assets/song.mp3", UriKind.Relative));
				songMediaPlayer.Volume = 0.2;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Error al reproducir un sonido: {ex.Message}");
			}
		}

		// Lógica del juego
		private void OnRendering(object sender, EventArgs e)
		{
			if (!isGameRunning) return;

			double deltaTime = 16;
			MoverFondo(deltaTime);
			GenerarObstaculos(deltaTime);
			MoverObstaculos(deltaTime);

			tiempoAcumuladoPuntuacion += deltaTime;
			if (tiempoAcumuladoPuntuacion >= 1000)
			{
				puntuacion += 1;
				ActualizarPuntuacion();
				tiempoAcumuladoPuntuacion = 0;
			}
		}

		private void MoverFondo(double deltaTime)
		{
			velocidadFondo += incrementoVelocidad;
			double newLeft1 = Canvas.GetLeft(FondoImage1) - velocidadFondo;
			double newLeft2 = Canvas.GetLeft(FondoImage2) - velocidadFondo;

			Canvas.SetLeft(FondoImage1, newLeft1);
			Canvas.SetLeft(FondoImage2, newLeft2);

			double imageWidth = FondoImage1.ActualWidth;

			if (newLeft1 + imageWidth <= 0)
			{
				Canvas.SetLeft(FondoImage1, newLeft2 + imageWidth);
			}
			if (newLeft2 + imageWidth <= 0)
			{
				Canvas.SetLeft(FondoImage2, newLeft1 + imageWidth);
			}
		}

		private void GenerarObstaculos(double deltaTime)
		{
			tiempoTranscurrido += deltaTime;

			if (tiempoTranscurrido >= tiempoEntreObstaculos)
			{
				string obstaculoAleatorio = obstaculosDisponibles[random.Next(obstaculosDisponibles.Count)];
				SpawnObstaculo(obstaculoAleatorio);

				tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);
				tiempoTranscurrido = 0;
			}
		}

		private void MoverObstaculos(double deltaTime)
		{
			double tortugaX = Canvas.GetLeft(TortugaImage);

			foreach (var child in GameCanvas.Children.OfType<Image>().ToList())
			{
				if (child.Source.ToString().Contains("Pulpo") || child.Source.ToString().Contains("Tiburon") || child.Source.ToString().Contains("Pajaro"))
				{
					double left = Canvas.GetLeft(child);
					Canvas.SetLeft(child, left - 10);

					if (left < tortugaX + 400)
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

					if (DetectColision(TortugaImage, child))
					{
						GameOver();
						return;
					}

					if (left + child.Width < 0)
					{
						GameCanvas.Children.Remove(child);
					}
				}
			}
		}

		private bool DetectColision(Image tortuga, Image obstaculo)
		{
			double tortugaLeft = Canvas.GetLeft(tortuga);
			double tortugaTop = Canvas.GetTop(tortuga);
			double tortugaWidth = tortuga.ActualWidth * 0.8;
			double tortugaHeight = tortuga.ActualHeight * 0.8;

			double obstaculoLeft = Canvas.GetLeft(obstaculo);
			double obstaculoTop = Canvas.GetTop(obstaculo);
			double obstaculoWidth = obstaculo.ActualWidth * 0.8;
			double obstaculoHeight = obstaculo.ActualHeight * 0.8;

			bool colisionX = tortugaLeft + tortugaWidth * 0.2 < obstaculoLeft + obstaculoWidth * 0.8 &&
							 tortugaLeft + tortugaWidth * 0.8 > obstaculoLeft + obstaculoWidth * 0.2;

			bool colisionY = tortugaTop + tortugaHeight * 0.2 < obstaculoTop + obstaculoHeight * 0.8 &&
							 tortugaTop + tortugaHeight * 0.8 > obstaculoTop + obstaculoHeight * 0.2;

			return colisionX && colisionY;
		}

		// Eventos y acciones del jugador
		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && !isGameRunning && GameOverScene.Visibility != Visibility.Visible)
			{
				IniciarJuego();
			}

			if (e.Key == Key.Space && canJump && isGameRunning)
			{
				PerformJump();
			}
		}

		private void IniciarJuego()
		{
			puntuacion = 0;
			ActualizarPuntuacion();

			MenuScene.Visibility = Visibility.Hidden;
			GameScene.Visibility = Visibility.Visible;

			isGameRunning = true;
			animationTimer.Start();

			songMediaPlayer.Play();
			songMediaPlayer.MediaEnded += (sender, e) =>
			{
				songMediaPlayer.Position = TimeSpan.Zero;
				songMediaPlayer.Play();
			};

			SpawnObstaculo("assets/Pulpo idle.png");

			tiempoTranscurrido = 0;
			tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

			GameScene.UpdateLayout();
			TortugaImage.UpdateLayout();

			originalYPosition = Canvas.GetTop(TortugaImage);
		}

		private void PerformJump()
		{
			if (!isGameRunning) return;

			canJump = false;
			jumpMediaPlayer.Position = TimeSpan.Zero;
			jumpMediaPlayer.Play();

			TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga salto.png", UriKind.Relative));
			Canvas.SetTop(TortugaImage, originalYPosition - 160);

			DispatcherTimer jumpTimer = new DispatcherTimer();
			jumpTimer.Interval = TimeSpan.FromMilliseconds(800);
			jumpTimer.Tick += (sender, e) =>
			{
				if (!isGameRunning) return;

				Canvas.SetTop(TortugaImage, originalYPosition);
				TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga idle.png", UriKind.Relative));
				jumpTimer.Stop();
				StartCooldown();
			};
			jumpTimer.Start();
		}

		private void StartCooldown()
		{
			cooldownTimer = new DispatcherTimer();
			cooldownTimer.Interval = TimeSpan.FromMilliseconds(6);
			cooldownTimer.Tick += (sender, e) =>
			{
				canJump = true;
				cooldownTimer.Stop();
			};
			cooldownTimer.Start();
		}

		// Finalización del juego
		private void GameOver()
		{
			isGameRunning = false;
			animationTimer.Stop();
			CompositionTarget.Rendering -= OnRendering;

			songMediaPlayer.Stop();

			MediaPlayer muerteMediaPlayer = new MediaPlayer();
			muerteMediaPlayer.Open(new Uri("assets/muerte.mp3", UriKind.Relative));
			muerteMediaPlayer.Volume = 1;
			muerteMediaPlayer.Play();

			TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga salto.png", UriKind.Relative));
			canJump = false;

			DispatcherTimer delayTimer = new DispatcherTimer();
			delayTimer.Interval = TimeSpan.FromSeconds(2);
			delayTimer.Tick += (sender, e) =>
			{
				delayTimer.Stop();
				MostrarPantallaGameOver();
			};
			delayTimer.Start();
		}

		private void MostrarPantallaGameOver()
		{
			GameScene.Visibility = Visibility.Hidden;
			GameOverScene.Visibility = Visibility.Visible;
			GameOverPuntuacionText.Text = $"SCORE: {puntuacion}";
		}

		private void ReiniciarButton_Click(object sender, RoutedEventArgs e)
		{
			GameOverScene.Visibility = Visibility.Hidden;

			Canvas.SetLeft(TortugaImage, 50);
			Canvas.SetTop(TortugaImage, originalYPosition);
			TortugaImage.Source = new BitmapImage(new Uri("assets/Tortuga idle.png", UriKind.Relative));

			GameCanvas.Children.Clear();

			if (!GameCanvas.Children.Contains(TortugaImage))
			{
				GameCanvas.Children.Add(TortugaImage);
			}

			if (!GameCanvas.Children.Contains(PuntuacionText))
			{
				GameCanvas.Children.Add(PuntuacionText);
			}

			puntuacion = 0;
			ActualizarPuntuacion();

			isGameRunning = true;
			canJump = true;
			animationTimer.Start();
			CompositionTarget.Rendering += OnRendering;

			velocidadFondo = 1;

			songMediaPlayer.Position = TimeSpan.Zero;
			songMediaPlayer.Play();

			SpawnObstaculo("assets/Pulpo idle.png");

			tiempoTranscurrido = 0;
			tiempoEntreObstaculos = random.Next(tiempoEntreObstaculosMin, tiempoEntreObstaculosMax);

			GameScene.Visibility = Visibility.Visible;

			if (cooldownTimer != null)
			{
				cooldownTimer.Stop();
				cooldownTimer = null;
			}

			originalYPosition = Canvas.GetTop(TortugaImage);
		}

		// Métodos auxiliares
		private void ActualizarPuntuacion()
		{
			PuntuacionText.Text = $"SCORE: {puntuacion}";
		}

		private void SpawnObstaculo(string imagenLejana)
		{
			Image obstaculo = new Image
			{
				Source = new BitmapImage(new Uri($"pack://application:,,,/{imagenLejana}", UriKind.Absolute)),
				Width = 170,
				Height = 170
			};

			double posicionY = 345;
			if (imagenLejana.Contains("Pajaro"))
			{
				posicionY = 100;
			}

			Canvas.SetLeft(obstaculo, 900);
			Canvas.SetTop(obstaculo, posicionY);
			GameCanvas.Children.Add(obstaculo);
		}

		private void FondoImage_Loaded(object sender, RoutedEventArgs e)
		{
			Canvas.SetLeft(FondoImage1, 0);
			Canvas.SetLeft(FondoImage2, FondoImage1.ActualWidth);
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

			isIdleFrame1 = !isIdleFrame1;
		}
	}
}

