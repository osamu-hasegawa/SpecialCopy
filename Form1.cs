using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SpecialCopy
{
    public partial class Form1 : Form
    {
		int type = 0;
		string startDate = "";
		string endDate = "";
		DateTime dt1;
		DateTime dt2;
		string ext = "";
        string srcfolderPath = "";
        string dstfolderPath = "";
		long totalFiles = 0;
		long fileCount = 0;

        public Form1()
        {
            InitializeComponent();
			label1.Text = "";
			label2.Text = "";
			label3.Text = "";
			button4.Enabled = false;
			//フォームが最大化されないようにする
			this.MaximizeBox = false;
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
			textBox1.Text = "";
            monthCalendar1.Visible = true;
			type = 1;
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
			DateTime start = e.Start;
			if(type == 1)
			{
				textBox1.Text = start.ToString("yyyy/MM/dd");
			}
			else if(type == 2)
			{
				textBox2.Text = start.ToString("yyyy/MM/dd");
			}
			monthCalendar1.Visible = false;
        }

        private void textBox2_MouseClick(object sender, MouseEventArgs e)
        {
			textBox2.Text = "";
            monthCalendar1.Visible = true;
			type = 2;
        }


        private void button1_Click(object sender, EventArgs e)
        {
			textBox3.Text = "";
			//FolderBrowserDialogクラスのインスタンスを作成
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			//上部に表示する説明テキストを指定する
			fbd.Description = "フォルダを指定してください。";
			//ルートフォルダを指定する
			//デフォルトでDesktop
			fbd.RootFolder = Environment.SpecialFolder.Desktop;
			//最初に選択するフォルダを指定する
			//RootFolder以下にあるフォルダである必要がある
			fbd.SelectedPath = @"C:\";
			//ユーザーが新しいフォルダを作成できるようにする
			//デフォルトでTrue
			fbd.ShowNewFolderButton = true;

			//ダイアログを表示する
			if (fbd.ShowDialog(this) == DialogResult.OK)
			{
			    //選択されたフォルダを表示する
				textBox3.Text = fbd.SelectedPath;
			}
        }

        private void button2_Click(object sender, EventArgs e)
        {
			textBox4.Text = "";
			//FolderBrowserDialogクラスのインスタンスを作成
			FolderBrowserDialog fbd = new FolderBrowserDialog();

			//上部に表示する説明テキストを指定する
			fbd.Description = "フォルダを指定してください。";
			//ルートフォルダを指定する
			//デフォルトでDesktop
			fbd.RootFolder = Environment.SpecialFolder.Desktop;
			//最初に選択するフォルダを指定する
			//RootFolder以下にあるフォルダである必要がある
			fbd.SelectedPath = @"C:\";
			//ユーザーが新しいフォルダを作成できるようにする
			//デフォルトでTrue
			fbd.ShowNewFolderButton = true;

			//ダイアログを表示する
			if (fbd.ShowDialog(this) == DialogResult.OK)
			{
			    //選択されたフォルダを表示する
				textBox4.Text = fbd.SelectedPath;
			}
        }

        private void button3_Click(object sender, EventArgs e)
        {
			if(textBox1.Text == "")
			{
				MessageBox.Show("開始日を指定してください。",
				"エラー",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				return;
			}
			if(textBox2.Text == "")
			{
				MessageBox.Show("終了日を指定してください。",
				"エラー",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				return;
			}
			if(textBox3.Text == "")
			{
				MessageBox.Show("コピー元フォルダを指定してください。",
				"エラー",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				return;
			}
			if(textBox4.Text == "")
			{
				MessageBox.Show("コピー先フォルダを指定してください。",
				"エラー",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				return;
			}
			if(textBox5.Text == "")
			{
				MessageBox.Show("コピーするファイルの拡張子を指定してください。",
				"エラー",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				return;
			}

			label1.Text = "実行中";
			ext = "." + textBox5.Text;

			startDate = textBox1.Text;
			startDate += " 00:00:00";
			dt1 = DateTime.Parse(startDate);

			endDate = textBox2.Text;
			endDate += " 23:59:59";
//			endDate += " 00:00:00";
			dt2 = DateTime.Parse(endDate);

            srcfolderPath = textBox3.Text + "\\";
            dstfolderPath = textBox4.Text + "\\";

			button3.Enabled = false;
			button4.Enabled = true;

            backgroundWorker1.WorkerSupportsCancellation = true;
            fileCount = 0;
            backgroundWorker1.RunWorkerAsync(0);
      }




		public void DeleteIfEmpty(string folder)
		{
			foreach (var subdir in Directory.GetDirectories(folder))
			{
				DeleteIfEmpty(subdir);
			}

			if(IsDirectoryEmpty(folder))
			{
				Directory.Delete(folder);
			}
		}

		 private bool IsDirectoryEmpty(string path)
		{
			return !Directory.EnumerateFileSystemEntries(path).Any();
		}

		//ディレクトリのコピー
		public void DirectoryCopy(string sourcePath, string destinationPath)
		{
			DirectoryInfo sourceDirectory = new DirectoryInfo(sourcePath);
			DirectoryInfo destinationDirectory = new DirectoryInfo(destinationPath);
			
			//コピー先のディレクトリがなければ作成する
			if(destinationDirectory.Exists == false)
			{
				destinationDirectory.Create();
				destinationDirectory.Attributes = sourceDirectory.Attributes;
			}

			//ファイルのコピー
			foreach(FileInfo fileInfo in sourceDirectory.GetFiles()) 
			{
				// キャンセル通知があればスレッドをキャンセルする
				if(backgroundWorker1.CancellationPending)
				{
					return;
				}

                //進捗率をUIスレッドに送信
                string msg = fileInfo.Name;
                int prog = (int)(((double)(fileCount + 1) / (double)totalFiles) * 100f);//[%]となる
                backgroundWorker1.ReportProgress(prog, msg);
                fileCount++;

				DateTime fileDate = System.IO.File.GetLastWriteTime(fileInfo.FullName);
				if(dt1 <= fileDate && fileDate <= dt2)//指定した期間に更新されたファイルか
				{
					if(System.IO.Path.GetExtension(fileInfo.FullName) == ext)//特定拡張子のみ
					{
						//同じファイルが存在していたら、常に上書きする
						fileInfo.CopyTo(destinationDirectory.FullName + @"\" + fileInfo.Name, true);
					}
				}
			}

			//ディレクトリのコピー（再帰を使用）
			foreach(System.IO.DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
			{
				DirectoryCopy(directoryInfo.FullName, destinationDirectory.FullName + @"\" + directoryInfo.Name);
			}
		}

		public long GetFolderSize(DirectoryInfo di)
		{
		    long size = 0;
		    // ファイルの一覧を取得し、ファイルサイズを加算する
		    foreach (FileInfo f in di.GetFiles())
		    {
                size++;
//		        size += f.Length; // ファイルサイズを加算（バイト）
		    }
		    // サブディレクトリに対して再帰処理
		    foreach (DirectoryInfo d in di.GetDirectories())
		    {
		        size += GetFolderSize(d);
		    }
		    return size;
		}

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
			//対象データフォルダ配下のファイルの総数を取得する→進捗に使用する
			//対象データフォルダ取得          
			DirectoryInfo di = new DirectoryInfo(srcfolderPath);
			//フォルダサイズ取得
			totalFiles = GetFolderSize(di);

			//フォルダをコピーしながら、指定のファイルのみをコピーする
			DirectoryCopy(srcfolderPath, dstfolderPath);
			//空のフォルダを削除する
			DeleteIfEmpty(dstfolderPath);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label2.Text = e.ProgressPercentage + " %";
            label3.Text = (string)e.UserState;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                label1.Text = "キャンセルされました";
				button4.Enabled = false;
                return;
            }

            label1.Text = "終了しました";
			label3.Text = "";
			button3.Enabled = true;
			button4.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
			if(backgroundWorker1.IsBusy)
			{
	            backgroundWorker1.CancelAsync();
	        }
        }
    }
}
