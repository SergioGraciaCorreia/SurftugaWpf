SURFTUGA WPF
🌊 Welcome to Surftuga WPF!
This is a simple game developed in WPF (Windows Presentation Foundation) where you control a surfing turtle that must dodge obstacles while the background moves faster and faster.

🎮 Game Description
The goal of the game is to survive as long as possible by dodging randomly appearing obstacles. The background speed increases over time, making the game more challenging. Aim for the highest score!

🛠️ Main Methods
Here are the key methods of the game:

1. Initialization
MainWindow(): Constructor for the main window. Initializes components and sets up the game.

ConfigurarJuego(): Configures timers, rendering quality, and other initial settings.

InicializarSonidos(): Initializes game sounds (background, jump, music, etc.).

2. Game Logic
OnRendering(object sender, EventArgs e): Executes on each frame. Controls background movement, obstacle generation, and score updates.

MoverFondo(double deltaTime): Moves background images to create a scrolling effect.

GenerarObstaculos(double deltaTime): Generates obstacles at random intervals.

MoverObstaculos(double deltaTime): Moves obstacles to the left and checks for collisions with the turtle.

DetectColision(Image tortuga, Image obstaculo): Detects collisions between the turtle and obstacles.

3. Player Actions
Window_KeyDown(object sender, KeyEventArgs e): Handles player key presses (Enter to start, Space to jump).

PerformJump(): Makes the turtle jump and plays the corresponding sound.

StartCooldown(): Starts a cooldown after a jump to prevent spamming.

4. Game Over
GameOver(): Stops the game, plays the "death" sound, and shows the Game Over screen after a short delay.

MostrarPantallaGameOver(): Displays the Game Over screen with the final score.

ReiniciarButton_Click(object sender, RoutedEventArgs e): Restarts the game when the player clicks the "Reset" button.

5. Helper Methods
ActualizarPuntuacion(): Updates the score text on the screen.

SpawnObstaculo(string imagenLejana): Creates and positions a new obstacle on the Canvas.

FondoImage_Loaded(object sender, RoutedEventArgs e): Positions background images when loaded.

AnimationTimer_Tick(object sender, EventArgs e): Alternates between turtle animation frames.

🎮 How to Play
Start the game: Press Enter on the start screen.

Jump: Press Space to make the turtle jump and dodge obstacles.

Reset: If you lose, click the "Reset" button to restart the game.

⚙️ System Requirements
OS: Windows 7 or higher.

.NET Framework: Version 4.7.2 or higher.

Resources: Ensure resource files (images and sounds) are in the assets folder.

📂 Project Structure
MainWindow.xaml: Contains the game's graphical interface.

MainWindow.xaml.cs: Contains the game logic.

assets/: Folder containing images and sounds used in the game.

🎨 Credits
Developed by: Sergio Gracia Correia.

Graphics and sound resources: Assets created by me using Aseprite and Photodraw. Sounds extracted from Pixabay.

Inspiration: The game is a classic endless runner inspired by Google's Dino Game. I first made it in Unity and then ported it to WPF to learn this technology and GitHub, which I had never used before.

📜 License
This project is licensed under the MIT License. Feel free to use, modify, and distribute it.
