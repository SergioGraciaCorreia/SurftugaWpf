SurftugaWPF

¡Bienvenido a SurftugaWPF! Este es un juego simple desarrollado en WPF (Windows Presentation Foundation) donde controlas a una tortuga surfera que debe esquivar obstáculos mientras el fondo se mueve cada vez más rápido.

Descripción del juego
El objetivo del juego es sobrevivir el mayor tiempo posible, esquivando obstáculos que aparecen aleatoriamente. La velocidad del fondo aumenta con el tiempo, lo que hace que el juego sea cada vez más desafiante. ¡Consigue la mayor puntuación posible!

Métodos principales
A continuación se describen los métodos clave del juego:

1. Inicialización
MainWindow(): Constructor de la ventana principal. Inicializa los componentes y configura el juego.

ConfigurarJuego(): Configura los temporizadores, la calidad de renderizado y otros ajustes iniciales.

InicializarSonidos(): Inicializa los sonidos del juego (fondo, salto, música, etc.).

2. Lógica del juego
OnRendering(object sender, EventArgs e): Método que se ejecuta en cada fotograma. Controla el movimiento del fondo, la generación de obstáculos y la actualización de la puntuación.

MoverFondo(double deltaTime): Mueve las imágenes del fondo para crear el efecto de desplazamiento.

GenerarObstaculos(double deltaTime): Genera obstáculos en intervalos aleatorios.

MoverObstaculos(double deltaTime): Mueve los obstáculos hacia la izquierda y verifica colisiones con la tortuga.

DetectColision(Image tortuga, Image obstaculo): Detecta si hay una colisión entre la tortuga y un obstáculo.

3. Eventos y acciones del jugador
Window_KeyDown(object sender, KeyEventArgs e): Maneja las teclas presionadas por el jugador (Enter para iniciar el juego, Espacio para saltar).

PerformJump(): Realiza el salto de la tortuga y reproduce el sonido correspondiente.

StartCooldown(): Inicia el enfriamiento después de un salto para evitar saltos repetidos.

4. Finalización del juego
GameOver(): Detiene el juego, reproduce el sonido de "muerte" y muestra la pantalla de Game Over después de un breve retraso.

MostrarPantallaGameOver(): Muestra la pantalla de Game Over con la puntuación final.

ReiniciarButton_Click(object sender, RoutedEventArgs e): Reinicia el juego cuando el jugador hace clic en el botón "Reset".

5. Métodos auxiliares
ActualizarPuntuacion(): Actualiza el texto de la puntuación en la pantalla.

SpawnObstaculo(string imagenLejana): Crea y posiciona un nuevo obstáculo en el Canvas.

FondoImage_Loaded(object sender, RoutedEventArgs e): Posiciona las imágenes del fondo al cargarse.

AnimationTimer_Tick(object sender, EventArgs e): Alterna entre los frames de animación de la tortuga.

Cómo jugar
Iniciar el juego: Presiona Enter en la pantalla de inicio.

Saltar: Presiona Espacio para hacer que la tortuga salte y esquive obstáculos.

Reiniciar: Si pierdes, haz clic en el botón "Reset" para reiniciar el juego.

Requisitos del sistema
Sistema operativo: Windows 7 o superior.

.NET Framework: Versión 4.7.2 o superior.

Recursos: Asegúrate de que los archivos de recursos (imágenes y sonidos) estén en la carpeta assets.

Estructura del proyecto
MainWindow.xaml: Contiene la interfaz gráfica del juego.

MainWindow.xaml.cs: Contiene la lógica del juego.

assets/: Carpeta que contiene las imágenes y sonidos utilizados en el juego.

Créditos
Desarrollado por: Sergio Gracia Correia.

Recursos gráficos y de sonido: Los assets gráficos han sido creados por mi con Aseprite y Photodraw. Los sonidos han sido extraídos de https://pixabay.com/es/sound-effects/

Inspiración: El juego es un endless runner clásico inspirado en el juego del dinosaurio de Google. Lo hice en Unity primero y luego hice un port a WPF para aprender esta tecnología y  además aprender como funciona Github, que nunca lo había usado.

Licencia
Este proyecto está bajo la licencia MIT. Siéntete libre de usarlo, modificarlo y distribuirlo.
