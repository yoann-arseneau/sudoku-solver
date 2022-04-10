using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuSolver;
using System;
using System.Linq;

namespace SudokuSolverTests {
	[TestClass]
	public class CellGroupTests {
		[TestMethod]
		public void CellGroup_CtorTest() {
			Cell cell = new();
			Assert.ThrowsException<ArgumentException>(() => new CellGroup(Enumerable.Repeat<Cell>(null, 9)));
			Assert.ThrowsException<ArgumentException>(() => new CellGroup(Enumerable.Repeat(cell, 8)));
			Assert.ThrowsException<ArgumentException>(() => new CellGroup(Enumerable.Repeat(cell, 10)));
		}
	}
}
