using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



namespace ScienceChecklist
{
	class StatusWindow : Window<ScienceChecklistAddon>
	{
		public event EventHandler				NoiseEvent;

//		private GUIStyle						_labelStyle;
//		private GUIStyle						sectionStyle;
		private readonly ExperimentFilter		_filter;
		private readonly ScienceChecklistAddon	_parent;
		private readonly Logger					_logger;
		private GUIStyle						_experimentButtonStyle;
		private GUIStyle						_experimentLabelStyle;
		private	IList<ModuleScienceExperiment>	_moduleScienceExperiments;
		private Dictionary<string, bool>		_availableScienceExperiments;

		public StatusWindow( ScienceChecklistAddon Parent )
			: base( "[x] Science! Here and Now", 250, 30 )
		{
			_parent = Parent;
			_logger = new Logger( this );
			_filter = new ExperimentFilter( _parent );
			_filter.DisplayMode = DisplayMode.CurrentSituation;
			_filter.EnforceLabLanderMode = true;

			_availableScienceExperiments = new Dictionary<string, bool>( );

			_parent.Config.HideCompleteEventsChanged += ( s, e ) => RefreshFilter( s, e );
			_parent.Config.CompleteWithoutRecoveryChanged += ( s, e ) => RefreshFilter( s, e );

			_parent.ScienceEventHandler.FilterUpdateEvent += ( s, e ) => RefreshFilter( s, e );
			_parent.ScienceEventHandler.SituationChanged += ( s, e ) => UpdateSituation( s, e );
			this.Resizable = false;
			_filter.UpdateFilter( );
		}



		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles( );

/*			if( labelStyle == null )
			{
				labelStyle = new GUIStyle( _skin.label );
				labelStyle.wordWrap = true;
				labelStyle.fontStyle = FontStyle.Normal;
				labelStyle.normal.textColor = Color.white;
				labelStyle.stretchWidth = true;
				labelStyle.stretchHeight = false;
				labelStyle.margin.bottom -= 2;
				labelStyle.padding.bottom -= 2;
			}
			if( sectionStyle == null )
			{
				sectionStyle = new GUIStyle( labelStyle );
				sectionStyle.fontStyle = FontStyle.Bold;
			}*/
			_experimentButtonStyle = new GUIStyle( _skin.button )
			{
				fontSize = 18
			};
			_experimentLabelStyle = new GUIStyle( _experimentButtonStyle )
			{
				fontSize = 18,
				normal = { textColor = Color.black }
			};
		}



		protected override void DrawWindowContents( int windowID )
		{
			GUILayout.Space( 25 );
			GUILayout.BeginVertical( );
			if( _filter.DisplayScienceInstances != null )
			{
				for( var i = 0; i < _filter.DisplayScienceInstances.Count; i++ )
				{

					var rect = new Rect( 5, 20 * i, 0, 20 );
					var experiment = _filter.DisplayScienceInstances[ i ];
					DrawExperiment( experiment, rect );
				}
			}
			else
				_logger.Trace( "DisplayExperiments is null" );

			GUILayout.EndVertical( );

/*			GUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
			if( GUILayout.Button( new GUIContent( "Kill Warp", "Kill Warp" ), GUILayout.Width( 100 ), GUILayout.Height( 23 ) ) )
			{
				GameHelper.StopTimeWarp( );
			}

			if( GUILayout.Button( new GUIContent( "Play Noise", "Play Noise" ), GUILayout.Width( 100 ), GUILayout.Height( 23 ) ) )
			{
				PlayNoise( );
			}
			GUILayout.EndHorizontal( );*/

			GUILayout.Space( 2 );
		}



		public override void DrawWindow( )
		{
			windowPos.height = 50;
			base.DrawWindow( );
		}



		private void PlayNoise( )
		{
			if( NoiseEvent != null )
			{
				NoiseEvent( this, EventArgs.Empty );
			}
		}



		private void DrawExperiment( ScienceInstance exp, Rect rect )
		{
			bool ExperimentRunnable = CanRunExperiment( exp, true );
			Rect progressRect = new Rect(rect) {
				xMin = rect.xMax - ( 105),
				xMax = rect.xMax - (40),
				y = rect.y + ( 3)
			};
			
			if( ExperimentRunnable )
			{
				_experimentButtonStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
				if( GUILayout.Button( exp.ShortDescription, _experimentButtonStyle ) )
				{
					_logger.Trace( "Pop" );
					RunExperiment( exp );
				}
			}
			else
			{
				GUILayout.Label( exp.ShortDescription, _experimentLabelStyle );
			}




//			ProgressBar(progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience);
		}



