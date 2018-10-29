/*
Ported from One Lone Coder Snake to C#
Check out the original at https://github.com/OneLoneCoder/videos/blob/master/OneLoneCoder_Snake.cpp
*/

using System;
using System.Collections.Generic;
using PixelEngine;

namespace Examples
{
	public class Snake : Game
	{
		private List<SnakeSegment> snake; // Store all segments of snake

		// Coordinates of the food
		private int foodX;
		private int foodY;

		private int score; // Player's score

		private int dir; // Direction of snake

		private bool dead; // Is the snake dead?
		private bool started; // Has the game been started?

		static void Main(string[] args)
		{
			// Create an instance
			Snake s = new Snake();
			// Construct the game
			s.Construct(50, 50, 10, 10, 30);
			// Start the game
			s.Start();
		}

		// A part of the snake
		private struct SnakeSegment
		{
			public SnakeSegment(int x, int y) : this()
			{
				this.X = x;
				this.Y = y;
			}

			public int X { get; private set; } // X location
			public int Y { get; private set; } // Y location
		}

		// Set the title of the window
		public Snake() => AppName = "SNAKE!";

		// Start the game
		public override void OnCreate()
		{
			// Uncomment to make the game fullscreen
			//Enable(Subsystem.Fullscreen);

			Enable(Subsystem.HrText);

			Reset();
		}

		// Reset all fields
		private void Reset()
		{
			// Init and make the snake
			snake = new List<SnakeSegment>();
			for (int i = 0; i < 9; i++)
				snake.Add(new SnakeSegment(i + 20, 15));

			// Set the variables to default values
			foodX = 30;
			foodY = 15;
			score = 0;
			dir = 3;
			dead = false;

			Seed();
		}

		public override void OnUpdate(float elapsed)
		{
			CheckStart();
			UpdateSnake();
			DrawGame();
		}

		// Draw the game
		private void DrawGame()
		{
			// Clear the screen
			Clear(Pixel.Presets.Black);

			if (started) // Inform the player of their score
				DrawTextHr(new Point(15, 15), "Score: " + score, Pixel.Presets.Green, 2);
			else // Inform the player to start by pressing enter
				DrawTextHr(new Point(15, 15), "Press Enter To Start", Pixel.Presets.Green, 2);

			// Draw the border
			DrawRect(new Point(0, 0), ScreenWidth - 1, ScreenHeight - 1, Pixel.Presets.Grey);

			// Render snake
			for (int i = 1; i < snake.Count; i++)
				Draw(snake[i].X, snake[i].Y, dead ? Pixel.Presets.Blue : Pixel.Presets.Yellow);

			// Draw snake head
			Draw(snake[0].X, snake[0].Y, dead ? Pixel.Presets.Green : Pixel.Presets.Magenta);

			// Draw food
			Draw(foodX, foodY, Pixel.Presets.Red);
		}

		// Update the snake's position
		private void UpdateSnake()
		{
			// End game if snake is dead
			if (dead)
				started = false;

			// Turn right
			if (GetKey(Key.Right).Pressed)
			{
				dir++;
				if (dir == 4)
					dir = 0;
			}

			// Turn left
			if (GetKey(Key.Left).Pressed)
			{
				dir--;
				if (dir == -1)
					dir = 3;
			}

			if (started)
			{
				// Move in the direction
				switch (dir)
				{
					case 0: // UP
						snake.Insert(0, new SnakeSegment(snake[0].X, snake[0].Y - 1));
						break;
					case 1: // RIGHT
						snake.Insert(0, new SnakeSegment(snake[0].X + 1, snake[0].Y));
						break;
					case 2: // DOWN
						snake.Insert(0, new SnakeSegment(snake[0].X, snake[0].Y + 1));
						break;
					case 3: // LEFT
						snake.Insert(0, new SnakeSegment(snake[0].X - 1, snake[0].Y));
						break;
				}

				// Pop the tail
				snake.RemoveAt(snake.Count - 1);

				CheckCollision();
			}
		}

		// Check for snake's collision
		private void CheckCollision()
		{
			// Check collision with food
			if (snake[0].X == foodX && snake[0].Y == foodY)
			{
				score++;
				RandomizeFood();

				snake.Add(new SnakeSegment(snake[snake.Count - 1].X, snake[snake.Count - 1].Y));
			}

			// Check wall collision
			if (snake[0].X <= 0 || snake[0].X >= ScreenWidth || snake[0].Y <= 0 || snake[0].Y >= ScreenHeight - 1)
				dead = true;

			// Check self collision
			for (int i = 1; i < snake.Count; i++)
				if (snake[i].X == snake[0].X && snake[i].Y == snake[0].Y)
					dead = true;
		}

		// Check if the game is started
		private void CheckStart()
		{
			if (!started)
			{
				// Check if game has to be started
				if (GetKey(Key.Enter).Pressed)
				{
					Reset();
					started = true;
				}
			}
		}

		// Set random location for food
		private void RandomizeFood()
		{
			// Loop while the food is not on empty cell
			while (GetScreenPixel(foodX, foodY) != Pixel.Presets.Black)
			{
				// Set food to random point
				foodX = Random(ScreenWidth);
				foodY = Random(ScreenHeight);
			}
		}
	}
}