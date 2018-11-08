﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.Localization;


namespace ScienceChecklist
{
	class StatusWindow : Window<ScienceChecklistAddon>
	{
		public event EventHandler				NoiseEvent;
		private readonly Texture2D				_emptyTexture;

		private readonly Texture2D				_progressTexture;
		private readonly Texture2D				_completeTexture;

		private readonly Texture2D				_GfxTimeWarp;
		private readonly Texture2D				_GfxTimeWarpOff;
		private readonly Texture2D				_GfxAudioAlert;
		private readonly Texture2D				_GfxAudioAlertOff;
		private readonly Texture2D				_GfxResultsWindow;
		private readonly Texture2D				_GfxResultsWindowOff;

		private readonly ExperimentFilter		_filter;
		private readonly ScienceChecklistAddon	_parent;
		private readonly Logger					_logger;
		private int								_previousNumExperiments;
		private float							 _previousUiScale;
		private GUIStyle						_experimentButtonStyle;
		private GUIStyle						_experimentLabelStyle;
		private GUIStyle						_situationStyle;
		private GUIStyle						_horizontalScrollbarOnboardStyle;
		private GUIStyle						_progressLabelStyle;
		private	IList<ModuleScienceExperiment>	_moduleScienceExperiments;
		private IList<ModuleScienceExperiment>	_DMModuleScienceAnimates;
		private IList<ModuleScienceExperiment>	_DMModuleScienceAnimateGenerics;
		private Dictionary<string, bool>		_availableScienceExperiments;

		public event EventHandler OnCloseEvent;
		public event EventHandler OnOpenEvent;

		public StatusWindow( ScienceChecklistAddon Parent )
			: base( Localizer.Format("#autoLOC_[x]_Science!_129")/*[x] Science! Here and Now*/, 250, 30 )
		{
			_parent = Parent;
			UiScale = _parent.Config.UiScale;
			_logger = new Logger( this );
			_filter = new ExperimentFilter( _parent );
			_filter.DisplayMode = DisplayMode.CurrentSituation;
			_filter.EnforceLabLanderMode = true;

			_emptyTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			_emptyTexture.SetPixels(new[] { Color.clear });
			_emptyTexture.Apply();
			_progressTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.scienceProgress.png", 13, 13 );
			_completeTexture =						TextureHelper.FromResource( "ScienceChecklist.icons.scienceComplete.png", 13, 13 );

			_GfxTimeWarp =							TextureHelper.FromResource( "ScienceChecklist.icons.time-warp.png", 13, 13 );
			_GfxTimeWarpOff =						TextureHelper.FromResource( "ScienceChecklist.icons.time-warp-x.png", 13, 13 );
			_GfxAudioAlert =						TextureHelper.FromResource( "ScienceChecklist.icons.audio-alert.png", 13, 13 );
			_GfxAudioAlertOff =						TextureHelper.FromResource( "ScienceChecklist.icons.audio-alert-off.png", 13, 13 );
			_GfxResultsWindow =						TextureHelper.FromResource( "ScienceChecklist.icons.report.png", 13, 13 );
			_GfxResultsWindowOff =					TextureHelper.FromResource( "ScienceChecklist.icons.report-x.png", 13, 13 );

			_availableScienceExperiments = new Dictionary<string, bool>( );

			_parent.Config.HideCompleteEventsChanged += ( s, e ) => RefreshFilter( s, e );
			_parent.Config.CompleteWithoutRecoveryChanged += ( s, e ) => RefreshFilter( s, e );

			_parent.ScienceEventHandler.FilterUpdateEvent += ( s, e ) => RefreshFilter( s, e );
			_parent.ScienceEventHandler.SituationChanged += ( s, e ) => UpdateSituation( s, e );
			this.Resizable = false;
			_filter.UpdateFilter( );
			_parent.Config.UiScaleChanged += OnUiScaleChange;
		}



