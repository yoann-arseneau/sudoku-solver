using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuSolver;
using System;

namespace SudokuSolverTests {
	[TestClass]
	public class BoardTests {
		[TestMethod]
		public void Board_CtorTest() {
			Board board = new();
			Assert.AreEqual(9 * 9, board.Count);

			for (var i = 0; i < 9 * 9; ++i) {
				var cell = board[i];
				Assert.AreSame(cell, board[i, CellOrder.RowMajor]);
				Assert.AreSame(cell, board[(i % 9 * 9) + i / 9, CellOrder.ColumnMajor]);
				Assert.AreSame(cell, board[i / 9, i % 9]);
			}

			for (var i = 0; i < 9; ++i) {
				var row = board.Rows[i];
				var col = board.Columns[i];
				for (var j = 0; j < 9; ++j) {
					Assert.AreSame(board[i, j], row[j]);
					Assert.AreSame(board[j, i], col[j]);
				}

				var sqr = board.Squares[i];
				var sqri = ((i / 3) * 27) + ((i % 3) * 3);
				for (var j = 0; j < 9; j += 3) {
					var sqrj = j * 3;
					Assert.AreSame(board[sqri + sqrj + 0], sqr[j + 0]);
					Assert.AreSame(board[sqri + sqrj + 1], sqr[j + 1]);
					Assert.AreSame(board[sqri + sqrj + 2], sqr[j + 2]);
				}
			}
		}

		[TestMethod]
		public void Board_WhenOrderChanges_BoardIsTransposed() {
			var state = string.Create(9 * 9, new Random(), (span, rand) => {
				for (var i = 0; i < span.Length; ++i) {
					span[i] = (char)('0' + rand.Next(0, 10));
				}
			});

			var rowBoard = Board.FromString(state, CellOrder.RowMajor);
			var colBoard = Board.FromString(state, CellOrder.ColumnMajor);
			for (var rowI = 0; rowI < 9; ++rowI) {
				for (var colI = 0; colI < 9; ++colI) {
					var rowMajorNumber = rowBoard[rowI, colI].Number;
					var character = state[rowI * 9 + colI];
					Assert.AreEqual(
						character,
						rowMajorNumber switch {
							var n and >= 1 and <= 9 => '0' + n,
							null => '0',
							var n => throw new InvalidOperationException($"bad cell?! '{n}'"),
						});
					Assert.AreEqual(rowMajorNumber, colBoard[colI, rowI].Number);
				}
			}
		}
	}
}
