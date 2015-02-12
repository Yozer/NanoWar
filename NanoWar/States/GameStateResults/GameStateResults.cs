namespace NanoWar.States.GameStateResults
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using NanoWar.Shapes;
    using NanoWar.States.GameStateMenu;

    using SFML.Graphics;
    using SFML.System;
    using SFML.Window;

    internal class GameStateResults : GameState
    {
        private Sprite _background;

        private List<Text> _bestResults = new List<Text>();

        private Button _buttonBack;

        public GameStateResults()
        {
            _background = new Sprite(ResourceManager.Instance["menu/background"] as Texture);

            Game.Instance.AudioManager.PauseAllBackground();
            Game.Instance.AudioManager.PlaySound("menu/background_music", true);
            Game.Instance.Window.MouseButtonReleased += Window_MouseButtonReleased;
            Game.Instance.Window.KeyReleased += KeyReleased;

            LoadResults();
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.Escape)
            {
                Game.Instance.StateMachine.PushState(new GameStateMenu());
            }
        }

        private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            _buttonBack.HandleInput(e);
        }

        private Text CreateTextItem(string text, Font font, uint size, float xPos, float yPos)
        {
            var textItem = new Text(text, font, size);
            textItem.Origin = new Vector2f(
                textItem.GetLocalBounds().Left + textItem.GetLocalBounds().Width / 2, 
                textItem.GetLocalBounds().Top + textItem.GetLocalBounds().Height / 2);
            textItem.Position = new Vector2f(xPos, yPos);

            return textItem;
        }

        private string GetOdmiana(int value)
        {
            if (value == 1)
            {
                return "a";
            }

            if (value % 10 >= 2 && value % 10 <= 4 && (value >= 20 || value <= 10))
            {
                return "y";
            }

            return string.Empty;
        }

        private void LoadResults()
        {
            var font = ResourceManager.Instance["fonts/bebas_neue"] as Font;
            List<string> lines = null;

            if (File.Exists(Game.BestResultsFileName))
            {
                lines = File.ReadAllLines(Game.BestResultsFileName, Encoding.UTF8).ToList();
                lines.RemoveAll(string.IsNullOrEmpty);
            }

            float yPos = 0;

            if (lines == null || lines.Count == 0)
            {
                _bestResults.Add(
                    CreateTextItem("Brak wyników!", font, 70, Game.Instance.Width / 2, Game.Instance.Height / 2));
                yPos = Game.Instance.Height / 2 + _bestResults.Last().GetGlobalBounds().Height + 60f;
            }
            else
            {
                _bestResults.Add(CreateTextItem("Nick - Czas", font, 40, Game.Instance.Width / 2, yPos));

                yPos = Game.Instance.Height / 2
                       - (lines.Count + 2) * (_bestResults.Last().GetGlobalBounds().Height + 20f) / 2;
                _bestResults.Last().Position = new Vector2f(Game.Instance.Width / 2, yPos);

                yPos += _bestResults.Last().GetGlobalBounds().Height + 45f;

                var count = 1;
                foreach (var line in lines)
                {
                    var splited = line.Split(':');

                    var time = string.Empty;
                    var timeSpan = TimeSpan.FromMilliseconds(Convert.ToDouble(splited[1]));

                    var minutes = (int)timeSpan.TotalMinutes;
                    var seconds = timeSpan.Seconds;

                    if (minutes != 0)
                    {
                        time += minutes + " minut" + GetOdmiana(minutes) + " ";
                    }

                    if (seconds != 0)
                    {
                        time += seconds + " sekund" + GetOdmiana(seconds);
                    }

                    time = time.Trim();

                    _bestResults.Add(
                        CreateTextItem(
                            count++ + ". " + splited[0] + " - " + time, 
                            font, 
                            40, 
                            Game.Instance.Width / 2, 
                            yPos));
                    yPos += _bestResults.Last().GetGlobalBounds().Height + 20f;
                }
            }

            _buttonBack = new Button("Wróć", font, 40, new Vector2f(Game.Instance.Width / 2, yPos + 45), 0.9f);
            _buttonBack.OnClick += (sender, args) => Game.Instance.StateMachine.PushState(new GameStateMenu());
        }

        public override void Draw()
        {
            Game.Instance.Window.Draw(_background);
            Game.Instance.Window.Draw(_buttonBack);
            _bestResults.ForEach(Game.Instance.Window.Draw);
        }

        public override void Update(float delta)
        {
            _buttonBack.Update(delta);
        }

        public override void HandleInput()
        {
        }

        public override void Dispose()
        {
            _background.Dispose();
            _buttonBack.Dispose();
            _bestResults.ForEach(t => t.Dispose());
            Game.Instance.Window.MouseButtonReleased -= Window_MouseButtonReleased;
        }
    }
}