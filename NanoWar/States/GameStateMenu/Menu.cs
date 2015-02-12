namespace NanoWar.States.GameStateMenu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    public class Menu : Drawable
    {
        private string _path;

        public List<MenuItem> Items = new List<MenuItem>();

        public Menu()
        {
            ItemNumber = 0;

            Game.Instance.Window.KeyPressed += WindowKeyPressed;
            Game.Instance.Window.MouseButtonReleased += WindowMouseButtonReleased;
        }

        public int ItemNumber { get; private set; }

        public string Path
        {
            get
            {
                return _path;
            }

            set
            {
                _path = value;
                if (OnMenuChange != null)
                {
                    OnMenuChange(this, null);
                }
            }
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            Items.ForEach(target.Draw);
        }

        public event EventHandler OnMenuChange;

        public void AlignMenuItems()
        {
            const float padding = 15;
            var totalHeight = Items.Sum(menuItem => menuItem.GetLocalBounds().Height + padding);
            var currentHeight = (Game.Instance.Height - totalHeight) / 2;

            foreach (var menuItem in Items)
            {
                menuItem.Position = new Vector2f(Game.Instance.Width / 2, currentHeight);
                currentHeight += menuItem.GetLocalBounds().Height + padding;
            }
        }

        public void Update(float delta)
        {
            var selectedByMouse = false;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].GetGlobalBounds()
                    .Contains(Mouse.GetPosition(Game.Instance.Window).X, Mouse.GetPosition(Game.Instance.Window).Y))
                {
                    Items[i].IsActive = selectedByMouse = true;
                    ItemNumber = i;
                }
                else
                {
                    Items[i].IsActive = false;
                }
            }

            // if we are not selecting any menu item by mouse so select last known one;
            if (!selectedByMouse)
            {
                Items[ItemNumber].IsActive = true;
            }

            // update menu items
            Items.ForEach(t => t.Update(delta));
        }

        public void Dispose()
        {
            Game.Instance.Window.KeyPressed -= WindowKeyPressed;
            Game.Instance.Window.MouseButtonReleased -= WindowMouseButtonReleased;
            Items.ForEach(t => t.Dipose());
        }

        private void WindowKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Up)
            {
                ItemNumber--;
            }
            else if (e.Code == Keyboard.Key.Down)
            {
                ItemNumber++;
            }

            if (ItemNumber >= Items.Count)
            {
                ItemNumber = 0;
            }
            else if (ItemNumber < 0)
            {
                ItemNumber = Items.Count - 1;
            }

            if (e.Code == Keyboard.Key.Space || e.Code == Keyboard.Key.Return)
            {
                Path = Items[ItemNumber].LinkPath;
            }
            else if (e.Code == Keyboard.Key.Escape)
            {
                // stupid assumptions - last option in menu doesn't have to be "Back" option...
                ItemNumber = Items.Count - 1;
                Path = Items.Last().LinkPath;
            }
        }

        private void WindowMouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            var mousePoint = new Vector2f(e.X, e.Y);
            for (var i = 0; i < Items.Count; i++)
            {
                if (!Items[i].GetGlobalBounds().Contains(mousePoint))
                {
                    continue;
                }

                ItemNumber = i;
                Path = Items[i].LinkPath;
                break;
            }
        }
    }
}