		protected override void ConfigureStyles( )
		{
			base.ConfigureStyles( );

			_progressLabelStyle = new GUIStyle( _skin.label )
			{
				fontStyle = FontStyle.BoldAndItalic,
				alignment = TextAnchor.MiddleCenter,
				fontSize = wScale(11),
				normal = {
					textColor = new Color(0.322f, 0.298f, 0.004f)
				}
			};

			_horizontalScrollbarOnboardStyle = new GUIStyle( _skin.horizontalScrollbar )
			{
				normal = { background = _emptyTexture }
			};

			_situationStyle = new GUIStyle( _skin.label )
			{
				fontSize = wScale(13),
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Normal,
				fixedHeight = wScale(25),
				contentOffset = wScale(new Vector2(0, 6)),
				wordWrap = true,
				normal = {
					textColor = new Color(0.7f, 0.8f, 0.8f)
				}
			};
			_experimentButtonStyle = new GUIStyle( _skin.button )
			{
				fontSize = wScale(14)
			};
			_experimentLabelStyle = new GUIStyle( _experimentButtonStyle )
			{
				fontSize = wScale(14),
				normal = { textColor = Color.black }
			};
		}



		private void OnUiScaleChange( object sender, EventArgs e )
		{
			UiScale = _parent.Config.UiScale;
			_progressLabelStyle = null;
			_horizontalScrollbarOnboardStyle = null;
			_situationStyle = null;
			_experimentButtonStyle = null;
			_experimentLabelStyle = null;

			base.OnUiScaleChange( );
			ConfigureStyles( );
		}



		protected override void DrawWindowContents( int windowID )
		{
			GUILayout.BeginVertical( );

			if( _filter.CurrentSituation != null && _parent.Science.CurrentVesselScience != null)
			{
				var desc = _filter.CurrentSituation.Description;
				GUILayout.Box
				(
					new GUIContent
					(
						char.ToUpper( desc[ 0 ] ) + desc.Substring( 1 ),
						MakeSituationToolTip( )
					),
					_situationStyle,
					GUILayout.Width(wScale(250))
				);
			}
			int Top = wScale(65);
			if( _filter.DisplayScienceInstances != null )
			{
				for( var i = 0; i < _filter.DisplayScienceInstances.Count; i++ )
				{
					var rect = new Rect(wScale(5), Top, wScale(250), wScale(30));
					var experiment = _filter.DisplayScienceInstances[ i ];
					DrawExperiment( experiment, rect );
					Top += wScale(35);
				}
			}
			else
				_logger.Trace("DisplayExperiments is null");



			if( _filter.DisplayScienceInstances.Count > 0 )
				GUILayout.Space( wScale( _filter.DisplayScienceInstances.Count * 35 ) ); // Leave space for experiments, as drawn above
			GUILayout.Space( wScale( 10 ) );

			GUILayout.BeginHorizontal( );
			GUIContent Content = null;
			if( _parent.Config.StopTimeWarp )
				Content = new GUIContent( _GfxTimeWarp, Localizer.Format("#autoLOC_[x]_Science!_131")/*Time warp will be stopped*/ );
			else
				Content = new GUIContent( _GfxTimeWarpOff, Localizer.Format("#autoLOC_[x]_Science!_132")/*Time warp will not be stopped*/ );
			if( GUILayout.Button( Content, GUILayout.Width( wScale( 36 ) ), GUILayout.Height( wScale( 32 ) ) ) )
			{
				_parent.Config.StopTimeWarp = !_parent.Config.StopTimeWarp;
				_parent.Config.Save( );
			}



			if( _parent.Config.PlayNoise )
				Content = new GUIContent( _GfxAudioAlert, Localizer.Format("#autoLOC_[x]_Science!_133")/*Audio alert will sound*/ );
			else
				Content = new GUIContent( _GfxAudioAlertOff, Localizer.Format("#autoLOC_[x]_Science!_134")/*No audio alert*/ );
			if( GUILayout.Button( Content, GUILayout.Width( wScale( 36 ) ), GUILayout.Height( wScale( 32 ) ) ) )
			{
				_parent.Config.PlayNoise = !_parent.Config.PlayNoise;
				_parent.Config.Save( );
			}
			
			
			
			if( _parent.Config.ShowResultsWindow )
				Content = new GUIContent( _GfxResultsWindow, Localizer.Format("#autoLOC_[x]_Science!_135")/*Show results window*/ );
			else
				Content = new GUIContent( _GfxResultsWindowOff, Localizer.Format("#autoLOC_[x]_Science!_136")/*Supress results window*/ );
			if( GUILayout.Button( Content, GUILayout.Width( wScale( 36 ) ), GUILayout.Height( wScale( 32 ) ) ) )
			{
				_parent.Config.ShowResultsWindow = !_parent.Config.ShowResultsWindow;
				_parent.Config.Save( );
			}
			GUILayout.EndHorizontal( );
			GUILayout.EndVertical( );

			GUILayout.Space(wScale(2));

		}



