using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuSolver;

namespace SudokuSolverTests {
	[TestClass]
	public class BoardTests {
		[TestMethod]
		public void Board_MappingTest() {
			Board board = new();
			for (var i = 0; i < 9; ++i) {
				var row = board.GetRow(i);
				var col = board.GetColumn(i);
				for (var j = 0; j < 9; ++j) {
					Assert.AreSame(board[i, j], row[j]);
					Assert.AreSame(board[j, i], col[j]);
				}
				var sqri = ((i / 3) * 27) + ((i % 3) * 3);
				var sqr = board.GetSquare(i);
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
