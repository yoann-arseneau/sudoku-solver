using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SudokuSolver {
	public class Board : IReadOnlyList<Cell> {
		public Cell this[int index] => _Cells[index];

		public int Count => _Cells.Length;

		private readonly Cell[] _Cells;
		private readonly CellGroup[] _Rows;
		private readonly CellGroup[] _Columns;
		private readonly CellGroup[] _Squares;

		public Board() {
			_Cells = new Cell[9 * 9];
			for (var i = 0; i < _Cells.Length; ++i) {
				_Cells[i] = new();
			}
			for (var gi = 0; gi < 9; ++gi) {
				_Rows[gi] = new(_Cells[gi..(gi + 9)]);
				_Columns[gi] = new(_Cells.Where((_, ci) => ci % 9 == ci));
				_Squares[gi] = new(_Cells.Where((_, ci) => (ci % 9) / 3 == gi % 3 && (ci / 27) == gi % 3));
			}
		}

		public CellGroup GetRow(int index) => _Rows[index];
		public CellGroup GetColumn(int index) => _Columns[index];
		public CellGroup GetSquare(int index) => _Squares[index];

		public IEnumerator<Cell> GetEnumerator() => ((IEnumerable<Cell>)_Cells).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	public class CellGroup : IReadOnlyList<Cell> {
		public Cell this[int index] => _Cells[index];

		public int Count => _Cells.Length;

		private readonly Cell[] _Cells;

		public CellGroup(IEnumerable<Cell> cells) {
			if (cells is null)
				throw new ArgumentNullException(nameof(cells));

			_Cells = new Cell[9];
			var i = 0;
			foreach (var cell in cells) {
				if (i >= _Cells.Length) {
					throw new ArgumentException("expecting 9 cells", nameof(cells));
				}
				if (cell is null) {
					throw new ArgumentException("contains null cell", nameof(cells));
				}
				_Cells[i] = cell;
			}
		}

		public NumberSet GetAnnotation() {
			NumberSet set = new();
			for (int i = 0; i < _Cells.Length; i++) {
				if (_Cells[i].Number is int number) {
					set[number] = true;
				}
			}
			return set;
		}

		public IEnumerator<Cell> GetEnumerator() => (IEnumerator<Cell>)_Cells.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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
	public struct NumberSet {
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
	}
}
