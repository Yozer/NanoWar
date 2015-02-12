namespace NanoWar.States.GameStateMenu
{
    using System;
    using System.Xml.Linq;

    using SFML.Graphics;
    using SFML.System;

    internal class MenuManager : Drawable
    {
        private Sprite _background;

        private Menu _menu;

        public MenuManager(string path)
        {
            _menu = CreateMenu(path);
            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            target.Draw(_background);
            target.Draw(_menu);
        }

        private void menu_OnMenuChange(object sender, EventArgs e)
        {
            Game.Instance.AudioManager.PlaySound("menu/click_sound");
            if (_menu.Items[_menu.ItemNumber].LinkType == "screen")
            {
                Game.Instance.StateMachine.PushState(_menu.Path);
            }
            else if (_menu.Items[_menu.ItemNumber].LinkType == "menu")
            {
                _menu.OnMenuChange -= menu_OnMenuChange;
                _menu.Dispose();
                _menu = CreateMenu(_menu.Path);
            }
        }

        public Menu CreateMenu(string path)
        {
            var doc =
                XDocument.Load(SfmlFactories.StreamAttributeParse(ResourceManager.Instance.GetAssetNameByPath(path)));

            var menu = new Menu();
            var fontSize = (uint)SfmlFactories.IntegerAttributeParse(doc.Element("menu").Element("font"), "size");

            foreach (var xElement in doc.Element("menu").Elements("item"))
            {
                var menuItem = new MenuItem
                                   {
                                       DisplayedString = xElement.Element("text").Value, 
                                       Font =
                                           ResourceManager.Instance[doc.Element("menu").Element("font").Value] as
                                           Font, 
                                       CharacterSize = fontSize
                                   };

                menuItem.Origin = new Vector2f(
                    menuItem.GetLocalBounds().Left + menuItem.GetLocalBounds().Width / 2, 
                    menuItem.GetLocalBounds().Top);
                menuItem.LinkType = xElement.Element("link").Attribute("type").Value;
                menuItem.LinkPath = xElement.Element("link").Value;
                menu.Items.Add(menuItem);
            }

            menu.Path = path;
            menu.AlignMenuItems();
            menu.OnMenuChange += menu_OnMenuChange;

            return menu;
        }

        public void Update(float delta)
        {
            _menu.Update(delta);
        }

        public void Dispose()
        {
            _menu.Dispose();
        }
    }
}