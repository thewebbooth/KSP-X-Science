using ScienceChecklist.Buttons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/*
 * Situation needs to take body or make more classes static.
 * SOI check for progress tracking
 * Instruments Class
 * Config file to fix DMagic and Solar Science problems
 * RECOVERY science
 * SCANSAT
 * RESOURCES
 * DMAGIC
 * Config for experiments
 * Asteroid science
 * 
*/



namespace ScienceChecklist {
	/// <summary>
	/// The main entry point into the addon. Constructed by the KSP addon loader.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public sealed class ScienceChecklistAddon : MonoBehaviour {

		#region METHODS (PUBLIC)

		/// <summary>
		/// Called by Unity once to initialize the class.
		/// </summary>
		public void Awake () {
			if (_addonInitialized == true) {
				// For some reason the addon can be instantiated several times by the KSP addon loader (generally when going to/from the VAB),
				// even though we set onlyOnce to true in the KSPAddon attribute.
				return;
			}

			Config.Load();

			_addonInitialized = true;
			_active = false;
			_logger = new Logger(this);
			_logger.Trace("Awake");
			_window = new ScienceWindow();
			_window.Settings.UseBlizzysToolbarChanged += Settings_UseBlizzysToolbarChanged;
			_window.OnCloseEvent += OnWindowClosed;
			
			_nextSituationUpdate = DateTime.Now;
			GameEvents.onGUIApplicationLauncherReady.Add(Load);
			GameEvents.onGUIApplicationLauncherDestroyed.Add(Unload);

			GameEvents.onVesselWasModified.Add( new EventData<Vessel>.OnEvent( this.VesselWasModified ) );
			GameEvents.onVesselChange.Add( new EventData<Vessel>.OnEvent( this.VesselChange ) );
			GameEvents.onEditorShipModified.Add( new EventData<ShipConstruct>.OnEvent( this.EditorShipModified ) );

			GameEvents.onGameStateSave.Add( new EventData<ConfigNode>.OnEvent( this.GameStateSave ) );
			GameEvents.OnPartPurchased.Add( new EventData<AvailablePart>.OnEvent( this.PartPurchased ) );
			GameEvents.OnTechnologyResearched.Add( new EventData<GameEvents.HostTargetAction<RDTech, RDTech.OperationResult>>.OnEvent( this.TechnologyResearched ) );
			GameEvents.OnScienceChanged.Add( new EventData<float, TransactionReasons>.OnEvent( this.ScienceChanged ) );
			GameEvents.OnScienceRecieved.Add( new EventData<float, ScienceSubject, ProtoVessel, bool>.OnEvent( this.ScienceRecieved ) );
			GameEvents.onVesselRename.Add( new EventData<GameEvents.HostedFromToAction<Vessel, string>>.OnEvent( this.VesselRename ) );
			GameEvents.OnKSCFacilityUpgraded.Add( new EventData<Upgradeables.UpgradeableFacility, int>.OnEvent( this.FacilityUpgrade ) );
		}

		/// <summary>
		/// Called by Unity once to initialize the class, just before Update is called.
		/// </summary>
		public void Start () {
			_logger.Trace("Start");
			DontDestroyOnLoad(this);
		}

		/// <summary>
		/// Called by Unity when the application is destroyed.
		/// </summary>
		public void OnApplicationQuit () {
			_logger.Trace("OnApplicationQuit");
			if (_button != null) {
				_button.Remove();
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button = null;
			}
		}

		/// <summary>
		/// Called by Unity when this instance is destroyed.
		/// </summary>
		public void OnDestroy () {
			if (_button != null) {
				_button.Remove();
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button = null;
			}
		}

		/// <summary>
		/// Called by Unity once per frame.
		/// </summary>
		public void Update( )
		{
			if (!_window.IsVisible) {
				return;
			}

			if (_nextSituationUpdate > DateTime.Now) {
				return;
			}

			_nextSituationUpdate = DateTime.Now.AddSeconds(0.5);
			_window.RecalculateSituation();
		}

		/// <summary>
		/// Called by Unity to draw the GUI - can be called many times per frame.
		/// </summary>
		public void OnGUI () {
			_window.Draw();
		}

		#endregion

		#region METHODS (PRIVATE)



