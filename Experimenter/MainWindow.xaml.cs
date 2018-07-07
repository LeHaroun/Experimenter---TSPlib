using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

namespace Experimenter
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : System.Windows.Window
	{
		public int N_Experiments =0;
		

	   


		public MainWindow()
		{
			InitializeComponent();
		}

		private void Drop_DragEnter(object sender, System.Windows.DragEventArgs e)
		{
			
		}

		private void Drop_DragLeave(object sender, System.Windows.DragEventArgs e)
		{
			InformationLabel.Content = "Drop The Excel Template Here";
		}

		private void Drop_Drop(object sender, System.Windows.DragEventArgs e)
		{
			
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				InformationLabel.Content = "Processing The File...";

				string[] docPath = (string[])e.Data.GetData(DataFormats.FileDrop);

				this.UpdateLayout();
				InformationLabel.UpdateLayout();

				ReadFile(docPath[0]);
			   
			}

		   

		}

		private void Drop_DragOver(object sender, System.Windows.DragEventArgs e)
		{
			InformationLabel.Content = "Release The file !";
		}


		private void OpenExcel(string file)
		{

			Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();

			if (xlApp == null)
			{
				InformationLabel.Content = "EXCEL could not be started";
				return;
			}
			xlApp.Visible = true;

			Workbook wb = xlApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
			Worksheet ws = (Worksheet)wb.Worksheets[1];

			if (ws == null)
			{
				InformationLabel.Content = "Worksheet could not be created";
			}

			// Select the Excel cells, in the range c1 to c7 in the worksheet.
			Range aRange = ws.get_Range("C1", "C7");

			if (aRange == null)
			{
				InformationLabel.Content = "Could not get a range";
			}

			// Fill the cells in the C1 to C7 range of the worksheet with the number 6.
			Object[] args = new Object[1];
			args[0] = 6;
			aRange.GetType().InvokeMember("Value", BindingFlags.SetProperty, null, aRange, args);

			// Change the cells in the C1 to C7 range of the worksheet to the number 8.
			aRange.Value2 = 8;


		}

		private void ReadFile(string file)
		{

			Microsoft.Office.Interop.Excel.Application xlApp;
			Workbook xlWorkBook;
			Worksheet xlWorkSheet;
			object misValue = System.Reflection.Missing.Value;

			xlApp = new Microsoft.Office.Interop.Excel.Application();
			xlWorkBook = xlApp.Workbooks.Open(file, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
			xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

			
			// Counting the number of Experiments 
			for (int i = 6; i < 100; i++)
			{
				
				string str_ = str("A", i); 

				if (xlWorkSheet.get_Range(str_, str_).Value2 != null)

				N_Experiments ++ ; else break;
			}

			
			InformationLabel.Content = "Number Of Experiments : " + N_Experiments.ToString() ;




			//



			for (int i = 6; i < 6 + N_Experiments  ; i++)
			{
			  //System.Threading.
				string dir ;
			  double alpha, beta, rho, Q; 

			  int numCities, numAnts, maxTime;
			  dir = Convert.ToString(xlWorkSheet.get_Range(str("B",i), str("B",i)).Value2);
			  numCities = Convert.ToInt32( xlWorkSheet.get_Range(str("C",i), str("C",i)).Value2) ;
			  numAnts = Convert.ToInt32(xlWorkSheet.get_Range(str("D", i), str("D", i)).Value2);
			  alpha = xlWorkSheet.get_Range(str("E", i), str("E", i)).Value2;
			  beta = xlWorkSheet.get_Range(str("F", i), str("F", i)).Value2;
			  rho = xlWorkSheet.get_Range(str("G", i), str("G", i)).Value2;
			  Q = xlWorkSheet.get_Range(str("H", i), str("H", i)).Value2;
			  maxTime = Convert.ToInt32(xlWorkSheet.get_Range(str("I", i), str("I", i)).Value2);

			  Ant_Colony_Optim_TSP ACO = new Ant_Colony_Optim_TSP(dir, alpha, beta, rho, Q, numCities, numAnts, maxTime);
				
				//Ant_Colony_Optim ACO = new Ant_Colony_Optim(dir, alpha, beta, rho, Q, numCities, numAnts, maxTime);

				//Dynamic_Ant_Colony_Optim ACO = new Dynamic_Ant_Colony_Optim(dir, alpha, beta, rho, Q, numCities, numAnts, maxTime);

				xlWorkSheet.Cells[i, 10] = ACO.Time ;

				xlWorkSheet.Cells[i, 11] = ACO.BestInitial ;

				xlWorkSheet.Cells[i, 12] = ACO.Best;

				xlWorkSheet.Cells[i, 13] = (ACO.BestInitial - ACO.Best) / ACO.BestInitial * 100;

				ProgressLabel.Content = "Solving Problem "+ (i-5) + " of "+ N_Experiments;

				Progress.Value += 100 / N_Experiments;

				this.UpdateLayout();

				
			}


			xlWorkBook.Save();
			xlWorkBook.Close(true, misValue, misValue);
			xlApp.Quit();
			N_Experiments = 0;
			releaseObject(xlWorkSheet);
			releaseObject(xlWorkBook);
			releaseObject(xlApp);


		}

		private string str (string s, int i)
		{
			StringBuilder str = new StringBuilder();

			str.Append(s);
			str.Append(i.ToString());
			string str_ = str.ToString();
			return str_;
			
		}

		private void releaseObject(object obj)
		{
			try
			{
				System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
				obj = null;
			}
			catch (Exception ex)
			{
				obj = null;
				MessageBox.Show("Unable to release the Object " + ex.ToString());
			}
			finally
			{
				GC.Collect();
			}
		} 

	   
	}
}