		private void ProgressBar( Rect rect, float curr, float total, float curr2 )
		{
/*			var completeTexture = compact ? _completeTextureCompact : _completeTexture;
			var progressTexture = compact ? _progressTextureCompact : _progressTexture;
			var complete = curr > total || (total - curr < 0.1);
			if (complete) {
				curr = total;
			}
			var progressRect = new Rect(rect) {
				y = rect.y + (compact ? 3 : 1),
			};

			if (curr2 != 0 && !complete) {
				var complete2 = false;
				if (curr2 > total || (total - curr2 < 0.1)) {
					curr2 = total;
					complete2 = true;
				}
				_skin.horizontalScrollbarThumb.normal.background = curr2 < 0.1
					? _emptyTexture
					: complete2
						? completeTexture
						: progressTexture;

				GUI.HorizontalScrollbar(progressRect, 0, curr2 / total, 0, 1, _horizontalScrollbarOnboardStyle);
			}

			_skin.horizontalScrollbarThumb.normal.background = curr < 0.1
				? _emptyTexture
				: complete ? completeTexture : progressTexture;

			GUI.HorizontalScrollbar(progressRect, 0, curr / total, 0, 1);

			if (showValues) {
				var labelRect = new Rect(rect) {
					y = rect.y - 1,
				};
				GUI.Label(labelRect, string.Format("{0:0.#}  /  {1:0.#}", curr, total), _progressLabelStyle);
			}*/
		}



		#region Events called when science changes
		// Refreshes the experiment filter.
		// This is the lightest update used when the vessel changes
		public void RefreshFilter( object sender, EventArgs e )
		{
//			_logger.Trace( "RefreshFilter" );

			if( _moduleScienceExperiments != null )
				_moduleScienceExperiments.Clear( );
			if( _availableScienceExperiments != null )
				_availableScienceExperiments.Clear( );


			Vessel v = FlightGlobals.ActiveVessel;
			if( v != null && HighLogic.LoadedScene == GameScenes.FLIGHT )
				_moduleScienceExperiments = v.FindPartModulesImplementing<ModuleScienceExperiment>( );



			_filter.UpdateFilter( );
		}



		// Bung new situation into filter and recalculate everything
		public void UpdateSituation( object sender, NewSituationData e )
		{
//			_logger.Trace( "UpdateSituation" );
			_filter.CurrentSituation = new Situation( e._body, e._situation, e._biome, e._subBiome );
			RefreshFilter( sender, e );
			if( _filter.TotalCount > 0 )
			{
				if( _filter.TotalCount - _filter.CompleteCount > 0 )
				{
					GameHelper.StopTimeWarp( );
					PlayNoise( );
				}
			}
		}
		#endregion



		public bool CanRunExperiment( ScienceInstance s, bool runSingleUse = true )
		{
			bool IsAvailable = false;
			if( _availableScienceExperiments.ContainsKey( s.ScienceExperiment.id ) )
				return _availableScienceExperiments[ s.ScienceExperiment.id ];
			if( _moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0 )
			{
				IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
					x.experimentID == s.ScienceExperiment.id &&
					!(x.GetScienceCount() > 0) &&
					(x.rerunnable || runSingleUse) &&
					!x.Inoperable
					));

				IsAvailable = lm.Count( ) != 0;
				_availableScienceExperiments[ s.ScienceExperiment.id ] = IsAvailable;
			}
			return IsAvailable;
		}



		public ModuleScienceExperiment FindExperiment( ScienceInstance s, bool runSingleUse = true )
		{

			ModuleScienceExperiment m = null;

			if( _moduleScienceExperiments != null && _moduleScienceExperiments.Count > 0 )
			{
				IEnumerable<ModuleScienceExperiment> lm = _moduleScienceExperiments.Where(x => (
					x.experimentID == s.ScienceExperiment.id &&
					!(x.GetScienceCount() > 0) &&
					(x.rerunnable || runSingleUse) &&
					!x.Inoperable
					));
				if (lm.Count() != 0)
					m = lm.First();
			}
			return m;
		}



		public void RunExperiment( ScienceInstance s, bool runSingleUse = true )
		{
			ModuleScienceExperiment m = FindExperiment( s, runSingleUse );
			if( m != null )
			{
				_logger.Trace( "Running Experiment " + m.experimentID + " on part " + m.part.partInfo.name );
				RunStandardModuleScienceExperiment( m );
				return;
			}
		}



		public void RunStandardModuleScienceExperiment( ModuleScienceExperiment exp )
		{
			if( exp.Inoperable ) return;

/*			if (Config.HideExperimentResultsDialog)
			{
				if (!exp.useStaging)
				{
					exp.useStaging = true;
					exp.OnActive();
					exp.useStaging = false;
				}
				else
					exp.OnActive();
			}
			else*/
				exp.DeployExperiment( );
		}
	}
}
