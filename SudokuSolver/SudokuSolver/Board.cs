﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Threading;

namespace SudokuSolver {
	public class Board : IReadOnlyList<Cell> {
		public Cell this[int index] => _Cells[index];
		public Cell this[int row, int col] {
			get {
				if (row is < 0 or >= 9) {
					throw new ArgumentOutOfRangeException(nameof(row));
				}
				if (col is < 0 or >= 9) {
					throw new ArgumentOutOfRangeException(nameof(col));
				}
				return _Cells[row * 9 + col];
			}
		}

		public ReadOnlyCollection<Cell> Cells { get; }
		public ReadOnlyCollection<CellGroup> Rows { get; }
		public ReadOnlyCollection<CellGroup> Columns { get; }
		public ReadOnlyCollection<CellGroup> Squares { get; }

		public int Count => _Cells.Length;

		private readonly Cell[] _Cells;
		private readonly CellGroup[] _Rows;
		private readonly CellGroup[] _Columns;
		private readonly CellGroup[] _Squares;

		public Board() {
			_Cells = new Cell[9 * 9];
			_Rows = new CellGroup[9];
			_Columns = new CellGroup[9];
			_Squares = new CellGroup[9];

			Cells = new(_Cells);
			Rows = new(_Rows);
			Columns = new(_Columns);
			Squares = new(_Squares);

			// populate cells
			for (var i = 0; i < _Cells.Length; ++i) {
				_Cells[i] = new();
			}

			// populate groups
			for (var i = 0; i < 9; ++i) {
				_Rows[i] = new(getRow(i));
				_Columns[i] = new(getCol(i));
				_Squares[i] = new(getSqr(i));
			}

			return;

			IEnumerable<Cell> getRow(int ri) {
				var rOffset = ri * 9;
				for (var cOffset = 0; cOffset < 9; ++cOffset) {
					yield return this[rOffset + cOffset];
				}
			}
			IEnumerable<Cell> getCol(int ci) {
				for (var ri = 0; ri < 9; ++ri) {
					yield return this[ri, ci];
				}
			}
			IEnumerable<Cell> getSqr(int si) {
				var sOffset = (si / 3 * 27) + (si % 3) * 3;
				for (var ri = 0; ri < 3; ++ri) {
					var rOffset = ri * 9;
					yield return this[sOffset + rOffset + 0];
					yield return this[sOffset + rOffset + 1];
					yield return this[sOffset + rOffset + 2];
				}
			}
		}

		public IEnumerator<Cell> GetEnumerator() => ((IEnumerable<Cell>)_Cells).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	public class CellGroup : IReadOnlyList<Cell> {
		public Cell this[int index] => _Cells[index];

		public int Count => _Cells.Length;

		private readonly Cell[] _Cells;

		public CellGroup(IEnumerable<Cell> cells) {
			if (cells is null) {
				throw new ArgumentNullException(nameof(cells));
			}

			_Cells = new Cell[9];
			var i = 0;
			foreach (var cell in cells) {
				if (i >= _Cells.Length) {
					throw new ArgumentException("expecting 9 cells", nameof(cells));
				}
				if (cell is null) {
					throw new ArgumentException("contains null cell", nameof(cells));
				}
				_Cells[i++] = cell;
			}
			if (i != 9) {
				throw new ArgumentException("expecting 9 cells", nameof(cells));
			}
		}

		public NumberSet GetAnnotation() {
			NumberSet set = new();
			for (int i = 0; i < _Cells.Length; i++) {
				set |= _Cells[i];
			}
			return set;
		}

		public IEnumerator<Cell> GetEnumerator() => (IEnumerator<Cell>)_Cells.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override string ToString() => new StringBuilder("Group(").AppendJoin(", ", (object[])_Cells).Append(')').ToString();
	}
	public class Cell {
		public NumberSet Annotations => _Annotations;
		public int? Number {
			get => _Number;
			set {
				if (Number is < 1 or > 9) {
					throw new ArgumentOutOfRangeException(nameof(Number));
				}
				if (value is not null) {
					Annotations.Clear();
				}
				_Number = value;
			}
		}

		private NumberSet _Annotations;
		private int? _Number;

		public void SetAnnotation(int number, bool value) => _Annotations[number] = value;
		public void ClearAnnotations() => _Annotations.Clear();
	}
	public struct NumberSet : IEquatable<NumberSet> {
		public static bool operator ==(NumberSet lhs, NumberSet rhs) => lhs._bits == rhs._bits;
		public static bool operator !=(NumberSet lhs, NumberSet rhs) => !(lhs == rhs);

		public static NumberSet operator |(NumberSet lhs, Cell rhs) {
			if (rhs.Number is int n and >= 1 and <= 9) {
				lhs[n] = true;
			}
			return lhs;
		}
		public static NumberSet operator |(NumberSet lhs, NumberSet rhs) {
			return new() { _bits = lhs._bits | rhs._bits };
		}

		public static NumberSet operator &(NumberSet lhs, NumberSet rhs) => new() { _bits = lhs._bits & rhs._bits };

		public bool this[int number] {
			get {
				if (number is < 1 or > 9) {
					throw new ArgumentOutOfRangeException(nameof(number));
				}

				return (_bits & (1 << (number - 1))) != 0;
			}
			set {
				if (number is < 1 or > 9) {
					throw new ArgumentOutOfRangeException(nameof(number));
				}

				var mask = 1u << (number - 1);
				if (value) {
					_bits |= mask;
				}
				else {
					_bits &= ~mask;
				}
			}
		}

		public int Count => BitOperations.PopCount(_bits);

		private uint _bits;

		public void Clear() => _bits = 0;

		public bool Equals(NumberSet other) => this == other;

		public override bool Equals(object obj) => obj is NumberSet other && this == other;
		public override int GetHashCode() => _bits.GetHashCode();
	}
}
