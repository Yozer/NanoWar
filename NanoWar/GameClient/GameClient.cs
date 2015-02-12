namespace NanoWar.GameClient
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    using NanoWar.States.GameStateStart;

    using SFML.System;

    public class GameClient
    {
        private static GameClient _instance;

        private static object _sendLocker = new object();

        private BinaryReader _binaryReader;

        private BinaryWriter _binaryWriter;

        private NetworkStream _networkStream;

        private TcpClient _tcpClient;

        public string Ip;

        public int Port = 6881;

        public static GameClient Instance
        {
            get
            {
                return _instance ?? (_instance = new GameClient());
            }
        }

        public static bool Connected { get; private set; }

        public static void Dispose()
        {
            if (_instance != null && Instance._tcpClient != null)
            {
                Instance._tcpClient.Close();
                Instance._tcpClient = null;
                Connected = false;
            }
        }

        private Dictionary<int, Lobby> ReadLobbyList()
        {
            var header = Read<HeaderPacket>();
            if (header.Type == PacketType.LobbyList)
            {
                var lobbies = new Dictionary<int, Lobby>();
                var count = Read<int>();

                for (var i = 0; i < count; i++)
                {
                    var lobbyPacket = Read<InLobbyPacket>();
                    lobbies.Add(
                        lobbyPacket.Id, 
                        new Lobby(
                            lobbyPacket.Id, 
                            lobbyPacket.Name, 
                            lobbyPacket.MaxNumberOfPlayers, 
                            lobbyPacket.NumberOfPlayers));
                }

                return lobbies;
            }

            throw new ArgumentException();
        }

        public Dictionary<int, Lobby> UpdateLobbyList()
        {
            Write(new HeaderPacket { Type = PacketType.GetLobbyList, Length = 0 });
            return ReadLobbyList();
        }

        private void Write(object message)
        {
            var data = RawSerializer.RawSerialize(message);

            lock (_networkStream)
            {
                _binaryWriter.Write(data);
            }
        }

        public T Read<T>(int size = -1)
        {
            if (size == -1)
            {
                size = Marshal.SizeOf(typeof(T));
            }

            var data = new byte[size];
            lock (_networkStream)
            {
                _binaryReader.Read(data, 0, size);
            }

            return RawSerializer.RawDeserialize<T>(data);
        }

        public Dictionary<int, PlayerInstance> UpdatePlayersInLobby()
        {
            Write(new HeaderPacket { Type = PacketType.GetPlayerListLobby, Length = 0 });
            var count = Read<int>();

            var players = new Dictionary<int, PlayerInstance>();
            for (var i = 0; i < count; i++)
            {
                var nick = Read<OutNickName>().NickName;
                var player = new PlayerInstance(nick) { IsReady = Read<int>() != 0, Id = Read<int>() };
                players.Add(player.Id, player);
            }

            if (count == -1)
            {
                throw new LobbyClosedException();
            }

            return players;
        }

        public Lobby CreateLobby()
        {
            Write(new HeaderPacket { Type = PacketType.CreateLobby, Length = Marshal.SizeOf(typeof(OutCreateLobby)) });
            Write(new OutCreateLobby { Name = "Gra " + Game.Instance.Player.Name });

            var lobbyPacket = Read<InLobbyPacket>();
            return new Lobby(
                lobbyPacket.Id, 
                lobbyPacket.Name, 
                lobbyPacket.MaxNumberOfPlayers, 
                lobbyPacket.NumberOfPlayers);
        }

        public void JoinLobby(int id)
        {
            Write(new HeaderPacket { Type = PacketType.JoinLobby, Length = Marshal.SizeOf(typeof(int)) });
            Write(id);
            var result = Read<InJoinLobbyResult>();
            if (result.Success == 0)
            {
                throw new InvalidOperationException("Pełne lobby!");
            }
        }

        public int PeekEvent()
        {
            lock (_networkStream)
            {
                if (_networkStream.CanRead && !_networkStream.DataAvailable)
                {
                    return -1;
                }

                try
                {
                    return Read<int>();
                }
                catch
                {
                    return -1;
                }
            }
        }

        public void SetNickName(string nick)
        {
            Write(new HeaderPacket { Type = PacketType.SetNickname, Length = Marshal.SizeOf(typeof(OutNickName)) });
            Write(new OutNickName { NickName = nick });
        }

        public int InitConnection()
        {
            if (Connected)
            {
                return Game.Instance.Player.Id;
            }

            _tcpClient = new TcpClient();
            _tcpClient.Connect(Ip, Port);

            _networkStream = _tcpClient.GetStream();
            _binaryWriter = new BinaryWriter(_networkStream);
            _binaryReader = new BinaryReader(_networkStream);

            var header = new HeaderPacket { Type = PacketType.InitConnection, Length = 0 };
            Write(header);
            var id = Read<int>();
            SetNickName(Game.Instance.Player.Name);
            Connected = true;
            return id;
        }

        public void LeaveLobby()
        {
            Write(new HeaderPacket { Type = PacketType.LeaveLobby, Length = 0 });
        }

        public void MarkAsReady()
        {
            Write(new HeaderPacket { Type = PacketType.ReadyToPlay, Length = 0 });
        }

        public List<MovableCellInfoIn> ReadCellsInfo()
        {
            var count = Read<int>() / Marshal.SizeOf(typeof(MovableCellInfoIn));

            var cells = new List<MovableCellInfoIn>(count);

            for (var i = 0; i < count; i++)
            {
                var cell = Read<MovableCellInfoIn>();
                cells.Add(cell);
            }

            return cells;
        }

        public void SendGeneratedCells(List<Cell> cells)
        {
            Write(new HeaderPacket {Type = PacketType.SendGeneratedCells, Length = Marshal.SizeOf(typeof(CellInfo)) * cells.Count});
            foreach (Cell cell in cells)
            {
                var cellInfo = new CellInfo
                {
                    CellId = cell.Id,
                    PlayerId = cell.Player == null ? -1 : cell.Player.Id,
                    PositionX = (int)cell.Position.X,
                    PositionY = (int)cell.Position.Y,
                    Type = (int)cell.Type,
                    Units = cell.Units
                };
                Write(cellInfo);
            }
            //var buffor = new byte[Marshal.SizeOf(typeof(CellInfo)) * cells.Count + Marshal.SizeOf(typeof(HeaderPacket))];
            //Array.Copy(BitConverter.GetBytes((int)PacketType.SendGeneratedCells), 0, buffor, 0, 4);
            //Array.Copy(BitConverter.GetBytes(Marshal.SizeOf(typeof(CellInfo)) * cells.Count), 0, buffor, 4, 4);

            //var position = 8;

            //foreach (var cell in cells)
            //{
            //    var cellInfo = new CellInfo
            //                       {
            //                           CellId = cell.Id, 
            //                           PlayerId = cell.Player == null ? -1 : cell.Player.Id, 
            //                           PositionX = (int)cell.Position.X, 
            //                           PositionY = (int)cell.Position.Y, 
            //                           Type = (int)cell.Type, 
            //                           Units = cell.Units
            //                       };

            //    Array.Copy(RawSerializer.RawSerialize(cellInfo), 0, buffor, position, Marshal.SizeOf(typeof(CellInfo)));
            //    position += Marshal.SizeOf(typeof(CellInfo));
            //}

            //lock (_networkStream)
            //{
            //    _binaryWriter.Write(buffor);
            //    _binaryWriter.Flush();
            //}
        }

        public List<Cell> ReadGeneratedCells()
        {
            var header = Read<HeaderPacket>();

            if (header.Type == PacketType.RecieveGeneratedCells)
            {
                var count = header.Length / Marshal.SizeOf(typeof(CellInfo));
                var cells = new List<Cell>(count);

                for (var i = 0; i < count; i++)
                {
                    var cellInfo = Read<CellInfo>();
                    var cell = new Cell(
                        new Vector2f(cellInfo.PositionX, cellInfo.PositionY), 
                        cellInfo.Units, 
                        (Cell.CellSizeEnum)cellInfo.Type, 
                        cellInfo.CellId)
                                   {
                                       Player =
                                           cellInfo.PlayerId == -1
                                               ? null
                                               : Game.Instance.AllPlayers[cellInfo.PlayerId]
                                   };
                    cells.Add(cell);
                }

                return cells;
            }

            throw new ArgumentException();
        }

        public void SendCells(List<UnitCell> movableCellList)
        {
            lock (_sendLocker)
            {
                if (movableCellList.Count == 0)
                {
                    return;
                }

                Write(
                    new HeaderPacket
                        {
                            Type = PacketType.SendCell, 
                            Length = movableCellList.Count * Marshal.SizeOf(typeof(MovableCellInfoOut))
                        });

                foreach (var movableCell in movableCellList)
                {
                    var cellInfo = new MovableCellInfoOut
                                       {
                                           ToCellId = movableCell.TargetCell.Id, 
                                           FromCellId = movableCell.SourceCell.Id, 
                                           Units = movableCell.Units, 
                                           StatTime = 0
                                       };

                    Write(cellInfo);
                }
            }
        }

        public long GetServerTime()
        {
            var buffer = new byte[8];
            lock (_networkStream)
            {
                _binaryReader.Read(buffer, 0, 8);
            }

            return BitConverter.ToInt64(buffer, 0);
        }

        public long GetTimeGameStarts()
        {
            var buffer = new byte[8];
            lock (_networkStream)
            {
                _binaryReader.Read(buffer, 0, 8);
            }

            return BitConverter.ToInt64(buffer, 0);
        }

        public void CellsReachedTarget(int reachedTarget)
        {
            lock (_sendLocker)
            {
                Write(new HeaderPacket { Type = PacketType.CellReachedTarget, Length = sizeof(int) });
                Write(reachedTarget);
            }
        }
    }

    public class LobbyClosedException : Exception
    {
    }
}