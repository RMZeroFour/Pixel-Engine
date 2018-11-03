# Pixel-Engine

A simple game engine for C# using OpenGL (earlier DirectX). Inspired by the olcPixelGameEngine.

This game engine was ported from C++ to C# in the hopes that it will help people develop their ideas faster and not waste their time in creating the same graphics code and game loop again and again.

This engine uses OpenGL and the .NET Framework, so it's quite fast due to OpenGL being hardware accelerated.

## Getting Started

1. Grab the library from the Libraries folder.
2. Create a new C# project.
  2.1. Add a reference to the library in your project.
4. Subclass the Game class in the engine.
  4.1. Add a Main method.
    4.1.1. Instantiate the class.
    4.1.2. Call its Construct method.
    4.1.3. Call its Start method.
  4.2. Override the required methods.
5. Start the application.

```C#
using PixelEngine;

namespace Examples
{
	public class RandomPixels : Game
	{
		static void Main(string[] args)
		{
			// Create an instance
			RandomPixels rp = new RandomPixels();
      
      // Construct the 100x100 game window with 5x5 pixels
			rp.Construct(100, 100, 5, 5); 
      
      // Start and show a window
			rp.Start(); 
		}

	  public override void OnUpdate(float elapsed)
		{
			// Loop through all the pixels
			for (int i = 0; i < ScreenWidth; i++)
				for (int j = 0; j < ScreenHeight; j++)
					Draw(i, j, Pixel.Random()); // Draw a random pixel
		}
	}
}
```

### Prerequisites

There are no additional dependencies outside the Windows Api, which is present in all windows installations.

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [Windows Api](https://docs.microsoft.com/en-us/windows/desktop/apiindex/windows-api-list) - The core system to display windows
* [OpenGL](https://www.opengl.org/) - The rendering system

## Acknowledgments

Check out the [olcPixelGameEngine](https://github.com/OneLoneCoder/olcPixelGameEngine) for C++ and its creator, [Javidx9](https://www.youtube.com/channel/UC-yuWVUplUJZvieEligKBkA), his website [OneLoneCoder](https://onelonecoder.com/), and the [OLC-3 License](https://github.com/DevChrome/Pixel-Engine/blob/master/Licences.txt) in the original project.
