namespace NanoWar.GameClient
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct HeaderPacket
    {
        public PacketType Type;

        public int Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct InLobbyPacket
    {
        public int Id;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
        public string Name;

        public int MaxNumberOfPlayers;

        public int NumberOfPlayers;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct OutNickName
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
        public string NickName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct OutCreateLobby
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 60)]
        public string Name;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct InJoinLobbyResult
    {
        public int Success;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct OutGetPlayerListLobby
    {
        public int LobbyId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MovableCellInfoOut
    {
        public int FromCellId;

        public int ToCellId;

        public int Units;

        public int StatTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MovableCellInfoIn
    {
        public int Id;

        public int PlayerId;

        public int FromCellId;

        public int ToCellId;

        public int Units;

        public int StatTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct CellInfo
    {
        public int PlayerId;

        public int CellId;

        public int Units;

        public int Type;

        public int PositionX;

        public int PositionY;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct ShortCellInfo
    {
        public int Id;

        public int OwnerId;

        public int Units;
    }

    internal enum PacketType
    {
        InitConnection, 

        GetLobbyList, 

        LobbyList, 

        SetNickname, 

        CreateLobby, 

        JoinLobby, 

        GetPlayerListLobby, 

        LeaveLobby, 

        ReadyToPlay, 

        SendCell, 

        SendGeneratedCells, 

        RecieveGeneratedCells, 

        CellReachedTarget
    }
}