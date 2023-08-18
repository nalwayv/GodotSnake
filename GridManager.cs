using Godot;
using System;

public enum CellStatus
{
	TAKEN, OPEN, None
}

public partial class GridManager 
{
	public float CellSize { get; set; }
	public Vector2 Dimentions {get; set; }
	public Vector2 Position { get; set;}

	Godot.Collections.Array<Vector2> _coords = new();
	Godot.Collections.Array<CellStatus> _data = new();

    public GridManager(float cellSize, Vector2 dimentions, Vector2 position)
    {
        CellSize = cellSize;
		Dimentions = dimentions;
		Position = position;

		for (int x = 0; x < Dimentions.X; x++)
		{
			for (int y = 0; y < Dimentions.Y; y++)
			{
				var coord = new Vector2(x, y);
				_coords.Add(coord);
				_data.Add(CellStatus.OPEN);
			}
		}
    }

	public Vector2 RandomCoord()
	{
		Vector2 result = Vector2.Zero;
		Godot.Collections.Array<CellStatus> data = new(_data);

		while(data.Count > 0)
		{
			int rngIdx = GD.RandRange(0, data.Count);
			if(rngIdx < _coords.Count)
			{
				result = _coords[rngIdx];
				if(GetCoordDataAt(result) == CellStatus.TAKEN)
				{
					data.RemoveAt(rngIdx);
				}
				else
				{
					break;
				}
			}
		}
		return result;
	}

	public void SetCoordDataAt(Vector2 coord, CellStatus status)
	{
		if(!CoordOnGrid(coord))
		{
			return;
		}

		int x = ((int)coord.X);
		int y = ((int)coord.Y);
		int at = y * ((int)Dimentions.X) + x; 
		_data[at] = status;
		
	}
	
	public CellStatus GetCoordDataAt(Vector2 coord)
	{
		if(!CoordOnGrid(coord))
		{
			return CellStatus.None;
		}

		int x = ((int)coord.X);
		int y = ((int)coord.Y);

		int at = y * ((int)Dimentions.X) + x; 
		return _data[at];
	}

	public bool IsCoordAvailable(Vector2 coord)
	{
		return GetCoordDataAt(coord) == CellStatus.OPEN;
	}

    public void SetCoordDataToTaken(Vector2 coord)
    {
        SetCoordDataAt(coord, CellStatus.TAKEN);
    }

    public void SetCoordDataToAvailable(Vector2 coord)
    {
        SetCoordDataAt(coord, CellStatus.OPEN);
    }

    public void SetWorldPositionToTaken(Vector2 pos)
    {
		SetCoordDataToTaken(WorldPositionToCoord(pos));
    }

    public void SetWorldPositionToAvailable(Vector2 pos)
    {
        SetCoordDataToAvailable(WorldPositionToCoord(pos));
    }

	public void ResetGridData()
	{
		if(_data.Count > 0)
		{
			for (int i = 0; i < _data.Count; i++)
			{
				_data[i] = CellStatus.OPEN;
			}
		}
	}

	public Godot.Collections.Array<Vector2> GetCoords()
	{
		return _coords;
	}

	public bool CoordOnGrid(Vector2 range)
	{
		bool checkX = range.X >= 0 && range.X < Dimentions.X;
		bool checkY = range.Y >= 0 && range.Y < Dimentions.Y;
		return checkX && checkY;
	}

	public Vector2 WorldPositionToCoord(Vector2 pos)
	{
		Vector2 coord = (pos - Position) * (1.0f / CellSize);
		return coord.Floor();
	}

	public Vector2 CoordToWorldPosition(Vector2 coord)
	{
		Vector2 offset = Vector2.One * (CellSize * 0.5f);

		Vector2 startAt = coord * CellSize;
		return startAt + Position + offset;
	}

	public Vector2 Wrap(Vector2 coord)
	{
		if(coord.X >= Dimentions.X)
		{
			coord.X = 0.0f;
		}

		if(coord.X < 0.0f)
		{
			coord.X = Dimentions.X - 1.0f;
		}

		if(coord.Y >= Dimentions.Y)
		{
			coord.Y = 0.0f;
		}

		if(coord.Y < 0.0f)
		{
			coord.Y = Dimentions.Y - 1.0f;
		}

		return coord;
	}
}
