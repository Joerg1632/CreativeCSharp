using System;
using System.Collections.Generic;
using System.Linq;

namespace Battleship
{
    public enum Orientation { Horizontal, Vertical }
    public enum ShotResult { Miss, Hit, Sunk, AlreadyShot, Invalid }
    public enum CellState { Empty, Ship, Hit, Miss }

    public record struct Position(int Row, int Col)
    {
        public override string ToString() => $"{(char)('A' + Row)}{Col + 1}";
        public bool InBounds(int size) => Row >= 0 && Col >= 0 && Row < size && Col < size;
    }

    public enum ShipType
    {
        Battleship4 = 4,
        Cruiser3 = 3,
        Destroyer2 = 2,
        Patrol1 = 1
    }

    public class Ship
    {
        public ShipType Type { get; }
        public List<Position> Cells { get; }
        private HashSet<Position> hits = new();

        public Ship(ShipType type, IEnumerable<Position> cells)
        {
            Type = type;
            Cells = cells.ToList();
        }

        public bool Occupies(Position p) => Cells.Contains(p);
        public void RegisterHit(Position p) { if (Occupies(p)) hits.Add(p); }
        public bool IsSunk() => hits.Count >= Cells.Count;
    }

    public class Board
    {
        public int Size { get; }
        private CellState[,] grid;
        private List<Ship> ships = new();
        private Dictionary<Position, Ship> posToShip = new();

        public Board(int size = 10)
        {
            Size = size;
            grid = new CellState[size, size];
            for (int r = 0; r < size; r++) for (int c = 0; c < size; c++) grid[r, c] = CellState.Empty;
        }

        public IReadOnlyList<Ship> Ships => ships.AsReadOnly();

        public bool CanPlaceShip(Position start, Orientation orient, int length, bool forbidTouching = true)
        {
            var positions = GetPositionsForShip(start, orient, length);
            if (positions == null) return false;
            foreach (var p in positions)
            {
                if (!p.InBounds(Size)) return false;
                if (grid[p.Row, p.Col] != CellState.Empty) return false;
                if (forbidTouching)
                {
                    // check neighbors
                    for (int dr = -1; dr <= 1; dr++)
                        for (int dc = -1; dc <= 1; dc++)
                        {
                            var np = new Position(p.Row + dr, p.Col + dc);
                            if (np.InBounds(Size) && grid[np.Row, np.Col] == CellState.Ship) return false;
                        }
                }
            }
            return true;
        }

        public bool PlaceShip(Position start, Orientation orient, ShipType type, bool forbidTouching = true)
        {
            int length = (int)type;
            var positions = GetPositionsForShip(start, orient, length);
            if (positions == null) return false;
            if (!CanPlaceShip(start, orient, length, forbidTouching)) return false;

            var ship = new Ship(type, positions);
            ships.Add(ship);
            foreach (var p in positions)
            {
                grid[p.Row, p.Col] = CellState.Ship;
                posToShip[p] = ship;
            }
            return true;
        }

        private IEnumerable<Position>? GetPositionsForShip(Position start, Orientation orient, int length)
        {
            var list = new List<Position>();
            for (int i = 0; i < length; i++)
            {
                var p = orient == Orientation.Horizontal ? new Position(start.Row, start.Col + i) : new Position(start.Row + i, start.Col);
                list.Add(p);
            }
            return list;
        }

        public ShotResult ReceiveShot(Position p)
        {
            if (!p.InBounds(Size)) return ShotResult.Invalid;
            var state = grid[p.Row, p.Col];
            if (state == CellState.Hit || state == CellState.Miss) return ShotResult.AlreadyShot;

            if (state == CellState.Empty)
            {
                grid[p.Row, p.Col] = CellState.Miss;
                return ShotResult.Miss;
            }

            // Ship present
            grid[p.Row, p.Col] = CellState.Hit;
            var ship = posToShip[p];
            ship.RegisterHit(p);
            if (ship.IsSunk()) return ShotResult.Sunk;
            return ShotResult.Hit;
        }

        public bool AllShipsSunk() => ships.All(s => s.IsSunk());

