using Godot;
using System;

public enum Direction
{
    UP, DOWN, LEFT, RIGHT, NONE
}

public partial class Head
    : Area2D
{
    [Signal] public delegate void HitPillEventHandler(string name);
    [Signal] public delegate void GameOverEventHandler();
    [Signal] public delegate void MovedEventHandler();

    const int MaxInc = 30;

    Godot.Collections.Array<Direction> _inputDirections = new();
    Direction _currentDirection = Direction.NONE;
    bool _isActive = false;
    bool _isHitStop;
    float _moveDistance = 0.0f;
    int _incCounter = 0;

    public Vector2 LastPosition { get; private set; }
    public Vector2 NextPosition { get; private set; }
    public Vector2 NextDirection { get; private set; }

    public void Setup(Vector2 position, float tileSize)
    {
        Position = position;

        _isActive = true;
        _currentDirection = Direction.NONE;
        _moveDistance = tileSize;

        LookAt(Position + Vector2.Right);
    }

    public void Reset()
    {
        _isActive = false;
        _incCounter = 0;
        _currentDirection = Direction.NONE;

        if (_inputDirections.Count > 0)
        {
            _inputDirections.Clear();
        }
    }

    public void UpdatePosition()
    {
        Position = NextPosition;
        LookAt(Position + NextDirection);
    }

    public void UpdatePosition(Vector2 position)
    {
        Position = position;
        LookAt(Position + NextDirection);
    }

    Vector2 DirectionToVector(Direction dir)
    {
        Vector2 result = Vector2.Zero;
        switch (dir)
        {
            case Direction.UP:
                result = Vector2.Up;
                break;
            case Direction.DOWN:
                result = Vector2.Down;
                break;
            case Direction.LEFT:
                result = Vector2.Left;
                break;
            case Direction.RIGHT:
                result = Vector2.Right;
                break;
        }

        return result;
    }

    #region signals
    void OnAreaEntered(Area2D area)
    {
        if (area.IsInGroup("Pill"))
        {
            EmitSignal(SignalName.HitPill, area.Name);
            
            var hs = GetNode<Timer>("HitStop");
            hs.WaitTime = 0.05f;
            hs.OneShot = true;
            hs.Start();

            _isHitStop = true;
        }
        else if (area.IsInGroup("Body") || area.IsInGroup("Blank"))
        {
            EmitSignal(SignalName.GameOver);
        }
    }

    void OnHitStopTimeout()
    {
        _isHitStop = false;
    }
    #endregion

    #region override


    public override void _Process(double delta)
    {
        if (!_isActive || _isHitStop)
        {
            return;
        }

        if (Input.IsActionJustPressed("Up"))
        {
            if (_currentDirection != Direction.DOWN)
            {
                _inputDirections.Add(Direction.UP);
            }
        }
        else if (Input.IsActionJustPressed("Down"))
        {
            if (_currentDirection != Direction.UP)
            {
                _inputDirections.Add(Direction.DOWN);
            }
        }
        else if (Input.IsActionJustPressed("Left"))
        {
            if (_currentDirection != Direction.RIGHT)
            {
                _inputDirections.Add(Direction.LEFT);
            }
        }
        else if (Input.IsActionJustPressed("Right"))
        {
            if (_currentDirection != Direction.LEFT)
            {
                _inputDirections.Add(Direction.RIGHT);
            }
        }

        if (MaxInc == (_incCounter++))
        {
            if (_inputDirections.Count > 0)
            {
                // to help stop input spamming that can causes snake to reverse on itself
                int lastIdx = _inputDirections.Count -1;
                _currentDirection = _inputDirections[lastIdx];
                _inputDirections.Clear();
            }

            NextDirection = DirectionToVector(_currentDirection);

            LastPosition = Position;
            NextPosition = Position + NextDirection * _moveDistance;
            _incCounter = 0;

            EmitSignal(SignalName.Moved);
        }
    }
    #endregion
}
