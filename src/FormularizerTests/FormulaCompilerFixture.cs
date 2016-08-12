using System;
using System.Collections.Generic;
using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.FormulaBuilder;
using Formularizer.Core.FormulaCompiler;
using Formularizer.Core.FormulaDefinition;
using NUnit.Framework;

namespace FormularizerTests
{
	[TestFixture]
	public class FormulaCompilerFixture
	{
		[TestCaseSource(typeof(FormulaCompilerFixture), "SimpleFormulasOneSheet")]
		public void Simple_Formulas_One_Sheet_Test(Input input)
		{
			RunFormulas(input);
		}

		[TestCaseSource(typeof(FormulaCompilerFixture), "SimpleFormulasTwoSheets")]
		public void Simple_Formulas_Two_Sheet_Test(Input input)
		{
			RunFormulas(input);
		}

		[TestCaseSource(typeof(FormulaCompilerFixture), "DifferentComplexCases")]
		public void Different_Complex_Cases(Input input)
		{
			RunFormulas(input);
		}

		private void RunFormulas(Input input)
		{
			var compiler = new FormulaCompiler(input.DocumentInfo, new FailOnFailedFormulasCompilerStrategy());

			var compiledDoc = compiler.Compile();
			Assert.That(compiledDoc.Success, Is.True, compiledDoc.Error);

			var actualFormulas =
				compiledDoc.Value.Sheets.ToDictionary(kv => kv.Key,
					kv => kv.Value.Formulas.Select(f => FormulaInfo.Create(f.Reference, f.Value.Value.GetFormulaText())));

			foreach (var sheet in input.ExpectedFormulas)
			{
				Assert.That(actualFormulas.ContainsKey(sheet.Key), Is.True);
				var actualSheet = actualFormulas[sheet.Key];
				var expectedSheet = sheet.Value;

				foreach (var expectedFormula in expectedSheet)
				{
					var actualFormula =
						actualSheet.FirstOrDefault(
							f => f.Reference.Equals(expectedFormula.Reference,
								StringComparison.InvariantCultureIgnoreCase));

					Assert.That(actualFormula, Is.Not.Null);
					Assert.That(actualFormula.Value, Is.EqualTo(expectedFormula.Text));
				}
			}
		}

		public IEnumerable<Input> SimpleFormulasOneSheet
		{
			get
			{
				yield return Input.OneSheet("static",
					new[] {BuildFormula("A1", "=static text")},
					new[] {new FormulaResult("A1", "=static text")}
					);

				yield return Input.OneSheet("identity",
					new[] {BuildFormula("A1", "{id=1}")},
					new[] {new FormulaResult("A1", "")}
					);

				yield return Input.OneSheet("this",
					new[] {BuildFormula("A1", "{this}")},
					new[] {new FormulaResult("A1", "A1")}
					);

				yield return Input.OneSheet("cell scope = column",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("B4", "{cell '1' column}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("B4", "B2")
					}
					);

				yield return Input.OneSheet("cell scope = row",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("D2", "{cell '1' row}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("D2", "B2")
					}
					);

				yield return Input.OneSheet("cell scope = all",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("D2", "{cell '1' all}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("D2", "A1")
					}
					);

				yield return Input.OneSheet("cells scope = column",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("A2", "{id=1}"),
						BuildFormula("B1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("B4", "{cells '1' column}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", ""),
						new FormulaResult("B1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("B4", "B1, B2")
					}
					);

				yield return Input.OneSheet("cells scope = row",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("A2", "{id=1}"),
						BuildFormula("B1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("D2", "{cells '1' row}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", ""),
						new FormulaResult("B1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("D2", "A2, B2")
					}
					);

				yield return Input.OneSheet("cells scope = all",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("A2", "{id=1}"),
						BuildFormula("B1", "{id=1}"),
						BuildFormula("B2", "{id=1}"),
						BuildFormula("D2", "{cells '1' all}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", ""),
						new FormulaResult("B1", ""),
						new FormulaResult("B2", ""),
						new FormulaResult("D2", "A1, A2, B1, B2")
					}
					);

				yield return Input.OneSheet("column",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B1", "{id=1}"),
						BuildFormula("B4", "{column '1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B1", ""),
						new FormulaResult("B4", "A:A")
					}
					);
			}
		}


		public IEnumerable<Input> SimpleFormulasTwoSheets
		{
			get
			{
				yield return Input.TwoSheet("cell",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}")
					},
					new[]
					{
						BuildFormula("B4", "{cell '1' column 'sheet1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", "")
					},
					new[]
					{
						new FormulaResult("B4", "sheet1!B2")
					}
					);

				yield return Input.TwoSheet("cells",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("A2", "{id=1}")
					},
					new[]
					{
						BuildFormula("A4", "{cells '1' column 'sheet1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", "")
					},
					new[]
					{
						new FormulaResult("A4", "sheet1!A1, sheet1!A2")
					}
					);

