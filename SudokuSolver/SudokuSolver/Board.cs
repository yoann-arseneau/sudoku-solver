using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;

namespace SudokuSolver {
	/// <summary>Mapping for linear addressing.</summary>
	public enum CellOrder {
		/// <summary>Left to right, top to bottom.</summary>
		RowMajor,
		/// <summary>Top to bottm, left to right.</summary>
		ColumnMajor,
	}

	/// <summary>Represents the state of a Sudoku game board.</summary>
	public class Board : IReadOnlyList<Cell>, IList<Cell> {
		/// <summary>
		///     <para>Creates a new <see cref="Board"/> and populates it
		///     according to <paramref name="state"/>. The ordering of the cells
		///     is indicated by <paramref name="order"/>. Having too many cell
		///     values is always an error. Not having enough cells is only an
		///     error if <paramref name="allowMissing"/> is
		///     <see langword="false"/>. Valid characters and their purpose are
		///     defined below; any other character is considered an
		///     error.</para>
		/// 
		///     <list type="table">
		///         <item>
		///             <term><c>[1-9]</c></term>
		///             <description>a cell value</description>
		///         </item>
		///         <item>
		///             <term><c>[0._]</c></term>
		///             <description>an empty cell</description>
		///         </item>
		///         <item>
		///             <term><c>[ \r\n|+-]</c></term>
		///             <description>ignored</description>
		///         </item>
		///     </list>
		/// </summary>
		/// <param name="state">
		///     The state of the new <see cref="Board"/>.
		/// </param>
		/// <param name="order">
		///     The order of cell values.
		/// </param>
		/// <param name="allowMissing">
		///     Whether running out of cells is acceptable.
		/// </param>
		/// <returns>
		///     A new <see cref="Board"/> as defined by <paramref name="state"/>.
		/// </returns>
		public static Board FromString(string state, CellOrder order, bool allowMissing = false) {
			if (state is not { Length: >= 9 * 9 }) {
				throw new ArgumentException($"expecting at least {9 * 9} characters");
			}
			if (!Enum.IsDefined(order)) {
				throw new ArgumentException($"unexpected order '{order}'", nameof(order));
			}

			Board board = new();
			var rowI = 0;
			var colI = 0;
			for (var i = 0; i < state.Length; ++i) {
				bool insert = false;
				switch (state[i]) {
					case var c and >= '1' and <= '9':
						set(board, rowI, colI, c - '0');
						insert = true;
						break;
					case '0' or '.' or '_':
						set(board, rowI, colI, null);
						insert = true;
						break;
					case ' ' or '\r' or '\n' or '|' or '-' or '+':
						break;
					case var c:
						throw new ArgumentException($"unexpected character at index {i}: '{c}'", nameof(state));
				}
				if (insert) {
					switch (order) {
						case CellOrder.RowMajor:
							increment(ref colI, ref rowI);
							break;
						case CellOrder.ColumnMajor:
							increment(ref rowI, ref colI);
							break;
						default:
							throw new InvalidOperationException($"unexpected order?! '{order}'");
					}
				}
			}

			if (!allowMissing && (rowI, colI) is not (0, 9) and not (9, 0)) {
				var number = order switch {
					CellOrder.RowMajor => rowI * 9 + colI,
					CellOrder.ColumnMajor => colI * 9 + rowI,
					_ => throw new InvalidOperationException($"unexpected order?! '{order}'"),
				};
				throw new InvalidOperationException($"expecting {9 * 9} cells; found {number}");
			}

			return board;

			static void set(Board board, int rowI, int colI, int? value) {
				Debug.Assert(board is not null);
				Debug.Assert(rowI >= 0);
				Debug.Assert(colI >= 0);
				Debug.Assert(value is null or (>= 1 and <= 9));

				if (rowI >= 9 || colI >= 9) {
					throw new ArgumentException($"more than {9 * 9} cells found", nameof(state));
				}

				board[rowI, colI].Number = value;
			}
			static void increment(ref int counter, ref int overflow) {
				if (counter == 8) {
					counter = 0;
					overflow += 1;
				}
				else {
					counter += 1;
				}
			}
		}

