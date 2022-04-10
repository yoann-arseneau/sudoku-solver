using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuSolver;

namespace SudokuSolverTests {
	[TestClass]
	public class BoardTests {
		[TestMethod]
		public void Board_CtorTest() {
			Board board = new();
			Assert.AreEqual(9 * 9, board.Count);
			Assert.AreEqual(board.Count, board.Cells.Count);

			for (var rowi = 0; rowi < 9; ++rowi) {
				for (var coli = 0; coli < 9; ++coli) {
					var cell = board[rowi, coli];
					Assert.IsNotNull(cell);
					var celli = rowi * 9 + coli;
					Assert.AreSame(cell, board[celli]);
					Assert.AreSame(cell, board.Cells[celli]);
				}
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
	}
}