        // Visualization (for debug / console). If hideShips==true, show ships as empty (for opponent view)
        public void Print(bool hideShips = false)
        {
            Console.Write("   ");
            for (int c = 0; c < Size; c++) Console.Write($"{c+1,2} ");
            Console.WriteLine();
            for (int r = 0; r < Size; r++)
            {
                Console.Write($" {(char)('A'+r)} ");
                for (int c = 0; c < Size; c++)
                {
                    var state = grid[r, c];
                    char ch = '.';
                    switch (state)
                    {
                        case CellState.Empty: ch = '.'; break;
                        case CellState.Miss: ch = 'o'; break;
                        case CellState.Hit: ch = 'X'; break;
                        case CellState.Ship: ch = hideShips ? '.' : 'S'; break;
                    }
                    Console.Write($" {ch} ");
                }
                Console.WriteLine();
            }
        }

        // Helper to parse positions like "A5" or "J10"
        public static bool TryParsePosition(string input, int size, out Position pos)
        {
            pos = default;
            input = input.Trim().ToUpper();
            if (string.IsNullOrEmpty(input)) return false;
            char rowChar = input[0];
            int row = rowChar - 'A';
            if (row < 0 || row >= size) return false;
            if (!int.TryParse(input.Substring(1), out int col)) return false;
            col -= 1;
            if (col < 0 || col >= size) return false;
            pos = new Position(row, col);
            return true;
        }
    }

    public interface IPlayer
    {
        string Name { get; }
        Board OwnBoard { get; }
        Board OpponentViewBoard { get; } // tracks hits/misses on opponent
        void PlaceShipsRandomly(IEnumerable<ShipType> shipTypes);
        Position GetNextShot(); // for human: ask; for AI: compute
    }

    public abstract class Player : IPlayer
    {
        public string Name { get; protected set; }
        public Board OwnBoard { get; protected set; }
        public Board OpponentViewBoard { get; protected set; }
        protected Random rng = new();

        public Player(string name, int boardSize = 10)
        {
            Name = name;
            OwnBoard = new Board(boardSize);
            OpponentViewBoard = new Board(boardSize);
        }

        public virtual void PlaceShipsRandomly(IEnumerable<ShipType> shipTypes)
        {
            foreach (var type in shipTypes)
            {
                bool placed = false;
                int tries = 0;
                while (!placed && tries++ < 1000)
                {
                    var orient = rng.Next(2) == 0 ? Orientation.Horizontal : Orientation.Vertical;
                    int r = rng.Next(OwnBoard.Size);
                    int c = rng.Next(OwnBoard.Size);
                    placed = OwnBoard.PlaceShip(new Position(r, c), orient, type);
                }
                if (!placed) throw new Exception($"Не удалось разместить корабль {type}");
            }
        }

        public abstract Position GetNextShot();
    }

    public class HumanPlayer : Player
    {
        public HumanPlayer(string name, int boardSize = 10) : base(name, boardSize) { }

        public override Position GetNextShot()
        {
            while (true)
            {
                Console.Write($"{Name}, введите координату для выстрела (например A5): ");
                var input = Console.ReadLine() ?? "";
                if (Board.TryParsePosition(input, OwnBoard.Size, out var pos)) return pos;
                Console.WriteLine("Неверный ввод, попробуйте еще.");
            }
        }
    }

    public class RandomAiPlayer : Player
    {
        private HashSet<Position> tried = new();
        public RandomAiPlayer(string name, int boardSize = 10) : base(name, boardSize) { }

        public override Position GetNextShot()
        {
            while (true)
            {
                int r = rng.Next(OwnBoard.Size);
                int c = rng.Next(OwnBoard.Size);
                var p = new Position(r, c);
                if (tried.Contains(p)) continue;
                tried.Add(p);
                return p;
            }
        }
    }

    public class Game
    {
        private IPlayer p1;
        private IPlayer p2;
        private IEnumerable<ShipType> shipTypes = new[]
        {
            ShipType.Battleship4,
            ShipType.Cruiser3, ShipType.Cruiser3,
            ShipType.Destroyer2, ShipType.Destroyer2, ShipType.Destroyer2,
            ShipType.Patrol1, ShipType.Patrol1, ShipType.Patrol1, ShipType.Patrol1
        };