		/// <summary>
		///     Gets cell at zero-based rank <paramref name="index"/>, in
		///     row-major order.
		/// </summary>
		/// <param name="index">The zero-based rank of the cell.</param>
		/// <returns>The <see cref="Cell"/>.</returns>
		public Cell this[int index] {
			get {
				if (index is < 0 or >= 9 * 9) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				return _Cells[index];
			}
		}

		/// <summary>
		///     Gets cell at zero-based rank <paramref name="index"/>, in the
		///     specified <paramref name="order"/>.
		/// </summary>
		/// <param name="index">The zero-based rank of the cell.</param>
		/// <returns>The <see cref="Cell"/>.</returns>
		public Cell this[int index, CellOrder order] {
			get {
				if (index is < 0 or >= 9 * 9) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				return order switch {
					CellOrder.RowMajor => _Cells[index],
					CellOrder.ColumnMajor => _Cells[(index % 9 * 9) + index / 9],
					_ => throw new ArgumentException($"unexpected order '{order}'", nameof(order)),
				};
			}
		}
		/// <summary>
		///     Gets cell at zero-based coordinates <c>(<paramref name="row"/>,
		///     <paramref name="col"/>)</c>.
		/// </summary>
		/// <param name="row">The zero-based row of the cell.</param>
		/// <param name="col">The zero-based column of the cell.</param>
		/// <returns>The <see cref="Cell"/>.</returns>
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

		/// <summary>
		///     A read-only collection containing all the rows on this board in
		///     top-to-bottom order.
		/// </summary>
		public ReadOnlyCollection<CellGroup> Rows { get; }
		/// <summary>
		///     A read-only collection containing all the columns on this board
		///     in left-to-right order.
		/// </summary>
		public ReadOnlyCollection<CellGroup> Columns { get; }
		/// <summary>
		///     <para>A read-only collection containing all the squares on this
		///     board in row-major order.</para>
		/// 
		///     See also <seealso cref="CellOrder.RowMajor"/>.
		/// </summary>
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

			Rows = new(_Rows);
			Columns = new(_Columns);
			Squares = new(_Squares);

			// populate cells
			for (var i = 0; i < _Cells.Length; ++i) {
				_Cells[i] = new();
			}

