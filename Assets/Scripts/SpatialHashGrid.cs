using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[BurstCompatible]
public struct SpatialHashGrid<T> : IDisposable where T : struct, IEquatable<T>
{
	private readonly int3 _cells;
	private readonly float3 _cellSize;
	private readonly float3 _gridMin;
	private readonly float3 _gridMax;
	private NativeMultiHashMap<int, T> _map;

	public bool IsCreated => _map.IsCreated;

	public SpatialHashGrid(Bounds bounds, float3 cellSize, int capacity, Allocator allocator)
	{
		_cellSize = cellSize;
		float3 boundsSize = bounds.size;
		int xCells = Mathf.CeilToInt(boundsSize.x / _cellSize.x);
		int yCells = Mathf.CeilToInt(boundsSize.y / _cellSize.y);
		int zCells = Mathf.CeilToInt(boundsSize.z / _cellSize.z);
		_cells = new int3(xCells, yCells, zCells);

		float3 actualSize = new(xCells * _cellSize.x, yCells * _cellSize.y, zCells * _cellSize.z);
		float3 center = bounds.center;
		_gridMin = center - actualSize;
		_gridMax = center + actualSize;
		_map = new NativeMultiHashMap<int, T>(capacity, allocator);
	}

	public void Add(float3 position, T item)
	{
		int index = PositionToIndex(position);
		_map.Add(index, item);
	}

	public void Remove(float3 position, T item)
	{
		int index = PositionToIndex(position);
		_map.Remove(index, item);
	}

	public AreaEnumerator GetEnumerator(float3 position, float3 extents) => new(ref this, position, extents);

	public void Clear() => _map.Clear();

	public IEnumerable<T> GetElements(float3 position, float radius) => GetElements(position, (float3) radius);
	public IEnumerable<T> GetElements(float3 position, float3 extents)
	{
		using AreaEnumerator enumerator = GetEnumerator(position, extents);
		while (enumerator.MoveNext()) yield return enumerator.Current;
	}

	private int PositionToIndex(float3 position)
	{
		int3 gridIndex = ToCell(position);
		return CellToIndex(gridIndex);
	}
	private int CellToIndex(int3 cell) => cell.x + cell.y * _cells.x + cell.z * _cells.x * _cells.y; 

	private int3 ToCell(float3 position)
	{
		float3 gridSpace = position - _gridMin;
		int xCells = Mathf.FloorToInt(gridSpace.x / _cellSize.x);
		int yCells = Mathf.FloorToInt(gridSpace.y / _cellSize.y);
		int zCells = Mathf.FloorToInt(gridSpace.z / _cellSize.z);
		return new int3(xCells, yCells, zCells);
	}

	public void Dispose() => _map.Dispose();
	
	public struct AreaEnumerator : IEnumerator<T>
	{
		private SpatialHashGrid<T> _grid;
		private readonly int3 _cells;
		private readonly int3 _cellOffset;
		
		private int3 _cellIndex;
		private NativeMultiHashMap<int, T>.Enumerator _values;

		public AreaEnumerator(ref SpatialHashGrid<T> grid, Vector3 position, Vector3 extents)
		{
			_grid = grid;
			extents = new Vector3(Mathf.Abs(extents.x), Mathf.Abs(extents.y), Mathf.Abs(extents.z));
			
			Vector3 min = position - extents;
			Vector3 max = position + extents;

			int3 minIndex = grid.ToCell(min);
			int3 maxIndex = grid.ToCell(max);

			_cellOffset = minIndex;
			_cells = maxIndex - minIndex;
			
			_cellIndex = -1;
			_values = default;
		}
	
		public void Dispose()
		{
		}
	
		public bool MoveNext()
		{
			while (true)
			{
				bool2 move = PerformMove();
				if (!move.x) return false;
				if (move.y) return true;
			}
		}

		// .x displays if we can continue enumeration, .y shows if current cell has any elements in it
		private bool2 PerformMove()
		{
			if (_cellIndex.x != -1)
			{
				if (_values.MoveNext()) return true;
				if (++_cellIndex.x > _cells.x)
				{
					_cellIndex.x = 0;
					if (++_cellIndex.y > _cells.y)
					{
						_cellIndex.y = 0;
						if (++_cellIndex.z > _cells.z)
							return new bool2(false, true);
					}
				}
			}
			else _cellIndex = int3.zero;

			int3 cell = _cellIndex + _cellOffset;
			int index = _grid.CellToIndex(cell);
			_values = _grid._map.GetValuesForKey(index);
			return new bool2(true, _values.MoveNext());
		}
	
		public void Reset() => _cellIndex = -1;
	
		public T Current => _values.Current;
	
		object IEnumerator.Current => Current;
	}
}