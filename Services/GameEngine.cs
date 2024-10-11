using W8_assignment_template.Data;
using W8_assignment_template.Helpers;
using W8_assignment_template.Interfaces;
using W8_assignment_template.Models.Characters;

namespace W8_assignment_template.Services;

public class GameEngine
{
    private readonly IContext _context;
    private readonly MapManager _mapManager;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private readonly IRoomFactory _roomFactory;
    private ICharacter _player;
    private ICharacter _goblin;
    private ICharacter _uruk;
    private ICharacter _orrok;

    private List<IRoom> _rooms;

    public GameEngine(IContext context, IRoomFactory roomFactory, MenuManager menuManager, MapManager mapManager, OutputManager outputManager)
    {
        _roomFactory = roomFactory;
        _menuManager = menuManager;
        _mapManager = mapManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void AttackCharacter()
    {
        var target = _player.CurrentRoom.Characters[0];
        if (_player.CurrentRoom.Characters.Count > 1)
        {
            Console.WriteLine("Which monster would you like to attack?", ConsoleColor.Red);
            for (int i = 0; i < _player.CurrentRoom.Characters.Count; ++i)
            {
                Console.WriteLine($"{i + 1}. {_player.CurrentRoom.Characters[i].Name}, {_player.CurrentRoom.Characters[i].Type}");
            }
            int chosen = Convert.ToInt32(Console.ReadLine());
            target = _player.CurrentRoom.Characters[chosen - 1];
            _player.Attack(target);

            if (_player.CurrentRoom.Characters[chosen - 1].HP > 0)
            {
                target.Attack(_player);
            }
            else
            {
                _player.HP = 10;
                _player.CurrentRoom.RemoveCharacter(target);
            }
        }
        else if (_player.CurrentRoom.Characters.Count == 1)
        {
            target = _player.CurrentRoom.Characters[0];
            _player.Attack(target);

            if (_player.CurrentRoom.Characters[0].HP > 0)
            {
                target.Attack(_player);
            }
            else
            {
                _player.HP = 10;
                _player.CurrentRoom.RemoveCharacter(target);
            }
        }
        else
        {
            _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
        }
    }

    private void GameLoop()
    {
        while (true)
        {
            if(_player.HP > 0)
            {
                _mapManager.DisplayMap();
                _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
                _outputManager.WriteLine("1. Move North");
                _outputManager.WriteLine("2. Move South");
                _outputManager.WriteLine("3. Move East");
                _outputManager.WriteLine("4. Move West");

                // Check if there are characters in the current room to attack
                if (_player.CurrentRoom.Characters.Any(c => c != _player))
                {
                    _outputManager.WriteLine("5. Attack");
                }

                _outputManager.WriteLine("6. Exit Game");

                _outputManager.Display();

                var input = Console.ReadLine();

                string? direction = null;
                switch (input)
                {
                    case "1":
                        direction = "north";
                        break;
                    case "2":
                        direction = "south";
                        break;
                    case "3":
                        direction = "east";
                        break;
                    case "4":
                        direction = "west";
                        break;
                    case "5":
                        if (_player.CurrentRoom.Characters.Any(c => c != _player))
                        {
                            _outputManager.Clear();
                            AttackCharacter();
                        }
                        else
                        {
                            _outputManager.WriteLine("No characters to attack.", ConsoleColor.Red);
                        }

                        break;
                    case "6":
                        _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                        _outputManager.Display();
                        Environment.Exit(0);
                        break;
                    default:
                        _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                        break;
                }

                // Update map manager with the current room after movement
                if (!string.IsNullOrEmpty(direction))
                {
                    _outputManager.Clear();
                    _player.Move(direction);
                    _mapManager.UpdateCurrentRoom(_player.CurrentRoom);
                }
            }
            else
            {
                _outputManager.WriteLine($"{_player.Name} has fallen in battle.", ConsoleColor.Gray);
                _outputManager.WriteLine($"Would you like to play again?", ConsoleColor.Gray);
                _outputManager.WriteLine("1. Yes");
                _outputManager.WriteLine("2. No");
                _outputManager.Display();

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        _outputManager.Clear();
                        _player.HP = 10;
                        break;
                    case "2":
                        _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                        _outputManager.Display();
                        Environment.Exit(0);
                        break;
                    default:
                        _outputManager.WriteLine("Invalid selection. Please choose a valid option.", ConsoleColor.Red);
                        break;
                }
            }
        }
    }

    private void LoadMonsters()
    {
        _goblin = _context.Characters.OfType<Goblin>().FirstOrDefault();
        _uruk = _context.Characters.OfType<Uruk>().FirstOrDefault();
        _orrok = _context.Characters.OfType<Orrok>().FirstOrDefault();

        var random = new Random();
        var randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_goblin); // Use helper method

        randomRoom = _rooms[random.Next(_rooms.Count)];
        randomRoom.AddCharacter(_uruk);
        randomRoom.AddCharacter(_orrok);

        // TODO Load your two new monsters here into the same room
    }

    private void SetupGame()
    {
        var startingRoom = SetupRooms();
        _mapManager.UpdateCurrentRoom(startingRoom);

        _player = _context.Characters.OfType<Player>().FirstOrDefault();
        _player.Move(startingRoom);
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause for a second before starting the game loop
        Thread.Sleep(1000);
        GameLoop();
    }

    private IRoom SetupRooms()
    {
        // TODO Update this method to create more rooms and connect them together

        var entrance = _roomFactory.CreateRoom("entrance", _outputManager);
        var treasureRoom = _roomFactory.CreateRoom("treasure", _outputManager);
        var dungeonRoom = _roomFactory.CreateRoom("dungeon", _outputManager);
        var library = _roomFactory.CreateRoom("library", _outputManager);
        var armory = _roomFactory.CreateRoom("armory", _outputManager);
        var garden = _roomFactory.CreateRoom("garden", _outputManager);
        var kitchen = _roomFactory.CreateRoom("kitchen", _outputManager);
        var diningRoom = _roomFactory.CreateRoom("dining", _outputManager);

        entrance.North = treasureRoom;
        entrance.West = library;
        entrance.East = garden;

        treasureRoom.South = entrance;
        treasureRoom.West = dungeonRoom;

        dungeonRoom.East = treasureRoom;

        library.West = kitchen;
        library.East = entrance;
        library.South = armory;

        armory.North = library;

        garden.West = entrance;

        kitchen.East = library;
        kitchen.North = diningRoom;

        diningRoom.South = kitchen;

        // Store rooms in a list for later use
        _rooms = new List<IRoom> { entrance, treasureRoom, dungeonRoom, library, armory, garden };

        return entrance;
    }
}