			// populate groups
			for (var i = 0; i < 9; ++i) {
				_Rows[i] = new(this, getRow(i));
				_Columns[i] = new(this, getCol(i));
				_Squares[i] = new(this, getSqr(i));
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

		/// <summary>Checks whether the board has any errors.</summary>
		/// <returns>
		///     <see langword="true"/> if there are no errors; otherwise,
		///     <see langword="false"/>.
		/// </returns>
		public bool IsValid() {
			for (var i = 0; i < 9; ++i) {
				if (!_Rows[i].IsValid()) {
					return false;
				}
				if (!_Columns[i].IsValid()) {
					return false;
				}
				if (!_Squares[i].IsValid()) {
					return false;
				}
			}
			return true;
		}

		public IEnumerator<Cell> GetEnumerator() => ((IEnumerable<Cell>)_Cells).GetEnumerator();

		Cell IList<Cell>.this[int index] {
			get => this[index];
			set => throw new NotSupportedException();
		}
		int IList<Cell>.IndexOf(Cell item) => Array.IndexOf(_Cells, item);
		void IList<Cell>.Insert(int index, Cell item) => throw new NotSupportedException();
		void IList<Cell>.RemoveAt(int index) => throw new NotSupportedException();

		bool ICollection<Cell>.IsReadOnly => true;
		void ICollection<Cell>.Add(Cell item) => throw new NotImplementedException();
		void ICollection<Cell>.Clear() => throw new NotSupportedException();
		bool ICollection<Cell>.Contains(Cell item) => Array.IndexOf(_Cells, item) >= 0;
		void ICollection<Cell>.CopyTo(Cell[] array, int arrayIndex) => _Cells.CopyTo(array, arrayIndex);
		bool ICollection<Cell>.Remove(Cell item) => throw new NotImplementedException();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	/// <summary>Represent a group of cells on a Sudoku game board.</summary>
	public class CellGroup : IReadOnlyList<Cell>, IList<Cell> {
		/// <summary>
		///     <para>Gets cell at zero-based rank <paramref name="index"/>, in
		///     row-major order.</para>
		/// 
		///     See also <seealso cref="CellOrder.RowMajor"/>.
		/// </summary>
		/// <param name="index">The zero-based rank of the cell.</param>
		/// <returns>The <see cref="Cell"/>.</returns>
		public Cell this[int index] => _Cells[index];

		/// <summary>The board to which this group belongs.</summary>
		public Board Board { get; }

		public int Count => _Cells.Length;

		private readonly Cell[] _Cells;

		public CellGroup(Board board, IEnumerable<Cell> cells) {
			if (cells is null) {
				throw new ArgumentNullException(nameof(cells));
			}

			Board = board ?? throw new ArgumentNullException(nameof(board));
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

		/// <summary>
		///     Accumulate all unique numbers in this group into a
		///     <see cref="NumberSet"/>.
		/// </summary>
		public NumberSet GetAnnotation() {
			NumberSet set = new();
			for (int i = 0; i < _Cells.Length; i++) {
				set |= _Cells[i];
			}
			return set;
		}

		/// <summary>Checks whether the cell group has any errors.</summary>
		/// <returns>
		///     <see langword="true"/> if there are no errors; otherwise,
		///     <see langword="false"/>.
		/// </returns>
		public bool IsValid() {
			NumberSet set = new();
			for (int i = 0; i < _Cells.Length; i++) {
				if (_Cells[i].Number is int n && set[n]) {
					return false;
				}
				set |= _Cells[i];
			}
			return true;
		}

		public IEnumerator<Cell> GetEnumerator() => (IEnumerator<Cell>)_Cells.GetEnumerator();

		public override string ToString() => new StringBuilder("Group(").AppendJoin(", ", (object[])_Cells).Append(')').ToString();

		Cell IList<Cell>.this[int index] {
			get => this[index];
			set => throw new NotSupportedException();
		}
		int IList<Cell>.IndexOf(Cell item) => Array.IndexOf(_Cells, item);
		void IList<Cell>.Insert(int index, Cell item) => throw new NotSupportedException();
		void IList<Cell>.RemoveAt(int index) => throw new NotSupportedException();

		bool ICollection<Cell>.IsReadOnly => true;
		void ICollection<Cell>.Add(Cell item) => throw new NotImplementedException();
		void ICollection<Cell>.Clear() => throw new NotSupportedException();
		bool ICollection<Cell>.Contains(Cell item) => Array.IndexOf(_Cells, item) >= 0;
		void ICollection<Cell>.CopyTo(Cell[] array, int arrayIndex) => _Cells.CopyTo(array, arrayIndex);
		bool ICollection<Cell>.Remove(Cell item) => throw new NotImplementedException();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	/// <summary>Represents a cell in a Sudoku game board.</summary>
	public class Cell {
		/// <summary>A copy of this cell's annotations.</summary>
		public NumberSet Annotations => _Annotations;
		/// <summary>
		///     The current value of this cell, or <see langword="null"/> if this
		///     cell is empty.
		/// </summary>
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


		/// <summary>
		///     Set whether <paramref name="number"/> is annotated on this cell.
		/// </summary>
		/// <param name="number">
		///     The number to set or clear from this cell's annotations.
		/// </param>
		/// <param name="value">
		///     Whether annotation should be set or cleared.
		/// </param>
		public void SetAnnotation(int number, bool value) => _Annotations[number] = value;
		/// <summary>Overwrites the annotations on this cell.</summary>
		/// <param name="values">The new annotations for this cell.</param>
		public void SetAnnotations(NumberSet values) => _Annotations = values;
		/// <summary>Clears all annotations on this cell.</summary>
		public void ClearAnnotations() => _Annotations.Clear();
	}
	/// <summary>
	///     Represents a mutable set of numbers which is a subset of
	///     <c>[1-9]</c>.
	/// </summary>
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

		public static NumberSet operator &(NumberSet lhs, NumberSet rhs) {
			return new() { _bits = lhs._bits & rhs._bits };
		}

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

		/// <summary>Clear the set.</summary>
		public void Clear() => _bits = 0;

		public bool Equals(NumberSet other) => this == other;

		public override bool Equals(object obj) => obj is NumberSet other && this == other;
		public override int GetHashCode() => _bits.GetHashCode();
	}
}
