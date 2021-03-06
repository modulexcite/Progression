﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SLaks.Progression.Display;
using System.Windows.Input;
using System.Windows.Controls;

namespace SLaks.Progression.Tests.Display {
	[TestClass]
	public class ProgressModelTest : ProgressReporterTestBase {
		protected override IProgressReporter CreateReporter() { return new ProgressModel(); }

		public override bool SupportsCancellation { get { return true; } }

		[TestMethod]
		public void TestPropertyChanged() {
			PropertyChangedVerifier.TestPropertyChanged(new ProgressModel(),
				pm => pm.Maximum = 50,
				pm => pm.ScaledMaximum = 10,
				pm => pm.Progress = 25,
				pm => pm.ScaledMaximum = 100,
				pm => pm.Caption = "abc",
				pm => pm.AllowCancellation = true,
				pm => pm.WasCanceled = true,
				pm => pm.AllowCancellation = false
			);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CantCancelIllegally() {
			new ProgressModel().WasCanceled = true;
		}

		[TestMethod]
		public void TestCancelCommand() {
			var m = new ProgressModel();
			TestCancel(m, ProgressModel.GeneralCancelCommand, m);

			m = new ProgressModel();
			TestCancel(m, m.CancelCommand, null);
		}
		void TestCancel(ProgressModel model, ICommand cancelCommand, object parameter) {
			Assert.IsFalse(cancelCommand.CanExecute(parameter));
			int changeCount = 0;
			EventHandler changeHandler = delegate { changeCount++; };
			try {
				cancelCommand.CanExecuteChanged += changeHandler;

				model.AllowCancellation = true;
				Assert.AreEqual(1, changeCount, "Changing AllowCancellation didn't raise CanExecuteChanged");
				Assert.IsTrue(cancelCommand.CanExecute(parameter));

				cancelCommand.Execute(parameter);
				Assert.IsTrue(model.WasCanceled);

				model.AllowCancellation = false;
				Assert.AreEqual(2, changeCount, "Changing AllowCancellation didn't raise CanExecuteChanged");
				Assert.IsFalse(cancelCommand.CanExecute(parameter));

			} finally { cancelCommand.CanExecuteChanged -= changeHandler; }
		}

		[TestMethod]
		public void TestScaledMaximum() {
			var pm = new ProgressModel();
			pm.Maximum = 10;
			pm.Progress = 5;

			pm.ScaledMaximum = 2;
			Assert.AreEqual(1, pm.ScaledProgress);

			pm.ScaledMaximum = 100;
			Assert.AreEqual(10, pm.ScaledMaximum);
			Assert.AreEqual(5, pm.ScaledProgress);
		}

		[TestMethod]
		public void TestSourceProperty() {
			var pm = new ProgressModel { Progress = 50, ScaledMaximum = 2 };

			var bar = new ProgressBar();
			ProgressModel.SetSource(bar, pm);
			Assert.AreEqual(1, bar.Value);
			Assert.AreEqual(2, bar.Maximum);

			pm.Progress = null;

			Assert.AreEqual(true, bar.IsIndeterminate);
		}
	}
}