		/// <summary>
		/// Initializes the addon if it hasn't already been loaded.
		/// Callback from onGUIApplicationLauncherReady
		/// </summary>
		private void Load () {
			_logger.Trace("Load");
			if (_active) {
				_logger.Info("Already loaded.");
				StartCoroutine( "WaitForRnDAndPartLoader" );

				return;
			}
			if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER && HighLogic.CurrentGame.Mode != Game.Modes.SCIENCE_SANDBOX) {
				_logger.Info("Game type is " + HighLogic.CurrentGame.Mode + ". Deactivating.");
				_active = false;
				return;
			}

			_logger.Info("Game type is " + HighLogic.CurrentGame.Mode + ". Activating.");
			_active = true;

			InitializeButton();
			_launcherVisible = true;
			ApplicationLauncher.Instance.AddOnShowCallback(Launcher_Show);
			ApplicationLauncher.Instance.AddOnHideCallback(Launcher_Hide);
			StartCoroutine( "WaitForRnDAndPartLoader" );
			StartCoroutine( "UpdateExperiments" );
			StartCoroutine( "RefreshFilter" );
		}



		/// <summary>
		/// Unloads the addon if it has been loaded.
		/// Callback from onGUIApplicationLauncherDestroyed
		/// </summary>
		private void Unload( )
		{
			if( !_active )
			{
				_logger.Info( "Already unloaded." );
				return;
			}
			_active = false;
			_logger.Info( "UNLOADING..." );

			if( _button != null )
				_button.Remove( );
			
			ApplicationLauncher.Instance.RemoveOnShowCallback( Launcher_Show );
			ApplicationLauncher.Instance.RemoveOnHideCallback( Launcher_Hide );
			_launcherVisible = false;
			StopCoroutine( "WaitForRnDAndPartLoader" );
			StopCoroutine( "UpdateExperiments" );
			StopCoroutine( "RefreshFilter" );
			_logger.Info( "UNLOADED" );
		}

		

		private void VesselWasModified( Vessel V )
		{
//			_logger.Trace( "Callback: VesselWasModified" );
			_filterRefreshPending = true;
		}

		private void VesselChange( Vessel V )
		{
//			_logger.Trace( "Callback: VesselChange" );
			_filterRefreshPending = true;
		}

		private void EditorShipModified( ShipConstruct S )
		{
//			_logger.Trace( "Callback: EditorShipModified" );
			_filterRefreshPending = true;
		}

		private void GameStateSave( ConfigNode C )
		{
//			_logger.Trace( "Callback: GameStateSave" );
			ScheduleExperimentUpdate( );
		}

		private void PartPurchased( AvailablePart P )
		{
//			_logger.Trace( "Callback: PartPurchased" );
			ScheduleExperimentUpdate( true );
		}

		private void TechnologyResearched( GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> Data )
		{
			if( Data.target == RDTech.OperationResult.Successful )
			{
//				_logger.Trace( "Callback: TechnologyResearched" );
				ScheduleExperimentUpdate( true );
			}
//			else
//				_logger.Trace( "Callback: Technology Research Failed" );
		}



		private void ScienceChanged( float V, TransactionReasons R )
		{
//			_logger.Trace( "Callback: ScienceChanged" );
			ScheduleExperimentUpdate( );
		}

		private void ScienceRecieved( float V, ScienceSubject S, ProtoVessel P, bool F )
		{
//			_logger.Trace( "Callback: ScienceRecieved" );
			ScheduleExperimentUpdate( );
		}

		private void VesselRename( GameEvents.HostedFromToAction<Vessel, string> Data )
		{
//			_logger.Trace( "Callback: VesselRename" );
			ScheduleExperimentUpdate( );
		}

		private void FacilityUpgrade( Upgradeables.UpgradeableFacility Data, int V )
		{
//			_logger.Trace( "Callback: KSP Facility Upgraded" );
			ScheduleExperimentUpdate( true );
		}



		/// <summary>
		/// Waits for the ResearchAndDevelopment and PartLoader instances to be available.
		/// </summary>
		/// <returns>An IEnumerator that can be used to resume this method.</returns>
		private IEnumerator WaitForRnDAndPartLoader( )
		{
			_logger.Info( "WaitForRnDAndPartLoader STARTED" );
			if( !_active )
			{
				yield break;
			}

			while( ResearchAndDevelopment.Instance == null )
			{
				yield return 0;
			}

			_logger.Info( "Science ready" );

			while( PartLoader.Instance == null )
			{
				yield return 0;
			}

			_logger.Info("PartLoader ready");
			_window.RefreshExperimentCache( );
			_logger.Info( "WaitForRnDAndPartLoader STOPPED" );
		}



		/// <summary>
		/// Coroutine to throttle calls to _window.UpdateExperiments.
		/// </summary>
		/// <returns></returns>
		private IEnumerator UpdateExperiments( )
		{
			_logger.Info( "UpdateExperiments STARTED" );
			while (true) {
				if (_window.IsVisible && _nextExperimentUpdate != null && _nextExperimentUpdate.Value < DateTime.Now) {
					if( _mustDoFullRefresh )
						_window.RefreshExperimentCache( );
					else
						_window.UpdateExperiments( );
					_nextExperimentUpdate = null;
					_mustDoFullRefresh = false;
				}

				yield return 0;
			}
		}



		/// <summary>
		/// Coroutine to throttle calls to _window.RefreshFilter.
		/// </summary>
		/// <returns></returns>
		private IEnumerator RefreshFilter( )
		{
			_logger.Info( "RefreshFilter STARTED" );
			var nextCheck = DateTime.Now;
			while (true) {
				if (_window.IsVisible && _filterRefreshPending && DateTime.Now > nextCheck) {
					nextCheck = DateTime.Now.AddSeconds(0.5);
					_window.RefreshFilter();
					_filterRefreshPending = false;
				}

				yield return 0;
			}
		}



		/// <summary>
		/// Called when the toolbar button is toggled on.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Button_Open (object sender, EventArgs e) {
			if (!_active) {
				return;
			}
			_logger.Trace("Button_Open");
			_windowVisible = true;
			UpdateVisibility();
		}



		/// <summary>
		/// Called when the toolbar button is toggled off.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Button_Close (object sender, EventArgs e) {
			if (!_active) {
				return;
			}
			_logger.Trace("Button_Close");
			_windowVisible = false;
			UpdateVisibility();
		}


		public void OnWindowClosed( object sender, EventArgs e  )
		{
			_button.SetOff( );
			_windowVisible = false;
		}





		/// <summary>
		/// Called when the KSP toolbar is shown.
		/// </summary>
		private void Launcher_Show () {
			if (!_active) {
				return;
			}
			_logger.Trace("Open");
			_launcherVisible = true;
			UpdateVisibility();
		}



		/// <summary>
		/// Called when the KSP toolbar is hidden.
		/// </summary>
		private void Launcher_Hide () {
			if (!_active) {
				return;
			}
			_logger.Trace("Close");
			_launcherVisible = false;
			UpdateVisibility();
		}



		/// <summary>
		/// Shows or hides the ScienceWindow iff the KSP toolbar is visible and the toolbar button is toggled on.
		/// </summary>
		private void UpdateVisibility () {
			if (!_active) {
				return;
			}
			_logger.Trace("UpdateVisibility");
			_window.IsVisible = _launcherVisible && _windowVisible;
			ScheduleExperimentUpdate();
		}



		/// <summary>
		/// Schedules a full experiment update in 1 second.
		/// </summary>
		private void ScheduleExperimentUpdate ( bool FullRefresh = false )
		{
			_nextExperimentUpdate = DateTime.Now.AddSeconds( 1 );
			if( FullRefresh )
				_mustDoFullRefresh = true;
		}



		/// <summary>
		/// Handler for the UseBlizzysToolbarChanged event on _window.Settings.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The EventArgs of the event.</param>
		private void Settings_UseBlizzysToolbarChanged (object sender, EventArgs e) {
			InitializeButton();


			// Need to set this
			if( _windowVisible )
				_button.SetOn( );
			else
				_button.SetOff( );
		}



		/// <summary>
		/// Initializes the toolbar button.
		/// </summary>
		private void InitializeButton () {
			if (_button != null) {
				_button.Open -= Button_Open;
				_button.Close -= Button_Close;
				_button.Remove();
				_button = null;
			}

			if (Config.UseBlizzysToolbar && BlizzysToolbarButton.IsAvailable) {
				_button = new BlizzysToolbarButton();
			} else {
				_button = new AppLauncherButton();
			}
			_button.Open += Button_Open;
			_button.Close += Button_Close;
			_button.Add();
		}

		#endregion

		#region FIELDS
		private DateTime		_nextSituationUpdate;
		private bool			_active;
		private Logger			_logger;
		private IToolbarButton	_button;
		private bool			_launcherVisible;
		private bool			_windowVisible;
		private ScienceWindow	_window;
		private DateTime?		_nextExperimentUpdate;
		private bool			_mustDoFullRefresh;
		private bool			_filterRefreshPending;
		private static bool		_addonInitialized;
		#endregion
	}
}
