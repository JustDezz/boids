using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public struct SpatialHashGrid<T> : IDisposable where T : unmanaged
{
	private readonly Bounds _actualBounds;
	private readonly Vector3Int _cells;
	private readonly Vector3 _cellSize;
	private NativeMultiHashMap<int, T> _map;

	public SpatialHashGrid(Bounds bounds, Vector3 cellSize, int capacity, Allocator allocator)
	{
		_cellSize = cellSize;
		// Small padding
		Vector3 boundsSize = bounds.size + Vector3.one;
		int xCells = Mathf.CeilToInt(boundsSize.x / _cellSize.x);
		int yCells = Mathf.CeilToInt(boundsSize.y / _cellSize.y);
		int zCells = Mathf.CeilToInt(boundsSize.z / _cellSize.z);
		_cells = new Vector3Int(xCells, yCells, zCells);

		Vector3 actualSize = new(xCells * _cellSize.x, yCells * _cellSize.y, zCells * _cellSize.z);
		_actualBounds = new Bounds(bounds.center, actualSize);
		_map = new NativeMultiHashMap<int, T>(capacity, allocator);
	}

	public void Add(Vector3 position, T item)
	{
		int index = PositionToIndex(position);
		_map.Add(index, item);
	}

	public AreaEnumerator GetEnumerator(Vector3 position, Vector3 extents) => new(ref this, position, extents);

	public IEnumerable<T> GetElements(Vector3 position, float radius) => GetElements(position, Vector3.one * radius);
	public IEnumerable<T> GetElements(Vector3 position, Vector3 extents)
	{
		using AreaEnumerator enumerator = GetEnumerator(position, extents);
		while (enumerator.MoveNext()) yield return enumerator.Current;
	}

	private int PositionToIndex(Vector3 position)
	{
		Vector3Int gridIndex = ToCell(position);
		return CellToIndex(gridIndex);
	}
	private int CellToIndex(Vector3Int cell) => cell.x + cell.y * _cells.x + cell.z * _cells.x * _cells.y; 

	private Vector3Int ToCell(Vector3 position)
	{
		Vector3 gridSpace = position - _actualBounds.min;
		int xCells = Mathf.CeilToInt(gridSpace.x / _cellSize.x);
		int yCells = Mathf.CeilToInt(gridSpace.y / _cellSize.y);
		int zCells = Mathf.CeilToInt(gridSpace.z / _cellSize.z);
		return new Vector3Int(xCells, yCells, zCells);
	}

	public void Dispose() => _map.Dispose();
	
	public struct AreaEnumerator : IEnumerator<T>
	{
		private SpatialHashGrid<T> _grid;
		private readonly Vector3Int _cells;
		private readonly Vector3Int _cellOffset;
		
		private Vector3Int _cellIndex;
		private NativeMultiHashMap<int, T>.Enumerator _values;

		public AreaEnumerator(ref SpatialHashGrid<T> grid, Vector3 position, Vector3 extents)
		{
			_grid = grid;
			extents = new Vector3(Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z));
			
			Vector3 min = position - extents;
			Vector3 max = position + extents;

			Vector3Int minIndex = grid.ToCell(min);
			Vector3Int maxIndex = grid.ToCell(max);

			_cellOffset = minIndex;
			_cells = maxIndex - minIndex;
			
			_cellIndex = -Vector3Int.one;
			_values = default;
		}
	
		public void Dispose()
		{
		}
	
		public bool MoveNext()
		{
			while (PerformMove(out bool hasValues))
			{
				if (hasValues) return true;
			}

			return false;
		}

		private bool PerformMove(out bool hasValues)
		{
			if (_cellIndex != -Vector3Int.one)
			{
				hasValues = true;
				if (_values.MoveNext()) return true;
				if (IncrementIndex(0) && IncrementIndex(1) && IncrementIndex(2)) return false;
			}
			else _cellIndex = Vector3Int.zero;

			Vector3Int cell = _cellIndex + _cellOffset;
			int index = _grid.CellToIndex(cell);
			_values = _grid._map.GetValuesForKey(index);
			hasValues = _values.MoveNext();
			return true;
		}

		private bool IncrementIndex(int component)
		{
			_cellIndex[component]++;
			if (_cellIndex[component] < _cells[component]) return false;
			_cellIndex[component] = 0;
			return true;
		}
	
		public void Reset() => _cellIndex = -Vector3Int.one;
	
		public T Current => _values.Current;
	
		object IEnumerator.Current => Current;
	}
}