using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace Tresor
{
	internal sealed class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);			
			Application.Run(new ControllForm());
		}
		
	}
	
	#region FormDesigner
	partial class ControllForm
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button _StartTresorBtn = new System.Windows.Forms.Button();
		
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		private void InitializeComponent()
		{
			this._StartTresorBtn.Location = new System.Drawing.Point(10, 10);
			this._StartTresorBtn.Size = new System.Drawing.Size(200,20);
			this._StartTresorBtn.Text = "Erstelle neuen Tresor";
			this._StartTresorBtn.Click += new System.EventHandler(this.Btn_CreateNewTresor_Click);
			this.Controls.Add(this._StartTresorBtn);
			
			this.Text = "Ihr Tresor Verkäufer";
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}
	}
	
	partial class MainForm
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.TextBox _StatusBox = new System.Windows.Forms.TextBox();
		private System.Windows.Forms.Button _ChangeStatusBtn = new System.Windows.Forms.Button();
		private System.Windows.Forms.Button _CreateTresor = new System.Windows.Forms.Button();
		
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		private void InitializeComponent()
		{
			this._StatusBox.Location = new System.Drawing.Point(10, 10);
			this._StatusBox.Size = new System.Drawing.Size(200, 20);
			this.Controls.Add(this._StatusBox);
			
			this._ChangeStatusBtn.Location = new System.Drawing.Point(10, 40);
			this._ChangeStatusBtn.Size = new System.Drawing.Size(200,20);
			this._ChangeStatusBtn.Enabled = false;
			this._ChangeStatusBtn.Click += new System.EventHandler(this.Btn_ChangeStatus_Click);
			this.Controls.Add(this._ChangeStatusBtn);
			
			this._CreateTresor.Location = new System.Drawing.Point(10, 80);
			this._CreateTresor.Size = new System.Drawing.Size(200,40);
			this._CreateTresor.Text = "Tresor Einrichten";
			this._CreateTresor.Click += new System.EventHandler(this.Btn_CreateTresor_Click);
			this.Controls.Add(this._CreateTresor);
			
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		}
	}
	#endregion FormDesigner
	
	#region FrontEnd
	public interface IFormObserver
    {
        void Update();
    }
	
	public partial class ControllForm : Form
	{		
		public ControllForm()
		{
			InitializeComponent();
		}
		
		private void Btn_CreateNewTresor_Click(object sender, EventArgs e) {
			new Thread(() => 
			{
			    Thread.CurrentThread.IsBackground = false;
			    Application.Run(new MainForm());
			}).Start();
		}
	}
	
	public partial class MainForm : Form, IFormObserver
	{
		private Tresor _tresor = null;
		
		public MainForm()
		{
			InitializeComponent();
		}
		
		private void Btn_ChangeStatus_Click(object sender, EventArgs e) {
			this._tresor.ChangeStatus(this._StatusBox.Text);
		}
		
		private void Btn_CreateTresor_Click(object sender, EventArgs e) {
			if (this._StatusBox.Text == "") {
				MessageBox.Show("Bitte vergeben Sie ein Passwort!");
			} else {
				this._ChangeStatusBtn.Enabled = true;
				this._CreateTresor.Enabled = false;
				this._tresor = new Tresor(StateClose.GetInstance(), this._StatusBox.Text);
				this._tresor.Attach(this);
			}
		}
		
		void IFormObserver.Update()
		{
			if (this._tresor.GetStatus() == StateOpen.GetInstance())
			{
				this.Text = "Offen";
				this._ChangeStatusBtn.Text = "Abschließen";
			}
			else if (this._tresor.GetStatus() == StateClose.GetInstance()) 
			{
				this.Text = "Geschlossen";
				this._ChangeStatusBtn.Text = "Öffnen";
			}
			else {
				MessageBox.Show("Error :-(");
			}
		}
	}
	
	#endregion FrontEnd
	
	#region Status
	public interface ITresorState
	{
		void ChangeState(Tresor tr);
	}
	
	public class StateOpen : ITresorState
	{
		private static ITresorState _tresorState = null;
		
		public static ITresorState GetInstance()
		{
			if (_tresorState == null)
			{
				_tresorState = new StateOpen();
			}
			return _tresorState;
		}
		
		public void ChangeState(Tresor tr)
		{
			tr.SetStatus(StateClose.GetInstance());
		}
	}
	
	public class StateClose : ITresorState
	{
		private static ITresorState _tresorState = null;
		
		public static ITresorState GetInstance()
		{
			if (_tresorState == null)
			{
				_tresorState = new StateClose();
			}
			return _tresorState;
		}
		
		public void ChangeState(Tresor tr)
		{
			tr.SetStatus(StateOpen.GetInstance());
		}
	}
	
	#endregion Status
	
	#region TresorLogik
	public abstract class Subject
	{
	    private readonly List<IFormObserver> formObserverList = new List<IFormObserver>();
	
	    public void Attach(IFormObserver newObserver)
	    {
	        formObserverList.Add(newObserver);
	        Notify();
	    }
	
	    public void Detach(IFormObserver newObserver)
	    {
	        formObserverList.Remove(newObserver);
	    }
	
	    protected void Notify()
	    {
	    	foreach (IFormObserver ob in formObserverList) {
	    		ob.Update();
	    	}
	    }
	}
	
	public class Tresor : Subject
	{
		private String _secret = "geheim-1";
		private ITresorState _tresorStatus = null;
		
		public Tresor(ITresorState newState, String ownDefaultSecret) {
			if (validSecret(ownDefaultSecret)) {
				this._secret = ownDefaultSecret;
			} else {
				MessageBox.Show("Ihr definiertes Start-Passwort ust ungültig. Es wird der Standard verwendet!");
			}
			this.SetStatus(newState);
		}
		
		public Tresor(ITresorState newState) {
			this.SetStatus(newState);
		}
		
		public ITresorState GetStatus() {
			return _tresorStatus;
		}
		
		public void SetStatus(ITresorState newTresorStatus) {
			_tresorStatus = newTresorStatus;
			Notify();
		}
		
		public void ChangeStatus(string InSecret) {
			if (this._tresorStatus == StateOpen.GetInstance())
			{
				if (validSecret(InSecret)) {
					this._secret = InSecret;
					_tresorStatus.ChangeState(this);	
				} else {
					MessageBox.Show("Das Passwort muss länger als 7 Zeichen sein, Buchstaben und Zahlen beinhalten.");
				}
			}
			else if (this._tresorStatus == StateClose.GetInstance()) 
			{
				if (InSecret == this._secret) {
					_tresorStatus.ChangeState(this);
				} else {
					MessageBox.Show("Falsches Passwort");
				}
			}
			else {
				MessageBox.Show("Error :-(");
			}
			Notify();
		}
		
		private Boolean validSecret(String Secret) {
			if (Secret.Length >= 8 &&
			    Secret.Any(char.IsDigit) &&
			    Secret.Any(char.IsLetter)) {
				return true;
			}
			return false;
		}
	}
	#endregion TresorLogik
}