		public override void DrawWindow( )
		{
			// The window needs to get smaller when the number of experiments drops.
			// This allows that while preventing flickering.
			if (_previousNumExperiments != _filter.DisplayScienceInstances.Count || _parent.Config.UiScale != _previousUiScale)
			{
				windowPos.height = wScale(30) + ((_filter.DisplayScienceInstances.Count + 1) * wScale(35));
				windowPos.width = wScale(defaultWindowSize.x);
				_previousNumExperiments = _filter.DisplayScienceInstances.Count;
				_previousUiScale = _parent.Config.UiScale;
			}

			base.DrawWindow( );
		}



		private string MakeSituationToolTip( )
		{
			string Text = "";

			

			if( _filter.CurrentSituation != null )
			{
				Body Body = _filter.CurrentSituation.Body;
				Text += Localizer.Format("#autoLOC_[x]_Science!_111")/*Body: */ + GameHelper.LocalizeBodyName( Body.CelestialBody.displayName ) + "\n";
				Text += Body.Type;
				if( Body.IsHome )
					Text += Localizer.Format("#autoLOC_[x]_Science!_112")/* - Home!*/;
				Text += "\n\n";
				Text += Localizer.Format("#autoLOC_[x]_Science!_113")/*Space high: */ + (Body.CelestialBody.scienceValues.spaceAltitudeThreshold/1000) + "km\n";

				if( Body.HasAtmosphere )
				{
					Text += Localizer.Format("#autoLOC_[x]_Science!_137")/*Atmos depth: */ + (Body.CelestialBody.atmosphereDepth/1000) + "km\n";
					Text += Localizer.Format("#autoLOC_[x]_Science!_138")/*Flying high: */ + (Body.CelestialBody.scienceValues.flyingAltitudeThreshold/1000) + "km\n";
					if( Body.HasOxygen )
						Text += Localizer.Format("#autoLOC_[x]_Science!_139")/*Has oxygen - jets work\n*/;
				}
				else
					Text += Localizer.Format("#autoLOC_[x]_Science!_140")/*No kind of atmosphere\n*/;

				if( Body.HasSurface )
				{
					if( Body.HasOcean )
						Text += Localizer.Format("#autoLOC_[x]_Science!_141")/*Has oceans\n*/;
				}
				else
					Text += Localizer.Format("#autoLOC_[x]_Science!_142")/*No surface\n*/;

				Text += "\n";
			}

			Text += Localizer.Format("#autoLOC_[x]_Science!_143")/*Current vessel: */ + _parent.Science.CurrentVesselScience.Count( ) + Localizer.Format("#autoLOC_[x]_Science!_144")/* stored experiments*/;

			return Text;
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
			Rect buttonRect = new Rect(rect) { xMax = wScale(200) };
			
			if( ExperimentRunnable )
			{
				_experimentButtonStyle.normal.textColor = exp.IsComplete ? Color.green : Color.yellow;
				if( GUI.Button( buttonRect, exp.ShortDescription, _experimentButtonStyle ) )
				{
					RunExperiment( exp );
				}
			}
			else
			{
				GUI.Label( buttonRect, exp.ShortDescription, _experimentLabelStyle );
			}
			int Dif = (int)(((rect.yMax - rect.yMin) - wScale(13)) / 2);
			Rect progressRect = new Rect(wScale(205), rect.yMin + Dif, wScale(50), wScale(13));
			ProgressBar( progressRect, exp.CompletedScience, exp.TotalScience, exp.CompletedScience + exp.OnboardScience );
		}



