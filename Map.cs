using Godot;
using System;

public partial class Map 
    : TileMap
{

    enum Data { TAKEN, OPEN };

    Godot.Collections.Array<Data> _data = new();
    Vector2I _dimentions = Vector2I.Zero;

    int GridSize()
    {
        return _dimentions.X * _dimentions.Y;
    }

    Vector2I ConvertTo2DCoords(int at)
    {
        Vector2I result = Vector2I.Zero;
        if (at < 0 || at >= GridSize())
        {
            return result;
        }

        result.X = at % _dimentions.X;
        result.Y = at / _dimentions.X;

        return result;
    }

    int ConvertTo1DCoords(Vector2I coord)
    {
        int at = coord.Y * _dimentions.X + coord.X;
        return (at < 0 || at >= GridSize()) ? 0 : at;
    }

    public Vector2I TileSize()
    {
        return TileSet.TileSize;
    }

    public bool CoordOnGrid(Vector2I coord)
    {
        bool checkX = coord.X >= 0 && coord.X < _dimentions.X;
        bool checkY = coord.Y >= 0 && coord.Y < _dimentions.Y;
        return checkX && checkY;
    }

    public void Setup(Vector2I dimentions, Vector2 position)
    {
        _dimentions = dimentions;
        Position = position;

        for (int x = 0; x < _dimentions.X; x++)
        {
            for (int y = 0; y < _dimentions.Y; y++)
            {
                SetCell(0, new(x, y), 0, Vector2I.Zero);
                _data.Add(Data.OPEN);
            }
        }
    }

    public Vector2I RngCoord()
    {
        Vector2I result = Vector2I.Zero;
        Godot.Collections.Array<Data> data = new(_data);

        while (data.Count > 0)
        {
            int rng = GD.RandRange(0, data.Count);
            if (rng < GridSize())
            {
                result = ConvertTo2DCoords(rng);
                int at = ConvertTo1DCoords(result);

                if (_data[at] == Data.TAKEN)
                {
                    data.RemoveAt(rng);
                }
                else
                {
                    break;
                }
            }
        }

        return result;
    }

    public void SetCoordToOpen(Vector2I coord)
    {
        if (CoordOnGrid(coord))
        {
            int at = ConvertTo1DCoords(coord);
            _data[at] = Data.OPEN;
        }
    }

    public void SetCoordToTaken(Vector2I coord)
    {
        if (CoordOnGrid(coord))
        {
            int at = ConvertTo1DCoords(coord);
            _data[at] = Data.TAKEN;
        }
    }

    public void ResetCoords()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            _data[i] = Data.OPEN;
        }
    }

    public Vector2I WorldToCoord(Vector2 position)
    {
        var tileSize = TileSet.TileSize;
        Vector2 coord = (position - Position) * (1.0f / tileSize.X);
        coord = coord.Floor();

        return new(((int)coord.X), ((int)coord.Y));
    }

    public Vector2 CoordToWorld(Vector2I coord)
    {
        var tileSize = TileSet.TileSize;

        Vector2 startAt = coord * tileSize.X;
        Vector2 offset = Vector2.One * (tileSize.X * 0.5f);

        return Position + startAt + offset;
    }

    public Vector2I Wrap(Vector2I coord)
    {
        if (coord.X >= _dimentions.X)
        {
            coord.X = 0;
        }

        if (coord.X < 0.0f)
        {
            coord.X = _dimentions.X - 1;
        }

        if (coord.Y >= _dimentions.Y)
        {
            coord.Y = 0;
        }

        if (coord.Y < 0)
        {
            coord.Y = _dimentions.Y - 1;
        }
        return coord;
    }

    public Godot.Collections.Array<Vector2I> GetNeighbours(Vector2I coord)
    {
        return GetSurroundingCells(coord);
    }

	public Rect2 GetRect()
	{
		return new Rect2(Position, _dimentions * TileSize().X);
	}


    #region override
    // Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    // }

    // // Called every frame. 'delta' is the elapsed time since the previous frame.
    // public override void _Process(double delta)
    // {
    // }
    #endregion
}