				yield return Input.TwoSheet("column",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}")
					},
					new[]
					{
						BuildFormula("B4", "{column '1' 'sheet1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", "")
					},
					new[]
					{
						new FormulaResult("B4", "sheet1!A:A")
					}
					);

				// sheet name with whitespace
				yield return Input.TwoSheetWithWhitespace("cell",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}")
					},
					new[]
					{
						BuildFormula("B4", "{cell '1' column 'sheet 1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", "")
					},
					new[]
					{
						new FormulaResult("B4", "'sheet 1'!B2")
					}
					);

				yield return Input.TwoSheetWithWhitespace("cells",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("A2", "{id=1}")
					},
					new[]
					{
						BuildFormula("A4", "{cells '1' column 'sheet 1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", "")
					},
					new[]
					{
						new FormulaResult("A4", "'sheet 1'!A1, 'sheet 1'!A2")
					}
					);

				yield return Input.TwoSheetWithWhitespace("column",
					new[]
					{
						BuildFormula("A1", "{id=1}"),
						BuildFormula("B2", "{id=1}")
					},
					new[]
					{
						BuildFormula("B4", "{column '1' 'sheet 1'}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("B2", "")
					},
					new[]
					{
						new FormulaResult("B4", "'sheet 1'!A:A")
					}
					);
			}
		}


		public IEnumerable<Input> DifferentComplexCases
		{
			get
			{
				yield return Input.OneSheet("use own id",
					new[]
					{
						BuildFormula("A1", "{id=id_1}"),
						BuildFormula("A2", "{id=id_2}"),
						BuildFormula("A3", "{id=id_3}"),
						BuildFormula("A4", "{id=id_1}"),
						BuildFormula("A7", "{id=1}{cells 'id_*' column}")
					},
					new[]
					{
						new FormulaResult("A1", ""),
						new FormulaResult("A2", ""),
						new FormulaResult("A3", ""),
						new FormulaResult("A4", ""),
						new FormulaResult("A7", "A1, A4")
					}
					);
			}
		}

		public class FormulaResult
		{
			public FormulaResult(string reference, string text)
			{
				Reference = reference;
				Text = text;
			}

			public string Reference { get; set; }
			public string Text { get; set; }
		}

		public class Input
		{
			private Input(string description, DocumentInfo<IFormulaPartDef> documentInfo,
				IDictionary<string, FormulaResult[]> expectedFormulas)
			{
				Description = description;
				DocumentInfo = documentInfo;
				ExpectedFormulas = expectedFormulas;
			}

			public string Description { get; private set; }
			public DocumentInfo<IFormulaPartDef> DocumentInfo { get; private set; }
			public IDictionary<string, FormulaResult[]> ExpectedFormulas { get; private set; }

			public static Input OneSheet(string description, IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs,
				FormulaResult[] sheetResults1)
			{
				return new Input(description + " (one sheet)", GenerateOneSheetTestInfo(formulaDefs),
					new Dictionary<string, FormulaResult[]> {{"sheet1", sheetResults1}});
			}

			public static Input TwoSheet(string description, IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs1,
				IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs2, FormulaResult[] sheetResults1, FormulaResult[] sheetResults2)
			{
				return new Input(description + " (two sheets)", GenerateTwoSheetTestInfo(formulaDefs1, formulaDefs2),
					new Dictionary<string, FormulaResult[]>
					{
						{"sheet1", sheetResults1},
						{"sheet2", sheetResults2}
					});
			}

			public static Input TwoSheetWithWhitespace(string description, IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs1,
				IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs2, FormulaResult[] sheetResults1, FormulaResult[] sheetResults2)
			{
				return new Input(description + " (two sheets w/ whitespace)",
					GenerateTwoSheetTestInfoWithWhitespace(formulaDefs1, formulaDefs2),
					new Dictionary<string, FormulaResult[]>
					{
						{"sheet 1", sheetResults1},
						{"sheet 2", sheetResults2}
					});
			}

			public override string ToString()
			{
				return Description;
			}
		}

		private static FormulaInfo<IFormulaPartDef> BuildFormula(string reference, string rawFormula)
		{
			var builder = new FormulaDefinitionBuilder();
			return FormulaInfo.Create(reference, builder.BuildDef(FormulaInfo.Create(reference, rawFormula)).Value);
		}

		private static DocumentInfo<IFormulaPartDef> GenerateOneSheetTestInfo(
			IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs)
		{
			var sheets = new Dictionary<string, SheetInfo<IFormulaPartDef>>
			{
				{"sheet1", SheetInfo.Create("rId1", "sheet1", formulaDefs.ToList())}
			};

			return DocumentInfo.Create(sheets);
		}

		private static DocumentInfo<IFormulaPartDef> GenerateTwoSheetTestInfo(
			IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs1,
			IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs2)
		{
			var sheets = new Dictionary<string, SheetInfo<IFormulaPartDef>>
			{
				{"sheet1", SheetInfo.Create("rId1", "sheet1", formulaDefs1.ToList())},
				{"sheet2", SheetInfo.Create("rId2", "sheet2", formulaDefs2.ToList())}
			};

			return DocumentInfo.Create(sheets);
		}

		private static DocumentInfo<IFormulaPartDef> GenerateTwoSheetTestInfoWithWhitespace(
			IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs1,
			IEnumerable<FormulaInfo<IFormulaPartDef>> formulaDefs2)
		{
			var sheets = new Dictionary<string, SheetInfo<IFormulaPartDef>>
			{
				{"sheet 1", SheetInfo.Create("rId1", "sheet 1", formulaDefs1.ToList())},
				{"sheet 2", SheetInfo.Create("rId2", "sheet 2", formulaDefs2.ToList())}
			};

			return DocumentInfo.Create(sheets);
		}
	}
}