        public Game(IPlayer player1, IPlayer player2)
        {
            p1 = player1;
            p2 = player2;
        }

        public void Setup()
        {
            p1.PlaceShipsRandomly(shipTypes);
            p2.PlaceShipsRandomly(shipTypes);
        }

        public void Run()
        {
            IPlayer current = p1;
            IPlayer other = p2;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"Ход игрока: {current.Name}");
                Console.WriteLine("Ваша доска:");
                current.OwnBoard.Print(hideShips: false);
                Console.WriteLine("\nВид на противника (X - попадание, o - промах):");
                current.OpponentViewBoard.Print(hideShips: true);

                var pos = current.GetNextShot();
                var result = other.OwnBoard.ReceiveShot(pos);

                // update current's view
                if (result == ShotResult.Miss) current.OpponentViewBoard.ReceiveShot(pos); // mark miss
                else if (result == ShotResult.Hit || result == ShotResult.Sunk)
                {
                    // mark as hit on opponent view: we simulate by setting cell to Hit in view
                    // using ReceiveShot on a board with same coords but no ship will mark Miss; so we directly set cell
                    // simpler: reflect hit/miss using internal access (we'll implement small helper)
                    // For now, call a small helper via reflection-less approach: place a "marker" ship is not suitable.
                    // Instead add a small method to Board to MarkShotOnView -- but to keep sample compact, we'll use the pattern:
                    // directly set OpponentViewBoard grid via internal API is not available, so we'll rely on a second ReceiveShot-like call:
                }

                // For correct marking we will extend Board with MarkShot(Position, ShotResult)
                // To keep this example functional, let's implement MarkShot using a small wrapper below (we'll add method)
                // But here, to keep code consistent, let's call a static helper:
                MarkShotOnBoard(current.OpponentViewBoard, pos, result);

                switch (result)
                {
                    case ShotResult.Invalid:
                        Console.WriteLine("Координата вне поля. Ход пропущен.");
                        break;
                    case ShotResult.AlreadyShot:
                        Console.WriteLine("Вы уже стреляли в эту клетку ранее.");
                        break;
                    case ShotResult.Miss:
                        Console.WriteLine($"{current.Name} промахнулся в {pos}.");
                        // смена игрока
                        (current, other) = (other, current);
                        break;
                    case ShotResult.Hit:
                        Console.WriteLine($"{current.Name} попал в {pos}!");
                        break;
                    case ShotResult.Sunk:
                        Console.WriteLine($"{current.Name} потопил корабль в {pos}!");
                        break;
                }

                if (other.OwnBoard.AllShipsSunk())
                {
                    Console.WriteLine($"\n{current.Name} победил!");
                    Console.WriteLine("Финальная доска проигравшего:");
                    other.OwnBoard.Print(hideShips: false);
                    break;
                }

                Console.WriteLine("Нажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        // Helper to mark result on player's view board
        private static void MarkShotOnBoard(Board viewBoard, Position pos, ShotResult result)
        {
            // using ReceiveShot-like semantics but ensuring we mark Hit/Miss explicitly
            // Reflection avoided — we simulate by calling ReceiveShot on a temporary board? Simpler:
            // direct access not available here, so we'll create a simple small extension via workaround:
            // We'll rely on: if viewBoard grid cell is Empty, set to Miss/Hit by crafting a fake placement for Hit.
            // But to keep sample correct and simple, let's add a tiny method to Board in real code.
            // For now assume we can call a safe method — since code is in single file, we can add it:
            viewBoard.MarkShot(pos, result);
        }
    }

    // We'll add an extension within same namespace: extend Board with MarkShot to allow view updates.
    public static class BoardExtensions
    {
        public static void MarkShot(this Board board, Position p, ShotResult result)
        {
            if (!p.InBounds(board.Size)) return;
            // Reflect Hit or Miss on board grid by internal knowledge (we will use reflection-like direct access by re-creating simple logic):
            // Because grid is private, but we're in same assembly & file: use board.ReceiveShot hack? Can't call ReceiveShot because it
