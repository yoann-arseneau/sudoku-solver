using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuSolver;
using System;

namespace SudokuSolverTests {
	[TestClass]
	public class NumberSetTests {
		[TestMethod]
		public void NumberSet_IntConversion() {
			Assert.AreEqual(NumberSet.Empty, null);
			for (var i = 1; i <= 9; ++i) {
				NumberSet expected = new() { [i] = true };
				Assert.AreEqual(expected, i);
				Assert.AreEqual(expected, (int?)i);
			}
		}

		[TestMethod]
		public void NumberSet_FromMaskTests() {
			Assert.AreEqual(NumberSet.Empty, NumberSet.FromMask(0));
			for (var i = 1; i <= 9; ++i) {
				Assert.AreEqual(i, NumberSet.FromMask(1u << (i - 1)));
			}
			Assert.AreEqual(NumberSet.Empty | 1 | 3 | 5 | 7 | 9, NumberSet.FromMask(0b101010101));
			Assert.AreEqual(NumberSet.Empty | 2 | 4 | 6 | 8, NumberSet.FromMask(0b010101010));
		}

		[TestMethod]
		public void NumberSet_OperatorTests() {
			var a = NumberSet.FromMask(0b01011);
			var b = NumberSet.FromMask(0b11110);
#pragma warning disable CS1718 // Comparison made to same variable
			Assert.IsTrue(a == a);
			Assert.IsTrue(b == b);
			Assert.IsFalse(a != a);
			Assert.IsFalse(b != b);
#pragma warning restore CS1718 // Comparison made to same variable
			Assert.IsTrue(a != b);
			Assert.IsFalse(a == b);
			Assert.AreEqual(NumberSet.FromMask(0b000011111), a | b);
			Assert.AreEqual(NumberSet.FromMask(0b000001010), a & b);
			Assert.AreEqual(NumberSet.FromMask(0b111101011), a | ~b);
			Assert.AreEqual(NumberSet.FromMask(0b000010100), ~a & b);
		}

		[TestMethod]
		public void NumberSet_ToStringTests() {
			Assert.AreEqual("NumberSet()", NumberSet.Empty.ToString());
			for (var i = 1; i <= 9; ++i) {
				Assert.AreEqual($"NumberSet({i})", ((NumberSet)i).ToString());
			}
			Assert.AreEqual("NumberSet(124578)", (NumberSet.Empty | 1 | 2 | 4 | 5 | 7 | 8).ToString());
			Assert.AreEqual("NumberSet(13579)", (NumberSet.Empty | 1 | 3 | 5 | 7 | 9).ToString());
			Assert.AreEqual("NumberSet(2468)", (NumberSet.Empty | 2 | 4 | 6 | 8).ToString());
		}
	}
}