		private void ProgressBar( Rect rect, float curr, float total, float curr2 )
		{
			var completeTexture = _completeTexture;
			var progressTexture = _progressTexture;
			var complete = curr > total || (total - curr < 0.1);
			if( complete )
				curr = total;
			var progressRect = new Rect(rect.xMin, rect.yMin, rect.width, wScale(13));

			GUI.skin.horizontalScrollbar.fixedHeight = wScale(13);
			GUI.skin.horizontalScrollbarThumb.fixedHeight = wScale(13);

			if (curr2 != 0 && !complete)
			{
				var complete2 = false;
				if (curr2 > total || (total - curr2 < 0.1))
				{
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

			var showValues = true;
			if( showValues )
			{
				var labelRect = new Rect( progressRect )
				{
					y = progressRect.y + 1,
				};
				GUI.Label(labelRect, string.Format("{0:0.#}  /  {1:0.#}", curr, total), _progressLabelStyle);
			}
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
			if( _DMModuleScienceAnimates != null )
				_DMModuleScienceAnimates.Clear( );
			if( _DMModuleScienceAnimateGenerics != null )
				_DMModuleScienceAnimateGenerics.Clear( );



			Vessel v = FlightGlobals.ActiveVessel;
			if( v != null && HighLogic.LoadedScene == GameScenes.FLIGHT )
			{
				_moduleScienceExperiments = v.FindPartModulesImplementing<ModuleScienceExperiment>( );
				_DMModuleScienceAnimates = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => _parent.DMagic.inheritsFromOrIsDMModuleScienceAnimate(x)).ToList();
				_DMModuleScienceAnimateGenerics = v.FindPartModulesImplementing<ModuleScienceExperiment>().Where(x => _parent.DMagic.inheritsFromOrIsDMModuleScienceAnimateGeneric(x)).ToList();
			}



			_filter.UpdateFilter( );
		}



		// Bung new situation into filter and recalculate everything
		public void UpdateSituation( object sender, NewSituationData e )
		{
//			_logger.Trace( "UpdateSituation" );
			if( e == null )
			{
				_filter.CurrentSituation = null;
				return;
			}
			else
				_filter.CurrentSituation = new Situation( e._body, e._situation, e._biome, e._subBiome );
			RefreshFilter( sender, e );



			_logger.Trace("ScienceThisBiome:" + _filter.TotalCount + " / " + _filter.CompleteCount );
			if( _filter.TotalCount > 0 )
			{
				var anyRunnableExperiments = false;
				for( var i = 0; i < _filter.DisplayScienceInstances.Count; i++ )
				{
					var experiment = _filter.DisplayScienceInstances[ i ];
					var Id = experiment.ScienceExperiment.id;

					if( Id == "crewReport" || Id == "surfaceSample" || Id == "evaReport" ) // Always pop UI for Kerbal experiments
					{
						anyRunnableExperiments = true;
						break;
					}
					else
					{
						if( CanRunExperiment( experiment ) )
						{
							anyRunnableExperiments = true;
							break;
						}
					}
				}
				
				if( anyRunnableExperiments )
				{
					if( IsVisible( ) )
					{
						if( _parent.Config.StopTimeWarp )
							GameHelper.StopTimeWarp( );
						if( _parent.Config.PlayNoise )
							PlayNoise( );
						if( _parent.Config.StopTimeWarp || _parent.Config.PlayNoise )
							ScreenMessages.PostScreenMessage( Localizer.Format("#autoLOC_[x]_Science!_146")/*New Situation: */ + _filter.CurrentSituation.Description );
					}
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

		

		public void RunExperiment(ScienceInstance s, bool runSingleUse = true)
		{
			_logger.Trace( Localizer.Format("#autoLOC_[x]_Science!_147")/*Finding Module for Science Report: */ + s.ScienceExperiment.id );
			ModuleScienceExperiment m = null;



			// If possible run with DMagic new API
			if( _DMModuleScienceAnimateGenerics != null && _DMModuleScienceAnimateGenerics.Count > 0)
			{
				DMModuleScienceAnimateGeneric NewDMagicInstance = _parent.DMagic.GetDMModuleScienceAnimateGeneric( );
				if( NewDMagicInstance != null )
				{
					IEnumerable<ModuleScienceExperiment> lm = _DMModuleScienceAnimateGenerics.Where(x => (
						x.experimentID == s.ScienceExperiment.id &&
						!x.Inoperable &&
						((int)x.Fields.GetValue("experimentLimit") > 1 ? NewDMagicInstance.canConduct(x) : NewDMagicInstance.canConduct(x) && (x.rerunnable || runSingleUse))
						));
					if (lm.Count() != 0)
						m = lm.First();

					if (m != null)
					{
						_logger.Debug("Running DMModuleScienceAnimateGenerics Experiment" + m.experimentID + "on part" + m.part.partInfo.name);
						NewDMagicInstance.gatherScienceData( m, !_parent.Config.ShowResultsWindow );
						return;
					}
				}
			}



			// If possible run with DMagic DMAPI
			if( _DMModuleScienceAnimates != null && _DMModuleScienceAnimates.Count > 0)
			{
				DMAPI DMAPIInstance = _parent.DMagic.GetDMAPI( );
				if( DMAPIInstance != null )
				{
					IEnumerable<ModuleScienceExperiment> lm = _DMModuleScienceAnimates.Where(x =>
					{
						return x.experimentID == s.ScienceExperiment.id &&
						!x.Inoperable &&
						((int)x.Fields.GetValue("experimentLimit") > 1 ? DMAPIInstance.experimentCanConduct(x) : DMAPIInstance.experimentCanConduct(x) && (x.rerunnable || runSingleUse));
					});
					if (lm.Count() != 0)
						m = lm.First();

					if (m != null)
					{
						_logger.Trace("Running DMModuleScienceAnimateGenerics Experiment" + m.experimentID + "on part" + m.part.partInfo.name);
						DMAPIInstance.deployDMExperiment( m, !_parent.Config.ShowResultsWindow );
						return;
					}
				}
			}



			// Do stock run
			m = FindExperiment( s, runSingleUse );
			if( m != null )
			{
				_logger.Trace("Running Experiment" + m.experimentID + "on part" + m.part.partInfo.name );
				RunStandardModuleScienceExperiment( m );
				return;
			}


		}



		public void RunStandardModuleScienceExperiment( ModuleScienceExperiment exp )
		{
			if( exp.Inoperable ) return;

			if( _parent.Config.ShowResultsWindow )
				exp.DeployExperiment( );
			else
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
		}





		public WindowSettings BuildSettings( )
		{
//_logger.Info( "BuildSettings" );
			WindowSettings W = new WindowSettings( ScienceChecklistAddon.WINDOW_NAME_STATUS );
			W.Set( "Top", (int)windowPos.yMin );
			W.Set( "Left", (int)windowPos.xMin );
			W.Set( "Visible", IsVisible( ) );

			return W;
		}



		public void ApplySettings( WindowSettings W )
		{
			windowPos.yMin = W.GetInt( "Top", 40 );
			windowPos.xMin = W.GetInt( "Left", 40 );
			windowPos.yMax = windowPos.yMin + wScale( 30 );
			windowPos.xMax = windowPos.xMin + wScale( 250 );


			bool TempVisible = false;
			TempVisible = W.GetBool( "Visible", false );
			if( TempVisible )
				OnOpenEvent( this, EventArgs.Empty );
			else
				OnCloseEvent( this, EventArgs.Empty );
		}




/*
		
		/// </summary>


		/// <summary>
		/// Run one instance of each type of experiment on the vessel
		/// </summary>
		/// <param name="onlyIncomplete">
		/// Only run experiments that aren't complete.
		/// </param>
		public void RunExperimentsOnce(bool onlyIncomplete)
		{
			IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

			foreach (ScienceInstance s in dsi)
			{
				RunExperiment(s);
			}
		}

		/// <summary>
		/// Run every single experiment on a vessel
		/// </summary>
		/// <param name="onlyIncomplete">
		/// Only run experiments that aren't complete.
		/// </param>
		public void RunEveryExperiment(bool onlyIncomplete)
		{
			IList<ScienceInstance> dsi = _filter.DisplayScienceInstances.Where(x => (!x.IsCollected || !onlyIncomplete)).ToList();

			foreach (ScienceInstance s in dsi)
			{
				if (ModuleScienceExperiments != null && ModuleScienceExperiments.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = ModuleScienceExperiments.Where(x => (x.experimentID == s.ScienceExperiment.id && !(x.GetScienceCount() > 0) && !x.Inoperable));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						RunStandardModuleScienceExperiment(mse);
					}
				}

				if (DMModuleScienceAnimates != null && DMModuleScienceAnimates.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimates.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMAPI.experimentCanConduct(x)));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running DMModuleScienceAnimates Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						DMagic.DMAPI.deployDMExperiment(mse);
					}
				}

				if (DMModuleScienceAnimateGenerics != null && DMModuleScienceAnimateGenerics.Count > 0)
				{
					IEnumerable<ModuleScienceExperiment> lm = DMModuleScienceAnimateGenerics.Where(x => (x.experimentID == s.ScienceExperiment.id && !x.Inoperable && DMagic.DMModuleScienceAnimateGeneric.canConduct(x)));
					foreach (ModuleScienceExperiment mse in lm)
					{
						_logger.Trace("Running DMModuleScienceAnimateGenerics Experiment " + mse.experimentID + " on part " + mse.part.partInfo.name);
						DMagic.DMModuleScienceAnimateGeneric.gatherScienceData(mse);
					}
				}
			}
		}

}*/

















	}
}
