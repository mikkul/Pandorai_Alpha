using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Pandorai.Mechanics
{
    public delegate void KeyPressedHandler(Keys key);
    public delegate void MouseHandler(Vector2 mousePos);

	public class InputManager
	{
        Game1 game;

        public event KeyPressedHandler SingleKeyPress;
        public event MouseHandler LMBClick;
        public event MouseHandler LMBRelease;       
        public event MouseHandler RMBClick;
        public event MouseHandler RMBRelease;
        public event MouseHandler MouseMove;

        public KeyboardState PreviousKeyboardState;
        public KeyboardState KeyboardState;

        public MouseState PreviousMouseState;
        public MouseState MouseState;

        public Vector2 MousePos;

        public InputManager(Game1 _game)
		{
            game = _game;
		}

        public void Update()
		{
            KeyboardState = Keyboard.GetState();
            MouseState = Mouse.GetState();

            MousePos = MouseState.Position.ToVector2();

            Keys[] pressedKeys = KeyboardState.GetPressedKeys();

			foreach (var key in pressedKeys)
			{
                if(GetSingleKeyPress(key))
				{
                    SingleKeyPress?.Invoke(key);
				}
			}

            if(!MouseState.Position.Equals(PreviousMouseState.Position))
			{
                MouseMove?.Invoke(MousePos);
			}

            if(GetSingleMouseButtonPress(0))
			{
                LMBClick?.Invoke(MousePos);
			}
            else if(GetMouseButtonRelease(0))
			{
                LMBRelease?.Invoke(MousePos);
			}
            else if (GetSingleMouseButtonPress(1))
            {
                RMBClick?.Invoke(MousePos);
            }
            else if (GetMouseButtonRelease(1))
            {
                RMBRelease?.Invoke(MousePos);
            }

            PreviousKeyboardState = KeyboardState;
            PreviousMouseState = MouseState;
		}

		public CustomInput GetPlayerInput()
		{
            Vector2 movement = Vector2.Zero;
            InputAction action = InputAction.None;

            if (KeyboardState.IsKeyDown(Keys.W) || KeyboardState.IsKeyDown(Keys.Up) || KeyboardState.IsKeyDown(Keys.NumPad8))
            {
                movement.Y = -1;
            }
            else if (KeyboardState.IsKeyDown(Keys.S) || KeyboardState.IsKeyDown(Keys.Down) || KeyboardState.IsKeyDown(Keys.NumPad2))
            {
                movement.Y = 1;
            }
            if (KeyboardState.IsKeyDown(Keys.A) || KeyboardState.IsKeyDown(Keys.Left) || KeyboardState.IsKeyDown(Keys.NumPad4))
            {
                movement.X = -1;
            }
            else if (KeyboardState.IsKeyDown(Keys.D) || KeyboardState.IsKeyDown(Keys.Right) || KeyboardState.IsKeyDown(Keys.NumPad6))
            {
                movement.X = 1;
            }

            if (KeyboardState.IsKeyDown(Keys.NumPad7))
            {
                movement.Y = -1;
                movement.X = -1;
            }
            else if (KeyboardState.IsKeyDown(Keys.NumPad9))
            {
                movement.Y = -1;
                movement.X = 1;
            }
            else if (KeyboardState.IsKeyDown(Keys.NumPad1))
            {
                movement.X = -1;
                movement.Y = 1;
            }
            else if (KeyboardState.IsKeyDown(Keys.NumPad3))
            {
                movement.X = 1;
                movement.Y = 1;
            }            

            if(KeyboardState.IsKeyDown(Keys.Space))
			{
                action = InputAction.None;
			}

            return new CustomInput
            {
                Movement = movement,
                Action = action
            };
		}

        public bool IsHoldingKey(Keys key)
		{
            return KeyboardState.IsKeyDown(key);
		}

        public bool GetSingleKeyPress(Keys key)
		{
            return KeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
		}

        public bool GetSingleMouseButtonPress(int which)
		{
            if(which == 0)
			{
                return MouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton != ButtonState.Pressed;
			}
            else if (which == 1)
            {
                return MouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton != ButtonState.Pressed;
            }
            else
			{
                return false;
			}
        }

        public bool GetMouseButtonRelease(int which)
        {
            if (which == 0)
            {
                return MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;
            }
            else if (which == 1)
            {
                return MouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;
            }
            else
            {
                return false;
            }
        }
    }
}
