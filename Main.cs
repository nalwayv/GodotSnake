using Godot;
using System;


public partial class Main
    : Node2D
{
    [ExportGroup("Scenes")]
    [Export] public PackedScene PillScene { get; set; }
    [Export] public PackedScene BlankScene { get; set; }
    [Export] public PackedScene BodyScene { get; set; }

    Vector2I _headStartAtCoord = new(1, 1);
    Vector2I _gridDimentions = new(13, 16);
    Vector2 _gridPosition = new(32, 60);

    const int MaxBlanks = 70;

    float _tileSize = 32.0f;
    int _scoreTally = 0;
    int _pillsTally = 0;
    int _currentBlanksCount = 0;

    bool _addBodyPart = false;
    bool _headMoved = false;
    bool _isRunning = false;

    Hud _hud;
    Head _head;
    Map _map;

    Node _blankContainer;
    Node _bodyContainer;
    Node _pillContainer;
    Timer _pillSpawnTimer;
    Timer _blankSpawnTimer;


    int PillTally
    {
        get
        {
            return _pillsTally;
        }
        set
        {
            _pillsTally = (value < 0) ? 0 : value;
        }
    }

    void ClearBody()
    {
        if (_bodyContainer.GetChildCount() > 0)
        {
            for (int i = 0; i < _bodyContainer.GetChildCount(); i++)
            {
                _bodyContainer.GetChild(i).QueueFree();
            }
        }
    }

    void ClearPills()
    {
        if (_pillContainer.GetChildCount() > 0)
        {
            for (int i = 0; i < _pillContainer.GetChildCount(); i++)
            {
                _pillContainer.GetChild(i).QueueFree();
            }
        }
    }

    void StopPillTimers()
    {
        if (_pillContainer.GetChildCount() > 0)
        {
            for (int i = 0; i < _pillContainer.GetChildCount(); i++)
            {
                Pill pill = (Pill)_pillContainer.GetChild(i);
                pill.StopTimer();
            }
        }
    }

    void ClearBlanks()
    {
        if (_blankContainer.GetChildCount() > 0)
        {
            for (int i = 0; i < _blankContainer.GetChildCount(); i++)
            {
                _blankContainer.GetChild(i).QueueFree();
            }
        }
    }

    void ClearBoard()
    {
        ClearBody();
        ClearPills();
        ClearBlanks();

        _map.ResetCoords();
    }

    #region signals
    void OnHudStartGame()
    {
        ClearBoard();

        Vector2 headStartPos = _map.CoordToWorld(_headStartAtCoord);
        _head.Setup(headStartPos, _tileSize);
        _head.Show();

        _map.SetCoordToTaken(_headStartAtCoord);

        _scoreTally = 0;
        _hud.UpdateScore(_scoreTally);

        // init a spawn timer for pills that will be altered
        // with each pill spawned later
        _pillSpawnTimer.WaitTime = GD.RandRange(1, 3);
        _pillSpawnTimer.Start();

        _blankSpawnTimer.Start();

        _isRunning = true;
    }

    void OnHeadHitPill(string name)
    {
        GetNode<Camera>("Camera").Shake(0.1f, 25.0f, 5.0f);

        var pill = _pillContainer.GetNode<Pill>(name);
        pill.PillTimedOut -= OnPillTimedOut;
        pill.QueueFree();

        _addBodyPart = true;

        PillTally -= 1;
        _scoreTally += 1;
        _hud.UpdateScore(_scoreTally);
    }

    void OnPillTimedOut()
    {
        PillTally -= 1;
    }

    void OnHeadGameOver()
    {
        GetNode<Camera>("Camera").Shake(0.1f, 25.0f, 5.0f);

        StopPillTimers();

        _scoreTally = 0;
        _isRunning = false;

        _pillSpawnTimer.Stop();
        _blankSpawnTimer.Stop();
        _head.Reset();
        _hud.ShowGameOver();
    }

    void OnHeadMoved()
    {
        // update within process
        _headMoved = true;
    }

    void OnPillSpawnTimerTimeout()
    {
        PillTally += 1;

        Vector2I rngCoord = _map.RngCoord();

        var pill = PillScene.Instantiate<Pill>();
        pill.SetUp(_map.CoordToWorld(rngCoord));
        pill.PillTimedOut += OnPillTimedOut;

        _pillContainer.AddChild(pill);

        _pillSpawnTimer.WaitTime = GD.RandRange(1, 3);
        _pillSpawnTimer.Start();
    }

    void OnBlankSpawnTimerTimeout()
    {
        _currentBlanksCount += 1;
        if (_currentBlanksCount >= MaxBlanks)
        {
            return;
        }

        var neighbours = _map.GetNeighbours(_map.WorldToCoord(_head.Position));
        Vector2I rngCoord = Vector2I.Zero;
        do
        {
            // dont spawn in front of head thats just rude!
            // rngPosition = _grid.RandomCoord();
            rngCoord = _map.RngCoord();
        } while (neighbours.Contains(rngCoord));

        var blank = BlankScene.Instantiate<Blank>();
        blank.SetUp(_map.CoordToWorld(rngCoord));
        _blankContainer.AddChild(blank);

        _map.SetCoordToTaken(_map.WorldToCoord(blank.Position));
    }
    #endregion

    #region  override
    public override void _Ready()
    {
        _head = GetNode<Head>("Head");
        _hud = GetNode<Hud>("Hud");
        _map = GetNode<Map>("Map");

        _bodyContainer = GetNode<Node>("BodyContainer");
        _blankContainer = GetNode<Node>("BlankContainer");
        _pillContainer = GetNode<Node>("PillContainer");
        _pillSpawnTimer = GetNode<Timer>("PillSpawnTimer");
        _blankSpawnTimer = GetNode<Timer>("BlankSpawnTimer");

        _head.Hide();

        _map.Setup(_gridDimentions, _gridPosition);
    }

    public override void _Process(double delta)
    {
        if (!_isRunning)
        {
            return;
        }

        if (_addBodyPart)
        {
            var body = BodyScene.Instantiate<Body>();

            if (_bodyContainer.GetChildCount() > 0)
            {
                var last = _bodyContainer.GetChild<Body>(_bodyContainer.GetChildCount() - 1);
                body.SetUp(last.LastPosition);
                _bodyContainer.AddChild(body);
            }
            else
            {
                body.SetUp(_head.LastPosition);
                _bodyContainer.AddChild(body);
            }
            _addBodyPart = false;
        }


        if (_headMoved)
        {
            // next position has been made check if its on grid then move with updatePos
            // to prevent head going off grid for a split second then teleporting to
            // opposite side!
            Vector2I currentHeadCoord = _map.WorldToCoord(_head.NextPosition);
            if (!_map.CoordOnGrid(currentHeadCoord))
            {
                Vector2I wrapCoord = _map.Wrap(currentHeadCoord);
                _head.UpdatePosition(_map.CoordToWorld(wrapCoord));
            }
            else
            {
                _head.UpdatePosition();
            }

            _map.SetCoordToTaken(_map.WorldToCoord(_head.Position));
            _map.SetCoordToOpen(_map.WorldToCoord(_head.LastPosition));

            if (_bodyContainer.GetChildCount() > 0)
            {
                Vector2 lastPos = _head.LastPosition;
                foreach (var node in _bodyContainer.GetChildren())
                {
                    var body = node as Body;
                    body.MoveTo(lastPos);

                    _map.SetCoordToTaken(_map.WorldToCoord(body.Position));
                    _map.SetCoordToOpen(_map.WorldToCoord(body.LastPosition));

                    lastPos = body.LastPosition;
                }
            }

            _headMoved = false;
        }
    }
    #endregion
}
